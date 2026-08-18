# Implementar una tarea con TDD

Implementa únicamente la tarea `*-test-red` o `*-impl` que indique el usuario.

## Preparación

1. Lee `AGENTS.md`.
2. Lee `.agents/skills/tdd/SKILL.md`.
3. Lee `MVP-SCOPE.md` y el requisito afectado.
4. Revisa `IMPLEMENTATION-PLAN.md`.
5. Confirma que la tarea solicitada es la primera tarea pendiente válida.
6. Mantén una sola tarea en estado `in_progress`.

## Red

Si la tarea termina en `*-test-red`:

1. Crea una sola prueba para un comportamiento.
2. No edites código de producción.
3. Ejecuta únicamente esa prueba:

```powershell
dotnet test backend/MealBridge.Tests/MealBridge.Tests.csproj `
  --filter "FullyQualifiedName~NOMBRE_DE_LA_PRUEBA"
```

4. Verifica que falle por comportamiento ausente, no por configuración, sintaxis o dependencias.
5. Registra inmediatamente en `AI-USE.md`:
   - Comando exacto.
   - Nombre de la prueba.
   - Motivo real del fallo.
6. Marca la tarea `*-test-red` como `completed`.
7. Detente si el rojo no es válido.

## Green

Si la tarea termina en `*-impl`:

1. Confirma que su pareja `*-test-red` está `completed` y tiene evidencia en `AI-USE.md`.
2. Implementa el mínimo código necesario; no agregues otro comportamiento.
3. Ejecuta nuevamente la prueba enfocada.
4. Si pasa, ejecuta la suite:

```powershell
dotnet test backend/MealBridge.Tests/MealBridge.Tests.csproj
```

5. No debilites ni elimines aserciones para obtener verde.
6. Registra el comando y resultado verde en `AI-USE.md`.

## Refactor

Con las pruebas verdes:

1. Elimina duplicación y mejora nombres sin cambiar comportamiento.
2. Conserva las invariantes en Domain.
3. Conserva Application como coordinador.
4. Ejecuta de nuevo la prueba enfocada y la suite.
5. Marca `*-impl` como `completed` solo si ambas pasan.

## Respuesta final

Informa:

- Tarea ejecutada.
- Prueba y resultado Red o Green.
- Archivos modificados.
- Verificación ejecutada.
- Siguiente tarea pendiente.

No continúes automáticamente con otra tarea.
