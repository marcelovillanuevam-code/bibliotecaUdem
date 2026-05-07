# DEVLER — Prompts Marcelo (Tech Lead)
# Módulo: Préstamos + Renovaciones + BookCopy UI + Dashboard KPIs

> **Cómo usar este archivo**: cada `## Lote N` es un prompt
> independiente. Ejecutalos uno a la vez en ventana limpia (`/clear`
> antes de cada uno). El contexto que necesita el agente está
> autocontenido en cada lote.
> **Lectura previa obligatoria**: `CONTRATOS_COMPARTIDOS.md`. Si el
> agente no lo conoce, pegalo al inicio del prompt o pedile que lo
> abra.

---

## Contexto del proyecto (a incluir SIEMPRE al inicio de cada prompt)

```
Sos un agente trabajando en DEVLER, un sistema de gestión de
biblioteca universitaria para la UDEM. Stack: ASP.NET Core
(.NET 10) + PostgreSQL 17 + Angular 21, Clean Architecture,
SCRUM, equipo de 3.

Mi rol: Marcelo (Tech Lead). Mi módulo: Préstamos, Renovaciones,
BookCopy en Angular, y Dashboard KPIs.

Reglas duras del proyecto (no negociables):
- Clean Architecture: Domain sin dependencias, Application solo
  depende de Domain, Infrastructure y Persistence implementan
  interfaces de Application, API es el host.
- Cada cambio de schema = migración EF nueva con nombre PascalCase.
  Nunca editar migración aplicada.
- Soft-delete (DeletedAt nullable + HasQueryFilter) en todas las
  entidades de dominio.
- DTOs separados de entidades. Nunca exponer entidades EF por API.
- [Authorize(Policy = "...")] explícito por endpoint.
- Identificadores en inglés, comentarios y commits en español.
- Un commit por lote. Mensaje: "Lote MX-N: <título>".
- AsNoTracking en queries de lectura.
- Concurrency token (UseXminAsConcurrencyToken) en Loan.
- NO llamar services de otros módulos directamente. Usar eventos
  de dominio (IDomainEventDispatcher).
- NO instalar paquetes nuevos sin justificarlo.
- Después de cada lote: parar, mostrar diff resumido + comandos
  curl de verificación. Esperar OK antes de continuar.

Branching: rama feature/prestamos. Mergeo a develop al cerrar
cada lote, previo PR + review.

El documento CONTRATOS_COMPARTIDOS.md tiene los schemas de
entidades, endpoints, eventos y reglas de negocio compartidas
con Frank y Max. Es ley. Si necesito cambiar algo de ahí, lo
discuto en grupo, no lo modifico unilateralmente.
```

---

## Lote MX-0 — Lote pendiente Sprint 0: BookCopy en Angular (CU-07 frontend)

**Objetivo**: cerrar el Lote 5 pendiente de Sprint 0. Sin esto,
no puedo registrar préstamos en UI porque no hay manera de ver
ejemplares.

**Contexto extra**: el backend ya tiene `BookCopiesController`
con GET/POST/PUT/DELETE en `/api/book-copies` y
`/api/libros/{libroId}/ejemplares` (Lote 3 de Sprint 0 cerrado).
Falta solo la UI.

**Tareas**:

1. Modelo `BookCopy` en `frontend/src/app/shared/models/book-copy.model.ts`:
   ```typescript
   export interface BookCopy {
     id: string;
     bookId: string;
     barcode: string;
     locationId: string | null;
     status: 'AVAILABLE' | 'MAINTENANCE' | 'LOST' | 'RETIRED'
           | 'LOANED' | 'RESERVED';
     condition: 'NEW' | 'GOOD' | 'WORN' | 'DAMAGED' | null;
     acquiredAt: string;
   }
   ```

2. `BookCopiesApiService` en `core/services/book-copies-api.service.ts`
   con métodos:
   - `getByBookId(bookId): Observable<BookCopy[]>`
   - `getById(id): Observable<BookCopy>`
   - `create(bookId, request): Observable<BookCopy>`
   - `update(id, request): Observable<BookCopy>`
   - `delete(id): Observable<void>`

