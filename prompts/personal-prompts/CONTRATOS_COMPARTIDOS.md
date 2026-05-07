# DEVLER — Contratos Compartidos Sprint 1 + 2

> **Lectura obligatoria antes de empezar cualquier módulo.**
> Este documento define los nombres, schemas, endpoints y eventos que
> comparten los tres módulos (Préstamos, Devoluciones, Reservas).
> Si necesitás cambiar algo de acá, **se discute en grupo antes de
> tocarlo**, no se modifica unilateralmente.

---

## 1. Reglas duras (heredadas de Sprint 0, aplican igual)

- Clean Architecture: Domain sin dependencias, Application solo
  depende de Domain, Infrastructure y Persistence implementan
  interfaces de Application, API es el host.
- Cada cambio de schema = migración EF nueva. **Nunca editar una
  migración ya aplicada.** Nombres en PascalCase: `AddLoans`,
  `AddFines`, `AddReservations`.
- Soft-delete (`DeletedAt` nullable + `HasQueryFilter`) en todas las
  entidades de dominio principal.
- Unicidad a nivel BD (índices únicos), no solo en código.
- DTOs separados de entidades. Nunca exponer entidades EF por la API.
- Autorización explícita por endpoint con policies nombradas. Sin
  `[Authorize]` = bug.
- Identificadores en inglés, comentarios y commits en español.
- DTOs validados con `[Required]`, `[StringLength]`, `[Range]`,
  `[EmailAddress]` cuando aplique.
- Operaciones que tocan más de una entidad raíz → transacción
  explícita (`BeginTransactionAsync`).
- `AsNoTracking()` en queries de lectura.
- Concurrency token (`UseXminAsConcurrencyToken`) en entidades con
  contención: `BookCopy`, `Loan`, `Reservation`, `Fine`.

---

## 2. Branching y merge

- Rama base: `develop` (debe estar siempre verde).
- Una rama por módulo: `feature/prestamos`, `feature/devoluciones`,
  `feature/reservas`.
- Cada lote cerrado → PR a `develop` → review de al menos uno de los
  otros dos → merge.
- **Antes de abrir PR**: rebase contra `develop`, `dotnet build`,
  `dotnet test`, `ng build` deben pasar.
- Conflictos de migraciones: el último que mergea regenera la
  migración. **No editar migraciones ya en `develop`.**

---

## 3. Orden de merges (importante)

Hay dependencia entre módulos. Este es el orden:

1. **Lotes pendientes de Sprint 0** (cada quien el suyo, en
   paralelo, primero que mergea gana).
2. **Marcelo: Loan + LoanRenewal entities + repos + service** (sin
   UI todavía). Frank y Max rebasean para tener `Loan` disponible.
3. **Frank: Return + Fine entities + repos + service** (depende de
   `Loan`). Max rebasea.
4. **Max: Reservation + Notification entities + repos + service**
   (depende de `BookCopy`, opcionalmente de `Loan` para evento de
   devolución → notificar siguiente en cola).
5. **Endpoints + UI** de los tres en paralelo.
6. **Dashboard, reportes, chatbot** al final.

---

## 4. Schemas de entidades nuevas (autoritativo)

> **Estos campos y nombres son ley.** Si un módulo necesita un campo
> extra solo para sí mismo, lo agrega sin tocar lo de abajo. Si
> necesita renombrar algo, se discute en grupo.

### 4.1 `Loan` (Marcelo)

```csharp
public class Loan
{
    public Guid Id { get; set; }                   // PK
    public Guid UserId { get; set; }               // FK → users.id
    public Guid BookCopyId { get; set; }           // FK → book_copies.id
    public DateTime LoanedAt { get; set; }         // momento del préstamo
    public DateTime DueAt { get; set; }            // fecha de vencimiento
    public DateTime? ReturnedAt { get; set; }      // null si está activo
    public LoanStatus Status { get; set; }         // ACTIVE, RETURNED, OVERDUE, LOST
    public int RenewalCount { get; set; }          // 0..2
    public Guid IssuedByUserId { get; set; }       // bibliotecario que registró
    public DateTime? DeletedAt { get; set; }
    public uint Xmin { get; set; }                 // concurrency token

    public Usuario User { get; set; }
    public BookCopy BookCopy { get; set; }
    public ICollection<LoanRenewal> Renewals { get; set; }
}

public enum LoanStatus { ACTIVE, RETURNED, OVERDUE, LOST }
```

