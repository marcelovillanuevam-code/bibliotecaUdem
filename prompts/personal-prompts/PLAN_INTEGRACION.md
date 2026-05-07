# DEVLER — Plan de Integración Sprint 1 + 2

> Documento del Tech Lead. Define el orden de merges, los puntos
> críticos de coordinación, y los tests de integración cross-módulo
> que se corren al final.

---

## 1. Orden de merges

Este es el orden recomendado para minimizar conflictos y rebases.
**Cada bloque debe estar mergeado a `develop` antes de pasar al
siguiente** (con excepciones marcadas).

### Bloque 0 — Lotes pendientes Sprint 0 (paralelo)

| Persona | Lote | Bloquea a |
|---|---|---|
| Frank | FK-0 (JWT hardening) | Nadie, pero conviene primero |
| Frank | FK-1 (hardening compartido) | Marcelo y Max para audit log |
| Marcelo | MX-0 (BookCopy UI) | Su propio MX-4 |
| Max | MA-0 (disponibilidad catálogo) | Su propio MA-5 (botón reservar) |
| Frank | FK-2 (tests Sprint 0 suyos) | Nadie |
| Marcelo | MX-1 (hardening suyo) | Después de FK-1 |
| Max | MA-1 (test Sprint 0 suyo) | Después de FK-1 |

**Punto crítico**: FK-1 desbloquea a los otros dos. Frank lo
prioriza.

### Bloque 1 — Domain + Persistence (orden estricto)

1. **Marcelo MX-2** (Loan + LoanRenewal). **Mergear primero.**
2. **Frank FK-3** (Return + Fine). Depende de Loan. Rebasear y
   mergear.
3. **Max MA-2** (Reservation + Notification). Depende de Loan
   (FK opcional `FulfilledByLoanId`). Rebasear y mergear.

**Punto crítico**: las migraciones se aplican en este orden. Si
dos crean migraciones en paralelo, el segundo en mergear regenera.

### Bloque 2 — Application + Endpoints (paralelo con coordinación)

Los tres pueden trabajar en paralelo, pero hay servicios
compartidos que uno provee y los otros consumen. Para no
bloquearse, usar **stubs temporales con TODOs**:

| Provee | Consumen | Cómo desbloquearse antes del merge |
|---|---|---|
| Frank: `IUserEligibilityService` | Marcelo (MX-3), Max (MA-3) | Definir interfaz en Application desde día 1, stub que devuelve `IsEligible = true` con TODO. Marcelo y Max usan la interfaz. Cuando Frank mergee la implementación real, el stub se reemplaza. |
| Max: `IReservationQueryService` | Marcelo (MX-3) | Igual. Stub devuelve `false` (no hay reservas). |
| Marcelo: evento `LoanCreated` | Max (handler en MA-4) | Marcelo define el evento como parte de MX-3 antes que Max lo consuma. |
| Frank: eventos `LoanReturned`, `FineCreated` | Marcelo (MX-5), Max (MA-3, MA-4) | Frank define los eventos como parte de FK-4. |

**Recomendación**: en una llamada de 30 min al inicio del Bloque
2, los tres definen las **interfaces y eventos** y los suben en un
PR conjunto chico. Después cada quien implementa su parte sin
bloquearse.

### Bloque 3 — UI Angular (paralelo, sin dependencias entre sí)

Los tres pueden trabajar la UI en paralelo. Lo único compartido
es:
- Posibles cambios en componentes shared (avatares, tablas, etc.).
  Si alguien necesita modificar un compartido, lo hace en un PR
  chico aparte y los demás rebasean.
- Routing principal: cada feature se registra como lazy-loaded en
  `app.routes.ts`. Tres entradas distintas, no debería haber
  conflicto.

### Bloque 4 — Features finales (paralelo)

| Persona | Lote |
|---|---|
| Marcelo | MX-6 (Dashboard KPIs) |
| Frank | FK-7 (Reportes) |
| Max | MA-6 (Chatbot) |

Sin dependencias entre estos.

### Bloque 5 — Documentación + Demo

Todos en paralelo:
- MX-7, FK-8, MA-7 (docs por módulo).
- Sesión grupal: actualizar `README.md`, `DEUDA_TECNICA.md`,
  consolidar.
- Demo end-to-end con flujo completo (registro → préstamo →
  devolución tarde → multa → pago → reserva → notificación).

---

## 2. Tabla de dependencias (resumen visual)

