# DEVLER — Prompts Frank
# Módulo: Devoluciones + Multas + Pagos + Reportes

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

Mi rol: Frank. Mi módulo: Devoluciones, Multas, Pagos, Reportes,
y JWT hardening (Lote 6 lite pendiente).

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
- Un commit por lote. Mensaje: "Lote FK-N: <título>".
- AsNoTracking en queries de lectura.
- Concurrency token (UseXminAsConcurrencyToken) en Fine.
- NO llamar services de otros módulos directamente. Usar eventos
  de dominio (IDomainEventDispatcher).
- NO instalar paquetes nuevos sin justificarlo.
- Después de cada lote: parar, mostrar diff resumido + comandos
  curl de verificación. Esperar OK antes de continuar.

Branching: rama feature/devoluciones. Mergeo a develop al
cerrar cada lote, previo PR + review.

El documento CONTRATOS_COMPARTIDOS.md tiene los schemas de
entidades, endpoints, eventos y reglas de negocio compartidas
con Marcelo y Max. Es ley. Si necesito cambiar algo de ahí, lo
discuto en grupo, no lo modifico unilateralmente.
```

---

## Lote FK-0 — Lote pendiente Sprint 0: JWT hardening (Lote 6 lite)

**Objetivo**: cerrar el Lote 6 lite pendiente de Sprint 0.
Crítico antes de cualquier feature nueva de auth.

**Tareas**:

1. **Validación de dominio @udem.edu**:
   - En `AuthService.RegisterAsync`, validar que el email termine
     en `@udem.edu` (case-insensitive). Si no, lanzar
     `ValidationException` con mensaje "Solo se permiten correos
     institucionales @udem.edu".
   - Hacer el dominio configurable vía
     `appsettings: Auth:AllowedEmailDomain` con default
     "udem.edu", para no hardcodear.
   - **NO** aplicar la validación en login — usuarios legacy ya
     creados deben poder seguir logueándose. La validación es solo
     en alta.

2. **JWT secret fuera de appsettings**:
   - Mover `Jwt:SecretKey` a User Secrets en desarrollo
     (`dotnet user-secrets set`) y a variable de entorno
     (`Jwt__SecretKey`) en docker-compose.
   - Eliminar el valor real de `appsettings.json`. Dejar la clave
     con string vacío o eliminarla por completo.
   - Crear `appsettings.example.json` con la clave en blanco para
     documentación.
   - En startup, validar que el secret esté presente y tenga al
     menos 32 caracteres. Si no, fallar el arranque con mensaje
     claro: "Jwt:SecretKey no configurada o demasiado corta
     (mínimo 32 caracteres). Configurar vía user secrets o
     variable de entorno Jwt__SecretKey."
   - Actualizar `docker-compose.yml` para leer la variable desde
     un `.env` (que va en `.gitignore`). Crear `.env.example` con
     la variable vacía.

3. **Validación de password mínima en registro**:
   - Mínimo 8 caracteres, al menos una letra y un número.
   - Implementar en el DTO `RegisterUserRequest` con
     `[RegularExpression]` o validación en
     `AuthService.RegisterAsync`.
   - Mensaje de error en español: "La contraseña debe tener al
     menos 8 caracteres, incluyendo letras y números."

**Criterios de aceptación**:
- POST /api/auth/register con email "user@gmail.com" → 400 con
  mensaje del dominio.
- POST /api/auth/register con email "user@UDEM.EDU" → permitido
  (case-insensitive).
- POST /api/auth/register con password "abc123" → 400 con mensaje
  de password.
- Levantar la app sin `Jwt__SecretKey` en env → falla con mensaje
  claro, NO arranca con default inseguro.
- `grep -r "SecretKey" appsettings*.json` no devuelve la secret
  real.
- `.env` está en `.gitignore`; `.env.example` está commiteado.

---

## Lote FK-1 — Hardening crítico: GlobalException + Audit log + Transacción + DTOs (Lote 7 Sprint 0)

**Objetivo**: cerrar los bugs de hardening compartidos. Esto
desbloquea a Marcelo y Max porque el audit log y las validaciones
los necesitan los tres.

**Tareas**:

1. **BUG-04 — GlobalExceptionMiddleware**: en el bloque `default`
   / `_` del switch, fijar `Detail = "Se produjo un error
   inesperado."`. La excepción completa debe seguir logueándose
   internamente con el logger ya inyectado. Las excepciones de
   negocio (ValidationException, NotFoundException,
   ConflictException) sí pueden exponer su Message — esas son
   seguras por diseño.