**Tabla**: `loans`. Índices: `(user_id, status)`, `(book_copy_id,
status)`, único parcial sobre `book_copy_id` cuando `status = ACTIVE`
(una copia no puede tener dos préstamos activos).

### 4.2 `LoanRenewal` (Marcelo)

```csharp
public class LoanRenewal
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public DateTime RenewedAt { get; set; }
    public DateTime PreviousDueAt { get; set; }
    public DateTime NewDueAt { get; set; }
    public Guid RenewedByUserId { get; set; }
}
```

**Tabla**: `loan_renewals`. Índice: `loan_id`.

### 4.3 `Return` (Frank)

```csharp
public class Return
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }               // FK → loans.id
    public DateTime ReturnedAt { get; set; }
    public ReturnCondition Condition { get; set; } // OK, DAMAGED, LOST
    public string? InspectionNotes { get; set; }
    public Guid ReceivedByUserId { get; set; }     // bibliotecario
    public DateTime? DeletedAt { get; set; }

    public Loan Loan { get; set; }
    public Fine? Fine { get; set; }                // 0..1
}

public enum ReturnCondition { OK, DAMAGED, LOST }
```

**Tabla**: `returns`. Índice único en `loan_id` (un préstamo se
devuelve una vez).

### 4.4 `Fine` (Frank)

```csharp
public class Fine
{
    public Guid Id { get; set; }
    public Guid ReturnId { get; set; }             // FK → returns.id
    public Guid UserId { get; set; }               // denormalizado para queries
    public FineReason Reason { get; set; }         // LATE, DAMAGE, LOSS
    public decimal Amount { get; set; }            // MXN
    public int? DaysLate { get; set; }             // solo si Reason = LATE
    public FineStatus Status { get; set; }         // PENDING, PAID, WAIVED
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? PaidByUserId { get; set; }        // tesorería
    public DateTime? DeletedAt { get; set; }
    public uint Xmin { get; set; }

    public Return Return { get; set; }
    public Usuario User { get; set; }
}

public enum FineReason { LATE, DAMAGE, LOSS }
public enum FineStatus { PENDING, PAID, WAIVED }
```

**Tabla**: `fines`. Índices: `(user_id, status)`, `return_id`.

### 4.5 `FineConfig` (Frank — tabla de configuración)

```csharp
public class FineConfig
{
    public Guid Id { get; set; }
    public decimal LateRatePerDayMxn { get; set; }     // ej: 5.00
    public decimal DamageFlatMxn { get; set; }         // ej: 200.00
    public decimal LossFlatMxn { get; set; }           // ej: 500.00 (o costo libro)
    public DateTime EffectiveFrom { get; set; }
}
```

**Tabla**: `fine_configs`. Solo el registro con mayor
`EffectiveFrom <= NOW()` es el activo. **Seed de un registro inicial
en migración.**

### 4.6 `Reservation` (Max)

```csharp
public class Reservation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }                 // se reserva el TÍTULO, no la copia
    public int QueuePosition { get; set; }           // 1, 2, 3...
    public ReservationStatus Status { get; set; }    // PENDING, READY, FULFILLED, CANCELLED, EXPIRED
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadyAt { get; set; }           // cuando llega su turno
    public DateTime? ExpiresAt { get; set; }         // 48h desde ReadyAt
    public DateTime? FulfilledAt { get; set; }
    public Guid? FulfilledByLoanId { get; set; }     // FK → loans.id (cuando se concreta)
    public DateTime? DeletedAt { get; set; }
    public uint Xmin { get; set; }
}

public enum ReservationStatus { PENDING, READY, FULFILLED, CANCELLED, EXPIRED }
```

**Tabla**: `reservations`. Índices: `(book_id, status,
queue_position)`, `(user_id, status)`. Único parcial sobre
`(user_id, book_id)` cuando `status IN (PENDING, READY)` — un usuario
no puede tener dos reservas activas del mismo título.

