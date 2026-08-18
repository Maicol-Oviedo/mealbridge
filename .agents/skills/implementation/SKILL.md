---
name: implementation
description: Aplica las reglas de implementación y estilo de MealBridge. Usar antes de crear o modificar código de producción, handlers, servicios, entidades, repositorios, controllers o componentes.
---

# Implementación de MealBridge

## Uso obligatorio

Aplicar esta skill en toda tarea `*-impl` y en cualquier cambio de código de producción. Si la tarea modifica Domain o Application, aplicar conjuntamente `.agents/skills/tdd/SKILL.md`; ninguna de las dos reemplaza a la otra.

## Preparación

Antes de editar código:

1. Leer `AGENTS.md` y la tarea activa de `IMPLEMENTATION-PLAN.md`.
2. Leer `AI-AIDED-FULLSTACK-CHALLENGE.md`, el requisito aplicable y `MVP-SCOPE.md`.
3. Si cambia comportamiento de Domain o Application, aplicar primero `.agents/skills/tdd/SKILL.md`.
4. Implementar únicamente lo descrito por la tarea activa.
5. Identificar qué apartados exactos de RF, RNF y alcance debe satisfacer la tarea.

## Mensajes como constantes

No escribir mensajes directamente dentro de llamadas, retornos, excepciones o asignaciones.
Todos los mensajes de error deben estar en español.

Esta regla aplica a:

- Templates de `ILogger`.
- Mensajes enviados en el envelope de la API.
- Mensajes de excepciones.
- Mensajes de validación.
- Mensajes visibles en el frontend.

Declarar cada mensaje como una constante dentro de la misma clase que lo utiliza:

```csharp
public sealed class ExampleHandler
{
    private const string UnexpectedErrorMessage =
        "Ocurrió un error inesperado.";

    public void Handle(Exception exception)
    {
        logger.LogError(exception, UnexpectedErrorMessage);
    }
}
```

Reglas:

1. Usar `private const string` cuando el mensaje solo pertenece a esa clase.
2. Dar a la constante un nombre descriptivo terminado en `Message` o `LogMessage`.
3. Reutilizar una sola constante cuando el texto sea exactamente el mismo.
4. Mantener en la clase tanto el template de log como el mensaje de respuesta que le pertenezcan.
5. No crear una clase global de constantes para mensajes que solo usa una clase.
6. No duplicar literales idénticos.
7. Mantener logging estructurado; sus templates también deben ser constantes.
8. Escribir en español todos los mensajes de error de logs, API, excepciones, validaciones y frontend.

Ejemplo incorrecto:

```csharp
logger.LogError(exception, "An unexpected error occurred.");
return ApiEnvelope<object>.Failure("An unexpected error occurred.");
```

Ejemplo correcto:

```csharp
private const string UnexpectedErrorMessage =
    "Ocurrió un error inesperado.";

logger.LogError(exception, UnexpectedErrorMessage);
return ApiEnvelope<object>.Failure(UnexpectedErrorMessage);
```

## Configuración sin valores hardcoded

No escribir directamente en código valores que puedan cambiar entre entornos o que pertenezcan a configuración.

Esta regla aplica a:

- Orígenes CORS.
- URLs, hosts y puertos.
- Connection strings y credenciales.
- Listas configurables de métodos o políticas.
- Identificadores de servicios externos.

Usar clases de configuración tipadas y cargar sus valores desde `appsettings.json`, variables de entorno o ambos. Mantener en código únicamente constantes que representen nombres internos de secciones o políticas.

Ejemplo incorrecto:

```csharp
policy.WithOrigins("http://localhost:5173")
    .WithMethods("GET", "POST", "PATCH", "OPTIONS");
```

Ejemplo correcto:

```csharp
policy.WithOrigins(corsSettings.AllowedOrigins)
    .WithMethods(corsSettings.AllowedMethods);
```

Agregar las claves sin secretos a `appsettings.json` o `.env.example`. Las variables de entorno deben poder sobrescribir los valores locales.

## Excepciones de negocio

Usar únicamente las excepciones necesarias de `backend/MealBridge.Domain/Exceptions/`:

- `InvalidArgumentException` para errores que la API traduce a `400`.
- `NotFoundException` para recursos inexistentes que la API traduce a `404`.
- `ConflictException` para reclamos duplicados o transiciones ilegales que la API traduce a `409`.

El código que lanza la excepción debe proporcionar un mensaje en español mediante una constante de su propia clase. `GlobalExceptionHandler` devuelve ese mensaje dentro del envelope.

