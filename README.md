# Biblioteca UDEM

Repositorio base del proyecto Biblioteca UDEM para la clase de Metodologias de Desarrollo de Software.

## Stack

- Backend: ASP.NET Core 10
- Arquitectura: Clean Architecture
- Persistencia: EF Core + PostgreSQL
- Infra local: Docker Compose
- Frontend: Angular 21 standalone + SCSS

## Estado actual

### Backend (ASP.NET Core 10 — Clean Architecture)

| Módulo | Endpoints clave | Estado |
|--------|-----------------|--------|
| Auth | `POST /api/auth/register`, `POST /api/auth/login` | ✅ completo |
| Usuarios | `GET/POST/PUT/DELETE /api/usuarios` | ✅ completo |
| Libros | `GET/POST /api/libros`, copias | ✅ completo |
| Préstamos | `POST /api/prestamos`, renovaciones | ✅ completo |
| Devoluciones | `POST /api/devoluciones` | ✅ completo |
| Multas | `GET/POST /api/multas`, pagos, condonaciones | ✅ completo |
| Reservas | `POST /api/reservas`, cola de espera | ✅ completo |
| Notificaciones | eventos de dominio → notificaciones | ✅ completo |
| Reportes | `GET /api/reportes/*` (AdminOnly) | ✅ completo |

### Frontend (Angular 21 standalone)

| Ruta | Descripción | Estado |
|------|-------------|--------|
| `/login` | Login institucional JWT | ✅ completo |
| `/dashboard` | Panel principal con stats | ✅ completo |
| `/usuarios` | Gestión de usuarios | ✅ completo |
| `/dashboard/catalogo` | Búsqueda de libros | ✅ completo |
| `/dashboard/libros` | Gestión del catálogo | ✅ completo |
| `/dashboard/prestamos` | Registrar y ver préstamos | ✅ completo |
| `/dashboard/devoluciones` | Devoluciones y nueva devolución | ✅ completo |
| `/dashboard/multas` | Listado, detalle, pago y condonación | ✅ completo |
| `/dashboard/configuracion/multas` | Configurar tarifas (Admin) | ✅ completo |
| `/dashboard/reportes` | Reportes ejecutivos con export CSV (Admin) | ✅ completo |
| `/dashboard/reservas` | Reservas y cola de espera | ✅ completo |
| `/dashboard/notificaciones` | Bandeja de notificaciones in-app | ✅ completo |

## Estructura del repositorio

- `backend/Biblioteca.API`: host ASP.NET Core 10 con controllers, Swagger y pipeline HTTP.
- `backend/Biblioteca.Application`: DTOs, interfaces, servicios, reglas de aplicacion y perfiles AutoMapper.
- `backend/Biblioteca.Domain`: entidades del dominio.
- `backend/Biblioteca.Infrastructure`: JWT, fecha/hora y servicios transversales.
- `backend/Biblioteca.Persistence`: EF Core, `DbContext`, configuraciones, migraciones y repositorios.
- `frontend/biblioteca-app`: aplicacion Angular con todas las rutas funcionales.
- `database`: dump legado, esquema SQL y datos semilla.
- `docker-compose.yml`: PostgreSQL local para desarrollo.
- `docs/`: documentacion del proyecto (ver abajo).

## Requisitos

- .NET SDK 10
- Docker Desktop o Docker Engine con Compose

## Configuracion local

La API usa por defecto esta conexion en desarrollo:

```json
Host=localhost;Port=5432;Database=biblioteca_udem;Username=postgres;Password=postgres
```

La configuracion vive en:

- [appsettings.json](c:/bibliotecaUdem/backend/Biblioteca.API/appsettings.json)
- [appsettings.Development.json](c:/bibliotecaUdem/backend/Biblioteca.API/appsettings.Development.json)

## Levantar PostgreSQL

Inicia el contenedor:

```powershell
docker compose up -d postgres
```

Verifica que este sano:

```powershell
docker compose ps
```

Detener el contenedor:

```powershell
docker compose down
```

Detener y borrar volumen de datos:

```powershell
docker compose down -v
```

## Alternativa Full Docker

Si quieres levantar toda la aplicacion con un solo comando, usa Docker Compose.

Esto levanta:

- PostgreSQL
- backend ASP.NET Core
- frontend Angular servido por Nginx

Comando unico:

```powershell
docker compose up --build
```

En segundo plano:

```powershell
docker compose up --build -d
```

URLs disponibles:

- Frontend: `http://localhost:4200`
- API: `http://localhost:5122`
- Swagger: `http://localhost:5122/swagger`
- Swagger via frontend proxy: `http://localhost:4200/swagger/`

Apagar stack:

```powershell
docker compose down
```

Apagar stack y borrar base de datos:

```powershell
docker compose down -v
```

Ver logs:

```powershell
docker compose logs -f
```

Ver solo backend:

```powershell
docker compose logs -f backend
```

Nota:

- el backend espera a PostgreSQL
- al arrancar aplica migraciones y seeders automaticamente
- el frontend usa proxy interno a la API, asi que no necesitas cambiar URLs

## Por que se uso Nginx y proxy

### Que es Nginx

`Nginx` es un servidor web liviano y muy usado para servir archivos estaticos y para actuar como reverse proxy.

En este proyecto lo use en el contenedor del frontend porque:

- Angular en produccion genera archivos estaticos, no necesita `ng serve`
- `Nginx` sirve esos archivos de forma mas simple y estable para un entorno Docker
- permite redirigir rutas como `/api` y `/swagger/` hacia el backend sin cambiar el codigo del frontend
- se parece mas a un despliegue real que dejar un servidor de desarrollo de Angular corriendo en contenedor

### Que es un proxy en este contexto

Aqui el proxy significa que el frontend recibe una peticion del navegador y la reenvia al backend.

Ejemplo:

- el navegador llama a `/api/auth/login`
- `Nginx` o el proxy local de Angular reenvia esa llamada al backend ASP.NET Core
- el navegador sigue viendo un mismo origen de acceso

### Por que se decidio usar proxy aqui

Lo decidi por estas razones especificas del proyecto:

- evita hardcodear URLs distintas en el frontend para local y para Docker
- permite que Angular use siempre `'/api'` como base
- reduce problemas de `CORS` porque el navegador entra por una sola puerta
- hace que el stack Docker se levante completo con un solo comando sin editar configuraciones
- nos deja una experiencia parecida entre desarrollo local y despliegue en contenedores

### Diferencia entre proxy local y proxy en Docker

En desarrollo normal:

- `npm.cmd start` usa [proxy.conf.json](c:/bibliotecaUdem/frontend/biblioteca-app/proxy.conf.json)
- `ng serve` tambien toma ese proxy porque quedo configurado en [angular.json](c:/bibliotecaUdem/frontend/biblioteca-app/angular.json)
- Angular reenvia `/api` a `http://localhost:5122`

En Docker:

- el frontend corre en `Nginx`
- [nginx.conf](c:/bibliotecaUdem/frontend/biblioteca-app/nginx.conf) reenvia `/api` y `/swagger/` al servicio `backend`

### Por que no deje la URL del backend fija en Angular

No deje algo como `http://localhost:5122/api` fijo porque eso rompe o complica estos escenarios:

- frontend en Docker llamando a backend en otro contenedor
- despliegues futuros detras de un gateway o reverse proxy
- cambios de host o puerto entre entornos

Con `'/api'` la aplicacion queda mas portable y mas facil de mantener.

## Restaurar dependencias

Restaura la solucion:

```powershell
dotnet restore BibliotecaUdem.slnx
```

Si necesitas restaurar herramientas locales:

```powershell
dotnet tool restore
```

## Aplicar base de datos

La API ahora aplica migraciones y seeders automaticamente al arrancar en desarrollo.

Adicionalmente, el proyecto API intenta inicializar la base automaticamente en el primer `build` del proyecto.
Ese intento solo marca completado si PostgreSQL ya esta disponible.

Flujo recomendado:

1. levantar PostgreSQL
2. compilar
3. ejecutar la API

En el primer arranque, la aplicacion:

- crea la base si no existe
- aplica migraciones pendientes
- ejecuta seeders por tabla

En el primer `dotnet build` de `Biblioteca.API`:

- se intenta ejecutar `--init-db` automaticamente
- si PostgreSQL no esta levantado, el build no falla
- la inicializacion se completara en el siguiente arranque de la API

Si necesitas hacerlo manualmente por soporte o troubleshooting, usa:

```powershell
dotnet tool run dotnet-ef database update --project backend/Biblioteca.Persistence --startup-project backend/Biblioteca.API
```

Si por alguna razon quieres cargar el script SQL de referencia manualmente desde Docker:

```powershell
Get-Content database\seed.sql | docker exec -i biblioteca-udem-postgres psql -U postgres -d biblioteca_udem
```

## Compilar

Compilar toda la solucion:

```powershell
dotnet build BibliotecaUdem.slnx --no-restore
```

Compilar solo la API:

```powershell
dotnet build backend\Biblioteca.API\Biblioteca.API.csproj --no-restore
```

Si la API esta abierta y solo quieres validar compilacion sin reemplazar el ejecutable en uso:

```powershell
dotnet build backend\Biblioteca.API\Biblioteca.API.csproj --no-restore -t:Compile
```

