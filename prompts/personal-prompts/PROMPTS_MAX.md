# DEVLER — Prompts Max
# Módulo: Reservas + Notificaciones + Chatbot + Catálogo extendido

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

Mi rol: Max. Mi módulo: Reservas, Notificaciones, Chatbot, y
mejoras al Catálogo (incluyendo Lote 4 pendiente de Sprint 0).

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
- Un commit por lote. Mensaje: "Lote MA-N: <título>".
- AsNoTracking en queries de lectura.
- Concurrency token (UseXminAsConcurrencyToken) en Reservation.
- NO llamar services de otros módulos directamente. Usar eventos
  de dominio (IDomainEventDispatcher).
- NO instalar paquetes nuevos sin justificarlo.
- Después de cada lote: parar, mostrar diff resumido + comandos
  curl de verificación. Esperar OK antes de continuar.

Branching: rama feature/reservas. Mergeo a develop al cerrar
cada lote, previo PR + review.

El documento CONTRATOS_COMPARTIDOS.md tiene los schemas de
entidades, endpoints, eventos y reglas de negocio compartidas
con Marcelo y Frank. Es ley. Si necesito cambiar algo de ahí, lo
discuto en grupo, no lo modifico unilateralmente.
```

---

## Lote MA-0 — Lote pendiente Sprint 0: Disponibilidad real en catálogo (Lote 4)

**Objetivo**: cerrar el Lote 4 pendiente de Sprint 0. El catálogo
debe mostrar disponibilidad real basada en BookCopy, no
hardcodeada.

**Contexto**: el backend ya tiene `BookCopy` con su CRUD (Lote 3
de Sprint 0 cerrado). Falta:
- Que el endpoint de catálogo proyecte conteos.
- Que la UI los muestre.

**Tareas**:

1. **DTO**: extender `LibroDto` o crear `LibroCatalogoDto` con:
   - `TotalCopies` (int)
   - `AvailableCopies` (int) — copias con `Status = AVAILABLE`

2. **`LibroRepository.GetAllAsync` y `GetByIdAsync`**: proyectar
   las copias en una sola query (no N+1). Usar LEFT JOIN con
   subquery o `GroupBy`. Verificar el SQL con logging de EF
   habilitado:
   ```csharp
   var query = from b in dbContext.Books.AsNoTracking()
               where b.DeletedAt == null
               select new LibroCatalogoDto
               {
                 Id = b.Id,
                 Title = b.Title,
                 // ...
                 TotalCopies = b.Copies.Count(c => c.DeletedAt == null),
                 AvailableCopies = b.Copies.Count(c =>
                   c.DeletedAt == null && c.Status == "AVAILABLE")
               };
   ```
   EF traduce esto a una sola query con subqueries correlacionadas
   o LATERAL JOIN según versión de PostgreSQL.

3. **AsNoTracking en lecturas de catálogo**: confirmar que está
   aplicado.

4. **Frontend** — en el catálogo:
   - Mostrar `availableCopies / totalCopies` en cada card.
   - Indicador visual: badge verde si `availableCopies > 0`, rojo
     si `= 0`, gris si `totalCopies = 0`.
   - Si la card del catálogo ya existe (Sprint 0), solo agregar
     los campos al modelo + binding en el template.

**Criterios de aceptación**:
- GET /api/libros devuelve cada libro con `totalCopies` y
  `availableCopies` correctos.
- Habilitar logging de EF (`"Microsoft.EntityFrameworkCore":
  "Information"`) y revisar logs: 1-2 queries totales para listar
  50 libros, no 51.
- Marcar una copia como MAINTENANCE → `availableCopies` decrementa.
- En el catálogo Angular, las cards muestran los conteos y el
  badge de color correcto.

---

## Lote MA-1 — Hardening compartido (parte de Lote 7 Sprint 0)

**Objetivo**: cerrar los bugs de hardening que tocan archivos de
mi módulo.

**Contexto**: Frank cierra el grueso del Lote 7 (BUGs 04, 07-13).
Yo me ocupo de:
- Verificar que mis DTOs futuros tendrán `[Required]` etc. (no
  hay que hacer nada todavía, pero recordarlo cuando los cree).
- Cuando Frank mergee el audit log, agregar mis entidades futuras
  (Reservation, Notification) a la lista de auditables.
- Test mínimo de Sprint 0 que me toca.

**Tareas**:

1. **Tests Sprint 0** (parte del Lote 8):
   - `BookAvailabilityTests` (porque cerré el Lote 4): listar
     libros incluye `availableCopies` correctos; cambiar status
     de copia → cambia el conteo.

**Criterios**: 1 test verde, fallaría si revertís el Lote MA-0.

---

## Lote MA-2 — Reservation + Notification: Domain + Persistence

**Objetivo**: schemas y persistencia.

**Contexto**: ver sección 4.6 y 4.7 de
`CONTRATOS_COMPARTIDOS.md`. Schema autoritativo.

**Importante**: Marcelo debe haber mergeado `Loan` antes de este
lote (porque Reservation tiene FK opcional a Loan vía
`FulfilledByLoanId`). Si todavía no, **rebasear a develop** o
hablar con él para coordinar.

**Tareas**:

1. **Entidades Domain** (`Biblioteca.Domain/Entities/`):
   - `Reservation.cs`.
   - `Notification.cs`.
   - Enums: `ReservationStatus { PENDING, READY, FULFILLED,
     CANCELLED, EXPIRED }`, `NotificationType { LOAN_RECEIPT,
     RETURN_RECEIPT, FINE_CREATED, RESERVATION_READY,
     DUE_REMINDER }`, `NotificationChannel { EMAIL, WHATSAPP,
     IN_APP }`, `NotificationStatus { PENDING, SENT, FAILED }`.

2. **Configuraciones EF**:
   - `ReservationConfig.cs`:
     - Tabla `reservations`.
     - Soft-delete + concurrency xmin.
     - Enum como string.
     - Índices:
       ```
       reservations_book_status_pos_idx
         (book_id, status, queue_position)
       reservations_user_status_idx
         (user_id, status)
       reservations_user_book_active_unique
         UNIQUE (user_id, book_id)
         WHERE status IN ('PENDING','READY')
       ```
       Único parcial: `HasFilter("status IN ('PENDING','READY')")`.
     - FK opcional a Loan (`FulfilledByLoanId`).
   - `NotificationConfig.cs`:
     - Tabla `notifications`.
     - Sin soft-delete (es histórico, no se borra).
     - Índice: `(status, created_at)` para el worker.
     - `payload_json` como columna `jsonb` en PostgreSQL:
       `HasColumnType("jsonb")`.

3. **DbSets**: `Reservations`, `Notifications`. Reservation va a
   la lista de auditables. Notification NO (es interno, mucho
   ruido).

4. **Migraciones** (en orden):
   - `AddReservations`
   - `AddNotifications`

5. **Repositorios**:
   - `IReservationRepository`:
     - GetByIdAsync, GetByUserAsync, GetByBookAsync (con filtros),
       GetActiveByUserAndBookAsync, GetNextInQueueAsync (la
       primera PENDING ordenada por queue_position),
       GetExpiredReadyAsync (READY con ExpiresAt < NOW), AddAsync,
       UpdateAsync, GetMaxQueuePositionAsync(bookId) — para
       calcular siguiente posición al agregar.
   - `INotificationRepository`:
     - AddAsync, GetPendingAsync(limit), UpdateAsync,
       GetByUserAsync(unreadOnly?, ct).

6. **`IReservationQueryService`** (Application/Common/Services/):
   - `Task<bool> HasActiveReservationsForBookAsync(Guid bookId,
     CancellationToken ct);`
   - **Esto lo consume Marcelo** para validar renovaciones. Lo
     implemento yo.

**Criterios de aceptación**:
- Migraciones aplican limpio.
- En psql: dos INSERTs de Reservation con mismo
  `(user_id, book_id)` y status PENDING → segundo falla por unique
  parcial.
- `dotnet build` y tests existentes verdes.

---

## Lote MA-3 — ReservationService + endpoints + tests

**Objetivo**: lógica de reservas con cola FIFO.

**Reglas clave**:
- Si el libro tiene `availableCopies > 0` → no permitir reserva
  (no tiene sentido, hacer préstamo directo). 409 con mensaje
  "Hay copias disponibles, registrá un préstamo en su lugar".
- Si `availableCopies = 0` → crear reserva con `QueuePosition =
  GetMaxQueuePositionAsync(bookId) + 1`, status PENDING.
- Bloqueo por deuda: validar
  `IUserEligibilityService.IsEligibleAsync` (Frank lo implementa;
  si no mergeó, stub que devuelve true con TODO).
- Un usuario no puede tener dos reservas activas del mismo libro
  (índice único protege; check previo da mejor mensaje).
- Cancelar: status CANCELLED, **promover** a los que tienen
  `queue_position > cancelled.queue_position` (decrementar en 1).
- Cuando una devolución llega y el libro tiene reservas PENDING:
  marcar la primera como READY, `ReadyAt = NOW`,
  `ExpiresAt = NOW + 48h`. Notificar al user.
- Si pasan 48h sin que se concrete préstamo → status EXPIRED,
  promover siguiente.

**Tareas**:

1. **DTOs**:
   - `CreateReservationRequest`: `BookId`.
   - `ReservationDto` con datos completos + título del libro.

2. **`ReservationService`**:
   - `CreateAsync(userId, CreateReservationRequest, ct)`:
     - Valida elegibilidad.
     - Valida `availableCopies = 0` (consultar repo de Books o
       calcularlo).
     - Valida no tiene reserva activa del mismo libro.
     - Calcula posición.
     - Crea Reservation status PENDING.
     - Notificación in-app: "Reserva confirmada, posición N".
   - `CancelAsync(reservationId, requestedBy, ct)`:
     - Solo dueño o ADMIN.
     - Solo si status PENDING o READY.
     - Status CANCELLED.
     - Promover los siguientes en cola.
   - `GetByUserAsync(userId, ct)`.
   - `GetQueueAsync(bookId, ct)` — cola del libro.

3. **Handler `LoanReturnedHandler`** (en mi módulo!):
   - Implementa `IDomainEventHandler<LoanReturned>`.
   - **Importante**: Marcelo también tiene un handler para
     `LoanReturned` (cierra el préstamo). Los dos handlers se
     ejecutan en orden de registro DI. **Coordinar para que el
     mío corra después** (necesito que el préstamo esté cerrado
     y la copia AVAILABLE primero — aunque para mi lógica, lo que
     importa es el bookId, no la copia).
   - Lógica:
     - Si Condition = LOST → no hacer nada (la copia está
       perdida, no resuelve reservas).
     - Si Condition = OK o DAMAGED:
       - Buscar `GetNextInQueueAsync(bookId)`.
       - Si hay → marcar status READY, ReadyAt = clock.Now,
         ExpiresAt = clock.Now + 48h.
       - Crear Notification(RESERVATION_READY) para ese usuario.

4. **Implementación de `IReservationQueryService`**:
   ```csharp
   public Task<bool> HasActiveReservationsForBookAsync(Guid bookId,
     CancellationToken ct) =>
     dbContext.Reservations
       .AnyAsync(r => r.BookId == bookId
                   && r.DeletedAt == null
                   && (r.Status == "PENDING" || r.Status == "READY"),
                 ct);
   ```

5. **Job de expiración** `ReservationExpirationBackgroundService`:
   - `IHostedService`, corre cada 15 min (configurable).
   - Lee `GetExpiredReadyAsync()`.
   - Para cada uno: status EXPIRED, promover siguiente (que pasa
     a READY, notificación).

6. **Controller** `ReservasController` (rutas según contrato 5.3).

7. **Tests** (`Biblioteca.Tests/Reservations/`):
   - `ReservationCreateTests`:
     - Libro con copias disponibles → 409.
     - Libro sin copias → reserva creada con posición 1.
     - Segunda reserva del mismo libro otro user → posición 2.
     - User con multa pendiente → 409.
     - Mismo user reservando dos veces el mismo libro → 409.
   - `ReservationCancelTests`:
     - Cancelar la posición 2 de 3 → la 3 queda en posición 2.
   - `LoanReturnedHandlerTests` (mi handler):
     - Devolución OK con cola → primera pasa a READY.
     - Devolución LOST → nadie pasa a READY.
   - `ReservationExpirationTests`:
     - READY con ExpiresAt en el pasado → tras correr el job,
       EXPIRED + siguiente queda READY.

**Criterios de aceptación** (curl):
```bash
TOKEN=$(curl -s -X POST localhost:5122/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"demo.lramirez","password":"Password123!"}' | jq -r .token)