2. **BUG-07 — Audit log automático**: override de
   `SaveChangesAsync` en `BibliotecaDbContext`:
   - Antes de SaveChanges, recorrer `ChangeTracker.Entries()`.
   - Para cada entrada de tipo Added/Modified/Deleted en entidades
     marcadas como auditables (Usuario, Libro, BookCopy como
     mínimo, **lista extensible**), crear un `RegistroAuditoria`
     con: EntityName, EntityId, Action, PerformedBy (leer del
     `ICurrentUserService` — créalo si no existe, que lea del
     HttpContext el user id del JWT), PerformedAt, Changes (JSON
     con campos modificados).
   - Si no hay HttpContext (jobs, seeds), PerformedBy = null.
   - **Todo dentro de la misma transacción que el SaveChanges
     principal** — agregar las entradas al ChangeTracker antes
     del SaveChanges, NO hacer un segundo SaveChanges aparte.
   - Definir interfaz/atributo `IAuditableEntity` o lista
     hardcodeada en el context — preferir lista hardcodeada para
     evitar tocar el Domain con cosas de infra.

3. **BUG-08 — EnsureReferenceDataAsync**: eliminar las llamadas en
   `UsuarioService.CreateAsync` y `UpdateAsync`. Verificar que
   `DatabaseInitializationExtensions` ya lo llama en startup; si
   no, agregarlo ahí.

4. **BUG-09 — Email index funcional**: agregar columna computada
   `value_lower` en user_contacts (igual que se hizo con
   username_lower en users) o crear índice funcional `CREATE INDEX
   ... ON user_contacts (type, lower(value))`. Migración
   `AddEmailLowerIndex`. En `EmailExistsAsync`, buscar contra esa
   columna/expresión usando `EF.Functions.ILike` o la columna
   computada según lo que hayas elegido.

5. **BUG-10 — CORS desde config**: leer `Cors:AllowedOrigins`
   (array) de IConfiguration. En appsettings poner
   `["http://localhost:4200"]` por default. Si está vacío en
   prod, fallar el startup con mensaje claro.

6. **BUG-12 — Validación de DTOs**: agregar `[Required]`,
   `[StringLength(N)]`, `[EmailAddress]` en `RegisterUserRequest`,
   `CreateUsuarioRequest`, `UpdateUsuarioRequest`,
   `CreateLibroRequest`, `UpdateLibroRequest`,
   `CreateBookCopyRequest`, `UpdateBookCopyRequest`.

7. **BUG-13 — Transacción en UsuarioService.UpdateAsync**:
   envolver en `await using var tx = await
   dbContext.Database.BeginTransactionAsync(ct)` igual que
   `RegisterAsync`.

**Criterios de aceptación**:
- Forzar un error 500 (ej: query con BD apagada) → response Detail
  exactamente "Se produjo un error inesperado.", logs internos
  con stack completo.
- Crear/modificar/borrar un usuario o libro → fila nueva en
  audit_logs con PerformedBy correcto.
- Levantar la app 100 veces seguidas → no se duplican filas en
  statuses ni roles.
- Buscar email con mayúsculas/minúsculas mezcladas → encuentra el
  usuario y `EXPLAIN` muestra Index Scan, no Seq Scan.
- POST con body vacío → 400 con ProblemDetails listando campos
  faltantes, no NullReferenceException.

**Coordinar con Marcelo y Max**: una vez mergeado, avisar para
que rebaseen y agreguen sus entidades al audit.

---

## Lote FK-2 — Tests Sprint 0 (mi parte del Lote 8)

**Objetivo**: tests de regresión de los bugs que cerré.

**Tareas** (`Biblioteca.Tests/Sprint0/`):
- `GlobalExceptionTests`: excepción genérica devuelve mensaje
  enmascarado (BUG-04).
- `AuditLogTests`: crear/modificar usuario → fila en audit_logs.
- `EmailDomainTests`: register con email no @udem.edu → 400;
  register con @UDEM.EDU → 201 (BUG-09 + Lote 6).
- `JwtSecretValidationTests`: app no arranca si Jwt:SecretKey
  está vacía o tiene menos de 32 caracteres.

**Criterios**: 4 tests verdes, cada uno fallaría si revertís el
fix correspondiente.

---

## Lote FK-3 — Return + Fine: Domain + Persistence

**Objetivo**: schemas y persistencia de devoluciones y multas. Sin
endpoints todavía.