## Arrancar la API

Ejecuta la API:

```powershell
dotnet run --project backend/Biblioteca.API
```

En el primer arranque veras que:

- se aplican las migraciones pendientes
- se ejecutan los seeders
- quedan listas las credenciales demo y el catalogo base

Swagger queda disponible en una URL como:

- `http://localhost:5122/swagger`
- `https://localhost:7xxx/swagger`

La URL exacta la imprime `dotnet run` en consola.

## Frontend Angular

Instalar dependencias del frontend:

```powershell
cd frontend\biblioteca-app
npm.cmd install
```

Arrancar en desarrollo:

```powershell
cd frontend\biblioteca-app
npm.cmd start
```

El comando local ahora usa proxy hacia la API en `http://localhost:5122`, por lo que el frontend trabaja con `/api` tambien fuera de Docker.

Build de produccion:

```powershell
cd frontend\biblioteca-app
npm.cmd run build
```

La aplicacion Angular queda disponible por defecto en:

- `http://localhost:4200`

Rutas disponibles:

- `/login`
- `/dashboard`
- `/usuarios`
- `/dashboard/catalogo`
- `/dashboard/libros`
- `/dashboard/prestamos`
- `/dashboard/devoluciones`
- `/dashboard/multas`
- `/dashboard/configuracion/multas` (Admin)
- `/dashboard/reportes` (Admin)

Credenciales demo locales:

- `admin1`
- `Admin123!`

Usuarios adicionales sembrados para pruebas:

- `demo.mlopez` / `Password123!`
- `demo.lramirez` / `Password123!`
- `demo.amartinez` / `Password123!`
- `demo.fsanchez` / `Password123!`

## Endpoints principales

Auth: `POST /api/auth/register` · `POST /api/auth/login`

Usuarios: `GET/POST/PUT/DELETE /api/usuarios`

Libros: `GET/POST /api/libros`

Préstamos: `POST /api/prestamos` · `GET /api/prestamos/{id}` · `POST /api/prestamos/{id}/renovar`

Devoluciones: `POST /api/devoluciones` · `GET /api/devoluciones/{id}`

Multas: `GET /api/multas` · `GET /api/multas/{id}` · `POST /api/multas/{id}/pagos` · `POST /api/multas/{id}/condonar`

Configuración de multas: `GET/PUT /api/configuracion-multas`

Reportes (AdminOnly):
- `GET /api/reportes/multas-recaudadas?from=&to=`
- `GET /api/reportes/multas-pendientes`
- `GET /api/reportes/devoluciones-tardias?from=&to=`
- `GET /api/reportes/condonaciones?from=&to=`

Reservas: `POST /api/reservas` · `DELETE /api/reservas/{id}`

Notificaciones: `GET /api/notificaciones` · `POST /api/notificaciones/{id}/leer`

Chatbot: `POST /api/chat`

Dashboard KPIs: `GET /api/dashboard/kpis`

## Préstamos y Dashboard

El módulo de préstamos controla el ciclo completo de entrega y devolución de ejemplares.
Los límites por rol y las duraciones por defecto están en `appsettings.json` bajo la
sección `Loans`. El dashboard muestra KPIs en tiempo real (préstamos activos, vencidos,
multas pendientes, libros disponibles, reservas en cola).

Ver [`docs/PRESTAMOS.md`](docs/PRESTAMOS.md) para el diagrama de estados de `Loan`,
reglas de renovación y configuración detallada.

Documentación completa en [`docs/`](docs/):
- [`MODULOS.md`](docs/MODULOS.md) — resumen de los 3 módulos, propietarios y endpoints
- [`PRESTAMOS.md`](docs/PRESTAMOS.md) — módulo Préstamos completo con diagrama de estados
- [`DEVOLUCIONES_MULTAS.md`](docs/DEVOLUCIONES_MULTAS.md) — módulo Frank completo
- [`DEUDA_TECNICA.md`](docs/DEUDA_TECNICA.md) — pendientes registrados
- [`api_contracts.md`](docs/api_contracts.md) — contratos Auth/Usuarios/Libros
- [`reglas_negocio.md`](docs/reglas_negocio.md) — reglas de negocio originales

## Flujo recomendado para desarrollo

1. `docker compose up -d postgres`
2. `dotnet restore BibliotecaUdem.slnx`
3. `dotnet tool restore`
4. `dotnet build BibliotecaUdem.slnx --no-restore`
5. `dotnet run --project backend/Biblioteca.API`
6. esperar a que termine la inicializacion automatica de base
7. `cd frontend\biblioteca-app`
8. `npm.cmd start`
9. abrir `http://localhost:4200`

