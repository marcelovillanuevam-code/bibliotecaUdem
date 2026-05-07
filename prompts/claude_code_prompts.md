# Claude Code Prompts — Biblioteca UDEM Bug Fixes

Prompts ordenados por causa raíz, no por número de bug.
Ejecuta en este orden: cada prompt depende de que el anterior esté resuelto.

---

## PROMPT 1 — [CRÍTICO] Arreglar endpoint `/api/devoluciones` (B1)

```
Tengo un sistema de biblioteca ASP.NET. El endpoint `/api/devoluciones` está roto de dos formas:

1. `GET /api/devoluciones` devuelve 405 Method Not Allowed
2. `POST /api/devoluciones` devuelve 400 Bad Request. El frontend envía el payload sin los campos `bookCopyId` ni `userId` porque el formulario los deja vacíos al seleccionar un préstamo del listado.

Tareas:
1. Encuentra el controlador que debería manejar `GET /api/devoluciones` y `POST /api/devoluciones`. Verifica que las rutas estén correctamente decoradas con `[HttpGet]` y `[HttpPost]`.
2. Para el GET: si el método existe pero con otra ruta o sin el atributo correcto, corrígelo.
3. Para el POST: encuentra el DTO que recibe el endpoint. Si requiere `bookCopyId` y `userId`, rastrea cómo el frontend obtiene esos valores al seleccionar un préstamo activo y corrige el binding del formulario para que los incluya en el payload.
4. Agrega validación explícita en el controlador para devolver 400 con mensaje descriptivo si `bookCopyId` o `userId` llegan nulos, en lugar de fallar silenciosamente.
5. Corre las pruebas existentes si las hay. Si no hay, crea al menos un test de integración básico para GET y POST /api/devoluciones.

No cambies la lógica de negocio de las devoluciones (estados DAMAGED, LOST, multas). Solo arregla el routing y el binding del DTO.
```

---

## PROMPT 2 — [MAYOR] Arreglar el patrón "500 pero la operación sí ocurrió" (B2, B3, B4)

> Este es el prompt más importante después del PROMPT 1. B2, B3 y B4 tienen la misma firma: el side-effect (INSERT en BD) se ejecuta, pero la construcción de la respuesta HTTP falla con 500. Probable causa: error en el serializador del DTO de salida o en la generación de la notificación post-operación.

```
En mi API de biblioteca ASP.NET tengo un patrón de bug que se repite en tres endpoints:

- `POST /api/prestamos` — a veces el préstamo se crea en BD pero el endpoint devuelve 500
- `POST /api/reservas` — la reserva siempre se crea en BD pero el endpoint siempre devuelve 500
- `POST /api/prestamos/{id}/renovaciones` — la renovación no parece ejecutarse y devuelve 500

El patrón sugiere que el INSERT se ejecuta exitosamente pero falla al construir la respuesta (serialización del DTO de salida, o al disparar la notificación post-operación).

Tareas:
1. Busca el handler/service de cada uno de estos tres endpoints.
2. Identifica qué ocurre DESPUÉS del INSERT/UPDATE: ¿hay una llamada a un servicio de notificaciones? ¿Se construye un DTO de respuesta con datos relacionados? ¿Hay una query adicional post-operación?
3. Encuentra el punto exacto donde se lanza la excepción (revisa logs, stack traces en el middleware de errores, o agrega try/catch temporales para aislar la línea).
4. Corrige cada uno sin alterar la lógica de negocio del INSERT/UPDATE. Si el error está en la generación de notificaciones, envuelve esa llamada en try/catch y loggea el error sin que aborte la respuesta HTTP.
5. Asegúrate de que si la operación lógica fue exitosa, el endpoint devuelva 201 Created con el DTO correcto, aunque la notificación falle.
6. Para `POST /api/prestamos`: cuando se viola el límite de préstamos (ej. STUDENT con 3 activos), el endpoint debe devolver **409 Conflict** con body `{ "error": "Límite de préstamos alcanzado para este rol" }`, no 500.
```

---

## PROMPT 3 — [MAYOR] Arreglar plantilla de notificaciones con placeholders sin sustituir (B5)

> Solo ejecuta este prompt después de tener el PROMPT 2 resuelto, porque la causa de B5 puede estar relacionada con el mismo bug de notificaciones.

```
En mi sistema de biblioteca, las notificaciones que llegan al usuario muestran placeholders sin sustituir:
- "Recibo de préstamo: **Libro**" (debería mostrar el título real)
- "Hola **Usuario**, registraste el préstamo del libro '**Libro**'" (debería mostrar nombre real y título real)

Tareas:
1. Encuentra el servicio de notificaciones y la plantilla de texto que genera estos mensajes.
2. Identifica los parámetros que se pasan al momento de crear la notificación (¿se pasa el objeto préstamo completo? ¿solo el ID?).
3. Si los datos del libro y usuario están disponibles en el contexto donde se dispara la notificación, corrige la sustitución de variables en la plantilla.
4. Si los datos NO están disponibles (se pasa solo un ID y no se resuelve el objeto), agrega la query necesaria para obtener título del libro y nombre del usuario antes de construir el mensaje.
5. Verifica que la corrección aplica para notificaciones de préstamo, renovación y reserva.
```

---

## PROMPT 4 — [MENOR] Arreglar búsqueda por código de barras en devoluciones y campos vacíos en preview (B6, B7)

