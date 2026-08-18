---
name: tdd
description: Guía cambios de comportamiento de Domain y Application en MealBridge mediante TDD estricto. Usar antes de crear o modificar reglas de donaciones, validaciones, reclamos, transiciones o casos de uso.
---

# TDD de MealBridge

## Regla obligatoria

No crear ni modificar comportamiento de producción en `backend/MealBridge.Domain/` o `backend/MealBridge.Application/` hasta que exista una prueba unitaria enfocada que falle por la razón correcta.

Todo comportamiento se implementa en este orden:

```text
Red → Green → Refactor
```

Esta regla no puede omitirse aunque el comportamiento parezca pequeño o evidente.

## Cuándo usar esta skill

- Al crear o modificar `DonationLot`.
- Al cambiar validaciones, enums o la ventana de recogida.
- Al implementar reclamaciones.
- Al modificar la máquina de estados.
- Al crear o cambiar casos de uso de Application.
- Al corregir un defecto de dominio o aplicación.

No es necesario aplicar TDD al CSS, documentación o configuración sin comportamiento. Las pruebas de API, persistencia y frontend pueden complementar las pruebas unitarias, pero no reemplazarlas.

## Preparación

Antes de comenzar:

1. Leer `FUNCTIONAL-REQUIREMENTS.md` y `MVP-SCOPE.md`.
2. Leer la tarea activa en `IMPLEMENTATION-PLAN.md`.
3. Confirmar que existe una pareja consecutiva:

```yaml
- id: comportamiento-test-red
  status: in_progress
  content: Agregar y ejecutar una prueba enfocada que falle.

- id: comportamiento-impl
  status: pending
  content: Implementar el mínimo código para dejar verde esa prueba.
```

Si la pareja no existe, usar `.agents/skills/planning/SKILL.md` para agregarla antes de editar código.

## Ciclo Red

1. Agregar una sola prueba para un solo comportamiento.
2. Nombrarla con el patrón `Method_Scenario_ExpectedResult`.
3. Ejecutar únicamente esa prueba.
4. Confirmar que falla por la razón esperada.
5. Registrar comando, nombre y causa del fallo en `AI-USE.md`.
6. Marcar la tarea `*-test-red` como `completed`.

Comando enfocado:

```powershell
dotnet test backend/MealBridge.Tests/MealBridge.Tests.csproj `
  --filter "FullyQualifiedName~NombreDeLaPrueba"
```

Un rojo válido puede ser:

- Método o tipo todavía inexistente.
- `NotImplementedException`.
- Aserción que demuestra que falta el comportamiento.
- Resultado actual diferente al exigido.

Un rojo inválido es:

- Proyecto roto por un error no relacionado.
- Dependencia sin restaurar.
- Base de datos o servicio externo apagado para una prueba unitaria.
- Error de sintaxis accidental.
- Prueba que falla de manera intermitente.

No pasar a Green hasta tener un rojo válido.

## Ciclo Green

1. Cambiar la tarea `*-impl` a `in_progress`.
2. Implementar el mínimo código de producción necesario.
3. No agregar comportamientos que la prueba actual no exige.
4. Ejecutar nuevamente la prueba enfocada.
5. Cuando pase, ejecutar las pruebas del archivo o área relacionada.
6. Registrar el resultado verde en `AI-USE.md`.

Comandos:

```powershell
dotnet test backend/MealBridge.Tests/MealBridge.Tests.csproj `
  --filter "FullyQualifiedName~NombreDeLaPrueba"

dotnet test backend/MealBridge.Tests/MealBridge.Tests.csproj
```

Si la prueba continúa fallando, corregir únicamente la causa observada. No debilitar aserciones ni eliminar casos para obtener verde.

## Ciclo Refactor

Con las pruebas verdes:

1. Eliminar duplicación y mejorar nombres.
2. Mantener las invariantes dentro de Domain.
3. Mantener Application como coordinador de casos de uso.
4. Evitar dependencias de Domain hacia HTTP, EF Core o Infrastructure.
5. Ejecutar otra vez la prueba enfocada y la suite.
6. Marcar `*-impl` como `completed` solo si ambas pasan.

Refactor no permite introducir comportamiento nuevo. Cualquier comportamiento nuevo inicia otro ciclo Red.

## Pruebas obligatorias del reto

Como mínimo deben existir y pasar estas cinco intenciones:

### 1. Creación válida

```text
Create_WhenValid_ReturnsAvailableLot
```

Debe comprobar:

- UUID generado por el servidor.
- `status == available`.
- `claimedBy == null`.
- `claimedAt == null`.
- `createdAt` y `updatedAt` asignados.

### 2. Entrada inválida

```text
Create_WhenQuantityLessThanOne_Rejects
```

Debe comprobar al menos el rechazo de `quantity < 1`. También deben cubrirse la ventana de fechas, campos obligatorios, longitudes y enums exigidos.

### 3. Reclamo exitoso

```text
Claim_WhenAvailable_SetsClaimedByAndStatus
```

Debe comprobar:

- Cambio de `available` a `claimed`.
- `claimedBy` con el primer coordinador.
- `claimedAt` asignado.
- `updatedAt` actualizado.

### 4. Segundo reclamo

```text
Claim_WhenAlreadyClaimed_Conflicts
```

Debe comprobar que el segundo reclamo se rechaza y no sobrescribe el primer `claimedBy`.

### 5. Transición ilegal

```text
ChangeStatus_WhenAvailableToPickedUp_Conflicts
```

Debe comprobar que `available → picked_up` se rechaza.

Agregar también:

```text
ChangeStatus_WhenClaimedToPickedUp_Succeeds
```

para demostrar la transición legal recomendada.

## Tabla de estados que deben proteger las pruebas

```text
available → claimed | cancelled | expired
claimed   → picked_up | cancelled
picked_up → ninguna
cancelled → ninguna
expired   → ninguna
```

Reglas adicionales:

- La creación siempre produce `available`.
- `claimed` solo se alcanza mediante `Claim`.
- Los estados terminales no aceptan cambios.
- El dominio expresa conflictos sin depender de códigos HTTP.
- La API traduce esos conflictos a `409`.

## Evidencia en AI-USE.md

Registrar cada ciclo con:

```markdown
### NombreDeLaPrueba

- Red command: `comando exacto`
- Red result: fallo esperado y motivo
- Green command: `comando exacto`
- Green result: prueba superada
- Production files: rutas modificadas
- Refactor: cambio realizado o `none`
```

No inventar resultados ni reconstruir evidencia después. Registrar Red antes de implementar.

## Anti-patrones prohibidos

- Escribir las pruebas después de la implementación.
- Crear producción y prueba en el mismo paso sin ejecutar Red.
- Saltar la confirmación del fallo.
- Ejecutar únicamente toda la suite sin una prueba enfocada.
- Usar solo pruebas end-to-end o de API.
- Probar controllers en lugar de las invariantes del dominio.
- Hacer mocks de `DonationLot` y no ejecutar sus reglas reales.
- Introducir varios comportamientos para satisfacer una sola prueba.
- Cambiar la prueba para adaptarla a una implementación incorrecta.
- Marcar tareas completas sin comandos y resultados reales.
