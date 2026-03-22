# bibliotecaUdem
Repositorio base del proyecto Biblioteca UDEM para la clase de Metodologias de Desarrollo de Software.

## Estructura inicial

- `backend/Biblioteca.API`: host ASP.NET Core 10 con controllers.
- `backend/Biblioteca.Application`: DTOs, interfaces, servicios y mapeos.
- `backend/Biblioteca.Domain`: entidades del dominio.
- `backend/Biblioteca.Infrastructure`: servicios transversales y opciones.
- `backend/Biblioteca.Persistence`: EF Core, contexto, configuraciones, seeds y repositorios.
- `frontend/biblioteca-app`: espacio reservado para Angular.
- `database`: scripts SQL de referencia.

## Siguientes pasos

1. Restaurar paquetes NuGet.
2. Configurar PostgreSQL local.
3. Crear la primera migracion en `Biblioteca.Persistence`.
4. Generar el proyecto Angular en `frontend/biblioteca-app`.