**Contexto**: ver sección 4.3, 4.4, 4.5 de
`CONTRATOS_COMPARTIDOS.md`. Schema autoritativo, no inventar
campos.

**Importante**: Marcelo ya debe haber mergeado `Loan`. Si todavía
no, **rebasear a develop primero**. Si su PR está abierto pero no
mergeado, esperar.

**Tareas**:

1. **Entidades Domain** (`Biblioteca.Domain/Entities/`):
   - `Return.cs` con FK a Loan.
   - `Fine.cs` con FK a Return + denormalización de UserId.
   - `FineConfig.cs`.
   - Enums: `ReturnCondition { OK, DAMAGED, LOST }`,
     `FineReason { LATE, DAMAGE, LOSS }`,
     `FineStatus { PENDING, PAID, WAIVED }`.

2. **Configuraciones EF**:
   - `ReturnConfig.cs`:
     - Tabla `returns`.
     - Soft-delete.
     - Índice único en `loan_id`.
     - FK a Loan con CASCADE NO (RESTRICT — no borrar préstamo
       que tiene devolución).
     - FK a `Usuario` (ReceivedBy).
   - `FineConfig.cs` (ojo, el config de EF, no la entidad
     FineConfig):
     - Tabla `fines`.
     - Soft-delete + concurrency token xmin.
     - Índices: `(user_id, status)`, `return_id`.
     - Enums como string.
   - `FineConfigConfig.cs`:
     - Tabla `fine_configs`.
     - Sin soft-delete (es config histórica, no se borra).

3. **DbSets** en `BibliotecaDbContext`:
   - `Returns`, `Fines`, `FineConfigs`.
   - Agregar a lista de auditables (excepto FineConfig que es
     config).

4. **Migraciones** (en este orden):
   - `AddReturns`
   - `AddFineConfigs` con seed de un registro inicial:
     ```csharp
     migrationBuilder.InsertData(
       table: "fine_configs",
       columns: new[] { "id", "late_rate_per_day_mxn",
                        "damage_flat_mxn", "loss_flat_mxn",
                        "effective_from" },
       values: new object[] { Guid.NewGuid(), 5.00m, 200.00m,
                              500.00m, DateTime.UtcNow });
     ```
   - `AddFines`

5. **Repositorios**:
   - `IReturnRepository`: GetByIdAsync, GetByLoanAsync, AddAsync.
   - `IFineRepository`: GetByIdAsync, GetByUserAsync(filter
     status), HasPendingByUserAsync, AddAsync, UpdateAsync.
   - `IFineConfigRepository`: GetActiveAsync (busca el de mayor
     EffectiveFrom <= NOW()), AddAsync.

**Criterios de aceptación**:
- Migraciones aplican limpio en BD vacía.
- `SELECT * FROM fine_configs` muestra el seed inicial con tarifas.
- Doble INSERT con mismo `loan_id` en `returns` falla por unique.
- `dotnet build` sin warnings nuevos, tests existentes verdes.

---

## Lote FK-4 — IUserEligibilityService + FineService + ReturnService

**Objetivo**: lógica de negocio. Esto desbloquea a Marcelo (que
necesita `IUserEligibilityService`) y a Max (que necesita los
eventos de devolución).

**Contexto**: reglas en sección 7 de `CONTRATOS_COMPARTIDOS.md`.
Cálculo:
- `LATE`: `daysLate * fineConfig.LateRatePerDayMxn`.
- Si vuelve OK pero tarde → solo LATE.
- Si vuelve DAMAGED → LATE (si tarde) + DAMAGE (separadas).
- Si vuelve LOST → solo LOSS, copia LOST, préstamo LOST.

**Tareas**:

1. **`IUserEligibilityService`** (Application/Common/Services/):
   - `Task<EligibilityResult> IsEligibleAsync(Guid userId,
     CancellationToken ct);`
   - `EligibilityResult { bool IsEligible; string? Reason; }`
   - Implementación: si tiene `Fine.Status = PENDING` →
     `IsEligible = false, Reason = "Multas pendientes"`.
   - Esto lo consumen Marcelo (préstamo) y Max (reserva).

2. **DTOs** (Application/Returns/, Fines/, Payments/):
   - `CreateReturnRequest`: `LoanId`, `Condition`,
     `InspectionNotes` (opt).
   - `ReturnDto` con datos completos.
   - `FineDto`.
   - `ConfirmPaymentRequest`: `Method` (CASH, TRANSFER, CARD,
     ACADEMIC_BLOCK), `Reference` (string opt).
   - `WaiveFineRequest`: `Reason` (required, string).
   - `UpdateFineConfigRequest`: tarifas + `EffectiveFrom`.

