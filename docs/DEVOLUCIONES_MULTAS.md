# Módulo Devoluciones, Multas y Pagos

Propietario: Frank (lotes FK-4 a FK-7).  
Última actualización: 2026-05-07.

---

## Resumen

El módulo gestiona el ciclo completo de devolución de libros prestados:
recepción física, inspección, generación automática de multas según condición y
retraso, registro de pagos en tesorería, condonaciones administrativas y reportes
ejecutivos para administración.

---

## Entidades de dominio

### `Return`

Registro de una devolución física de ejemplar.

| Campo              | Tipo               | Restricciones                          |
|--------------------|--------------------|----------------------------------------|
| `Id`               | `Guid`             | PK                                     |
| `LoanId`           | `Guid`             | FK → `loans`, único (un préstamo = una devolución) |
| `ReturnedAt`       | `DateTime` UTC     | obligatorio                            |
| `Condition`        | `ReturnCondition`  | `OK` · `DAMAGED` · `LOST`             |
| `InspectionNotes`  | `string?`          | max 1 000 chars                        |
| `ReceivedByUserId` | `Guid`             | FK → `users` (bibliotecario receptor) |
| `DeletedAt`        | `DateTime?`        | soft-delete                            |

### `Fine`

Multa asociada a una devolución (relación 1-a-1 con `Return`).

| Campo           | Tipo          | Restricciones                              |
|-----------------|---------------|--------------------------------------------|
| `Id`            | `Guid`        | PK                                         |
| `ReturnId`      | `Guid`        | FK → `returns`, único                      |
| `UserId`        | `Guid`        | FK → `users` (deudor)                      |
| `Reason`        | `FineReason`  | `LATE` · `DAMAGE` · `LOSS`                |
| `Amount`        | `decimal`     | `numeric(10,2)`, calculado al crear        |
| `DaysLate`      | `int?`        | sólo para `LATE`                           |
| `Status`        | `FineStatus`  | ver diagrama de estados                    |
| `CreatedAt`     | `DateTime`    | UTC, auto                                  |
| `PaidAt`        | `DateTime?`   | UTC, cuando se paga o condona              |
| `PaidByUserId`  | `Guid?`       | FK → `users` (tesorería / admin)           |
| `WaivedReason`  | `string?`     | max 500 chars, motivo escrito de condonación |
| `DeletedAt`     | `DateTime?`   | soft-delete                                |

> `Fine` usa `UseXminAsConcurrencyToken` (xmin de PostgreSQL) para detectar
> escrituras concurrentes durante pago/condonación.

### `FineConfig`

Tarifa vigente para el cálculo de multas. Sólo existe un registro activo
(el de mayor `EffectiveFrom` ≤ hoy).

| Campo               | Tipo       | Descripción                        |
|---------------------|------------|------------------------------------|
| `Id`                | `Guid`     | PK                                 |
| `LateRatePerDayMxn` | `decimal`  | MXN por día de retraso             |
| `DamageFlatMxn`     | `decimal`  | Cargo fijo por daño                |
| `LossFlatMxn`       | `decimal`  | Cargo fijo por pérdida             |
| `EffectiveFrom`     | `DateTime` | Fecha de vigencia                  |

---

## Diagrama de estados de `Fine`

```
                    ┌────────────────────────────────────────┐
                    │                                        │
  [Devolución]      ▼                                        │
  ──────────► PENDING ──── ConfirmPayment ────► PAID        │
             (tesorería / admin: TreasuryOrAdmin)            │
                    │                                        │
                    └──── WaiveFine ──────────► WAIVED      │
                         (admin: AdminOnly)                  │
                                                             │
  No existen transiciones de PAID ó WAIVED a ningún otro ───┘
  estado. Son terminales.
```

**Reglas de transición:**

| Desde     | Hacia    | Endpoint                        | Policy           |
|-----------|----------|---------------------------------|------------------|
| `PENDING` | `PAID`   | `POST /api/multas/{id}/pagos`   | `TreasuryOrAdmin`|
| `PENDING` | `WAIVED` | `POST /api/multas/{id}/condonar`| `AdminOnly`      |

---

## Cálculo de multas

`FineCalculator` evalúa cada `Return` y genera hasta tres `Fine` distintas
(una por razón que aplique):

| Razón    | Condición para generarla               | Fórmula                                      |
|----------|----------------------------------------|----------------------------------------------|
| `LATE`   | `ReturnedAt > Loan.DueAt`              | `DaysLate × FineConfig.LateRatePerDayMxn`   |
| `DAMAGE` | `Condition == DAMAGED`                 | `FineConfig.DamageFlatMxn`                   |
| `LOSS`   | `Condition == LOST`                    | `FineConfig.LossFlatMxn`                     |

`DaysLate` = días naturales enteros entre `Loan.DueAt` y `ReturnedAt` (redondeado
hacia arriba, mínimo 1).

Configuración activa por defecto (sembrada):

| Tarifa               | Valor    |
|----------------------|----------|
| Retraso por día      | $10 MXN  |
| Cargo por daño       | $200 MXN |
| Cargo por pérdida    | $500 MXN |

---

## Endpoints

### Devoluciones

| Método | Ruta                       | Policy           | Descripción                          |
|--------|----------------------------|------------------|--------------------------------------|
| `POST` | `/api/devoluciones`        | `AdminOrLibrarian` | Registra devolución y genera multas |
| `GET`  | `/api/devoluciones/{id}`   | `AdminOrLibrarian` | Detalle de una devolución            |