### 4.7 `Notification` (Max)

```csharp
public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }       // LOAN_RECEIPT,
                                                     // RETURN_RECEIPT,
                                                     // FINE_CREATED,
                                                     // RESERVATION_READY,
                                                     // DUE_REMINDER
    public NotificationChannel Channel { get; set; } // EMAIL, WHATSAPP, IN_APP
    public string Subject { get; set; }
    public string Body { get; set; }
    public NotificationStatus Status { get; set; }   // PENDING, SENT, FAILED
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; }          // datos estructurados
}

public enum NotificationType { LOAN_RECEIPT, RETURN_RECEIPT,
                                FINE_CREATED, RESERVATION_READY,
                                DUE_REMINDER }
public enum NotificationChannel { EMAIL, WHATSAPP, IN_APP }
public enum NotificationStatus { PENDING, SENT, FAILED }
```

**Tabla**: `notifications`. Índice: `(status, created_at)` para el
worker que despacha pendientes.

---

## 5. Endpoints REST (autoritativo)

> Convención: rutas en español, igual que Sprint 0 (`/api/libros`,
> `/api/usuarios`). Códigos HTTP estándar.

### 5.1 Préstamos (Marcelo)

| Método | Ruta | Policy | Descripción |
|---|---|---|---|
| POST   | `/api/prestamos` | AdminOrLibrarian | Registrar préstamo |
| GET    | `/api/prestamos/{id}` | Authenticated | Detalle |
| GET    | `/api/prestamos` | AdminOrLibrarian | Listar con filtros |
| GET    | `/api/usuarios/{userId}/prestamos` | Authenticated | Préstamos de un usuario (un user solo ve los suyos salvo admin/librarian) |
| POST   | `/api/prestamos/{id}/renovaciones` | Authenticated | Renovar (validaciones internas) |

### 5.2 Devoluciones, multas, pagos (Frank)

| Método | Ruta | Policy | Descripción |
|---|---|---|---|
| POST   | `/api/devoluciones` | AdminOrLibrarian | Registrar devolución (genera multa si aplica) |
| GET    | `/api/devoluciones/{id}` | AdminOrLibrarian | Detalle |
| GET    | `/api/multas` | AdminOrLibrarian | Listar multas con filtros |
| GET    | `/api/multas/{id}` | Authenticated | Detalle (user solo las suyas) |
| GET    | `/api/usuarios/{userId}/multas` | Authenticated | Multas de un usuario |
| POST   | `/api/multas/{id}/pagos` | "TreasuryOrAdmin" | Confirmar pago |
| POST   | `/api/multas/{id}/condonar` | AdminOnly | Waive (perdón administrativo) |
| GET    | `/api/configuracion-multas` | Authenticated | Config vigente |
| PUT    | `/api/configuracion-multas` | AdminOnly | Nueva config con EffectiveFrom |

### 5.3 Reservas y notificaciones (Max)

| Método | Ruta | Policy | Descripción |
|---|---|---|---|
| POST   | `/api/reservas` | Authenticated | Crear reserva |
| GET    | `/api/reservas/{id}` | Authenticated | Detalle |
| GET    | `/api/usuarios/{userId}/reservas` | Authenticated | Reservas de un usuario |
| GET    | `/api/libros/{libroId}/reservas` | AdminOrLibrarian | Cola de un libro |
| DELETE | `/api/reservas/{id}` | Authenticated | Cancelar (user solo las suyas) |
| GET    | `/api/notificaciones` | Authenticated | Inbox del user (in-app) |
| PATCH  | `/api/notificaciones/{id}/leida` | Authenticated | Marcar como leída |

---

## 6. Eventos de dominio (cómo se comunican los módulos)

> **No hay llamadas directas service-a-service entre módulos.** Cada
> uno publica eventos vía `IDomainEventDispatcher` y los otros se
> suscriben con handlers. Esto desacopla y evita ciclos.

Definir en `Biblioteca.Application/Common/Events/`:

