# Deuda Tecnica

Ultima actualizacion: 2026-05-07 (post Lote MX-7 / MA-7 — cierre Bloque 2).

---

## Alta prioridad

### DT-01 - `borrowerName` / `bookTitle` ausentes en `FineDto`
**Modulo:** Frank (Multas)  
**Descripcion:** El modelo Angular `FineRecord` tiene campos `borrowerName`,
`borrowerEmail` y `bookTitle` que el backend no devuelve actualmente.
`FinesPageComponent` y `FineDetailPageComponent` los muestran como `null`.  
**Causa:** El `FineDto` solo expone IDs. Requiere un Include de
`User -> Profile + Contacts` y del ejemplar -> libro en el repositorio de multas.  
**Esfuerzo estimado:** 2-3 h.

### DT-02 - Migraciones vacias
**Modulo:** Compartido  
**Descripcion:** Las migraciones `AddReturns`, `AddFineConfigs` y `AddFines`
tienen cuerpos `Up/Down` vacios porque el esquema se aplico via SQL directo.
Si se regenera la BD desde cero con `dotnet ef database update`, esas tablas
no se crearan.  
**Causa:** El esquema inicial fue disenado en SQL antes de tener las
configuraciones EF definitivas.  
**Esfuerzo estimado:** 4-6 h.

---

## Media prioridad

### DT-03 - Motivo de pago (`ConfirmPaymentRequest.Reference`) no persiste
**Modulo:** Frank (Pagos)  
**Descripcion:** `ConfirmPaymentRequest` tiene campos `Method` y `Reference`
que se reciben en el endpoint pero nunca se guardan en `Fine`.
No hay forma de auditar que metodo de pago se uso ni la referencia de
transferencia/tarjeta.  
**Esfuerzo estimado:** 2 h.

### DT-04 - Tests unitarios del modulo Devoluciones/Multas
**Modulo:** Frank  
**Descripcion:** `ReturnService`, `FineService` y `FineCalculator` no tienen
cobertura de tests unitarios. El proyecto `Biblioteca.Tests` existe pero
esta vacio en este modulo.  
**Esfuerzo estimado:** 4-6 h.

### DT-05 - Reportes: `promedioDiasRetraso` incluye nulos
**Modulo:** Frank (Reportes)  
**Descripcion:** En `ReportesService.GetDevolucionesTardiasAsync`, el promedio
se calcula con `.Where(d => d.HasValue).Average(...)`. Si `DaysLate` es `null`
en una multa LATE, esos registros se omiten del promedio sin alertar.  
**Esfuerzo estimado:** 1 h.

### DT-06 - `FineConfig` sin soft-delete
**Modulo:** Frank (Configuracion)  
**Descripcion:** `FineConfig` es la unica entidad del modulo sin `DeletedAt`
ni `HasQueryFilter`. Si se elimina un registro de config activo accidentalmente,
no hay forma de recuperarlo sin acceso a BD.  
**Esfuerzo estimado:** 1-2 h.

---

## Baja prioridad

### DT-07 - Paginacion en listados de multas y reportes
**Modulo:** Frank  
**Descripcion:** `GET /api/multas` y `GET /api/reportes/multas-pendientes`
devuelven todos los registros sin paginacion. En produccion con miles de
multas, esto puede causar timeouts o respuestas de varios MB.  
**Esfuerzo estimado:** 3-4 h.

### DT-08 - Export CSV server-side para reportes grandes
**Modulo:** Frank (Reportes)  
**Descripcion:** El export CSV actual se genera en cliente via `Blob`.
Para conjuntos de datos grandes, cargar todo el JSON en memoria del browser
y convertir a CSV puede ser lento.  
**Esfuerzo estimado:** 4 h.

### DT-09 - Validacion de rango de fechas en frontend
**Modulo:** Frank (Reportes)  
**Descripcion:** Si el usuario elige `from` > `to` en filtros de reportes,
el backend retorna `400 Bad Request`, pero el frontend muestra el mensaje
generico "No se pudo cargar el reporte".  
**Esfuerzo estimado:** 1 h.