```
En la vista de "Nueva devolución" de mi app de biblioteca Angular hay dos bugs relacionados:

**Bug 1 (B6):** Al buscar por código de barras (ej. `BC-CAS-001`), el resultado devuelve todos los préstamos activos del sistema en lugar de filtrar por el código ingresado.
- Revisa el componente de devoluciones y la llamada al API que se hace al buscar.
- Si el frontend no está enviando el código de barras como query param, corrígelo.
- Si el backend recibe el param pero no filtra, corrígelo en el endpoint correspondiente.

**Bug 2 (B7):** Al seleccionar un préstamo del listado en el flujo de devolución, los campos "Copia" y "Usuario" se muestran vacíos (`—`). Solo aparece el título del libro y la fecha.
- Encuentra el componente que renderiza el preview del préstamo seleccionado.
- Verifica qué propiedades del objeto préstamo espera y compara con lo que devuelve el endpoint (puede ser un mismatch de nombres de propiedad, ej. `bookCopy` vs `copy` vs `ejemplar`).
- Corrige el binding para que muestre el código de la copia y el nombre del usuario.

No cambies la lógica de devolución, solo el filtrado de búsqueda y el binding del preview.
```

---

## PROMPT 5 — [MENOR] Arreglar KPI "Ejemplares disponibles" en el dashboard (B8)

```
En el dashboard admin de mi app de biblioteca, el KPI "Ejemplares disponibles" muestra 0 cuando debería mostrar 2 (hay 7 ejemplares totales y 5 préstamos activos → 2 disponibles).

Tareas:
1. Encuentra el endpoint que provee los KPIs del dashboard (probablemente algo como `GET /api/dashboard` o `GET /api/estadisticas`).
2. Localiza la query que calcula "ejemplares disponibles".
3. Verifica si la query cuenta correctamente: total de ejemplares con estado activo MENOS ejemplares actualmente en préstamo activo.
4. Si hay una columna de estado en la tabla de ejemplares (`bookCopies`), verifica que los préstamos activos estén actualizando correctamente ese estado (puede ser que el estado no se sincronice y la query cuente 0 disponibles porque todos están marcados como "prestado").
5. Corrige la query o la lógica de sincronización de estado, lo que corresponda.
6. No cambies otros KPIs.
```

---

## PROMPT 6 — [MENOR] Arreglar chatbot para entender saludos y "ayuda" (B10)

```
El chatbot de mi app de biblioteca solo responde correctamente con keywords exactas (`mis prestamos`, `mis multas`, `buscar <título>`, `horario`, `ubicacion`, `contacto`). Para inputs como "ayuda", "hola", "hola que puedes hacer" cae en el fallback genérico.

Tareas:
1. Encuentra el servicio o controlador del chatbot y cómo hace el matching de intents.
2. Agrega los siguientes casos:
   - Saludos: palabras como "hola", "hi", "buenas", "buenos días/tardes" → responder con un mensaje de bienvenida y listar los comandos disponibles.
   - "ayuda" / "help" / "que puedes hacer" / "comandos" → responder con la lista de comandos disponibles con ejemplos.
3. Si el matching es por string exacto, conviértelo a matching case-insensitive y con trim() para evitar falsos negativos por espacios o mayúsculas.
4. No cambies la lógica de los intents que ya funcionan.
5. El mensaje de "ayuda" debe incluir ejemplos concretos: "Escribe 'mis prestamos' para ver tus préstamos activos", etc.
```

---

## PROMPT 7 — [UX] Propagar errores HTTP al usuario en lugar de fallar silenciosamente

> Este prompt es transversal. Ejecutar después de que los bugs principales estén corregidos.

```
En mi app Angular de biblioteca, varios endpoints devuelven error pero la UI falla silenciosamente (ej. el botón "Renovar" no hace nada visible, el botón "Reservar" muestra "error inesperado" pero igual cambia a "Reservado").

Tareas:
1. Encuentra el interceptor HTTP de Angular (si existe) o el servicio centralizado de manejo de errores.
2. Para errores 4xx: muestra un mensaje descriptivo al usuario usando el body del error si tiene un campo `message` o `error`. Ejemplo: 409 → "No puedes tener más de 3 préstamos activos."
3. Para errores 5xx: muestra un mensaje genérico "Ocurrió un error en el servidor. Intenta de nuevo." — no el stack trace.
4. El botón que disparó la acción NO debe cambiar de estado (ej. no cambiar a "Reservado") si el servidor devolvió un error. El estado del botón debe reflejar el estado real confirmado por el servidor.
5. Específicamente para el botón "Renovar": si el POST devuelve error, el botón debe quedarse habilitado y mostrar el error. No silenciar.
6. No cambies la lógica de negocio de ningún componente, solo el manejo de errores en las llamadas HTTP.
```

---

## Notas de ejecución

- **Orden obligatorio:** PROMPT 1 → PROMPT 2 → PROMPT 3 → el resto puede ir en paralelo.
- **PROMPT 2 es el más riesgoso:** toca tres endpoints. Pide a Claude Code que haga los cambios en ramas separadas o que genere diffs antes de aplicar.
- **Antes de ejecutar PROMPT 5:** verifica manualmente si los 5 préstamos activos tienen el estado correcto en la tabla `bookCopies`. Si no, el fix del KPI puede enmascarar un bug más profundo de sincronización de estados.
- **B9 (catálogo muestra 4 copias en lugar de 2):** no está en estos prompts porque puede ser un dato legítimo (ejemplares adicionales no documentados en el plan de pruebas). Investiga manualmente en la BD antes de asumir que es un bug.
- **B11 (tarifa $5 vs $10):** no es un bug del sistema, es una discrepancia con el plan de pruebas. No requiere prompt.
