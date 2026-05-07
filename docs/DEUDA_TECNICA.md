# Deuda Técnica

Última actualización: 2026-05-07 (post Lote FK-7).

---

## Alta prioridad

### DT-01 — `borrowerName` / `bookTitle` ausentes en `FineDto`
**Módulo:** Frank (Multas)  
**Descripción:** El modelo Angular `FineRecord` tiene campos `borrowerName`,
`borrowerEmail` y `bookTitle` que el backend no devuelve actualmente.
`FinesPageComponent` y `FineDetailPageComponent` los muestran como `null` (`—`).  
**Causa:** El `FineDto` sólo expone IDs. Require un Include de
`User → Profile + Contacts` y del ejemplar → libro en el repositorio de multas.  
**Esfuerzo estimado:** 2-3 h (modificar `FineRepository.GetByIdAsync` y
`GetByUserAsync` + actualizar `FineDto` y el mapper).

### DT-02 — Migraciones vacías
**Módulo:** Compartido  
**Descripción:** Las migraciones `AddReturns`, `AddFineConfigs` y `AddFines`
tienen cuerpos `Up/Down` vacíos porque el esquema se aplicó via SQL directo.
Si se regenera la BD desde cero con `dotnet ef database update`, esas tablas
no se crearán.  
**Causa:** El esquema inicial fue diseñado en SQL antes de tener las
configuraciones EF definitivas.  
**Esfuerzo estimado:** 4-6 h (reescribir los tres `Up/Down` con los
`migrationBuilder.*` correctos o consolidar en una sola migración inicial).

---

## Media prioridad

### DT-03 — Motivo de pago (`ConfirmPaymentRequest.Reference`) no persiste
**Módulo:** Frank (Pagos)  
**Descripción:** `ConfirmPaymentRequest` tiene campos `Method` y `Reference`
que se reciben en el endpoint pero nunca se guardan en `Fine`.
No hay forma de auditar qué método de pago se usó ni la referencia de
transferencia/tarjeta.  
**Esfuerzo estimado:** 2 h (agregar `PaymentMethod string?` y `PaymentReference string?`
a `Fine`, nueva migración, actualizar `FineService.ConfirmPaymentAsync`).

### DT-04 — Tests unitarios del módulo Devoluciones/Multas
**Módulo:** Frank  
**Descripción:** `ReturnService`, `FineService` y `FineCalculator` no tienen
cobertura de tests unitarios. El proyecto `Biblioteca.Tests` existe pero
está vacío en este módulo.  
**Esfuerzo estimado:** 4-6 h (mocks de repositorios, escenarios de cálculo
tardío, pago doble, condonación de multa ya pagada).

### DT-05 — Reportes: `promedioDiasRetraso` incluye nulos
**Módulo:** Frank (Reportes)  
**Descripción:** En `ReportesService.GetDevolucionesTardiasAsync`, el promedio
se calcula con `.Where(d => d.HasValue).Average(...)`. Si `DaysLate` es `null`
(multa LATE sin días registrados por algún bug de inserción), esos registros
se omiten del promedio sin alertar. Podría enmascarar datos corruptos.  
**Esfuerzo estimado:** 1 h (agregar log/warn cuando `DaysLate` es null en
una fine de tipo LATE).

### DT-06 — `FineConfig` sin soft-delete
**Módulo:** Frank (Configuración)  
**Descripción:** `FineConfig` es la única entidad del módulo sin `DeletedAt`
ni `HasQueryFilter`. Si se elimina un registro de config activo accidentalmente,
no hay forma de recuperarlo sin acceso a BD.  
**Esfuerzo estimado:** 1-2 h (agregar `DeletedAt`, `HasQueryFilter` y
migración correspondiente).

---

## Baja prioridad

### DT-07 — Paginación en listados de multas y reportes
**Módulo:** Frank  
**Descripción:** `GET /api/multas` y `GET /api/reportes/multas-pendientes`
devuelven todos los registros sin paginación. En producción con miles de
multas, esto puede causar timeouts o respuestas de varios MB.  
**Esfuerzo estimado:** 3-4 h (agregar `PagedResult<T>`, parámetros `page`/`pageSize`
a los repositorios y endpoints).

### DT-08 — Export CSV server-side para reportes grandes
**Módulo:** Frank (Reportes)  
**Descripción:** El export CSV actual se genera en cliente via `Blob`.
Para conjuntos de datos grandes (miles de multas), cargar todo el JSON
en memoria del browser y convertir a CSV puede ser lento.  
**Esfuerzo estimado:** 4 h (endpoint `GET /api/reportes/.../export.csv`
con `StreamingCsvResult`, sin cargar todo en memoria).

### DT-09 — Validación de rango de fechas en frontend
**Módulo:** Frank (Reportes)  
**Descripción:** Si el usuario elige `from` > `to` en los filtros de
reportes, el backend retorna `400 Bad Request`, pero el frontend no
muestra un mensaje de error diferenciado (muestra el genérico
"No se pudo cargar el reporte").  
**Esfuerzo estimado:** 1 h (validación reactiva en `ReportsPageComponent`
antes de llamar a la API).

### DT-10 — `FinePaid` sin handler activo
**Módulo:** Frank / Max  
**Descripción:** El evento de dominio `FinePaid` se despacha tras cada
pago confirmado pero no tiene handler registrado. Fue marcado como
"informativo" en CONTRATOS_COMPARTIDOS.md pero no se documentó si
Max planea consumirlo para notificaciones de confirmación de pago.  
**Esfuerzo estimado:** Decisión de equipo (0 h si se descarta,
2-3 h si se implementa el handler de notificación).
