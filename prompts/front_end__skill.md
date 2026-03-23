CONTEXTO:
Proyecto: Biblioteca UDEM
(ver docs/project_context.md)

Stack:
- ASP.NET Core 10
- Angular
- PostgreSQL

Arquitectura:
- Clean Architecture
- API REST (Controllers)
- Uso de DTOs, AutoMapper, JWT, Swagger

Módulos:
- Usuarios (Sprint 0)
- Libros (Sprint 1)

Convenciones:
- PascalCase en C#
- camelCase en Angular
- snake_case en DB

---

TAREA:
Quiero que desarrolles el front end en Angular para un “Sistema de Gestión de Biblioteca – UDEM”, tomando como referencia directa los screenshots adjuntos.

IMPORTANTE:

- Debes usar como criterio principal una dirección visual premium, institucional, profesional y accesible.
- La meta es construir una interfaz que conserve la esencia visual de los screenshots, pero mejor resuelta, más refinada y lista para evolucionar a producto real.
- No quiero una plantilla genérica tipo SaaS dashboard.
- No priorices componentes sueltos; prioriza composición, jerarquía, claridad operativa, espaciado y consistencia.

STACK OBLIGATORIO:

- Angular 17+ o la versión estable más reciente disponible
- Standalone components
- TypeScript estricto
- SCSS
- Angular Router
- Señales o estado simple idiomático de Angular cuando tenga sentido
- Arquitectura limpia y escalable para crecer a producto real
- Sin dependencia innecesaria de librerías visuales pesadas
- Puedes usar Angular CDK si ayuda en accesibilidad o estructura
- Si usas una librería UI, que sea mínima y no imponga look genérico
- Evita que el resultado se vea “Material default”

ANTES DE IMPLEMENTAR, define brevemente:

1. Visual thesis: una oración sobre el mood visual, materialidad y presencia
2. Content plan: cómo se organiza la experiencia entre login, navegación lateral, contenido principal y tablas
3. Interaction thesis: 2 o 3 ideas de interacción/motion sutiles y útiles

OBJETIVO VISUAL:
Quiero una estética:

- institucional
- moderna
- sobria
- premium
- calmada
- accesible
- con fuerte presencia visual
- basada en azul profundo institucional
- con superficies claras y limpias
- con tipografía fuerte y jerarquía clara
- con apariencia de sistema universitario serio y contemporáneo

NO QUIERO:

- dashboards genéricos de plantilla
- hero cards
- exceso de cards
- saturación de badges
- demasiados colores de acento
- sombras pesadas
- bordes exagerados
- visual noise
- widgets innecesarios
- copy de marketing
- secciones decorativas sin función
- layouts que parezcan template descargado

QUIERO ESTAS PANTALLAS:

1. Pantalla de login institucional
Basada en los screenshots:
- Layout dividido en 2 columnas a pantalla completa
- Lado izquierdo: imagen institucional dominante de biblioteca/campus/recursos académicos
- Sobre la imagen, overlay azul oscuro institucional que permita legibilidad perfecta
- En esa zona izquierda debe haber:
    - marca o identidad UDEM
    - título fuerte del sistema
    - breve descripción funcional
    - 2 o 3 métricas visuales sobrias
    - un pequeño texto de estado o nota inferior
- Lado derecho:
    - icono del sistema
    - encabezado “Bienvenido”
    - subtítulo breve
    - panel de acceso institucional
    - botón principal “Continuar con Google”
    - mensaje informativo de uso exclusivo para cuentas @udem.edu
    - acción secundaria de soporte o configuración si aplica
- Debe sentirse elegante, clara y lista para producción
- Nada de cajas innecesarias sobre la imagen
- Nada de desorden visual
1. Vista de dashboard / administración de usuarios
Basada en los screenshots:
- Sidebar izquierda fija
- Main content limpio y amplio
- Topbar sencilla con notificaciones y perfil
- Encabezado de módulo con título y subtítulo
- Buscador principal
- Filtro por roles
- Botón de “Agregar usuario”
- Tabla profesional y limpia con columnas tipo:
    - usuario
    - ID universitario
    - correo
    - rol
    - estado
    - acciones
- Badges sobrios para rol y estado
- Acciones por fila con íconos claros y accesibles
- Jerarquía visual fuerte
- Mucho aire y orden
- Debe parecer una herramienta real de administración

RUTAS MÍNIMAS:

- /login
- /dashboard
- /usuarios