# Reservar libro sin copias disponibles
curl -X POST localhost:5122/api/reservas \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"bookId":"<id>"}'
# → 201, queuePosition: 1

# Otro user reserva el mismo libro
# → 201, queuePosition: 2

# Devolver el libro (Frank)
# → mi handler corre: la primera reserva pasa a READY
# → notificación creada para el user

# Cancelar la primera reserva
curl -X DELETE localhost:5122/api/reservas/<id> \
  -H "Authorization: Bearer $TOKEN"
# → 204, las siguientes promueven posición
```

---

## Lote MA-4 — Notificaciones (servicio + worker + endpoints)

**Objetivo**: sistema de notificaciones con backend que envía
emails/WhatsApp y endpoint para inbox in-app.

**Decisión de diseño**:
- **Email**: usar SMTP via `System.Net.Mail.SmtpClient` con
  configuración en `appsettings: Notifications:Smtp` (host, port,
  username, password vía user-secrets/env, fromAddress). Para
  desarrollo, usar **MailHog** o un mock. En prod, configurar
  SMTP institucional UDEM.
- **WhatsApp**: stub. Implementar la interfaz pero la
  implementación real loguea y marca SENT. Documentar como deuda
  técnica que requiere integración con Twilio/WA Business API en
  producción.

**Tareas**:

1. **DTOs** y models:
   - `NotificationDto`.

2. **`INotificationService`**:
   - `EnqueueAsync(Guid userId, NotificationType type,
     NotificationChannel channel, string subject, string body,
     object payload, ct)` — crea row PENDING.
   - `GetInboxAsync(userId, unreadOnly, ct)`.
   - `MarkAsReadAsync(notificationId, userId, ct)`.

3. **Templating mínimo**: archivo `NotificationTemplates.cs`
   estático con métodos como:
   ```csharp
   public static (string subject, string body, object payload)
     LoanReceipt(LoanReceiptData data) => (
       subject: $"Recibo de préstamo: {data.BookTitle}",
       body: $"Hola {data.UserName}, registrás el préstamo del libro \"{data.BookTitle}\"...",
       payload: data);
   ```
   Uno por NotificationType. Sin engine de templates por ahora
   (deuda técnica documentada).

4. **Sender de email**: `IEmailSender` con
   `Task SendAsync(string to, string subject, string body,
   CancellationToken ct)`. Implementación con SmtpClient.

5. **Sender de WhatsApp stub**: `IWhatsAppSender`. Implementación
   que loguea y marca SENT.

6. **Worker** `NotificationDispatcherBackgroundService`:
   - `IHostedService`, corre cada 30 segundos (configurable).
   - Lee `GetPendingAsync(limit: 50)`.
   - Por cada Notification:
     - Switch por `Channel`:
       - EMAIL → IEmailSender (necesita resolver email del user).
       - WHATSAPP → IWhatsAppSender.
       - IN_APP → solo cambiar status SENT (la lectura es por
         query del user).
     - Si éxito → status SENT, SentAt.
     - Si falla → status FAILED, FailureReason. **Sin retry
       automático en Sprint 1**, deuda técnica.

7. **Handlers de eventos**:
   - `LoanCreatedHandler` → enqueue notificación LOAN_RECEIPT
     (canales EMAIL + IN_APP).
   - `FineCreatedHandler` → enqueue FINE_CREATED.
   - El de RESERVATION_READY ya lo creo desde
     `ReservationService` directamente.
   - Para DUE_REMINDER (recordatorio próximo a vencer): job
     opcional, deuda técnica si no entra en tiempo.

8. **Controller** `NotificacionesController`:
   - GET /api/notificaciones — inbox del user (filtra por user del
     JWT).
   - PATCH /api/notificaciones/{id}/leida — marca leída.

9. **Tests**:
   - `NotificationDispatcherTests`: PENDING → SENT tras correr el
     worker (mockear senders).
   - `NotificationEnqueueTests`: handler de LoanCreated crea row
     PENDING.

**Criterios de aceptación**:
- Crear préstamo → en BD, fila en `notifications` PENDING.
- Esperar 30s (o forzar el worker) → status SENT.
- Si SMTP configurado a MailHog/dev: el mail llega a la inbox
  local.
- GET /api/notificaciones del user → ve la notificación in-app.

---

## Lote MA-5 — UI de reservas + notificaciones en Angular

**Tareas**:

1. **Modelos y servicios**: `Reservation`, `Notification` models +
   `ReservationsApiService`, `NotificationsApiService`.

2. **Páginas en `features/reservas/`**:
   - `MyReservationsPageComponent` (`/dashboard/reservas`):
     - Lista de mis reservas con status visible (PENDING/READY/
       FULFILLED/CANCELLED/EXPIRED).
     - Si READY: contador regresivo de 48h, botón "Cancelar".
   - `BookQueuePageComponent` (`/dashboard/libros/:id/reservas`):
     - LIBRARIAN/ADMIN, ve la cola completa de un libro.

3. **Botón "Reservar" en catálogo**:
   - Si `availableCopies = 0` y user logueado → mostrar botón
     "Reservar".
   - Si tiene reserva activa: deshabilitado con tooltip "Ya tenés
     una reserva activa, posición N".
   - Si tiene multas pendientes: deshabilitado con tooltip.

4. **Notificaciones in-app**:
   - Componente `NotificationBellComponent` en el header con
     badge de no leídas.
   - Dropdown que muestra las últimas 10, click marca leída y
     navega al recurso (préstamo, multa, etc.).
   - Polling cada 60s (o websocket en futuro — deuda técnica).

**Criterios de aceptación**:
- Login STUDENT, en catálogo, libro sin copias → botón "Reservar".
- Click → reserva creada → toast → aparece en /dashboard/reservas.
- Cuando otro user (LIBRARIAN) registra devolución del libro: la
  reserva pasa a READY → notificación aparece en el bell.

---

## Lote MA-6 — Chatbot de ayuda

**Objetivo**: chatbot básico tipo "FAQ + buscador" — no requiere
LLM, deuda técnica documentada para integración con LLM real.

**Decisión de diseño**:
- Sprint 2 entrega chatbot **rule-based** que cubre:
  1. Búsqueda de libros por título/autor (consulta al endpoint de
     catálogo).
  2. "¿Tengo multas pendientes?" → endpoint /api/usuarios/me/
     multas.
  3. "¿Cuándo vence mi préstamo?" → /api/usuarios/me/prestamos.
  4. Preguntas frecuentes hardcodeadas (horarios, ubicación,
     contacto).
- Integración con LLM (Claude/OpenAI) queda como deuda
  documentada; la arquitectura (interfaz `IChatbotProvider`)
  permite swapear.

**Tareas**:

1. **DTOs**:
   - `ChatRequest { string Message; }`.
   - `ChatResponse { string Reply; List<ChatAction>? Actions; }`
     donde `ChatAction` puede ser `{ "label": "Ver libro", "url":
     "/dashboard/libros/<id>" }`.

2. **`IChatbotProvider`** + implementación
   `RuleBasedChatbotProvider`:
   - Match contra patrones simples (regex/keywords).
   - Para "buscar X" → consultar repos de Libros, devolver top 3.
   - Para "mis multas" → consultar `IFineRepository` con userId
     del JWT.
   - Para "mis préstamos" → consultar `ILoanRepository`.
   - FAQ hardcoded para keywords como "horario", "contacto",
     "ubicación".
   - Default: "No entendí, probá con: ..." (lista de comandos).

3. **Endpoint** `POST /api/chat` (Authenticated):
   - Recibe ChatRequest.
   - Llama `IChatbotProvider.AskAsync(message, userId, ct)`.
   - Devuelve ChatResponse.

4. **Frontend** `features/chatbot/`:
   - `ChatbotWidgetComponent`: botón flotante bottom-right en
     todas las páginas autenticadas.
   - Click abre panel lateral con historial de la sesión (en
     memoria, no se persiste).
   - Input de texto, envía a /api/chat, muestra respuesta + acciones.
   - Click en acción navega al url correspondiente.

5. **Tests**:
   - `RuleBasedChatbotTests`: queries comunes devuelven respuestas
     esperadas.

**Criterios de aceptación**:
- Login student → chatbot bottom-right.
- "buscar harry potter" → muestra top 3 libros con acción "Ver".
- "mis multas" → si tiene multa, la lista; si no, "Sin multas
  pendientes".
- "horario" → FAQ hardcoded.

---

## Lote MA-7 — Documentación final

**Tareas**:

1. `docs/RESERVAS_NOTIFICACIONES.md`: módulo, diagrama de estados
   de Reservation, política de cola y expiración.
2. `docs/CHATBOT.md`: arquitectura del chatbot, cómo agregar
   nuevas reglas, cómo migrar a LLM en futuro.
3. Actualizar `docs/DEUDA_TECNICA.md` con:
   - WhatsApp sender real (Twilio).
   - Templating engine para notificaciones (Razor/Scriban).
   - Retry automático en NotificationDispatcher.
   - Job de DUE_REMINDER si no se implementó.
   - Chatbot LLM-powered.
   - Polling → websocket para notificaciones.
4. Actualizar `README.md`.

---

## Checklist final Max

- [ ] MA-0 Disponibilidad real catálogo mergeado
- [ ] MA-1 Tests Sprint 0
- [ ] MA-2 Reservation + Notification domain + persistence
- [ ] MA-3 ReservationService + handler + endpoints + tests
- [ ] MA-4 Notification dispatcher + senders
- [ ] MA-5 UI reservas + notification bell
- [ ] MA-6 Chatbot
- [ ] MA-7 Docs
- [ ] PR final mergeado
- [ ] Demo end-to-end con Marcelo y Frank
