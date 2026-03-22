#  Sistema de Biblioteca UDEM

##  Situación Organizacional Actual

La Universidad Ducky cuenta con diversos servicios, siendo uno de los más importantes la Biblioteca. Esta área concentra servicios como:

- Hemeroteca
- Fotocopiado
- Consulta de revistas
- Préstamo interno y externo de libros

Actualmente, el área de préstamos no cuenta con un sistema computacional, por lo que todos los procesos se realizan de manera manual.

### Problemáticas actuales

- Inconsistencia entre catálogo y disponibilidad real de libros
- Catálogo desactualizado
- Largos tiempos de atención en préstamos y devoluciones
- Dificultad para realizar inventarios
- Falta de control sobre existencias

###  Objetivo organizacional

Automatizar los procesos de la biblioteca para:

- Mejorar el servicio a alumnos
- Estandarizar procedimientos
- Cumplir con certificación ISO 9001:2015

---

##  Operación Actual de la Biblioteca

###  Adquisición de libros

1. El comprador solicita libros a editoriales según presupuesto
2. Las editoriales entregan libros y facturas
3. El comprador entrega los libros al administrador del catálogo
4. El administrador:
   - Etiqueta libros
   - Crea ficha bibliográfica
   - Registra en catálogo
5. El bibliotecario coloca los libros en los libreros

---

###  Consulta y préstamo

1. El alumno consulta el catálogo (autor, título, tema, etc.)
2. Solicita el libro al bibliotecario
3. El bibliotecario:
   - Verifica disponibilidad
   - Valida condiciones del alumno
4. Si procede:
   - Autoriza préstamo
   - Entrega recibo

---

###  Devoluciones y renovaciones

- Se revisa la fecha de vencimiento
- Si hay retraso:
  - Se genera multa
- Si solicita renovación:
  - El administrador valida según reglas

---

###  Casos especiales

####  Libro perdido
- Se genera multa por reposición
- Se notifica a:
  - Servicios escolares
  - Tesorería

####  Pago de multa
- El alumno paga en tesorería
- Reporta al bibliotecario

####  Alumnos con adeudos
- Se genera lista de deudores
- Se reporta a servicios escolares
- Se bloquea participación en exámenes

####  Gestión de catálogo
- El administrador:
  - Elimina libros obsoletos/dañados
  - Marca libros como:
    - Reservados
    - Consulta interna

---

##  Reglas de Negocio

###  Regla 1: Autorización de préstamo

Un alumno puede solicitar préstamo si:

- No tiene multas pendientes
- Tiene máximo 2 libros prestados
- No tiene préstamos vencidos

Condiciones del libro:

- Debe estar disponible
- Debe permitir préstamo externo

---

###  Regla 2: Renovación de préstamo

Se permite renovación si:

- La fecha de vencimiento ya pasó
- No supera 2 renovaciones

---

###  Regla 3: Multas

- Costo: $10 por día de retraso

---

###  Regla 4: Reposición de libro

En caso de pérdida:

- Se paga el valor actual del libro

**Fórmula:**

Valor = Precio en dólares × tipo de cambio actual

---

###  Regla 5: Vigencia de préstamo (alumnos)

- 5 días hábiles
- No incluye fines de semana

---

###  Regla 6: Préstamos para maestros

- Máximo 3 renovaciones
- Multa: $10 por día
- Duración: 10 días hábiles

---

## 🧠 Notas para desarrollo

Estas reglas deben implementarse como:

- Validaciones en backend (Application/Domain)
- Posibles políticas o servicios de dominio
- Base para futuras funcionalidades (Sprint 2+):
  - Préstamos
  - Multas
  - Suspensiones