3. En `BooksPageComponent` (vista de gestión, NO catálogo):
   - Botón "Ver ejemplares" en cada fila/card.
   - Modal `BookCopiesModalComponent` que liste copias del libro
     con barcode, ubicación, status, condición.
   - Acciones: agregar copia (form reactive), editar status/
     ubicación, eliminar (con confirm).
   - Reactive form con validators: barcode requerido y no vacío.

4. En el catálogo (vista catalog) — espera, esto es del Lote 4
   de Max. **Acá solo gestión.** Si Max todavía no mergeó su
   Lote 4, dejá el catálogo como está; el indicador
   `availableCopies / totalCopies` lo agrega él.

**Criterios de aceptación**:
- Logueado como LIBRARIAN o ADMIN, en gestión de libros, abrir
  un libro existente → modal lista sus copias.
- Agregar copia con barcode "TEST-001" → aparece en la lista.
- Agregar otra con el mismo barcode → toast de error 409 con
  mensaje claro.
- Editar status de una copia a MAINTENANCE → guarda y refresca.
- Eliminar copia → confirma → desaparece de la lista.
- Logueado como STUDENT, en gestión de libros (si tiene acceso):
  no ve los botones de gestión. (Si su rol no entra a la página
  de gestión, mejor.)

**Mostrar al final**: diff resumido, comandos `ng test` si los hay,
checklist de criterios verificados manualmente.

---

## Lote MX-1 — Hardening compartido (parte de Lote 7 Sprint 0)

**Objetivo**: cerrar los bugs de hardening que tocan archivos del
módulo de préstamos antes de empezar a escribirlo.

**Contexto**: estos bugs son parte del Lote 7 de Sprint 0. Frank
y Max cierran los suyos en paralelo. Yo me ocupo de los que tocan
mis archivos.

**Tareas**:

1. **BUG-12 (Validación de DTOs)** — agregar atributos en los DTOs
   que voy a crear (`CreateLoanRequest`, `RenewLoanRequest`):
   `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`
   donde aplique. Verificar que ASP.NET devuelve 400 con
   ProblemDetails para body vacío.

2. **BUG-07 (audit log)** — verificar que el override de
   `SaveChangesAsync` ya está cerrado por quien lo implementó
   (Frank, según reparto). Si no está, **NO lo implementes vos**:
   abrí issue, dejá `TODO` con mi nombre, seguí. Si está, agregar
   `Loan` y `LoanRenewal` a la lista de entidades auditables.

3. **Test de regresión Lote 8 (mi parte)**:
   - `LoanAuthorizationTests` (placeholder por ahora) — no se
     puede escribir todavía porque la entidad Loan aún no existe.
     **Saltear, lo escribo en el lote MX-3.** Solo crear el
     archivo vacío con `// TODO Lote MX-3` para reservar el
     nombre.

**Criterios de aceptación**:
- `dotnet build` sin warnings nuevos.
- Si el audit log de Frank ya mergeó: crear un Loan en BD
  manualmente y verificar fila en `audit_logs`. (Si no mergeó:
  esperar a Frank para verificar, no bloquear.)

---

## Lote MX-2 — Loan + LoanRenewal: Domain + Persistence

**Objetivo**: schemas y persistencia. Sin endpoints todavía.

**Contexto**: ver sección 4.1 y 4.2 de
`CONTRATOS_COMPARTIDOS.md`. Schema autoritativo, no inventar
campos.

**Tareas**:

1. **Entidades Domain**:
   - `Biblioteca.Domain/Entities/Loan.cs` con campos del contrato.
   - `Biblioteca.Domain/Entities/LoanRenewal.cs`.
   - Enum `LoanStatus { ACTIVE, RETURNED, OVERDUE, LOST }`.