### DT-10 - `FinePaid` sin handler activo
**Modulo:** Frank / Max  
**Descripcion:** El evento de dominio `FinePaid` se despacha tras cada pago
confirmado pero no tiene handler registrado. Fue marcado como informativo en
`CONTRATOS_COMPARTIDOS.md`, pero no se documento si Max planea consumirlo para
notificaciones de confirmacion de pago.  
**Esfuerzo estimado:** Decision de equipo.

### DT-11 - Polling en notificaciones in-app
**Modulo:** Max (Notificaciones)  
**Descripcion:** `NotificationBellComponent` consulta el inbox cada 60 segundos.
Es suficiente para MA-5, pero deberia reemplazarse por websocket o SignalR
cuando el backend exponga un canal push.  
**Esfuerzo estimado:** 3-5 h.

---

## Deuda agregada en Lote MX-7 / MA-7

### DT-12 - WhatsApp / SMS real para notificaciones
**Modulo:** Max (Notificaciones)  
**Descripcion:** Las notificaciones del sistema son actualmente solo in-app. No existe
integracion con un proveedor de mensajeria externo. Para notificaciones criticas
(reserva lista, vencimiento proximo) se deberia conectar Twilio SMS/WhatsApp o
un proveedor similar implementando una nueva clase que implemente `INotificationChannel`
(o similar) en Infrastructure.  
**Esfuerzo estimado:** 4-6 h (incluye alta en Twilio y manejo de opt-in de usuarios).

### DT-13 - Templating engine para cuerpo de notificaciones
**Modulo:** Max (Notificaciones)  
**Descripcion:** `NotificationTemplates.cs` usa interpolacion manual de strings con
`string.Create` / concatenacion. Cuando se agreguen canales HTML (email enriquecido,
WhatsApp con formato) se recomienda sustituir por Razor o Scriban para mantener
plantillas editables sin recompilar.  
**Esfuerzo estimado:** 3-4 h.

### DT-14 - Retry automatico en NotificationDispatcher
**Modulo:** Max (Notificaciones)  
**Descripcion:** `DomainEventDispatcher` invoca los handlers directamente sin politica
de reintento. Si un handler falla (e.g. timeout de BD), la notificacion se pierde
silenciosamente. Se recomienda agregar Polly con exponential backoff o una outbox
pattern para garantizar entrega at-least-once.  
**Esfuerzo estimado:** 4-6 h.

### DT-15 - Job DUE_REMINDER no implementado
**Modulo:** Marcelo / Max  
**Descripcion:** No existe background service que envie un recordatorio al usuario
cuando su prestamo esta a 1-2 dias de vencer (`DueAt - now < 48h && Status == ACTIVE`).
Requiere un nuevo `IHostedService` y un template de notificacion `DUE_REMINDER`.  
**Esfuerzo estimado:** 2-3 h.

### DT-16 - Chatbot LLM-powered
**Modulo:** Max (Chatbot)  
**Descripcion:** `RuleBasedChatbotProvider` cubre los intents basicos pero no entiende
variaciones de lenguaje natural. La interfaz `IChatbotProvider` esta disenada para
ser sustituida. Implementar `LlmChatbotProvider` (Claude / OpenAI) con historial de
conversacion y function calling para las queries de BD.  
**Esfuerzo estimado:** 6-10 h (incluye gestion de costos, fallback y rate limiting).

### DT-17 - Polling → WebSocket para notificaciones in-app
**Modulo:** Max (Notificaciones)  
**Descripcion:** Ver DT-11. Se registra nuevamente con mayor contexto: el backend
deberia exponer un hub SignalR en `/hubs/notificaciones` y `NotificationService.EnqueueAsync`
deberia hacer push en tiempo real en lugar de esperar el proximo tick del cliente.  
**Esfuerzo estimado:** 5-8 h (incluye cambios en frontend Angular con `@microsoft/signalr`).
