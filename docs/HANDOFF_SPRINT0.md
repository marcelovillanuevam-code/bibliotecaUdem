# DEVLER — Punto de partida Sprint 0
**Fecha:** 2026-05-07 | **Repo:** `C:\bibliotecaUdem-main\bibliotecaUdem-main`

---

## Contexto del proyecto

Sistema de Gestión de Biblioteca UDEM (DEVLER).
- **Backend:** ASP.NET Core 10, Clean Architecture (Domain / Application / Infrastructure / Persistence / API)
- **Frontend:** Angular 21 standalone
- **BD:** PostgreSQL 17
- **Solución:** `BibliotecaUdem.slnx`

---

## Estado del entorno

```powershell
# Levantar entorno completo
cd C:\bibliotecaUdem-main\bibliotecaUdem-main
docker compose up -d

# Usuarios seed disponibles
# admin1        / Admin123!     → rol ADMIN
# demo.mlopez   / Password123!  → rol TEACHER
# demo.lramirez / Password123!  → rol STUDENT
# demo.amartinez/ Password123!  → rol STUDENT
# demo.fsanchez / Password123!  → rol LIBRARIAN (inactive - no puede loguear)

# Login: campo es "username" (no "usernameOrEmail"), respuesta tiene "accessToken"
# Puertos
# Backend:  http://localhost:5122  (Swagger: http://localhost:5122/swagger)
# Frontend: http://localhost:4200
# Postgres: localhost:5432  DB=biblioteca_udem user=postgres pass=postgres
```

**Nota de entorno:** .NET SDK no está instalado localmente. Para generar migraciones EF se usa un contenedor Docker SDK. Ver sección "Cómo generar migraciones" abajo.

---

## Bugs del Sprint 0 y estado

| Bug | Descripción | Estado |
|-----|-------------|--------|
| BUG-01 | Sin autorización por rol en endpoints | ✅ CERRADO en Lote 1 |
| BUG-02 | Hard-delete en libros (debería ser soft) | ✅ CERRADO en Lote 2 |
| BUG-03 | CU-07 completamente ausente (BookCopy) | ✅ CERRADO en Lote 3 |
| BUG-04 | Exception.Message expuesto en 500 | ✅ CERRADO en Lote 7 |
| BUG-05 | Google OAuth | ❌ Excluido (decisión de producto) |
| BUG-06 | Sin JWT refresh token | ❌ Excluido (deuda técnica Sprint 1) |
| BUG-07 | Audit log no escribe en BD | ✅ CERRADO en Lote 7 |
| BUG-08 | EnsureReferenceData en cada Create/Update | ✅ CERRADO en Lote 7 (residual en `AuthService.RegisterAsync` cerrado en pureba-max) |
| BUG-09 | Email lookup sin índice funcional | ✅ CERRADO en Lote 7 (`20260507010000_AddEmailLowerIndex`) |
| BUG-10 | CORS hardcodeado | ✅ CERRADO en Lote 7 |
| BUG-11 | Doble AsNoTracking en LibroRepository | ✅ CERRADO en Lote 2 |
| BUG-12 | Sin validación [Required] en DTOs | ✅ CERRADO en Lote 7 |
| BUG-13 | Sin transacción en UsuarioService.UpdateAsync | ✅ CERRADO en Lote 7 |
| BUG-14 | GetByIdForUpdateAsync sin Include(Contacts) | ✅ CERRADO en Lote 2 (era falso positivo, ya estaba incluido) |

---

## Lotes del plan y estado

| Lote | Objetivo | Estado |
|------|----------|--------|
| **Lote 0** | Verificación de build | ✅ Completado |
| **Lote 1** | Autorización por rol (BUG-01) | ✅ Completado |
| **Lote 2** | Soft-delete Libro + fix contacts (BUG-02, BUG-11, BUG-14) | ✅ Completado |
| **Lote 3** | Entidad BookCopy backend (CU-07, BUG-03) | ✅ Completado |
| **Lote 4** | Disponibilidad real en catálogo | ✅ Completado (mergeado como MA-4) |
| **Lote 5** | BookCopy Angular (CU-07 frontend) | ✅ Completado |
| **Lote 6** | JWT hardening mínimo (@udem.edu + secret seguro) | ✅ Completado |
| **Lote 7** | Hardening infraestructura (BUGs P1/P2 restantes) | ✅ Completado |
| **Lote 8** | Tests mínimos (xUnit + FluentAssertions) | ✅ Completado (28/28 verdes) |

**Sprint 0 cerrado.** Siguiente fase: Sprint 1 (Préstamos / Devoluciones / Reservas) según `prompts/personal-prompts/PLAN_INTEGRACION.md`. Próximo lote crítico: **MX-2 Loan + LoanRenewal** (Marcelo, mergea primero porque Frank y Max dependen).

---

## Archivos modificados hasta ahora