2. **Configuraciones EF** (`Biblioteca.Persistence/Configurations/`):
   - `LoanConfig.cs`:
     - Tabla `loans`.
     - Soft-delete con `HasQueryFilter(l => l.DeletedAt == null)`.
     - Concurrency token `UseXminAsConcurrencyToken()` mapeado a
       columna `xmin`.
     - Índices:
       ```
       loans_user_status_idx       (user_id, status)
       loans_book_copy_status_idx  (book_copy_id, status)
       loans_active_copy_unique    UNIQUE (book_copy_id) WHERE status = 'ACTIVE'
       ```
       El índice único parcial se hace con `HasFilter("status =
       'ACTIVE'")`.
     - Enum LoanStatus mapeado como string (`HasConversion<string>()`).
     - FK a `Usuario` (existe), `BookCopy` (existe), Issuer
       (Usuario nuevamente, sin cascada).
   - `LoanRenewalConfig.cs`:
     - Tabla `loan_renewals`.
     - FK a Loan con CASCADE.

3. **DbSets** en `BibliotecaDbContext`:
   - `public DbSet<Loan> Loans { get; set; }`
   - `public DbSet<LoanRenewal> LoanRenewals { get; set; }`
   - Aplicar configs.
   - Agregar a la lista de entidades auditables (ver punto 2 del
     Lote MX-1).

4. **Migración**: `AddLoans` (incluye Loan y LoanRenewal en una
   sola migración para mantener orden — el contrato lo permite,
   los nombré como dos pero pueden ir en una sola si EF los
   detecta juntos).

5. **Repositorios** (`Biblioteca.Application/Loans/Interfaces/`
   + `Biblioteca.Persistence/Repositories/`):
   - `ILoanRepository`:
     - `GetByIdAsync(id, ct)`
     - `GetActiveByBookCopyAsync(bookCopyId, ct)` — para validar
       que no haya doble préstamo
     - `GetActiveByUserAsync(userId, ct)`
     - `GetByUserAsync(userId, statusFilter?, ct)` — historial
     - `AddAsync(loan, ct)`
     - `UpdateAsync(loan, ct)`
     - `CountActiveByUserAsync(userId, ct)` — para validar
       máximo por rol
   - Implementación con `AsNoTracking` en lecturas que no van a
     escribir.

**Criterios de aceptación**:
- `dotnet ef migrations add AddLoans` genera migración limpia.
- `dotnet ef database update` aplica sin errores en BD vacía.
- Agregar manualmente un Loan en BD y otro con mismo `book_copy_id`
  y status ACTIVE → segundo INSERT falla por índice único parcial.
- `dotnet build` y `dotnet test` verdes (los tests existentes no
  deben romperse).

**Mostrar al final**: archivo de migración generado, schema en
psql (`\d loans`, `\d loan_renewals`), comandos para reproducir.

---

## Lote MX-3 — Loan: Application Service + endpoints + tests

**Objetivo**: lógica de negocio y API de préstamos.

**Contexto**: ver sección 5.1 (endpoints) y 7 (reglas de negocio)
de `CONTRATOS_COMPARTIDOS.md`. Reglas clave:
- Bloqueo por deuda → consultar `IUserEligibilityService` (Frank
  lo implementa; si todavía no mergeó, definir la interfaz y
  hacer un stub que siempre devuelva true con `TODO Frank`).
- Máximo activos por rol: `appsettings: Loans:MaxActivePerRole`
  (dict).
- Duración default por rol: `appsettings:
  Loans:DefaultDurationDaysPerRole`.
- Renovación: máximo 2, suma 7 días, NO si hay reserva activa →
  consultar `IReservationQueryService` (Max lo implementa; stub
  igual si no mergeó: `HasActiveReservationsForBook(bookId)`
  devuelve false con TODO).

**Tareas**:

1. **DTOs** (`Biblioteca.Application/Loans/Dtos/`):
   - `CreateLoanRequest`: `UserId` (Guid), `BookCopyId` (Guid),
     `DurationDaysOverride` (int?, opcional). Validators
     `[Required]`, `[Range(1, 90)]` para override.
   - `RenewLoanRequest`: vacío o con `Notes` opcional.
   - `LoanDto`: read-only con campos del contrato + nombre del
     usuario y título del libro (joins).
   - `LoanRenewalDto`.