**`POST /api/devoluciones`** — body:
```json
{
  "loanId": "uuid",
  "condition": "OK | DAMAGED | LOST",
  "inspectionNotes": "opcional, max 1000 chars"
}
```

### Multas

| Método | Ruta                             | Policy           | Descripción                        |
|--------|----------------------------------|------------------|------------------------------------|
| `GET`  | `/api/multas`                    | `AdminOrLibrarian` | Listar multas (filtros: userId, status) |
| `GET`  | `/api/multas/{id}`               | `Authenticated`  | Detalle (usuario sólo ve las suyas)|
| `GET`  | `/api/usuarios/{userId}/multas`  | `Authenticated`  | Multas de un usuario               |
| `POST` | `/api/multas/{id}/pagos`         | `TreasuryOrAdmin`| Confirmar pago                     |
| `POST` | `/api/multas/{id}/condonar`      | `AdminOnly`      | Condonar multa                     |
| `GET`  | `/api/configuracion-multas`      | `Authenticated`  | Tarifa vigente                     |
| `PUT`  | `/api/configuracion-multas`      | `AdminOnly`      | Actualizar tarifa                  |

**`POST /api/multas/{id}/pagos`** — body:
```json
{
  "method": "CASH | TRANSFER | CARD",
  "reference": "opcional"
}
```

**`POST /api/multas/{id}/condonar`** — body:
```json
{
  "reason": "motivo de la condonación, obligatorio"
}
```

### Reportes (AdminOnly)

| Método | Ruta                                               | Descripción                                     |
|--------|----------------------------------------------------|-------------------------------------------------|
| `GET`  | `/api/reportes/multas-recaudadas?from=&to=`        | Suma PAID por mes y por motivo en rango         |
| `GET`  | `/api/reportes/multas-pendientes`                  | Deudores ordenados por monto desc               |
| `GET`  | `/api/reportes/devoluciones-tardias?from=&to=`     | Conteo y promedio de días de retraso en rango   |
| `GET`  | `/api/reportes/condonaciones?from=&to=`            | Fines WAIVED con motivo y condonador en rango   |

Formato de fechas: `yyyy-MM-dd` (ISO 8601).

---

## Reglas de negocio críticas

1. **Bloqueo de préstamos**: un usuario con al menos una `Fine` en estado
   `PENDING` no puede recibir nuevos préstamos (validado en `IUserEligibilityService`).

2. **Una devolución por préstamo**: restricción de BD `uq_returns_loan_id`.
   Intentar devolver dos veces el mismo préstamo retorna `409 Conflict`.

3. **Multas son inmutables salvo vía endpoints autorizados**: la entidad `Fine`
   no expone setters públicos masivos; los únicos cambios de estado pasan por
   `FineService.ConfirmPaymentAsync` y `FineService.WaiveAsync`.

4. **El motivo de condonación se persiste**: `Fine.WaivedReason` almacena el
   texto libre enviado en `WaiveFineRequest.Reason` (max 500 chars), auditable
   en `audit_logs`.

5. **Tarifa de multa configurable en caliente**: `FineConfig` permite cambiar
   tarifas sin redespliegue. El cálculo usa siempre la config con mayor
   `EffectiveFrom` ≤ momento de la devolución.

---

## Flujo completo de devolución

```
Bibliotecario                    Backend                              Max (notif.)
     │                              │                                      │
     │  POST /api/devoluciones      │                                      │
     │─────────────────────────────►│                                      │
     │                              │  Valida Loan ACTIVE/OVERDUE          │
     │                              │  Crea Return                         │
     │                              │  FineCalculator → crea Fine(s)       │
     │                              │  Dispatch LoanReturned ──────────────►│
     │                              │  Dispatch FineCreated ───────────────►│
     │  ReturnDto + [FineDto]       │                                      │
     │◄─────────────────────────────│                                      │
     │                              │                                      │
  (usuario recibe notificación de multa vía Max)                          │
     │                              │                                      │
     │  POST /api/multas/{id}/pagos │                                      │
     │─────────────────────────────►│                                      │
     │                              │  Fine.Status PENDING → PAID          │
     │                              │  Dispatch FinePaid                   │
     │  FineDto (status: PAID)      │                                      │
     │◄─────────────────────────────│                                      │
```

---

## Eventos de dominio publicados

| Evento         | Publicado cuando                 | Consumido por               |
|----------------|----------------------------------|-----------------------------|
| `LoanReturned` | Devolución registrada            | Max: promueve cola reservas |
| `FineCreated`  | Se genera una multa              | Max: notifica al usuario    |
| `FinePaid`     | Multa confirmada como pagada     | (informativo, no hay handler activo) |

---

## Frontend

Rutas Angular (todas bajo `/dashboard`, requieren `authGuard`):

| Ruta                              | Componente               | Visible para         |
|-----------------------------------|--------------------------|----------------------|
| `/dashboard/devoluciones`         | `ReturnsPageComponent`   | Admin, Librarian     |
| `/dashboard/devoluciones/nueva`   | `NewReturnPageComponent` | Admin, Librarian     |
| `/dashboard/multas`               | `FinesPageComponent`     | Todos los autenticados |
| `/dashboard/multas/:id`           | `FineDetailPageComponent`| Todos (sólo propias) |
| `/dashboard/configuracion/multas` | `FineConfigPageComponent`| Admin                |
| `/dashboard/reportes`             | `ReportsPageComponent`   | Admin                |

La exportación CSV de reportes se genera completamente en cliente
(`Blob` + `URL.createObjectURL`) sin dependencias adicionales.