## Flujo recomendado con Docker

1. `docker compose up --build -d`
2. abrir `http://localhost:4200`
3. si quieres revisar backend: `http://localhost:5122/swagger`
4. para detener: `docker compose down`

## Apagar o resetear Docker

Apagar todo el stack:

```powershell
docker compose down
```

Apagar y borrar tambien el volumen de PostgreSQL:

```powershell
docker compose down -v
```

Forzar reconstruccion completa al volver a levantar:

```powershell
docker compose down
docker compose up --build -d
```

Reset completo del entorno Docker:

```powershell
docker compose down -v
docker compose up --build -d
```

## Ejemplos rapidos de prueba

Registro de usuario:

```bash
curl -X POST "http://localhost:5122/api/auth/register" \
  -H "Content-Type: application/json" \
  -H "accept: application/json" \
  -d '{
    "username": "mlopez",
    "firstName": "Maria",
    "lastName": "Lopez",
    "email": "maria.lopez@udem.edu",
    "password": "Password123!",
    "preferredLocale": "es_MX"
  }'
```

Login:

```bash
curl -X POST "http://localhost:5122/api/auth/login" \
  -H "Content-Type: application/json" \
  -H "accept: application/json" \
  -d '{
    "username": "admin1",
    "password": "Admin123!"
  }'
```

Listar usuarios con token:

```bash
curl -X GET "http://localhost:5122/api/usuarios" \
  -H "accept: application/json" \
  -H "Authorization: Bearer TU_TOKEN"
```

Crear usuario administrativo:

```bash
curl -X POST "http://localhost:5122/api/usuarios" \
  -H "Content-Type: application/json" \
  -H "accept: application/json" \
  -H "Authorization: Bearer TU_TOKEN" \
  -d '{
    "username": "prof.math",
    "firstName": "Elena",
    "lastName": "Suarez",
    "displayName": "Dra. Elena Suarez",
    "email": "e.suarez@udem.edu",
    "password": "Password123!",
    "roleCode": "TEACHER",
    "statusCode": "active",
    "preferredLocale": "es_MX",
    "documentType": "employee_id",
    "documentNumber": "PRF-2026-001"
  }'
```

Actualizar usuario:

```bash
curl -X PUT "http://localhost:5122/api/usuarios/USER_ID" \
  -H "Content-Type: application/json" \
  -H "accept: application/json" \
  -H "Authorization: Bearer TU_TOKEN" \
  -d '{
    "username": "prof.math",
    "firstName": "Elena",
    "lastName": "Suarez",
    "displayName": "Dra. Elena Suarez",
    "email": "e.suarez@udem.edu",
    "password": "Password123!",
    "roleCode": "TEACHER",
    "statusCode": "inactive",
    "preferredLocale": "es_MX",
    "documentType": "employee_id",
    "documentNumber": "PRF-2026-001"
  }'
```

Dar de baja usuario:

```bash
curl -X DELETE "http://localhost:5122/api/usuarios/USER_ID" \
  -H "accept: */*" \
  -H "Authorization: Bearer TU_TOKEN"
```

Prueba rapida del stack Docker:

```bash
curl http://localhost:5122/swagger/index.html
curl http://localhost:4200/
```

Alta de libro:

```bash
curl -X POST "http://localhost:5122/api/libros" \
  -H "Content-Type: application/json" \
  -H "accept: application/json" \
  -d '{
    "title": "El Aleph",
    "isbn": "9788420674208",
    "publisher": "Emece",
    "publicationYear": 1949,
    "edition": "1",
    "language": "es",
    "summaryJson": "{\"short\":\"Coleccion de cuentos\"}",
    "metadataJson": "{\"format\":\"tapa blanda\"}",
    "authors": [
      {
        "fullName": "Jorge Luis Borges",
        "contribution": "Autor"
      }
    ],
    "subjectCodes": [
      "LIT_LATAM"
    ]
  }'
```

## Notas

- `legacy_dump.sql` ya fue traducido a entidades C# y a la migracion inicial de EF Core.
- `book_copies` no se incluyo porque no forma parte del sprint actual.
- `dotnet-ef` esta instalado como herramienta local en `dotnet-tools.json`.
- Los seeders ahora viven en `backend/Biblioteca.Persistence/Seeding` y cubren tablas base, joins, sesion demo, reset token demo y audit log demo.
- La inicializacion automatica de base se ejecuta al arrancar la API mediante `InitializeDatabaseAsync()`.
- `docker-compose.yml` ahora levanta `postgres`, `backend` y `frontend` con un solo comando.
- El frontend usa `'/api'` como base URL y en desarrollo local se apoya en `proxy.conf.json`.