```csharp
public interface IDomainEvent { }
public interface IDomainEventHandler<T> where T : IDomainEvent
{
    Task HandleAsync(T evt, CancellationToken ct);
}
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent evt, CancellationToken ct);
}
```

Implementación simple en Infrastructure: scan de DI por
`IDomainEventHandler<T>` e invocación secuencial. **Los handlers
corren dentro de la misma transacción del comando que dispara el
evento** — para eso, el dispatcher recibe el `DbContext` actual y
el handler puede agregar entidades al ChangeTracker pero NO llama
`SaveChangesAsync`.

### Eventos publicados

| Evento | Publica | Consume | Acción del consumer |
|---|---|---|---|
| `LoanCreated(LoanId, UserId, BookCopyId, DueAt)` | Marcelo | Max | Crear `Notification(LOAN_RECEIPT)` |
| `LoanReturned(LoanId, UserId, BookId, BookCopyId, Condition)` | Frank | Max | Si hay reservas PENDING para ese BookId → marcar la primera de la cola como READY + crear `Notification(RESERVATION_READY)` |
| `FineCreated(FineId, UserId, Amount, Reason)` | Frank | Max | Crear `Notification(FINE_CREATED)` |
| `ReservationReady(ReservationId, UserId, BookId)` | Max | Max | (interno, ya cubierto) |
| `FinePaid(FineId, UserId)` | Frank | — | (informativo, log) |

### Por qué esto y no llamadas directas

Si Frank llama a `_reservationService.NotifyNext(...)`, necesita
referencia al proyecto de Max y se acopla. Con eventos: Frank publica
`LoanReturned`, Max define el handler `LoanReturnedHandler` que vive
en su carpeta. Cero acoplamiento al merge.

---

## 7. Reglas de negocio compartidas

Estas son las reglas que más de un módulo necesita conocer. Si
encontrás contradicción, **preguntá antes de implementar**.

1. **Bloqueo por deuda**: un usuario con `Fine.Status = PENDING` no
   puede crear préstamos ni reservas. El check vive en un
   `IUserEligibilityService` (Application) que Marcelo y Max
   consumen. Frank lo implementa porque es dueño de las multas.
2. **Máximo de préstamos activos por rol**:
   - STUDENT: 3
   - TEACHER: 5
   - LIBRARIAN/ADMIN: sin límite
   Los valores van a `appsettings: Loans:MaxActivePerRole` (dict).
3. **Duración default de préstamo por rol** (días):
   - STUDENT: 7
   - TEACHER: 21
   - LIBRARIAN/ADMIN: 14
   En `appsettings: Loans:DefaultDurationDaysPerRole`.
4. **Renovación**: máximo 2 renovaciones por préstamo, suma 7 días
   (igual para todos), **NO se puede renovar si hay reserva activa
   sobre el libro**. Esto requiere que Marcelo consulte reservas → la
   query la encapsula `IReservationQueryService` (Application) que
   Max implementa.
5. **Cálculo de multa por retraso**:
   `Amount = max(0, daysLate) * FineConfig.LateRatePerDayMxn`.
   Si la copia vuelve OK pero tarde → solo `LATE`.
   Si vuelve `DAMAGED` → `LATE` (si tarde) + `DAMAGE` separadas.
   Si vuelve `LOST` → solo `LOSS`, la copia se marca `LOST` y el
   préstamo `Status = LOST`.
6. **Reserva expira 48h después de `ReadyAt`** si no se concreta
   préstamo. Hay un job (Max) que corre cada hora y expira las
   vencidas, promoviendo al siguiente en cola.
7. **Auditoría**: todas las operaciones de escritura ya quedan
   auditadas por el override de `SaveChangesAsync` que cierra el
   Lote 7. Cualquier entidad nueva con dato relevante debe estar en
   la lista de "auditables" del `BibliotecaDbContext`.

---

## 8. Tabla de "quién toca qué archivo"

Para evitar que dos personas editen el mismo archivo:

| Archivo / carpeta | Dueño |
|---|---|
| `Biblioteca.Domain/Entities/Loan*.cs` | Marcelo |
| `Biblioteca.Domain/Entities/Return.cs`, `Fine*.cs` | Frank |
| `Biblioteca.Domain/Entities/Reservation.cs`, `Notification.cs` | Max |
| `Biblioteca.Application/Loans/**` | Marcelo |
| `Biblioteca.Application/Returns/**`, `Fines/**`, `Payments/**` | Frank |
| `Biblioteca.Application/Reservations/**`, `Notifications/**` | Max |
| `Biblioteca.Persistence/Configurations/Loan*.cs` | Marcelo |
| `Biblioteca.Persistence/Configurations/Return*.cs, Fine*.cs` | Frank |
| `Biblioteca.Persistence/Configurations/Reservation*.cs, Notification*.cs` | Max |
| `Biblioteca.API/Controllers/PrestamosController.cs` | Marcelo |
| `...DevolucionesController.cs, MultasController.cs` | Frank |
| `...ReservasController.cs, NotificacionesController.cs` | Max |
| `frontend/.../prestamos/**` | Marcelo |
| `frontend/.../devoluciones/**, multas/**` | Frank |
| `frontend/.../reservas/**, notificaciones/**` | Max |

**Archivos compartidos** (cambios coordinados, idealmente un solo PR
los toca):
- `BibliotecaDbContext.cs` (DbSets nuevos): cada quien agrega sus
  DbSets en su PR.
- `ServiceCollectionExtensions.cs`: cada quien registra sus services
  en su PR.
- `Program.cs`: idealmente solo cambia para agregar policies nuevas.
- Migraciones EF: ver sección 9.

---

## 9. Migraciones EF — orden y nombres

**Problema**: si tres personas crean migraciones en paralelo, los
timestamps colisionan y EF pierde el orden.

**Solución**: cada quien usa el prefijo de timestamp que EF genera
(no se toca), pero los **nombres están reservados** y se aplican en
este orden al merge:

1. `AddLoans` (Marcelo)
2. `AddLoanRenewals` (Marcelo)
3. `AddReturns` (Frank, depende de Loans)
4. `AddFineConfigs` (Frank)
5. `AddFines` (Frank, depende de Returns)
6. `AddReservations` (Max, depende de Books y Loans)
7. `AddNotifications` (Max)
8. `AddDashboardViews` (Marcelo, lote final)

Si al hacer rebase tu migración queda fuera de orden, **regenerás**
la migración (no la editás): borrás el archivo, hacés
`dotnet ef migrations add <Nombre>` de nuevo. EF reordena por
timestamp.

---

## 10. Frontend — convenciones compartidas

- Estructura de carpetas: `src/app/features/<modulo>/` con
  `pages/`, `components/`, `services/`, `models/`.
- Servicios HTTP: extender el patrón existente
  (`*ApiService` con `HttpClient`).
- Auth: el interceptor existente ya inyecta el Bearer token, no
  hace falta hacer nada.
- Reactive forms con validators sincronizados con los DTOs del
  backend.
- Mensajes de error en español.
- Estados de carga: skeleton o spinner consistente con lo que ya
  hay.
- Routing: lazy-loaded routes por feature.

---

## 11. Definition of Done por lote

Un lote está "Done" cuando:

1. Código compila (`dotnet build`, `ng build`) sin warnings nuevos.
2. Tests del lote pasan (`dotnet test`).
3. Migración aplicada limpia desde cero
   (`dotnet ef database drop -f && dotnet ef database update`).
4. Endpoints probados con curl o Postman, criterios de aceptación
   verificados.
5. UI navegable end-to-end del flujo del lote.
6. PR abierto, CI verde, review de uno de los otros dos, merge.
7. Si se detectó deuda técnica nueva → anotada en `DEUDA_TECNICA.md`.

---

## 12. Qué NO hacer

- **No** renombrar campos de este documento sin acuerdo grupal.
- **No** llamar al service de otro módulo directamente — usar
  eventos.
- **No** instalar paquetes nuevos sin justificarlo en el commit.
- **No** mergear con tests rojos "para arreglarlo después".
- **No** hacer fixups silenciosos en código de otro módulo. Si ves
  un bug ajeno, abrí issue y dejá `TODO` con tu nombre.
- **No** editar migraciones aplicadas. Siempre nueva migración para
  cambios.