```
                   ┌─────────────────────┐
                   │  Sprint 0 cerrado   │
                   │  (lotes 0-3 hechos) │
                   └──────────┬──────────┘
                              │
       ┌──────────────────────┼──────────────────────┐
       ▼                      ▼                      ▼
  ┌─────────┐           ┌─────────┐           ┌─────────┐
  │  FK-0   │           │  MX-0   │           │  MA-0   │
  │  JWT    │           │ BookCopy│           │ Disp.   │
  │         │           │   UI    │           │ catálogo│
  └────┬────┘           └────┬────┘           └────┬────┘
       │                     │                     │
       ▼                     │                     │
  ┌─────────┐                │                     │
  │  FK-1   │                │                     │
  │Hardening│ ───────────────┼─────────────────────┤
  │compart. │                │                     │
  └────┬────┘                │                     │
       │                     │                     │
       └─────────┬───────────┴───────┬─────────────┘
                 ▼                   ▼             ▼
            ┌─────────┐         ┌─────────┐   ┌─────────┐
            │  FK-2   │         │  MX-1   │   │  MA-1   │
            │ Tests   │         │ Tests   │   │ Test    │
            │ S0 (FK) │         │ S0 (MX) │   │ S0 (MA) │
            └─────────┘         └─────────┘   └─────────┘

                  ┌──────── BLOQUE 1 ────────┐
                              │
                         ┌────▼────┐
                         │  MX-2   │
                         │  Loan   │
                         │ Domain  │
                         └────┬────┘
                              │
                    ┌─────────┴─────────┐
                    ▼                   ▼
               ┌─────────┐         ┌─────────┐
               │  FK-3   │         │  MA-2   │
               │ Return  │         │Reserv.  │
               │ + Fine  │         │+ Notif. │
               └────┬────┘         └────┬────┘
                    │                   │
                  ┌─└─── BLOQUE 2 ──────┘─┐
                  │                       │
                  ▼                       ▼
             ┌─────────┐             ┌─────────┐
             │  MX-3   │  ◄────────► │  FK-4   │
             │ Loan    │  eligibilty │ Returns │
             │Service  │             │ Service │
             └────┬────┘             └────┬────┘
                  │                       │
                  ▼                       ▼
             ┌─────────┐             ┌─────────┐
             │  MX-5   │             │  MA-3   │
             │ Handler │  ◄────────► │ Reserv. │
             │overdue  │  events     │ Service │
             └─────────┘             └────┬────┘
                                          ▼
                                     ┌─────────┐
                                     │  MA-4   │
                                     │ Notific.│
                                     └─────────┘

                    ┌───── BLOQUE 3 (UI) ─────┐
                    │     paralelo total       │
                    ▼            ▼            ▼
               ┌────────┐  ┌────────┐   ┌────────┐
               │ MX-4   │  │ FK-5,6 │   │ MA-5   │
               │ UI     │  │ UI     │   │ UI     │
               │préstam.│  │devol.  │   │reserv. │
               └────────┘  └────────┘   └────────┘

                    ┌───── BLOQUE 4 ─────┐
                    │    paralelo total   │
                    ▼          ▼          ▼
               ┌────────┐ ┌────────┐ ┌────────┐
               │  MX-6  │ │  FK-7  │ │  MA-6  │
               │Dashboar│ │Reportes│ │Chatbot │
               └────────┘ └────────┘ └────────┘

                    ┌───── BLOQUE 5 ─────┐
                    │   docs + demo       │
                    └─────────────────────┘
```

---

## 3. Reglas de PR

1. **Título**: `Lote <CODIGO>: <descripción>`. Ej: `Lote MX-3:
   Loan service y endpoints`.
2. **Descripción** del PR debe incluir:
   - Bugs/lotes que cierra.
   - Archivos principales tocados.
   - Comandos para verificar (curl, dotnet test).
   - Screenshots si toca UI.
3. **CI** debe pasar (build + tests).
4. **Review** de uno de los otros dos. El reviewer corre el código
   localmente y verifica al menos un criterio de aceptación.
5. **Conflictos**: el dueño del PR resuelve. Si toca migraciones:
   regenera (no edita).
6. **Squash merge** o **rebase merge** según preferencia del
   equipo, pero **consistente**. Recomiendo rebase merge para
   mantener un commit por lote en `develop`.

---

## 4. Tests de integración cross-módulo

Después de mergear todo, correr el flujo end-to-end. Está descrito
acá como script de curl, pero idealmente se vuelve un test
automatizado en `Biblioteca.Tests/Integration/`.

