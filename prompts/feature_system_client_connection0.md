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
1. Sprint 0
Funcionalidad: Gestión de usuarios.
1.1 CU-S0-01: Creación de Perfiles de Usuario
Precondiciones
El sistema está disponible.
El usuario desea crear un perfil.
Se cuentan con los datos necesarios.
Flujo principal
El usuario selecciona la opción de crear perfil.
El sistema solicita los datos requeridos.
El usuario captura la información.
El sistema valida los datos.
El sistema genera el perfil correspondiente (estudiante, bibliotecario, profesor o administrador).
El sistema confirma la creación.


Flujos alternos
A1: Datos inválidos


El sistema detecta un error en la validación.
Solicita corrección.
Regresa al paso 3.
Postcondiciones
El perfil queda registrado en el sistema.


1.2 CU-S0-02: Baja de Usuarios
Actor principal: Administrador
Precondiciones
El usuario a eliminar existe en el sistema.
El administrador está autenticado.


Flujo principal
El administrador selecciona un usuario.
El administrador elige la opción “Dar de baja”.
El sistema elimina el perfil.
El sistema confirma la baja.


Flujos alternos
A1: Usuario no existe
El sistema informa que no se encuentra el perfil.
Postcondiciones
El usuario queda eliminado del sistema.





1.3 CU-S0-03: Actualización de Usuarios
Actor principal: Administrador
Precondiciones
El usuario existe.
El administrador tiene acceso al sistema.
Flujo principal
El administrador selecciona un usuario.
Elige la opción actualizar.
Modifica uno o varios campos.
El sistema guarda los cambios.
El sistema confirma la actualización.
Flujos alternos
A1: Datos inválidos
El sistema rechaza los cambios.
Solicita corrección.
Postcondiciones
El perfil queda actualizado.






1.4 CU-S0-04: Listar Usuarios
Actor principal: Administrador
Precondiciones
Existen usuarios registrados en el sistema.
Flujo principal
El administrador accede a la opción listar usuarios.
El sistema despliega todos los usuarios dados de alta.
El sistema muestra el estatus de cada usuario.
El administrador puede seleccionar usuarios para realizar cambios.
Flujos alternos
A1: No hay usuarios
El sistema muestra lista vacía.
Postcondiciones
El administrador visualiza los usuarios del sistema.


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