2. **`LoanService`** (`Biblioteca.Application/Loans/Services/`):
   Operaciones:
   - `CreateAsync(CreateLoanRequest, issuedBy, ct)`:
     - Validar que el `BookCopy` existe y status = AVAILABLE.
     - Validar `IUserEligibilityService.IsEligibleAsync` (sin
       multas pendientes).
     - Validar máximo activos por rol del usuario.
     - Crear `Loan` con `Status = ACTIVE`, `LoanedAt = clock.Now`,
       `DueAt = LoanedAt + duración del rol o override`.
     - Cambiar `BookCopy.Status` a `LOANED` (concurrency check
       vía xmin).
     - Publicar evento `LoanCreated(LoanId, UserId, BookCopyId,
       DueAt)`.
     - Todo en transacción.
   - `RenewAsync(loanId, requestedBy, ct)`:
     - Validar préstamo existe, status = ACTIVE, RenewalCount < 2.
     - Validar que `requestedBy` es el dueño del préstamo o
       LIBRARIAN/ADMIN.
     - Validar
       `!IReservationQueryService.HasActiveReservationsForBookAsync(bookId, ct)`.
     - Crear `LoanRenewal` con `PreviousDueAt = loan.DueAt`,
       `NewDueAt = DueAt + 7 días`.
     - Actualizar `loan.DueAt`, `loan.RenewalCount++`.
   - `GetByIdAsync(id, ct)`.
   - `GetActiveByUserAsync(userId, ct)`.
   - `GetByUserAsync(userId, statusFilter?, ct)`.
   - `MarkOverdueAsync(ct)` — método para job: actualiza
     `Status = OVERDUE` para préstamos con `DueAt < NOW()` y
     `Status = ACTIVE`. (No hace falta el job todavía, lo
     llamamos manualmente.)

3. **Eventos**:
   - Definir `LoanCreated` en
     `Biblioteca.Application/Common/Events/LoanCreated.cs`.
   - Verificar que `IDomainEventDispatcher` ya existe en el
     proyecto (lo necesitan los tres). Si no, crearlo:
     interfaz en Application, implementación simple en
     Infrastructure que recorre handlers registrados en DI.
     **Coordinar con Frank y Max si ya lo crearon.**

4. **Controller** `Biblioteca.API/Controllers/PrestamosController.cs`:
   - Rutas y policies según contrato 5.1.
   - `[Authorize(Policy = "...")]` por método.
   - Validación de "el user solo ve los suyos salvo
     admin/librarian": leer claim del JWT en `GetByUser`. Si
     `userId != claim.Sub && !User.IsInRole("ADMIN") &&
     !User.IsInRole("LIBRARIAN")` → 403.

5. **Configuración**:
   - `appsettings.json`:
     ```json
     "Loans": {
       "MaxActivePerRole": {
         "STUDENT": 3, "TEACHER": 5, "LIBRARIAN": 999, "ADMIN": 999
       },
       "DefaultDurationDaysPerRole": {
         "STUDENT": 7, "TEACHER": 21, "LIBRARIAN": 14, "ADMIN": 14
       }
     }
     ```
   - Bind a clase `LoansOptions` con `IOptions<>`.

6. **Tests** (`Biblioteca.Tests/Loans/`):
   - `LoanServiceCreateTests`:
     - Copia AVAILABLE + user elegible → crea, status ACTIVE,
       BookCopy queda LOANED.
     - Copia ya LOANED → 409.
     - User con multa pendiente (mockear `IUserEligibilityService`
       returning false) → 403 o 409 con mensaje.
     - User con 3 préstamos activos siendo STUDENT → 409.
   - `LoanServiceRenewTests`:
     - Renovación válida → DueAt += 7, RenewalCount = 1.
     - Tercera renovación → 409.
     - Reserva activa para el libro (mock) → 409.
   - `LoanAuthorizationTests` (el placeholder de MX-1, completar):
     - STUDENT con JWT propio puede ver sus préstamos.
     - STUDENT con JWT viendo préstamos de otro user → 403.
     - LIBRARIAN puede ver préstamos de cualquier user.

**Criterios de aceptación**:
```bash
# Login admin
TOKEN=$(curl -s -X POST localhost:5122/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin1","password":"Admin123!"}' | jq -r .token)

# Crear préstamo
curl -X POST localhost:5122/api/prestamos \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"userId":"<student-id>","bookCopyId":"<copy-id>"}'
# → 201, devuelve LoanDto

# Doble préstamo de la misma copia
# → 409

# Renovar
curl -X POST localhost:5122/api/prestamos/<id>/renovaciones \
  -H "Authorization: Bearer $TOKEN"
# → 200, RenewalCount = 1, DueAt += 7

# Renovar una tercera vez
# → 409 mensaje "Máximo de renovaciones alcanzado"
```