```bash
#!/bin/bash
# Flujo completo DEVLER
set -e

BASE=http://localhost:5122
ADMIN_TOKEN=$(curl -s -X POST $BASE/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin1","password":"Admin123!"}' | jq -r .token)

STUDENT_TOKEN=$(curl -s -X POST $BASE/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"demo.lramirez","password":"Password123!"}' | jq -r .token)

# 1. Admin crea libro y copia (Sprint 0)
BOOK_ID=$(curl -s -X POST $BASE/api/libros -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Book","isbn":"978-0000000001",...}' | jq -r .id)

COPY_ID=$(curl -s -X POST $BASE/api/libros/$BOOK_ID/ejemplares \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"barcode":"COPY-001"}' | jq -r .id)

# 2. Verificar disponibilidad en catálogo (MA-0)
curl -s $BASE/api/libros/$BOOK_ID -H "Authorization: Bearer $STUDENT_TOKEN" \
  | jq '.availableCopies' # debería ser 1

# 3. Admin registra préstamo a Student (MX-3)
LOAN_ID=$(curl -s -X POST $BASE/api/prestamos \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"userId\":\"<student-id>\",\"bookCopyId\":\"$COPY_ID\"}" | jq -r .id)

# 4. Verificar disponibilidad bajó
curl -s $BASE/api/libros/$BOOK_ID -H "Authorization: Bearer $STUDENT_TOKEN" \
  | jq '.availableCopies' # debería ser 0

# 5. Otro student reserva el libro (MA-3)
# (omito el otro login por brevedad)

# 6. Forzar préstamo vencido en BD
psql -c "UPDATE loans SET due_at = NOW() - interval '5 days' WHERE id = '$LOAN_ID'"

# 7. Devolver el libro con multa por retraso (FK-5)
RETURN_ID=$(curl -s -X POST $BASE/api/devoluciones \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"loanId\":\"$LOAN_ID\",\"condition\":\"OK\"}" | jq -r .id)

# 8. Verificar:
#    - Loan pasó a RETURNED (handler de Marcelo)
#    - BookCopy volvió a AVAILABLE
#    - Fine creada con monto = 5 * 5.00 (handler de Frank)
#    - Reserva pasó a READY (handler de Max)
#    - Notificaciones creadas (handler de Max para LoanCreated, FineCreated, ReservationReady)

curl -s $BASE/api/prestamos/$LOAN_ID -H "Authorization: Bearer $ADMIN_TOKEN" \
  | jq '.status' # "RETURNED"

curl -s $BASE/api/usuarios/<student-id>/multas -H "Authorization: Bearer $ADMIN_TOKEN" \
  | jq '.[0].amount' # 25.00

curl -s $BASE/api/notificaciones -H "Authorization: Bearer $STUDENT_TOKEN" \
  | jq 'length' # >= 2 (LOAN_RECEIPT + FINE_CREATED)

# 9. Student intenta nuevo préstamo con multa pendiente → 409
curl -s -X POST $BASE/api/prestamos \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"userId\":\"<student-id>\",\"bookCopyId\":\"$COPY_ID\"}" \
  -w "%{http_code}\n" | tail -1 # 409

# 10. Tesorería confirma pago
curl -s -X POST $BASE/api/multas/<fineId>/pagos \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"method":"CASH"}'

# 11. Ahora el préstamo sí debería poder crearse
# (el otro student que reservó tiene prioridad si está en READY)
```

Si todo este script pasa, el sistema está integrado correctamente.

---

## 5. Checklist final del proyecto

- [ ] Bloque 0: lotes pendientes Sprint 0 cerrados, develop verde
- [ ] Bloque 1: entidades de los 3 módulos en BD
- [ ] Bloque 2: services + endpoints, eventos funcionando
  cross-módulo
- [ ] Bloque 3: UI completa de los 3 flujos
- [ ] Bloque 4: dashboard, reportes, chatbot
- [ ] Script de integración end-to-end pasa completo
- [ ] `dotnet test` con todos los tests verdes
- [ ] `ng build --configuration production` sin warnings nuevos
- [ ] Migraciones aplican limpio desde BD vacía
- [ ] `README.md` actualizado
- [ ] `DEUDA_TECNICA.md` actualizado con todo lo postpuesto
- [ ] Demo grabada para Blackboard
- [ ] Carta de aceptación firmada por el PO

---

## 6. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Conflictos en migraciones EF | Orden estricto de merges (Bloque 1). El segundo regenera. |
| Eventos no se ejecutan en orden esperado | Tests de integración explícitos. Documentar el orden de registro DI en `ServiceCollectionExtensions`. |
| Frank tarda con `IUserEligibilityService` y bloquea a otros | Stub desde día 1 en `Biblioteca.Application/Common/Services/Stubs/`. Marcado con TODO. |
| Diferencias en cómo cada uno define DTOs | Code review obligatorio. Si se ve drift, refactor en PR aparte. |
| Auditoría no captura entidades nuevas | Cada quien agrega su entidad a la lista de auditables en su PR de domain. |
| Demo final falla por algo no testeado | Correr el script de integración 1 semana antes de la demo, no el día anterior. |

---

## 7. Cómo arrancar mañana

1. **Reunión de 30 min** entre los tres:
   - Cada quien lee su archivo de prompts y el de
     `CONTRATOS_COMPARTIDOS.md`.
   - Validan que los nombres de campos, endpoints y eventos están
     ok. Si alguien ve un problema, se ajusta antes de empezar.
   - Acuerdan quién mergea primero el Bloque 1 (Marcelo).
   - Crean las ramas: `feature/prestamos`, `feature/devoluciones`,
     `feature/reservas`.

2. **Cada quien arranca su Bloque 0** (lotes pendientes Sprint 0)
   en paralelo.

3. **Daily de 15 min** todos los días: qué hicieron ayer, qué
   harán hoy, blockers.

4. **Cuando alguien cierra un lote**: PR a develop, los otros
   reviewean en cuanto puedan, mergean. El dueño rebasea su
   siguiente lote sobre develop actualizado.