No agregar una excepción separada para duplicados: el doble reclamo es un conflicto. No agregar excepciones de autorización mientras la autenticación esté fuera del MVP. Los errores no controlados se traducen a `500` con un mensaje genérico que no expone detalles internos.

Reglas obligatorias:

1. Usar `InvalidArgumentException` para validaciones de entrada y argumentos inválidos.
2. Usar `NotFoundException` cuando un caso de uso no encuentre el lote solicitado.
3. Usar `ConflictException` para reclamos duplicados y transiciones ilegales.
4. No representar errores de negocio mediante `null`, `false`, códigos enteros, `ArgumentException` o `InvalidOperationException`.
5. No usar códigos HTTP dentro de Domain o Application.
6. Dejar la traducción a `400`, `404` y `409` exclusivamente en `GlobalExceptionHandler`.
7. Probar el tipo exacto de excepción esperado en cada regla de negocio.

## Evitar validaciones repetidas

Cuando una clase repita el mismo patrón de validación y lanzamiento de excepción para varios atributos:

1. Extraer uno o más métodos privados dentro de la misma clase.
2. Pasar como parámetros el valor, límite y mensajes necesarios.
3. Mantener separados los casos con semántica distinta, como texto obligatorio y texto opcional.
4. Conservar los mensajes y límites como constantes de la clase propietaria.
5. No crear helpers globales para lógica utilizada únicamente por una clase.
6. No introducir flags que hagan menos clara la intención cuando dos métodos pequeños sean más legibles.
7. Ejecutar las pruebas antes y después para demostrar que el refactor no cambia comportamiento.

Ejemplo esperado:

```csharp
ValidateRequiredText(
    businessName,
    BusinessNameMaxLength,
    BusinessNameRequiredMessage,
    BusinessNameTooLongMessage);

ValidateOptionalText(
    description,
    DescriptionMaxLength,
    DescriptionTooLongMessage);
```

## Flujo de aplicación

1. Revisar el archivo que se modificará.
2. Buscar mensajes y valores de configuración literales nuevos o modificados.
3. Crear constantes para mensajes en la clase propietaria.
4. Mover los valores configurables a opciones tipadas y configuración externa.
5. Confirmar que los errores esperados usan una de las excepciones de negocio permitidas.
6. Implementar exactamente el comportamiento exigido: no inventar, no omitir y no agregar alcance.
7. Compilar o ejecutar la validación del área.
8. Ejecutar la validación de cumplimiento antes de marcar la tarea como completa.

## Validación obligatoria de cumplimiento

Después de cada implementación:

1. Comparar el resultado con `AI-AIDED-FULLSTACK-CHALLENGE.md`.
2. Compararlo con los RF y RNF aplicables.
3. Compararlo con `MVP-SCOPE.md` y la tarea activa.
4. Confirmar que cada condición aplicable está implementada y probada.
5. Confirmar que no se añadió ningún campo requerido, endpoint, transición, actor o funcionalidad no solicitada.
6. Confirmar que no se omitió ninguna condición incluida en la tarea.
7. Revisar arquitectura, excepciones, mensajes, configuración y TDD.
8. Ejecutar las pruebas, build y lint correspondientes.

Registrar al terminar:

- Requisitos aplicables comprobados.
- Evidencia ejecutada.
- Elementos pendientes que pertenecen a tareas posteriores.
- Desviaciones encontradas y su corrección.

La conformidad se evalúa sobre el corte implementado, no sobre funcionalidades programadas para tareas posteriores. No afirmar que el MVP completo cumple al 100 % hasta finalizar `verify`.

No marcar una tarea `completed` si:

- Hace menos de lo exigido por su alcance.
- Agrega más comportamiento del solicitado.
- Contradice el brief, un RF, un RNF o el MVP.
- Quedan mensajes o datos configurables hardcoded.
- Usa errores genéricos para una regla de negocio.
- Falta la evidencia TDD requerida.

## Verificación

Para backend:

```powershell
dotnet build backend/MealBridge.sln
dotnet test backend/MealBridge.sln
```

Para frontend:

```powershell
npm run build --prefix frontend
npm run lint --prefix frontend
```

## Extensión de reglas

Cuando el usuario defina una nueva regla de implementación:

1. Agregarla a esta skill con un ejemplo correcto e incorrecto.
2. Aplicarla al código señalado.
3. Actualizar `AGENTS.md` si cambia el flujo general.
4. Verificar los archivos afectados antes de continuar.
