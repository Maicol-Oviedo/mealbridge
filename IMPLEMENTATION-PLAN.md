# Plan de implementación de MealBridge

Las tareas se ejecutan en este orden. Cada cambio de comportamiento de `Domain` o `Application` comienza con una tarea `*-test-red`; su tarea `*-impl` no puede iniciarse hasta confirmar que la prueba enfocada falla por la razón correcta. Toda tarea `*-impl` debe aplicar conjuntamente `.agents/skills/implementation/SKILL.md` y terminar con una comparación explícita contra el brief, RF, RNF y alcance aplicables.

## Tareas

```yaml
- id: workspace-planning-skill
  status: completed
  content: Crear `.agents/skills/planning/SKILL.md` con el formato obligatorio de tareas, orden de ejecución, división de tareas grandes, parejas `*-test-red`/`*-impl` y ciclo de actualización del plan.

- id: workspace-agents
  status: completed
  content: Crear `AGENTS.md` con la descripción de MealBridge, stack, comandos de API/frontend/pruebas, dependencias de Clean Architecture y enlaces a `.agents/skills/planning/SKILL.md` y `.agents/skills/tdd/SKILL.md`.

- id: workspace-rule
  status: completed
  content: Crear `.cursor/rules/mealbridge-tdd.mdc` con la regla test-first, la máquina de estados de MealBridge, el envelope HTTP y la dirección de dependencias API → Application → Domain.

- id: workspace-command
  status: completed
  content: Crear `.cursor/commands/tdd-implement.md` con instrucciones para ejecutar una sola prueba en rojo, confirmar la causa, implementar el mínimo código y volver a ejecutar la prueba y la suite.

- id: workspace-tdd-skill
  status: completed
  content: Crear `.agents/skills/tdd/SKILL.md` con el ciclo red → green → refactor, el comando xUnit enfocado, las cinco pruebas obligatorias de MealBridge y los anti-patrones prohibidos por el reto.

- id: workspace-ai-log
  status: completed
  content: Crear `AI-USE.md` con secciones Tools, Decisions, TDD evidence, Prompts y What broke; registrar la preparación actual sin inventar evidencia de pruebas en rojo.

- id: api-scaffold
  status: completed
  content: Ajustar `backend/MealBridge.Api/Program.cs` para registrar controllers, CORS, Swagger, manejo central de errores y futura composición de Application/Infrastructure, conservando `/health` y sin agregar reglas de negocio.

- id: workspace-implementation-skill
  status: completed
  content: Crear `.agents/skills/implementation/SKILL.md` con las reglas de mensajes de error en español y como constantes dentro de su clase propietaria; enlazarla desde `AGENTS.md` y aplicarla a `backend/MealBridge.Api/Middleware/GlobalExceptionHandler.cs`.

- id: api-cors-configuration
  status: completed
  content: Crear `backend/MealBridge.Api/Configuration/CorsSettings.cs`, mover orígenes y métodos CORS desde `backend/MealBridge.Api/Program.cs` hacia `backend/MealBridge.Api/appsettings.json`, documentar el override en `.env.example` y agregar a `.agents/skills/implementation/SKILL.md` la prohibición de datos configurables hardcoded.

- id: api-business-exception-mapping
  status: completed
  content: Crear en `backend/MealBridge.Domain/Exceptions/` únicamente `InvalidArgumentException`, `NotFoundException` y `ConflictException`; interceptarlas en `backend/MealBridge.Api/Middleware/GlobalExceptionHandler.cs` como 400, 404 y 409 con mensajes en español, conservando un 500 genérico sin filtrar información.

- id: domain-create-test-red
  status: completed
  content: Reemplazar `backend/MealBridge.Tests/UnitTest1.cs` por `backend/MealBridge.Tests/Domain/DonationLotCreationTests.cs`; agregar `Create_WhenValid_ReturnsAvailableLot`, ejecutar solo esa prueba y registrar en `AI-USE.md` el fallo esperado por entidad o comportamiento todavía inexistente.

- id: domain-create-impl
  status: completed
  content: Crear `backend/MealBridge.Domain/Donations/DonationLot.cs`, `FoodCategory.cs`, `DonationUnit.cs` y `DonationStatus.cs`; implementar la creación mínima con UUID, `available`, timestamps UTC y valores de reclamo nulos hasta dejar verde `Create_WhenValid_ReturnsAvailableLot`.

- id: domain-quantity-validation-test-red
  status: completed
  content: Agregar `Create_WhenQuantityLessThanOne_Rejects` en `backend/MealBridge.Tests/Domain/DonationLotCreationTests.cs`; exigir `InvalidArgumentException` y mensaje en español, ejecutar solo esa prueba y registrar el rojo.

- id: domain-quantity-validation-impl
  status: completed
  content: Implementar en `backend/MealBridge.Domain/Donations/DonationLot.cs` la validación `quantity >= 1` lanzando `InvalidArgumentException` con un mensaje constante en español; dejar verde la prueba enfocada y validar cumplimiento.

- id: domain-text-validation-test-red
  status: completed
  content: Agregar pruebas en `backend/MealBridge.Tests/Domain/DonationLotCreationTests.cs` para strings obligatorios y límites de `businessName`, `title`, `description` y `pickupAddress`; exigir `InvalidArgumentException` y ejecutar cada caso nuevo en rojo.

- id: domain-text-validation-impl
  status: completed
  content: Implementar en `backend/MealBridge.Domain/Donations/DonationLot.cs` las validaciones de presencia y longitud exactas con mensajes constantes en español; dejar verdes las pruebas enfocadas y validar cumplimiento.

- id: domain-text-validation-refactor
  status: completed
  content: Refactorizar `backend/MealBridge.Domain/Donations/DonationLot.cs` para centralizar validaciones de texto obligatorio y opcional en métodos privados, eliminar repetición y conservar verdes las pruebas sin cambiar comportamiento.

- id: domain-enum-validation-test-red
  status: completed
  content: Agregar pruebas en `backend/MealBridge.Tests/Domain/DonationLotCreationTests.cs` para valores indefinidos de `FoodCategory` y `DonationUnit`; exigir `InvalidArgumentException` y confirmar el rojo.

- id: domain-enum-validation-impl
  status: completed
  content: Implementar en `backend/MealBridge.Domain/Donations/DonationLot.cs` el rechazo de enums indefinidos con mensajes constantes en español; dejar verdes las pruebas enfocadas y validar cumplimiento.

- id: domain-window-validation-test-red
  status: completed
  content: Agregar `Create_WhenAvailableUntilIsNotAfterAvailableFrom_Rejects` en `backend/MealBridge.Tests/Domain/DonationLotCreationTests.cs`; exigir `InvalidArgumentException` y registrar el rojo.

- id: domain-window-validation-impl
  status: completed
  content: Implementar en `backend/MealBridge.Domain/Donations/DonationLot.cs` que `availableUntil` sea posterior a `availableFrom`, usando un mensaje constante en español; dejar verde la prueba y validar cumplimiento.

- id: domain-utc-test-red
  status: completed
  content: Agregar en `backend/MealBridge.Tests/Domain/DonationLotCreationTests.cs` una prueba que exija almacenar `availableFrom` y `availableUntil` en UTC cuando llegan con offset; ejecutar y registrar el rojo.

- id: domain-utc-impl
  status: completed
  content: Normalizar en `backend/MealBridge.Domain/Donations/DonationLot.cs` la ventana de disponibilidad a UTC sin alterar el instante; dejar verde la prueba enfocada y validar cumplimiento.

- id: domain-claim-test-red
  status: completed
  content: Crear `backend/MealBridge.Tests/Domain/DonationLotClaimTests.cs` con `Claim_WhenAvailable_SetsClaimedByAndStatus`; ejecutar solo esa prueba y registrar el rojo antes de editar el dominio.

- id: domain-claim-impl
  status: completed
  content: Implementar `Claim` en `backend/MealBridge.Domain/Donations/DonationLot.cs` para cambiar a `claimed`, asignar `claimedBy`/`claimedAt` y actualizar `updatedAt`; dejar verde la prueba de éxito y validar cumplimiento.

- id: domain-claim-name-validation-test-red
  status: completed
  content: Agregar en `backend/MealBridge.Tests/Domain/DonationLotClaimTests.cs` pruebas para `coordinatorName` en blanco y mayor a 120 caracteres; exigir `InvalidArgumentException` con mensajes en español y registrar el rojo.

- id: domain-claim-name-validation-impl
  status: completed
  content: Implementar en `backend/MealBridge.Domain/Donations/DonationLot.cs` la obligatoriedad y longitud máxima de `coordinatorName` mediante validación privada reutilizable e `InvalidArgumentException`; dejar verdes las pruebas y validar cumplimiento.

- id: domain-claim-conflict-test-red
  status: completed
  content: Agregar `Claim_WhenAlreadyClaimed_Conflicts` en `backend/MealBridge.Tests/Domain/DonationLotClaimTests.cs`; exigir `ConflictException` y comprobar en rojo que el segundo reclamo se rechaza conservando el primer `claimedBy`.

- id: domain-claim-conflict-impl
  status: completed
  content: Agregar en `backend/MealBridge.Domain/Donations/DonationLot.cs` el rechazo de reclamos cuando el estado no sea `available`, lanzando `ConflictException` con un mensaje constante en español y sin códigos HTTP.

- id: domain-status-test-red
  status: completed
  content: Crear `backend/MealBridge.Tests/Domain/DonationLotStatusTests.cs` con `ChangeStatus_WhenAvailableToPickedUp_Conflicts` exigiendo `ConflictException`, `ChangeStatus_WhenClaimedToPickedUp_Succeeds`, cancelaciones legales y rechazo de cambios desde estados terminales; ejecutar primero la prueba ilegal y registrar el rojo.

- id: domain-status-impl
  status: completed
  content: Crear `backend/MealBridge.Domain/Donations/DonationStatusTransitions.cs` e implementar `ChangeStatus` en `DonationLot.cs` con exactamente la tabla del reto, `ConflictException` para transiciones ilegales, mensajes constantes en español y actualización de `updatedAt`.

- id: application-create-test-red
  status: completed
  content: Crear `backend/MealBridge.Tests/Application/CreateDonationTests.cs` con un repositorio doble y una prueba del caso de uso que exija persistir y devolver el lote creado; ejecutar la prueba enfocada y confirmar el rojo.

- id: application-create-impl
  status: completed
  content: Crear `backend/MealBridge.Application/Donations/Ports/IDonationRepository.cs`, `Commands/CreateDonationCommand.cs` y `UseCases/CreateDonation.cs`; coordinar el dominio y la persistencia sin duplicar validaciones.

- id: application-query-test-red
  status: completed
  content: Crear `backend/MealBridge.Tests/Application/DonationQueryTests.cs` para lista vacía, filtros combinados por status/categoría, obtención existente e id desconocido; ejecutar cada prueba nueva antes de implementar su comportamiento.

- id: application-query-impl
  status: completed
  content: Crear `backend/MealBridge.Application/Donations/Queries/DonationFilters.cs`, `UseCases/ListDonations.cs` y `UseCases/GetDonation.cs`; delegar las consultas al puerto, devolver resultados tipados y lanzar `NotFoundException` con mensaje constante en español para un id inexistente.

- id: application-workflow-test-red
  status: completed
  content: Crear `backend/MealBridge.Tests/Application/DonationWorkflowTests.cs` para reclamar y cambiar estado mediante casos de uso, incluyendo not-found y conflicto; ejecutar las pruebas enfocadas y confirmar el rojo.

- id: application-workflow-impl
  status: completed
  content: Crear `backend/MealBridge.Application/Donations/UseCases/ClaimDonation.cs` y `ChangeDonationStatus.cs`; coordinar repositorio, transacción y métodos del dominio, sin traducir todavía a códigos HTTP.

- id: persistence-test-red
  status: completed
  content: Crear `backend/MealBridge.Tests/Infrastructure/PostgresDonationRepositoryTests.cs` para persistencia después de recrear el contexto, filtros y dos reclamos concurrentes; ejecutar contra el PostgreSQL local de prueba y confirmar el fallo antes del repositorio.

- id: persistence-impl
  status: completed
  content: Crear `backend/MealBridge.Infrastructure/Persistence/MealBridgeDbContext.cs`, `DonationLotConfiguration.cs` y `Repositories/PostgresDonationRepository.cs`; mapear todos los campos y usar una operación condicional o transacción que impida dos reclamos exitosos.

- id: persistence-migration
  status: completed
  content: Registrar PostgreSQL en `backend/MealBridge.Infrastructure/DependencyInjection.cs` y `backend/MealBridge.Api/Program.cs`; crear la migración `InitialCreate` bajo `backend/MealBridge.Infrastructure/Persistence/Migrations/` y aplicarla al contenedor local.

- id: api-contract-test-red
  status: completed
  content: Crear `backend/MealBridge.Tests/Api/DonationEndpointsTests.cs` con WebApplicationFactory para comprobar envelope, camelCase y respuestas 201/200/400/404/409 de las cinco rutas; ejecutar primero un caso por ruta y confirmar el rojo.

- id: api-contract-impl
  status: completed
  content: Crear DTOs en `backend/MealBridge.Api/Contracts/Donations/`, `backend/MealBridge.Api/Controllers/DonationsController.cs` y el mapeo central de errores; mantener controllers delgados, devolver siempre el envelope, serializar enums exactamente como `bakery`, `produce`, `dairy`, `prepared`, `other`, `portions`, `kg`, `loaves`, `boxes`, `available`, `claimed`, `picked_up`, `cancelled`, `expired`, y definir errores en español mediante constantes en su clase propietaria.

- id: backend-env-loading
  status: completed
  content: Ajustar `backend/MealBridge.Api/Program.cs` y centralizar en `backend/MealBridge.Tests/` la carga segura del `.env` gitignored desde la raíz; comprobar desde un proceso sin `ConnectionStrings__MealBridge` previo con `dotnet test backend/MealBridge.sln --no-restore`, `dotnet build backend/MealBridge.sln --no-restore` y una consulta Docker a `__EFMigrationsHistory`.

- id: api-malformed-json-envelope
  status: completed
  content: Agregar en `backend/MealBridge.Tests/Api/DonationEndpointsTests.cs` una prueba HTTP con JSON malformado que exija `400` y el envelope `{ succeeded, data, error }`; corregir la configuración API únicamente si la prueba demuestra una desviación y ejecutar la suite completa.

- id: frontend-api-client
  status: completed
  content: Crear `frontend/src/api/types.ts` y `frontend/src/api/donations.ts` con tipos camelCase, lectura de `VITE_API_URL`, filtros como query params y un desempaquetador que valide HTTP y `succeeded`.

- id: frontend-dashboard
  status: completed
  content: Reemplazar la pantalla de Vite en `frontend/src/App.tsx` y crear componentes bajo `frontend/src/features/donations/` para listar y filtrar lotes, mostrando estados loading, empty, filtered-empty y error.

- id: frontend-create
  status: completed
  content: Crear `frontend/src/features/donations/CreateDonationForm.tsx` con todos los campos del donante, labels, validación básica, submit deshabilitado durante la petición, error 400 visible y refresco de la lista al crear.

- id: frontend-workflow
  status: completed
  content: Crear `frontend/src/features/donations/DonationDetail.tsx` y `ClaimDonationForm.tsx` para reclamar, marcar `picked_up` o cancelar, ocultar acciones ilegales y mostrar errores 404/409 sin perder la pantalla.

- id: frontend-feedback
  status: completed
  content: Ajustar `frontend/src/App.css` y los componentes de donaciones para mostrar confirmaciones, estados ocupados y un layout utilizable a 1280 px sin agregar un design system.

- id: documentation
  status: completed
  content: Actualizar `README.md` con comandos ya comprobados de variables de entorno, migraciones, API, frontend y pruebas; completar `AI-USE.md` con decisiones reales, prompts y errores detectados.

- id: api-create-status-test-red
  status: completed
  content: Agregar en `backend/MealBridge.Tests/Api/DonationEndpointsTests.cs` una prueba que envíe `status` en `POST /api/donations`, exija `400` con envelope y confirme el rojo actual antes de modificar producción.

- id: api-create-status-impl
  status: completed
  content: Ajustar `backend/MealBridge.Api/Contracts/Donations/CreateDonationRequest.cs` y `Controllers/DonationsController.cs` para rechazar explícitamente `status` con un mensaje constante en español; dejar verde la prueba enfocada.

- id: api-contract-regression-tests
  status: completed
  content: Agregar en `backend/MealBridge.Tests/Api/DonationEndpointsTests.cs` comprobaciones para filtros desconocidos y `PATCH status: claimed`, exigiendo `400`/`409` con envelope; ejecutar la suite API.

- id: scaffold-cleanup
  status: completed
  content: Eliminar `Class1.cs`, assets Vite y `MealBridge.Api.http` sin referencias; limpiar reglas residuales de `frontend/src/index.css` y ajustar idioma/título en `frontend/index.html`; validar build y lint.

- id: alignment-documentation
  status: completed
  content: Corregir `README.md` y `AI-USE.md` para enlazar el guion real, registrar las correcciones y no afirmar evidencia UI que no se haya ejecutado.

- id: residual-cleanup
  status: completed
  content: Eliminar `frontend/README.md` de plantilla y el hook CSS huérfano `create-panel` en `frontend/src/features/donations/CreateDonationForm.tsx`; conservar ambos briefs originales como fuentes del reto y comprobar build/lint.

- id: frontend-floating-form
  status: completed
  content: Reorganizar `frontend/src/features/donations/CreateDonationForm.tsx` y `frontend/src/App.css` en una grilla responsive con etiquetas flotantes accesibles; comprobar visualmente en navegador y ejecutar build/lint.

- id: frontend-minimal-design
  status: completed
  content: Refinar `frontend/src/index.css` y `frontend/src/App.css` con una estética minimalista moderna, superficies neutras, un único acento verde y menor peso visual; conservar accesibilidad, responsive y comprobar navegador/build/lint.

- id: frontend-grayscale-palette
  status: completed
  content: Sustituir en `frontend/src/index.css` y `frontend/src/App.css` la paleta cromática por negro, blanco y grises; mantener contraste, estados distinguibles y comprobar navegador/build/lint.

- id: frontend-white-contrast
  status: completed
  content: Fijar en `frontend/src/index.css` un fondo blanco y superficies grises claras con contraste alto, eliminando la activación automática del modo oscuro; comprobar navegador/build/lint.

- id: frontend-spanish-validation
  status: completed
  content: Agregar validación nativa en español compartida en `frontend/src/features/donations/formValidation.ts` y conectarla a los campos requeridos de creación y reclamo; comprobar mensajes en navegador/build/lint.

- id: frontend-visual-hierarchy
  status: completed
  content: Reforzar en componentes de donaciones y `frontend/src/App.css` la jerarquía de títulos, botones y separación visible de creación, filtros, resultados y detalle sin perder minimalismo, accesibilidad ni responsive; comprobar navegador/build/lint.

- id: frontend-card-semantics
  status: completed
  content: Mejorar `DonationCard.tsx` y `frontend/src/App.css` con estados semánticos por color, título de producto destacado y bloques internos claros para categoría, recogida, disponibilidad y coordinador; comprobar navegador/build/lint.

- id: frontend-product-title-black
  status: completed
  content: Cambiar en `frontend/src/App.css` el título del producto de azul a negro manteniendo tamaño, peso y jerarquía; comprobar navegador/build/lint.

- id: verify
  status: completed
  content: Ejecutar `dotnet test backend/MealBridge.sln`, `dotnet build backend/MealBridge.sln`, `npm run build --prefix frontend` y `npm run lint --prefix frontend`; iniciar PostgreSQL/API/frontend, ejecutar los diez pasos del guion de `MVP-SCOPE.md`, reiniciar la API y registrar resultados reales en `AI-USE.md`.
```

## Regla de iteración

1. Seleccionar únicamente la primera tarea `pending`.
2. Si termina en `-test-red`, escribir y ejecutar solo la prueba indicada.
3. Confirmar que falla por la razón esperada y registrar la evidencia.
4. Cambiar esa tarea a `completed`.
5. Ejecutar inmediatamente su pareja `-impl` con el mínimo cambio necesario.
6. Ejecutar la prueba enfocada y después la suite relacionada.
7. Aplicar la validación final de `.agents/skills/implementation/SKILL.md` contra brief, RF, RNF y `MVP-SCOPE.md`.
8. Si una tarea resulta demasiado grande, dividirla antes de escribir código; cada subtarea debe nombrar una ruta y un resultado verificable.
9. No comenzar una tarea posterior mientras la actual tenga errores, desviaciones o validaciones pendientes.
