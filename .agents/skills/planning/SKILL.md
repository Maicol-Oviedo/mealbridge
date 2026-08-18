---
name: planning
description: Crea y mantiene planes de implementación precisos para MealBridge. Usar al entrar en Plan Mode, dividir funcionalidades, ordenar trabajo, actualizar tareas o preparar la siguiente iteración del MVP.
---

# Planning de MealBridge

## Propósito

Transformar requisitos de MealBridge en tareas pequeñas, ordenadas, ejecutables y verificables. Esta skill organiza el trabajo; no reemplaza la skill de TDD ni autoriza implementar producción antes de una prueba en rojo.

## Cuándo usarla

- Al crear o actualizar `IMPLEMENTATION-PLAN.md`.
- Antes de comenzar una funcionalidad o cambio de comportamiento.
- Cuando una tarea no cabe en una iteración corta.
- Cuando aparece un bloqueo o cambia una dependencia.
- Al decidir cuál es la siguiente tarea pendiente.

## Formato obligatorio

Cada tarea debe usar:

```yaml
- id: nombre-unico
  status: pending
  content: Acción concreta sobre `ruta/relativa/al/repositorio` con un resultado verificable.
```

Reglas:

1. `id` debe ser corto, único y describir un solo resultado.
2. `status` solo puede ser `pending`, `in_progress`, `completed`, `blocked` o `cancelled`.
3. `content` debe nombrar al menos una ruta relativa al repositorio.
4. `content` debe indicar qué crear o modificar, para qué y cómo comprobar que terminó.
5. Una tarea no debe mezclar dominio, API, persistencia y frontend.
6. Ordenar las tareas por dependencias; una tarea nunca debe depender de otra posterior.
7. Mantener como máximo una tarea `in_progress`.

## Precisión requerida

Una tarea está suficientemente especificada solo si responde:

- ¿Qué archivo o directorio cambia?
- ¿Qué comportamiento o artefacto produce?
- ¿Qué restricciones del reto debe respetar?
- ¿Qué comando, prueba o evidencia confirma el resultado?

Si falta una respuesta, dividir o reescribir la tarea antes de implementarla.

Ejemplo correcto:

```yaml
- id: domain-claim-test-red
  status: pending
  content: Agregar `Claim_WhenAvailable_SetsClaimedByAndStatus` en `backend/MealBridge.Tests/Domain/DonationLotClaimTests.cs`; ejecutar solo esa prueba y confirmar que falla porque `DonationLot.Claim` todavía no implementa la transición.
```

Ejemplos incorrectos:

```yaml
- id: backend
  content: Hacer el backend.

- id: service
  content: Actualizar el servicio.

- id: tests
  content: Agregar pruebas.
```

## Cómo dividir trabajo grande

Dividir por corte observable y por capa:

1. Regla o comportamiento del dominio.
2. Caso de uso de Application.
3. Adaptador de Infrastructure.
4. Contrato HTTP de API.
5. Interacción del frontend.
6. Verificación del corte completo.

Cada tarea debe poder completarse y validarse sin comenzar otra tarea grande. Si toca más de tres áreas o no tiene una comprobación concreta, todavía es demasiado amplia.

## Relación con TDD

TDD significa desarrollar comportamiento mediante este ciclo:

1. **Red:** escribir una prueba y comprobar que falla por la razón correcta.
2. **Green:** implementar el mínimo código para que pase.
3. **Refactor:** mejorar el código manteniendo las pruebas verdes.

Para todo comportamiento de `backend/MealBridge.Domain/` o `backend/MealBridge.Application/`, crear siempre dos tareas consecutivas:

```yaml
- id: comportamiento-test-red
  status: pending
  content: Agregar una prueba enfocada en `backend/MealBridge.Tests/...`; ejecutar el filtro exacto y confirmar el fallo esperado.

- id: comportamiento-impl
  status: pending
  content: Implementar el mínimo comportamiento en `backend/MealBridge.Domain/...` o `backend/MealBridge.Application/...`; ejecutar la prueba enfocada y dejarla verde.
```

La tarea `*-impl` no puede comenzar mientras su `*-test-red` no esté `completed` con evidencia real en `AI-USE.md`. Para ejecutar el ciclo, leer `.agents/skills/tdd/SKILL.md`.

## Flujo de una iteración

1. Leer `MVP-SCOPE.md`, los requisitos afectados y `IMPLEMENTATION-PLAN.md`.
2. Elegir la primera tarea `pending` cuyas dependencias estén completas.
3. Cambiar solo esa tarea a `in_progress`.
4. Ejecutar exactamente el trabajo descrito.
5. Ejecutar su verificación.
6. Si falla, mantenerla `in_progress` o marcarla `blocked` con la causa concreta.
7. Si pasa, cambiarla a `completed` y registrar evidencia cuando corresponda.
8. Revisar si la siguiente tarea continúa siendo válida antes de iniciarla.

## Manejo de hallazgos

- No ampliar el MVP silenciosamente.
- Si aparece trabajo obligatorio, insertarlo antes de la tarea que depende de él.
- Si aparece una mejora opcional, registrarla fuera del camino crítico y no iniciarla.
- Si cambia una ruta real, actualizar las tareas pendientes que la mencionan.
- Nunca marcar una tarea como `completed` basándose en una suposición.

## Tarea final obligatoria

El último todo debe tener `id: verify`, rutas o documentos afectados y comandos exactos:

```yaml
- id: verify
  status: pending
  content: Ejecutar `dotnet test backend/MealBridge.sln`, `dotnet build backend/MealBridge.sln`, `npm run build --prefix frontend` y `npm run lint --prefix frontend`; iniciar PostgreSQL/API/frontend, completar el guion de `MVP-SCOPE.md` y registrar los resultados en `AI-USE.md`.
```

## Anti-patrones

- Una única tarea “construir MealBridge”.
- Tareas vagas como “actualizar servicio” o “agregar pruebas”.
- Implementación antes de su tarea `*-test-red`.
- Varias tareas `in_progress`.
- Marcar tareas completas sin ejecutar su verificación.
- Incluir autenticación, RAG, colas u otra extensión dentro del MVP.
- Reescribir todo el plan por un hallazgo pequeño.
