# Módulo Préstamos y Renovaciones

Propietario: Marcelo (lotes MX-3 a MX-7).  
Última actualización: 2026-05-07.

---

## Resumen

El módulo gestiona el ciclo completo de préstamo de ejemplares: entrega al usuario,
control de límites por rol, renovaciones, detección automática de vencidos y
exposición de KPIs para el dashboard de administración.

---

## Entidades de dominio

### `Loan`

| Campo | Tipo | Restricciones |
|-------|------|---------------|
| `Id` | `Guid` | PK |
| `UserId` | `Guid` | FK → `users` |
| `BookCopyId` | `Guid` | FK → `book_copies` |
| `LoanedAt` | `DateTime` UTC | obligatorio |
| `DueAt` | `DateTime` UTC | obligatorio |
| `ReturnedAt` | `DateTime?` UTC | nulo hasta devolución |
| `Status` | `LoanStatus` | ver diagrama de estados |
| `RenewalCount` | `int` | 0-2 |
| `IssuedByUserId` | `Guid` | FK → `users` (bibliotecario) |
| `DeletedAt` | `DateTime?` | soft-delete |

### `LoanRenewal`

Historial de cada renovación sobre un préstamo.

| Campo | Tipo | Restricciones |
|-------|------|---------------|
| `Id` | `Guid` | PK |
| `LoanId` | `Guid` | FK → `loans` |
| `RenewedAt` | `DateTime` UTC | obligatorio |
| `PreviousDueAt` | `DateTime` UTC | `DueAt` antes de renovar |
| `NewDueAt` | `DateTime` UTC | `DueAt` después de renovar |
| `RenewedByUserId` | `Guid` | FK → `users` |

---

## Diagrama de estados de `Loan`

```
                    ┌─────────────────────────────────────────────────────┐
                    │                                                     │
  [Crear préstamo]  ▼           [Background job / 1h]                    │
  ────────────► ACTIVE ──────── MarkOverdue ──────────► OVERDUE          │
                    │            (DueAt < now)                │           │
                    │                                         │           │
                    └──────────── POST /devoluciones ─────────┴──► RETURNED
                                  (cualquier estado activo)
                                                              │
  [BookCopy perdida]                                          ▼
  Condition == LOST en devolución ──────────────────────► LOST
```

**Reglas de transición:**

| Desde | Hacia | Disparador |
|-------|-------|-----------|
| `ACTIVE` | `OVERDUE` | `OverdueLoansBackgroundService` (cada 1 h) |
| `ACTIVE` | `RETURNED` | `POST /api/devoluciones` con condition `OK` o `DAMAGED` |
| `OVERDUE` | `RETURNED` | `POST /api/devoluciones` con condition `OK` o `DAMAGED` |
| `ACTIVE` | `LOST` | `POST /api/devoluciones` con condition `LOST` |
| `OVERDUE` | `LOST` | `POST /api/devoluciones` con condition `LOST` |

`RETURNED` y `LOST` son estados terminales. El handler `LoanReturnedHandler` en
`Application/Loans/EventHandlers` actualiza `Loan.Status` y `BookCopy.Status`
al recibir el evento `LoanReturned`.

---

## Reglas de negocio críticas

1. **Límites por rol** (`appsettings.json › Loans.MaxActivePerRole`):

   | Rol | Máx. préstamos activos | Duración por defecto |
   |-----|------------------------|----------------------|
   | `STUDENT` | 3 | 7 días |
   | `TEACHER` | 5 | 21 días |
   | `LIBRARIAN` | 999 | 14 días |
   | `ADMIN` | 999 | 14 días |

   El bibliotecario puede sobrescribir la duración con `DurationDaysOverride` (1-60 días).

2. **Elegibilidad**: `IUserEligibilityService` bloquea el préstamo si el usuario tiene
   al menos una `Fine` en estado `PENDING`.

3. **Límite de renovaciones**: máximo 2 renovaciones por préstamo (`RenewalCount ≤ 2`).
   Cada renovación extiende `DueAt` en 7 días desde el momento de la solicitud.