ESTRUCTURA TÉCNICA ESPERADA:
Crea una estructura limpia y mantenible, por ejemplo:

- core/
    - layout
    - services
    - guards
- shared/
    - ui
    - models
    - utils
    - icons
- features/
    - auth/
    - dashboard/
    - users/

COMPONENTES ESPERADOS:

- app shell / layout principal
- sidebar institucional
- topbar
- login page
- users page
- reusable table wrapper o estructura clara para tablas
- badges de estado/rol
- search input
- filter dropdown
- action button primario
- avatar / user chip
- notification button
- support floating button opcional, si se integra bien

DATOS:

- Usa mock data bien redactada y realista en español
- Nombres, correos, roles y estados coherentes con entorno universitario
- No uses lorem ipsum
- No uses textos vacíos o genéricos

DISEÑO DE SISTEMA:
Define e implementa tokens o variables SCSS para:

- colores
- tipografía
- espaciado
- radios
- bordes
- estados
- focus ring
- layout widths
- elevación mínima
Todo debe ser consistente y escalable.

PALETA:

- Azul institucional profundo como color principal
- Blancos y grises limpios para superficies
- Acentos discretos para estados
- Verde suave para activo
- Tonos sobrios para profesor / estudiante / administrador
- El sistema no debe depender de muchos colores

TIPOGRAFÍA:

- Máximo dos familias tipográficas
- Preferencia por una sola familia bien trabajada
- Jerarquía clara entre títulos, subtítulos, labels, tabla y textos auxiliares
- Gran atención a tamaño, peso, tracking y line-height

ACCESIBILIDAD OBLIGATORIA:

- HTML semántico
- Contraste suficiente
- Navegación por teclado
- Estados focus visibles
- Hover y active claros
- Labels correctos
- aria-* cuando corresponda
- Targets cómodos
- Tabla legible
- Formularios y botones accesibles
- Nada debe sacrificar usabilidad por verse bonito

RESPONSIVE:

- Desktop prioritario
- Tablet funcional
- Mobile razonable
- En mobile, la composición puede reorganizarse, pero debe conservar identidad visual y claridad
- Sidebar puede colapsar
- Tablas pueden tener scroll horizontal controlado o versión adaptada

ANIMACIÓN / MOTION:
Implementa motion sutil e intencional, no decorativo:

- entrada suave del login
- transición ligera en sidebar y contenido
- hover refinado en botones, filas y filtros
- fade / slide suave en carga de vistas
- Duraciones cortas y profesionales
- Nada exagerado

CALIDAD DE CÓDIGO:

- Código ordenado, legible y modular
- Buen naming
- Separación clara de responsabilidades
- Estilos bien organizados
- Sin duplicación innecesaria
- Preparado para escalar a más módulos del sistema
- No hardcodear todo en un solo componente enorme

ENTREGABLE:
Quiero que generes:

1. La explicación breve de visual thesis, content plan e interaction thesis
2. La estructura del proyecto Angular
3. Los componentes necesarios
4. Las rutas
5. Los estilos globales y variables SCSS
6. Las páginas funcionales de login y usuarios
7. Mock data realista
8. Un resultado consistente con los screenshots, pero más refinado

CRITERIOS DE VALIDACIÓN ANTES DE TERMINAR:

- ¿La interfaz se siente como producto universitario real?
- ¿Se entiende la jerarquía desde el primer vistazo?
- ¿Se mantiene premium incluso sin sombras exageradas?
- ¿La composición es más importante que los componentes?
- ¿El resultado evita verse como template SaaS?
- ¿La accesibilidad está realmente cuidada?
- ¿La vista de login y la de usuarios parecen parte del mismo sistema?

IMPORTANTE FINAL:
No expliques teoría de diseño de forma extensa.
No generes solo wireframes.
No hagas solo una maqueta estática.
Construye el front end en Angular con código real, limpio y escalable.
Usa texto realista en español.
Mantén la esencia visual exacta de los screenshots: institucional, azul profundo, sidebar sólida, superficies limpias, tablas elegantes, acceso sobrio y composición seria.

---

ARCHIVOS:
(Lista de archivos que le pasas al modelo)

---

REGLAS:
- No exponer entidades directamente
- Usar DTOs
- Usar AutoMapper
- Seguir Clean Architecture
- Código limpio y estructurado

---

SALIDA:
- Código listo para pegar
- Sin explicaciones
- Separado por archivos si es necesario