biblioteca-udem/
│
├── docs/
│   ├── project_context.md
│   ├── reglas_negocio.md
│   └── backlog.md
│
├── backend/
│   ├── Biblioteca.API/       #endpoints
│   │   ├── Controllers/
│   │   ├── Middlewares/
│   │   ├── Extensions/
│   │   └── Program.cs
│   │
│   ├── Biblioteca.Application/  #reglas de negocio
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   ├── Mappings/
│   │   └── Features/
│   │
│   ├── Biblioteca.Domain/
│   │   └── Entities/
│   │
│   ├── Biblioteca.Infrastructure/
│   │   ├── Auth/
│   │   └── Services/
│   │
│   ├── Biblioteca.Persistence/
│   │   ├── Context/
│   │   │   └── BibliotecaDbContext.cs
│   │   │
│   │   ├── Configurations/      #  Fluent API (Entity configs)
│   │   │   ├── UsuarioConfig.cs
│   │   │   └── LibroConfig.cs
│   │   │
│   │   ├── Migrations/          # AQUÍ VAN LAS MIGRACIONES
│   │   │
│   │   ├── Seeds/               #  datos iniciales
│   │   │   └── SeedData.cs
│   │   │
│   │   └── Repositories/
│   │
│
├── frontend/
│   └── biblioteca-app/
│
├── database/                   
│   ├── schema.sql
│   └── seed.sql
│
└── prompts/