4. **Ejemplar disponible**: `BookCopy.Status` debe ser `AVAILABLE` al momento de crear
   el préstamo. El servicio lo marca `CHECKED_OUT` en la misma transacción.

5. **Atomicidad**: `LoanService.CreateAsync` usa `IUnitOfWork` con transacción explícita
   (BeginTransaction → Commit/Rollback) para garantizar que el préstamo y el cambio de
   estado del ejemplar sean atómicos.

---

## Endpoints

### Préstamos

| Método | Ruta | Policy | Descripción |
|--------|------|--------|-------------|
| `POST` | `/api/prestamos` | `AdminOrLibrarian` | Crear préstamo |
| `GET` | `/api/prestamos` | `AdminOrLibrarian` | Listar todos (filtro `?status=ACTIVE\|OVERDUE\|...`) |
| `GET` | `/api/prestamos/{id}` | `Authenticated` | Detalle (usuario sólo ve el suyo) |
| `POST` | `/api/prestamos/{id}/renovaciones` | `Authenticated` | Renovar préstamo |
| `GET` | `/api/usuarios/{userId}/prestamos` | `Authenticated` | Préstamos de un usuario |

**`POST /api/prestamos`** — body:
```json
{
  "userId": "uuid",
  "bookCopyId": "uuid",
  "durationDaysOverride": null
}
```

**`LoanDto`** — respuesta:
```json
{
  "id": "uuid",
  "userId": "uuid",
  "userFullName": "María López",
  "bookCopyId": "uuid",
  "bookTitle": "El Aleph",
  "isbn": "9788420674208",
  "loanedAt": "2026-05-07T10:00:00Z",
  "dueAt": "2026-05-14T10:00:00Z",
  "returnedAt": null,
  "status": "ACTIVE",
  "renewalCount": 0,
  "renewals": []
}
```

---

## Background Services

### `OverdueLoansBackgroundService`

- Intervalo: cada **1 hora**.
- Llama `LoanService.MarkOverdueAsync()`.
- Consulta todos los préstamos `ACTIVE` cuyo `DueAt < UtcNow`.
- Los transiciona a `OVERDUE` en lote con `SaveChangesAsync` (sin transacción individual
  por registro para reducir round-trips).

---

## Eventos de dominio publicados

| Evento | Publicado cuando | Consumido por |
|--------|-----------------|---------------|
| `LoanCreated` | Préstamo creado | Max: notificación `LOAN_RECEIPT` al usuario |
| `LoanReturned` | Devolución registrada (por Frank) | MX: actualiza `Loan.Status` + `BookCopy.Status`; Max: promueve cola de reservas |

---

## Frontend

Rutas Angular (todas bajo `/dashboard`, requieren `authGuard`):

| Ruta | Componente | Visible para |
|------|------------|-------------|
| `/dashboard/prestamos` | `LoansPageComponent` | Admin, Librarian |
| `/dashboard/prestamos/nuevo` | `NewLoanPageComponent` | Admin, Librarian |
| `/dashboard/prestamos/:id` | `LoanDetailPageComponent` | Todos (sólo propios) |
| `/dashboard` | `DashboardPageComponent` (KPIs) | Admin, Librarian |

### Dashboard KPIs

`DashboardPageComponent` consulta `/api/dashboard/kpis` al cargar. Los indicadores
mostrados incluyen préstamos activos, vencidos, devoluciones del mes, multas pendientes,
libros disponibles y reservas en espera.

---

## Configuración relevante (`appsettings.json`)

```json
{
  "Loans": {
    "MaxActivePerRole": {
      "STUDENT": 3,
      "TEACHER": 5,
      "LIBRARIAN": 999,
      "ADMIN": 999
    },
    "DefaultDurationDaysPerRole": {
      "STUDENT": 7,
      "TEACHER": 21,
      "LIBRARIAN": 14,
      "ADMIN": 14
    }
  }
}
```

Los valores son consumidos por `LoansOptions` (en `Application/Options`) y se inyectan
via `IOptions<LoansOptions>` en `LoanService`.
