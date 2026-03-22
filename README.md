# bibliotecaUdem
Repositorio base del proyecto Biblioteca UDEM para la clase de Metodologias de Desarrollo de Software.

## Estructura inicial

- `backend/Biblioteca.API`: host ASP.NET Core 10 con controllers.
- `backend/Biblioteca.Application`: DTOs, interfaces, servicios y mapeos.
- `backend/Biblioteca.Domain`: entidades del dominio.
- `backend/Biblioteca.Infrastructure`: servicios transversales y opciones.
- `backend/Biblioteca.Persistence`: EF Core, contexto, configuraciones, migrations y repositorios.
- `frontend/biblioteca-app`: espacio reservado para Angular.
- `database`: dump legado, script de esquema y seed de referencia.
- `docker-compose.yml`: PostgreSQL local para desarrollo.

## PostgreSQL con Docker

1. Ejecuta `docker compose up -d postgres`.
2. Verifica salud con `docker compose ps`.
3. Aplica migraciones con `dotnet tool run dotnet-ef database update --project backend/Biblioteca.Persistence --startup-project backend/Biblioteca.API`.
4. Si necesitas datos de referencia, usa `database/seed.sql` dentro del contenedor o desde tu cliente favorito.

La API ya apunta por defecto a `Host=localhost;Port=5432;Database=biblioteca_udem;Username=postgres;Password=postgres` en desarrollo.

## Notas

- `legacy_dump.sql` ya fue traducido a entidades y a la migracion inicial de EF Core.
- La tabla `book_copies` no se incluyo porque no forma parte de este sprint y no viene definida en el dump.
- `dotnet-ef` esta instalado como herramienta local en `dotnet-tools.json`.