3. **`FineCalculator`** (Application/Fines/Services/):
   - `CalculateAsync(Loan loan, ReturnCondition condition,
     CancellationToken ct): Task<List<FineDraft>>`
   - `FineDraft { FineReason Reason; decimal Amount; int?
     DaysLate; }`
   - Lee `IFineConfigRepository.GetActiveAsync()` una vez.
   - Devuelve lista vacía si OK y a tiempo, una si OK y tarde,
     dos si DAMAGED y tarde, una si LOST.

4. **`ReturnService`**:
   - `CreateAsync(CreateReturnRequest, receivedBy, ct)`:
     - Valida que el Loan existe, status ACTIVE u OVERDUE.
     - Valida que no tiene Return previo (índice único protege,
       pero verificar primero da mejor mensaje).
     - Crea Return.
     - Llama `FineCalculator.CalculateAsync` → si hay drafts,
       crea las Fine entities con status PENDING.
     - Publica evento `LoanReturned(LoanId, UserId, BookId,
       BookCopyId, Condition)` → Marcelo cierra el Loan, Max
       gestiona reservas.
     - Por cada Fine creada → publica `FineCreated(FineId,
       UserId, Amount, Reason)`.
     - **Todo en una transacción.**

5. **`FineService`**:
   - `GetByIdAsync(id, requesterUserId, ct)` con check de
     ownership.
   - `GetByUserAsync(userId, statusFilter?, ct)`.
   - `ConfirmPaymentAsync(fineId, ConfirmPaymentRequest, paidBy,
     ct)`:
     - Valida Fine existe, status PENDING.
     - Marca status PAID, PaidAt, PaidByUserId.
     - Publica `FinePaid(FineId, UserId)`.
   - `WaiveAsync(fineId, WaiveFineRequest, waivedBy, ct)`:
     - Solo ADMIN.
     - Status WAIVED, log del motivo en audit.

6. **`FineConfigService`**:
   - `GetActiveAsync(ct)`.
   - `UpdateAsync(UpdateFineConfigRequest, updatedBy, ct)` →
     crea registro nuevo con `EffectiveFrom`. NO update del
     anterior.

7. **Eventos**:
   - `LoanReturned`, `FineCreated`, `FinePaid` en
     `Application/Common/Events/`.

**Criterios de aceptación** (probar en tests del próximo lote):
- Crear Return con Loan ACTIVE on time + condition OK → no se crea
  Fine, se publica LoanReturned.
- Crear Return tarde con OK → se crea Fine LATE con monto
  correcto.
- Crear Return tarde con DAMAGED → se crean dos Fines (LATE +
  DAMAGE).
- Crear Return con LOST → una Fine LOSS, copia LOST.

---

## Lote FK-5 — Endpoints de devoluciones, multas, pagos + tests

**Objetivo**: API y tests.

**Tareas**:

1. **Controllers**:
   - `DevolucionesController` (rutas según contrato 5.2).
   - `MultasController`.
   - `ConfiguracionMultasController`.
   - Pagos: el contrato dice `POST /api/multas/{id}/pagos` y
     `POST /api/multas/{id}/condonar`. Va dentro de
     `MultasController`.
   - Policy `TreasuryOrAdmin`: definir en
     `ServiceCollectionExtensions`. Roles permitidos: ADMIN,
     TREASURY (rol nuevo, agregar al seed de roles si no existe).

2. **Tests** (`Biblioteca.Tests/Returns/, Fines/`):
   - `FineCalculatorTests`: 5 escenarios (on time OK, tarde OK,
     tarde DAMAGED, on time DAMAGED, LOST). Asserts sobre cantidad
     de FineDraft y montos.
   - `ReturnServiceCreateTests`: feliz path + error de Loan no
     ACTIVE + double return.
   - `FineServicePaymentTests`: pagar fine PENDING → PAID; pagar
     fine PAID otra vez → 409.
   - `UserEligibilityTests`: user con fine PENDING → NoEligible;
     pagada → Eligible.
   - `FineConfigTests`: crear nueva config con EffectiveFrom
     futuro → la activa sigue siendo la vieja hasta esa fecha.