**Mostrar al final**: salida de `dotnet test`, curl ejecutados,
diff resumido.

---

## Lote MX-4 — Préstamos en Angular

**Objetivo**: UI de préstamos.

**Tareas**:

1. **Modelo y servicio**:
   - `frontend/src/app/shared/models/loan.model.ts`.
   - `core/services/loans-api.service.ts` con métodos paralelos
     a los endpoints.

2. **Páginas** en `features/prestamos/`:
   - `LoansPageComponent` (`/dashboard/prestamos`):
     - Vista LIBRARIAN/ADMIN: tabla con filtros (status, usuario,
       fechas), paginación.
     - Vista STUDENT/TEACHER: solo sus préstamos activos +
       historial.
     - Determinar la vista según `AuthSessionService`.
   - `NewLoanPageComponent` (`/dashboard/prestamos/nuevo`):
     - Solo LIBRARIAN/ADMIN.
     - Form: buscar usuario por username/email (dropdown
       autocompletar), buscar libro por título/ISBN, seleccionar
       copia AVAILABLE, fecha de devolución (default según rol),
       botón "Registrar".
     - Al enviar: si ok → toast + navegar a detalle.
     - Si error 409 (multa pendiente, copia no disponible, max
       alcanzado) → toast con mensaje del backend.
   - `LoanDetailPageComponent` (`/dashboard/prestamos/:id`):
     - Datos del préstamo, historial de renovaciones.
     - Botón "Renovar" si user es dueño o LIBRARIAN/ADMIN, status
       ACTIVE, RenewalCount < 2.

3. **Routing**: lazy-loaded.
4. **Guards**: ya existe `authGuard`. Para rutas LIBRARIAN/ADMIN
   only, crear `roleGuard(['LIBRARIAN', 'ADMIN'])`.
5. **Reactive forms** con validators.

**Criterios de aceptación**:
- Login admin1 → /dashboard/prestamos/nuevo → registrar préstamo
  para demo.lramirez (STUDENT) de un libro existente con copia
  AVAILABLE → ok.
- Mismo flujo con copia ya LOANED → toast de error.
- Login demo.lramirez → /dashboard/prestamos → solo ve sus
  préstamos.
- Renovar préstamo activo desde detalle → DueAt actualizado en
  pantalla.

---

## Lote MX-5 — Job de overdue + integración con devoluciones

**Objetivo**: cerrar el ciclo cuando llega una devolución de Frank
y marcar préstamos vencidos automáticamente.

**Contexto**: Frank publica `LoanReturned(LoanId, UserId, BookId,
BookCopyId, Condition)`. Yo debo tener un handler que cierre el
préstamo:
- `Status = RETURNED`, `ReturnedAt = NOW()`.
- `BookCopy.Status` vuelve a AVAILABLE (salvo si Condition = LOST,
  que es responsabilidad de Frank → la copia queda LOST).

**Tareas**:

1. **Handler** `LoanReturnedHandler` en
   `Biblioteca.Application/Loans/EventHandlers/`:
   - Implementa `IDomainEventHandler<LoanReturned>`.
   - Carga el Loan, valida status ACTIVE u OVERDUE.
   - Actualiza Loan: status RETURNED, ReturnedAt = clock.Now.
   - Si Condition = OK o DAMAGED → BookCopy.Status = AVAILABLE.
   - Si Condition = LOST → BookCopy.Status = LOST, Loan.Status =
     LOST.
   - **No** llama SaveChanges (lo hace el comando que disparó el
     evento).

2. **Job de overdue**:
   - Servicio hosted (`IHostedService`) en Infrastructure:
     `OverdueLoansBackgroundService`. Corre cada hora.
   - Llama `LoanService.MarkOverdueAsync()`.
   - Configurable: `appsettings: Loans:OverdueCheckIntervalMinutes`
     default 60.
   - Registrar en `Program.cs`.