### Lote 1 — Autorización por rol
- `backend/Biblioteca.Infrastructure/DependencyInjection.cs` — eliminado `AddAuthorization()` (movido a API)
- `backend/Biblioteca.API/Extensions/ServiceCollectionExtensions.cs` — agregado `AddAuthorization` con 3 policies
- `backend/Biblioteca.API/Extensions/AuthPolicies.cs` — **nuevo** — constantes `AdminOnly`, `AdminOrLibrarian`, `Authenticated`
- `backend/Biblioteca.API/Controllers/LibrosController.cs` — policies por método (GET→Authenticated, POST/PUT/DELETE→AdminOrLibrarian)
- `backend/Biblioteca.API/Controllers/UsuariosController.cs` — todos los métodos → AdminOnly

### Lote 2 — Soft-delete Libro + fixes
- `backend/Biblioteca.Domain/Entities/Libro.cs` — agregado `DateTime? DeletedAt`
- `backend/Biblioteca.Persistence/Configurations/LibroConfig.cs` — mapeado `deleted_at` + `HasQueryFilter(l => l.DeletedAt == null)`
- `backend/Biblioteca.Persistence/Configurations/LibroAutorConfig.cs` — `HasQueryFilter` simétrico
- `backend/Biblioteca.Persistence/Configurations/LibroMateriaConfig.cs` — `HasQueryFilter` simétrico
- `backend/Biblioteca.Persistence/Repositories/LibroRepository.cs` — `IDateTimeProvider` inyectado; `Remove()` → soft-delete; eliminado segundo `AsNoTracking()`
- `backend/Biblioteca.Persistence/Repositories/UsuarioRepository.cs` — comentario documentando Include(Contacts) existente
- `backend/Biblioteca.Persistence/Migrations/20260507002823_AddDeletedAtToBooks.cs` — **nueva migración**
- `backend/Biblioteca.Persistence/Migrations/20260507002823_AddDeletedAtToBooks.Designer.cs` — **generado**
- `backend/Biblioteca.Persistence/Migrations/BibliotecaDbContextModelSnapshot.cs` — **actualizado por EF**

### Lote 3 — Entidad BookCopy backend
- `backend/Biblioteca.Domain/Entities/Libro.cs` — agregado `ICollection<BookCopy> Copies`
- `backend/Biblioteca.Domain/Entities/BookCopy.cs` — **ya existía**; sin cambios (BookCopyStatus + BookCopyCondition incluidos)
- `backend/Biblioteca.Application/Features/BookCopies/BookCopiesFeature.cs` — **nuevo** — constantes de rutas
- `backend/Biblioteca.Application/DTOs/Libros/BookCopyDto.cs` — **nuevo** — record de lectura
- `backend/Biblioteca.Application/DTOs/Libros/CreateBookCopyRequest.cs` — **nuevo**
- `backend/Biblioteca.Application/DTOs/Libros/UpdateBookCopyRequest.cs` — **nuevo**
- `backend/Biblioteca.Application/Interfaces/Libros/IBookCopyRepository.cs` — **nuevo**
- `backend/Biblioteca.Application/Interfaces/Libros/IBookCopyService.cs` — **nuevo**
- `backend/Biblioteca.Application/Services/Libros/BookCopyService.cs` — **nuevo**
- `backend/Biblioteca.Application/Mappings/BookCopyMappingProfile.cs` — **nuevo**
- `backend/Biblioteca.Application/DependencyInjection.cs` — registrar `IBookCopyService`
- `backend/Biblioteca.Application/Services/Libros/LibroService.cs` — inyectado `IBookCopyRepository`; `DeleteAsync` verifica copias activas → 409
- `backend/Biblioteca.Persistence/Configurations/BookCopyConfig.cs` — **nuevo** — tabla `book_copies`, FKs RESTRICT/SET NULL, índice único barcode, xmin concurrency token
- `backend/Biblioteca.Persistence/Repositories/BookCopyRepository.cs` — **nuevo**
- `backend/Biblioteca.Persistence/Context/BibliotecaDbContext.cs` — agregado `DbSet<BookCopy> BookCopies`
- `backend/Biblioteca.Persistence/DependencyInjection.cs` — registrar `IBookCopyRepository`
- `backend/Biblioteca.Persistence/Seeding/TableSeeders.cs` — agregado TODO sobre FindAsync + query filter (BUG-08 scope)
- `backend/Biblioteca.Persistence/Migrations/20260507004956_AddBookCopies.cs` — **nueva migración**
- `backend/Biblioteca.Persistence/Migrations/20260507004956_AddBookCopies.Designer.cs` — **generado**
- `backend/Biblioteca.API/Controllers/BookCopiesController.cs` — **nuevo** — 5 endpoints con policies correctas