**Criterios de aceptación** (curl):
```bash
TOKEN=$(curl -s -X POST localhost:5122/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin1","password":"Admin123!"}' | jq -r .token)

# Devolución a tiempo OK → sin multa
curl -X POST localhost:5122/api/devoluciones \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"loanId":"<id>","condition":"OK"}'
# → 201, sin Fine asociada

# Devolución tarde → multa LATE
# (modificar dueAt en BD a fecha pasada)
# → 201, Fine creada con monto = días * tarifa

# Listar multas del usuario
curl localhost:5122/api/usuarios/<userId>/multas \
  -H "Authorization: Bearer $TOKEN"
# → array con la multa PENDING

# Confirmar pago
curl -X POST localhost:5122/api/multas/<fineId>/pagos \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"method":"CASH"}'
# → 200, status PAID
```

---

## Lote FK-6 — UI de devoluciones, multas y pagos en Angular

**Tareas**:

1. **Modelos y servicios**:
   - `Return`, `Fine`, `FinePayment`, `FineConfig` models.
   - `ReturnsApiService`, `FinesApiService`.

2. **Páginas en `features/devoluciones/` y `features/multas/`**:
   - `ReturnsPageComponent` (`/dashboard/devoluciones`):
     LIBRARIAN/ADMIN, listado de devoluciones recientes.
   - `NewReturnPageComponent` (`/dashboard/devoluciones/nueva`):
     - Buscar préstamo por barcode de copia o por usuario.
     - Form: condition (radio: OK/DAMAGED/LOST), notes.
     - Al enviar: muestra resultado + multas generadas si aplica.
   - `FinesPageComponent` (`/dashboard/multas`):
     - LIBRARIAN/ADMIN ven todas con filtros.
     - STUDENT/TEACHER ven solo las suyas.
   - `FineDetailPageComponent` (`/dashboard/multas/:id`):
     - Datos de la multa.
     - Si rol permite y status PENDING: botón "Confirmar pago"
       (modal con método y referencia).
     - Si ADMIN: botón "Condonar" con motivo.
   - `FineConfigPageComponent` (`/dashboard/configuracion/multas`):
     - ADMIN, edita tarifas vigentes y crea config con
       EffectiveFrom.

3. **Indicador en header**: si user tiene multas PENDING, badge
   rojo en su perfil/menú.

**Criterios de aceptación**:
- Flujo completo: registrar devolución tarde → multa generada →
  visible en /multas → confirmar pago → status PAID → desbloqueo.
- Login STUDENT con multa pendiente → badge rojo + el módulo de
  préstamos rechaza nuevo préstamo (esto lo valida Marcelo, pero
  el endpoint de eligibility ya está).

---

## Lote FK-7 — Reportes

**Objetivo**: reportes ejecutivos para administración.

**Tareas**:

1. **Endpoints** `GET /api/reportes/...` (policy AdminOnly):
   - `/multas-recaudadas?from=<date>&to=<date>` → suma de Fines
     PAID en rango, agrupado por mes y por reason.
   - `/multas-pendientes` → lista de usuarios con multas PENDING
     ordenados por monto desc.
   - `/devoluciones-tardias?from=<date>&to=<date>` → conteo y
     promedio de días de retraso.
   - `/condonaciones?from=<date>&to=<date>` → fines WAIVED con
     motivo y quién las hizo.

2. **Frontend** `features/reportes/`:
   - `ReportsPageComponent`: tabs por reporte, selector de fechas,
     tabla + gráfico simple.
   - Botón "Exportar CSV" — generar client-side, sin dependencias
     nuevas.

**Criterios de aceptación**:
- Filtros funcionan, totales coinciden con queries directas en
  psql.
- Export CSV abre en Excel sin problemas.

---

## Lote FK-8 — Documentación final

**Tareas**:

1. `docs/DEVOLUCIONES_MULTAS.md`: detalle del módulo, diagrama de
   estados de Fine (PENDING → PAID/WAIVED), tabla de cálculo.
2. Actualizar `docs/DEUDA_TECNICA.md`: lo que haya quedado pendiente.
3. Actualizar `README.md`.

---

## Checklist final Frank

- [ ] FK-0 JWT hardening mergeado
- [ ] FK-1 Hardening compartido (BUGs 04, 07-13)
- [ ] FK-2 Tests Sprint 0
- [ ] FK-3 Return + Fine domain + persistence
- [ ] FK-4 Services (incl. IUserEligibilityService)
- [ ] FK-5 Endpoints + tests
- [ ] FK-6 UI devoluciones/multas/pagos
- [ ] FK-7 Reportes
- [ ] FK-8 Docs
- [ ] PR final mergeado
- [ ] Demo end-to-end con Marcelo y Max