3. **Tests**:
   - `LoanReturnedHandlerTests`: handler con condition OK →
     BookCopy AVAILABLE; con LOST → BookCopy LOST + Loan LOST.
   - `OverdueMarkingTests`: préstamo con DueAt en el pasado y
     status ACTIVE → tras `MarkOverdueAsync` queda OVERDUE.

**Criterios de aceptación** (verificar después de que Frank
mergee su Lote FK-3):
- Crear préstamo, simular devolución vía endpoint de Frank, verificar
  que el préstamo pasó a RETURNED y la copia a AVAILABLE.
- Crear préstamo con `DueAt = NOW() - 1 día` directamente en BD,
  esperar al job (o llamar el método manualmente), verificar
  status OVERDUE.

**Si Frank todavía no mergeó**: dejar el handler escrito y los
tests skip con explicación. NO bloquear.

---

## Lote MX-6 — Dashboard KPIs

**Objetivo**: dashboard ejecutivo con métricas del sistema. Es lo
que se demuestra en la presentación final.

**Tareas**:

1. **Endpoint** `GET /api/dashboard/kpis` (policy:
   AdminOrLibrarian):
   - Devuelve:
     ```json
     {
       "books": { "total": 68, "active": 65 },
       "copies": { "total": 250, "available": 41,
                   "loaned": 27, "maintenance": 5 },
       "users": { "total": 542, "active": 530 },
       "loans": { "active": 27, "overdue": 3,
                  "totalThisMonth": 89 },
       "fines": { "pending": 12, "totalAmountPendingMxn": 890.50,
                  "paidThisMonth": 23 },
       "reservations": { "active": 8, "ready": 2 },
       "recentActivity": [ ... últimas 10 operaciones ... ]
     }
     ```
   - Implementar como una sola consulta optimizada o varias en
     paralelo con `Task.WhenAll`.

2. **Vista materializada o consultas crudas**: si las queries son
   lentas, crear vista materializada `dashboard_kpis_mv` que se
   refresca cada 5 min. Para Sprint 2 puede ir con queries normales
   si tardan < 200ms.

3. **Migración** `AddDashboardViews`: si decidimos vista
   materializada, va en migración con `Sql("CREATE MATERIALIZED
   VIEW ...")`.

4. **Frontend**:
   - `DashboardPageComponent` ya existe. Reescribir para consumir
     el endpoint.
   - Cards con números grandes (Tailwind), gráfico simple de
     préstamos por día (últimos 30 días) si querés meter Recharts
     o Chart.js.
   - Tabla de actividad reciente.
   - Refresh manual con botón.

5. **Tests**:
   - `DashboardServiceTests`: con datos seed conocidos, los
     conteos coinciden.

**Criterios de aceptación**:
- Login admin → /dashboard → cards con números reales.
- Crear préstamo → refresh → contador de "active loans"
  incrementa.
- Login STUDENT → no ve dashboard ejecutivo (o ve versión
  reducida; definir).

---

## Lote MX-7 — Documentación final

**Objetivo**: actualizar docs antes de cierre.

**Tareas**:

1. `docs/MODULOS.md` (crear): resumen de los 3 módulos, quién
   los desarrolló, endpoints, decisiones clave.
2. `docs/PRESTAMOS.md`: detalle del módulo, diagrama de estados de
   Loan (ACTIVE → OVERDUE → RETURNED/LOST), reglas de renovación.
3. Actualizar `README.md` con sección de préstamos y dashboard.
4. Actualizar `docs/DEUDA_TECNICA.md` con lo que haya quedado
   pendiente.

---

## Checklist final Marcelo

- [ ] MX-0 BookCopy UI mergeado
- [ ] MX-1 Hardening compartido
- [ ] MX-2 Loan domain + persistence
- [ ] MX-3 Loan service + endpoints + tests
- [ ] MX-4 Loan UI
- [ ] MX-5 Handler de devolución + job overdue
- [ ] MX-6 Dashboard KPIs
- [ ] MX-7 Docs
- [ ] PR final mergeado a develop
- [ ] Demo end-to-end con Frank y Max validando integración