**Nota técnica Lote 3:** `UseXminAsConcurrencyToken()` no tiene overload genérico en Npgsql 10.0.1.
Workaround aplicado: `builder.HasAnnotation("Npgsql:UseXminAsConcurrencyToken", true)` — misma semántica.

---

## Lote 4 — Spec completa (siguiente a implementar)

Objetivo: GET /api/libros debe devolver disponibilidad real (TotalCopies, AvailableCopies), no hardcodeada.

Tareas:
1. Extender `LibroDto` con `TotalCopies` (int) y `AvailableCopies` (int — copias con status = AVAILABLE y sin soft-delete).
2. En `LibroRepository.GetAllAsync` y `GetByIdAsync`, proyectar las copias en **una sola query** (no N+1).
   - Usar `Select` con subquery o `GroupBy`. Verificar SQL con logging de EF.
3. Asegurar que las queries de catálogo usen `AsNoTracking`.

Criterio de aceptación:
- GET /api/libros devuelve cada libro con `totalCopies` y `availableCopies` correctos.
- Logs de EF: 1-2 queries totales para listar 50 libros, no 51.
- Marcar una copia como MAINTENANCE → `availableCopies` del libro decrementa en 1.

---

## Cómo generar migraciones (sin SDK local)

```powershell
# 1. Modificar archivos de código
# 2. Escribir script sin BOM (ajustar <NombreMigracion>)
[System.IO.File]::WriteAllText(
  "C:\bibliotecaUdem-main\bibliotecaUdem-main\ef-migrate.sh",
  "#!/bin/sh`nexport PATH=`"`$PATH:/root/.dotnet/tools`"`ndotnet tool install --global dotnet-ef --version 10.0.5 2>/dev/null || true`ndotnet restore backend/Biblioteca.API/Biblioteca.API.csproj`n/root/.dotnet/tools/dotnet-ef migrations add <NombreMigracion> --project backend/Biblioteca.Persistence --startup-project backend/Biblioteca.API --output-dir Migrations`n",
  [System.Text.Encoding]::ASCII
)
# 3. Ejecutar en Docker SDK
docker run --rm `
  --network bibliotecaudem_default `
  -v "C:\bibliotecaUdem-main\bibliotecaUdem-main:/src" `
  -w "/src" `
  -e "ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=biblioteca_udem;Username=postgres;Password=postgres" `
  -e "Jwt__SecretKey=CHANGE_ME_WITH_A_LONG_DEVELOPMENT_SECRET_KEY_32CHARS" `
  -e "Jwt__Issuer=BibliotecaUdem" `
  -e "Jwt__Audience=BibliotecaUdem.Client" `
  "mcr.microsoft.com/dotnet/sdk:10.0" `
  sh /src/ef-migrate.sh
# 4. Eliminar script temporal
Remove-Item "C:\bibliotecaUdem-main\bibliotecaUdem-main\ef-migrate.sh"
# 5. Reconstruir y reiniciar backend
docker compose build backend ; docker compose up -d backend
```

---

## Reglas duras del proyecto (recordatorio)

- **Clean Architecture**: Domain → Application → Infrastructure/Persistence → API. No violar.
- **Migrations**: nunca editar una ya aplicada. Nombre PascalCase descriptivo.
- **Soft-delete**: `DeletedAt` nullable + `HasQueryFilter` en EF. No hard-delete en entidades principales.
- **Autorización**: `[Authorize(Policy = "...")]` explícita en cada endpoint. Policies definidas en `AuthPolicies.cs`.
- **DTOs**: nunca exponer entidades EF por la API.
- **Unicidad**: índices únicos en BD, no solo validación en código.
- **Commits**: uno por lote, mensaje "Lote N: <título>", código en inglés, comentarios en español.
- **Sin nuevas deps** sin justificar en el commit. Permitidas: xUnit, FluentAssertions, EF InMemory, Testcontainers.PostgreSQL.
- **Login field**: campo es `username` (no `usernameOrEmail`); respuesta tiene `accessToken` (no `token`).

---

## Decisiones de producto confirmadas

- **Google OAuth**: NO se implementa en Sprint 0. Solo JWT propio con validación de dominio `@udem.edu` al registrar.
- **JWT Refresh Token**: deuda técnica, pospuesto a Sprint 1.
- **Rate limiting**: deuda técnica, pospuesto.
- Lote 6 lite incluye: validación `@udem.edu` en registro, secret fuera de appsettings, password mínimo 8 chars.

---

## Prompt de inicio para nuevo chat

```
Continuamos la implementación del Sprint 0 de DEVLER (Sistema Gestión Biblioteca UDEM).
El repositorio está en C:\bibliotecaUdem-main\bibliotecaUdem-main.
Lee el archivo docs/HANDOFF_SPRINT0.md para entender el estado actual y las reglas.
El SIGUIENTE paso es implementar el Lote 4 según el plan ahí documentado.
Antes de tocar código, confirmá que entendiste el estado leyendo el handoff.
```
