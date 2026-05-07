# Módulos del sistema

Última actualización: 2026-05-07 (cierre Bloque 2).

---

## Resumen

| Módulo | Propietario | Lotes | Estado |
|--------|-------------|-------|--------|
| Préstamos, Renovaciones y Dashboard KPIs | Marcelo (Tech Lead) | MX-3 → MX-7 | ✅ completo |
| Devoluciones, Multas, Pagos y Reportes | Frank | FK-4 → FK-7 | ✅ completo |
| Reservas, Notificaciones y Chatbot | Max | MA-3 → MA-7 | ✅ completo |

Infraestructura compartida (eventos de dominio, `IUnitOfWork`, `IUserEligibilityService`,
`IReservationQueryService`) fue implementada colaborativamente durante el Bloque 2.

---

## Módulo Préstamos y Renovaciones (Marcelo — MX)

**Propósito:** ciclo completo de entrega y devolución de ejemplares: crear préstamo, controlar
límites por rol, renovar hasta dos veces, y marcar overdue automáticamente.

**Endpoints principales:**

| Método | Ruta | Policy | Descripción |
|--------|------|--------|-------------|
| `POST` | `/api/prestamos` | `AdminOrLibrarian` | Crear préstamo |
| `GET` | `/api/prestamos` | `AdminOrLibrarian` | Listar (filtro `?status=`) |
| `GET` | `/api/prestamos/{id}` | `Authenticated` | Detalle (usuario sólo ve el suyo) |
| `POST` | `/api/prestamos/{id}/renovaciones` | `Authenticated` | Renovar |
| `GET` | `/api/usuarios/{id}/prestamos` | `Authenticated` | Préstamos de un usuario |

**Decisiones clave:**
- Límites de préstamos simultáneos y duración configurados en `appsettings.json` bajo la
  sección `Loans` por rol (`STUDENT:3/7d`, `TEACHER:5/21d`, `LIBRARIAN:999/14d`).
- Renovación extiende `DueAt` en 7 días fijos; máximo dos renovaciones por préstamo.
- `OverdueLoansBackgroundService` corre cada hora y transiciona `ACTIVE → OVERDUE`.
- Ver [`PRESTAMOS.md`](PRESTAMOS.md) para diagrama de estados y reglas de negocio completas.

---

## Módulo Devoluciones, Multas y Reportes (Frank — FK)

**Propósito:** recepción física del ejemplar, cálculo automático de multas por retraso/daño/pérdida,
cobro y condonación, y reportes ejecutivos para administración.

**Endpoints principales:**

| Método | Ruta | Policy | Descripción |
|--------|------|--------|-------------|
| `POST` | `/api/devoluciones` | `AdminOrLibrarian` | Registrar devolución |
| `GET` | `/api/devoluciones/{id}` | `AdminOrLibrarian` | Detalle de devolución |
| `GET` | `/api/multas` | `AdminOrLibrarian` | Listar multas |
| `GET` | `/api/multas/{id}` | `Authenticated` | Detalle (usuario sólo ve la suya) |
| `POST` | `/api/multas/{id}/pagos` | `TreasuryOrAdmin` | Confirmar pago |
| `POST` | `/api/multas/{id}/condonar` | `AdminOnly` | Condonar multa |
| `GET/PUT` | `/api/configuracion-multas` | `Authenticated/AdminOnly` | Tarifa vigente |
| `GET` | `/api/reportes/*` | `AdminOnly` | Reportes ejecutivos (4 variantes) |

**Decisiones clave:**
- `FineCalculator` genera hasta tres multas distintas por devolución (LATE + DAMAGE + LOSS pueden
  coexistir).
- La tarifa es configurable en caliente via `FineConfig`; nunca se edita un registro activo,
  se inserta uno nuevo con `EffectiveFrom`.
- `TreasuryOrAdmin` es policy compuesta con roles `ADMIN + TREASURY`.
- Ver [`DEVOLUCIONES_MULTAS.md`](DEVOLUCIONES_MULTAS.md) para diagrama de estados y flujo completo.

---

## Módulo Reservas, Notificaciones y Chatbot (Max — MA)

**Propósito:** cola FIFO de reservas por libro, notificaciones in-app por eventos de dominio,
y chatbot de ayuda reglas-based accesible desde el widget flotante del frontend.

**Endpoints principales:**

| Método | Ruta | Policy | Descripción |
|--------|------|--------|-------------|
| `POST` | `/api/reservas` | `Authenticated` | Crear reserva (cola FIFO) |
| `DELETE` | `/api/reservas/{id}` | `Authenticated` | Cancelar reserva |
| `GET` | `/api/usuarios/{id}/reservas` | `Authenticated` | Reservas de un usuario |
| `GET` | `/api/libros/{id}/reservas/cola` | `AdminOrLibrarian` | Cola de espera del libro |
| `GET` | `/api/notificaciones` | `Authenticated` | Inbox del usuario |
| `POST` | `/api/notificaciones/{id}/leer` | `Authenticated` | Marcar como leída |
| `POST` | `/api/chat` | `Authenticated` | Enviar mensaje al chatbot |

**Decisiones clave:**
- Reservas avanzan por cola FIFO; al devolver un libro, `LoanReturnedReservationHandler` promueve
  automáticamente la siguiente reserva `WAITING → READY`.
- Las reservas `READY` expiran en 48 h si no se convierten en préstamo
  (`ReservationExpirationBackgroundService` corre cada 15 min).
- El chatbot usa reglas estáticas (`RuleBasedChatbotProvider`); la interfaz `IChatbotProvider`
  permite sustituirlo por un LLM sin tocar el controller.
- `NotificationBellComponent` en Angular hace polling cada 60 s (ver DT-11 para migración a websocket).
- Ver [`RESERVAS_NOTIFICACIONES.md`](RESERVAS_NOTIFICACIONES.md) para detalles del módulo MA.
