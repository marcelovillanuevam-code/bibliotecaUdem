CONTEXTO:
Estás trabajando en el proyecto "Biblioteca UDEM".

El objetivo es implementar funcionalidades backend y/o frontend siguiendo buenas prácticas de arquitectura limpia, asegurando código mantenible, escalable y consistente con el stack definido.

Consulta los siguientes documentos como fuente de verdad del dominio y decisiones del sistema:
- docs/project_context.md
- util_memory.md


STACK TECNOLÓGICO:
- Backend: ASP.NET Core 10 (API REST con Controllers)
- Frontend: Angular
- Base de datos: PostgreSQL

ARQUITECTURA:
- Clean Architecture (separación por capas: Domain, Application, Infrastructure, API)
- Uso de DTOs para transferencia de datos
- AutoMapper para mapeo entre entidades y DTOs
- Autenticación con JWT
- Documentación con Swagger

MÓDULOS:
- Usuarios (Sprint 0)
- Libros (Sprint 1)

CONVENCIONES DE CÓDIGO:
- C#: PascalCase
- Angular: camelCase
- Base de datos: snake_case

---

TAREA:
2. Sprint 1A
Funcionalidad: Gestionar Libros
2.1 CU-S1A-01: Alta de Libro
Actor: Administrador
Precondiciones
El administrador está autenticado.
Flujo principal
El administrador selecciona “Alta libro”.
Captura los datos del libro.
El sistema guarda el nuevo libro.
El sistema confirma el registro.


Flujos alternos
A1: Datos incompletos
El sistema solicita completar información.
Postcondiciones
El libro queda registrado en el sistema.




2.2 CU-S1A-02: Baja de Libro
Actor: Administrador
Precondiciones
El libro existe en el sistema.
Flujo principal
El administrador selecciona el libro.
Elige la opción baja.
El sistema elimina el libro.
Confirma la baja.
Flujos alternos
A1: Libro no encontrado
El sistema informa error.
Postcondiciones
El libro deja de estar disponible en el sistema.







2.3 CU-S1A-03: Cambio de Libro
Actor: Administrador
Precondiciones
El libro existe.
Flujo principal
El administrador selecciona el libro.
Modifica los datos.
El sistema guarda los cambios.
Confirma actualización.
Flujos alternos
A1: Datos inválidos
El sistema solicita corrección.
Postcondiciones
El libro queda actualizado.







2.4 CU-S1A-04: Listado de Libros por Diversos Filtros
Actor: Administrador
Precondiciones
Existen libros en el catálogo.
Flujo principal
El administrador accede al catálogo.
Selecciona criterios de filtro (autor, materia, ISBN, etc.).
El sistema aplica los filtros.
El sistema muestra resultados.
Flujos alternos
A1: Sin resultados
El sistema muestra lista vacía.
Postcondiciones
Se muestra el catálogo filtrado.







2.5 CU-S1A-05: Consulta de Libro
Actor: Administrador
Precondiciones
El libro existe en el sistema.
Flujo principal
El administrador busca un libro específico.
El sistema muestra todos los datos.
Muestra ubicación en biblioteca.
Flujos alternos
A1: Libro no encontrado
El sistema informa que no existe.
Postcondiciones
El administrador visualiza la información del libro.

---

OBJETIVO DE IMPLEMENTACIÓN:
Generar el código necesario para cumplir con los casos de uso definidos en la sección TAREA.

El código debe incluir:
- Controladores (API)
- DTOs (Request/Response)
- Servicios / Casos de uso (Application Layer)
- Entidades (Domain, si es necesario)
- Repositorios (Interfaces en Application, implementación en Infrastructure)
- Configuración de AutoMapper
- Validaciones básicas

Asegurar que cada caso de uso esté correctamente representado en endpoints REST.

---

ARCHIVOS:
(Lista de archivos que le pasas al modelo)

---

REGLAS:
- No exponer entidades directamente en los endpoints
- Usar DTOs para entrada y salida
- Usar AutoMapper para conversiones
- Respetar separación por capas (Clean Architecture)
- Manejar errores básicos (validaciones, no encontrado, etc.)
- Código limpio, legible y consistente con el stack

---

SALIDA:
- Código listo para copiar y pegar
- Sin explicaciones
- Separado por archivos
- Incluir nombres de archivo como encabezado