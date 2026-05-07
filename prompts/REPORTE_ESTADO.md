# Reporte de estado — BibliotecaUdem
**Fecha:** 2026-05-07
**Rama:** `develop` (sincronizada con `main`)

---

## Bugs documentados en `claude_code_prompts.md`

| # | Prompt | Descripción | Estado |
|---|--------|-------------|--------|
| B1 | PROMPT 1 | `GET /api/devoluciones` → 405; `POST` → 400 por DTO incompleto | **RESUELTO** |
| B2–B4 | PROMPT 2 | "500 pero el INSERT sí ocurrió" en préstamos, reservas y renovaciones | **RESUELTO** |
| B5 | PROMPT 3 | Notificaciones con placeholders sin sustituir (`**Libro**`, `**Usuario**`) | **RESUELTO** |
| B6 | PROMPT 4 | Búsqueda por código de barras devuelve todos los préstamos en lugar de filtrar | **RESUELTO** |
| B7 | PROMPT 4 | Preview de devolución muestra `—` en Copia y Usuario | **RESUELTO** |
| B8 | PROMPT 5 | Dashboard KPI "Ejemplares disponibles" muestra 0 | **RESUELTO** |
| B10 | PROMPT 6 | Chatbot ignora saludos ("hola", "ayuda", "help") | **RESUELTO** |
| — | PROMPT 7 | Errores HTTP silenciosos en botones Renovar / Reservar / Cancelar | **RESUELTO** |

---

## Lo que se hizo (sesión 2026-05-07)

### Nuevo archivo: `frontend/.../shared/utils/http-error.ts`

Función `resolveHttpError(error, fallback)` compartida entre componentes:

- **5xx** → devuelve siempre `"Ocurrió un error en el servidor. Intenta de nuevo."` — nunca el stack trace ni el body crudo.
- **4xx** → extrae el mensaje del body en orden de prioridad: `detail` → `message` → `error` → `title`.
  Cubre el caso concreto del 409 de préstamos que devuelve `{ "error": "Límite de préstamos alcanzado..." }` y antes se perdía.
- **Otro** → devuelve el `fallback` que ya tenía cada llamada.

### Tres componentes actualizados

Solo se cambió el manejo de errores HTTP, sin tocar la lógica de negocio:

| Componente | Botón / acción afectada |
|------------|------------------------|
| `loan-detail-page.component.ts` | Botón Renovar |
| `books-page.component.ts` | Botón Reservar + operaciones CRUD de libros |
| `my-reservations-page.component.ts` | Cancelación de reservas |

En los tres se eliminó el helper privado `resolveErrorMessage` duplicado y se reemplazó por la función compartida.

### Commit y rama

```
8fa53a2  fix: manejo de errores HTTP en frontend Angular + mejoras backend
edbaba1  Merge branch 'main' into develop — fix manejo de errores HTTP Angular
```

---

## Qué falta

### 1. Manejo de errores incompleto en componentes adicionales

**`new-return-page.component.ts` — submit (línea 112–113)**

```typescript
// Actual: solo lee .detail, ignora .message/.error, no distingue 5xx
const detail = (error as { error?: { detail?: string } })?.error?.detail;
this.submitError.set(detail ?? 'No fue posible registrar la devolución...');
```

Solución: reemplazar con `resolveHttpError` igual que en los otros tres componentes.

**`new-return-page.component.ts` — búsqueda (línea 70–73)**

El bloque `error:` de la búsqueda hardcodea el mensaje sin intentar leer el body del servidor.

**`dashboard-page.component.ts`**

Los errores al cargar KPIs tienen `error: () => {}` (vacío). Los widgets simplemente no cargan sin ningún aviso al usuario.

**`notification-bell.component.ts`**

El polling de notificaciones tiene `error: () => {}`. Falla en silencio.

---

### 2. TODO en producción

**`RuleBasedChatbotProvider.cs:13`**
```
// TODO deuda técnica: reemplazar por LLMChatbotProvider (Claude/OpenAI) implementando IChatbotProvider.
```

**`RuleBasedChatbotProvider.cs:118`**
```
// TODO: cambiar a /dashboard/libros/{id} cuando exista la ruta de detalle de libro
```
Esta ruta **no existe** en `app.routes.ts`. Si el usuario hace clic en un resultado del chatbot, el router cae en el wildcard `**` y redirige al login.

---

### 3. Ruta de detalle de libro faltante

El chatbot genera links a `/dashboard/libros/{id}` pero esa ruta no está definida en `app.routes.ts`.  
Mientras no exista la página, el TODO del chatbot debería cambiar el link a `/dashboard/catalogo` o similar para no romper la navegación.

---

### 4. Test con TODO sin implementar

`backend/Biblioteca.Tests/Tests/LoanAuthorizationTests.cs`:
```
// TODO Lote MX-3: implementar LoanAuthorizationTests cuando exista la entidad Loan y el controlador LoansController
```
El archivo está commiteado pero los tests no están escritos.

---

### 5. Deuda conocida (no son bugs del sistema)

| Ítem | Descripción | Acción |
|------|-------------|--------|
| B9 | El catálogo muestra 4 copias en lugar de 2 (plan de pruebas) | Verificar manualmente en BD antes de asumir bug |
| B11 | Tarifa $5 vs $10 en el plan de pruebas | Discrepancia de datos, no de código |

---

## Próximos pasos recomendados

1. **Aplicar `resolveHttpError`** en `new-return-page.component.ts` (submit y búsqueda) — mismo patrón de esta sesión, ~10 min.
2. **Mensajes de error visibles** en `dashboard-page` y `notification-bell` al fallar las cargas — riesgo bajo.
3. **Crear la ruta `/dashboard/libros/{id}`** o cambiar el link del chatbot a `/dashboard/catalogo` para no romper la navegación del chatbot.
4. **Implementar `LoanAuthorizationTests`** para el módulo de préstamos.
5. **Investigar B9** consultando directamente la BD (`SELECT * FROM BookCopies WHERE ...`).
