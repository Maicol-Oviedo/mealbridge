# Uso de IA en MealBridge

Esta bitácora registra el uso real de herramientas de IA durante el reto. Se actualizará después de cada ciclo TDD y de cada decisión relevante.

## Tools

- **Editor:** Cursor.
- **Modelo:** GPT-5.6 Sol.
- **Agente:** agente de programación de Cursor con acceso al repositorio y terminal local.
- **Skills del proyecto:**
  - `.agents/skills/planning/SKILL.md`
  - `.agents/skills/tdd/SKILL.md`
- **Regla persistente:** `.cursor/rules/mealbridge-tdd.mdc`.
- **Comando reutilizable:** `.cursor/commands/tdd-implement.md`.
- **Backend:** .NET 10 SDK, ASP.NET Core, Entity Framework Core, Npgsql y xUnit.
- **Frontend:** React, TypeScript, Vite, npm y Oxlint.
- **Base de datos:** PostgreSQL en Docker Desktop.
- **MCP, RAG y embeddings:** no utilizados.

## Decisions

### PostgreSQL en lugar de SQLite

Se eligió PostgreSQL porque fue una decisión explícita del usuario. Se reutiliza un contenedor local de Docker Desktop publicado en `localhost:5455`, evitando instalar otro servidor o depender de la nube.

### ASP.NET Core y React

Se aceptó el stack recomendado por el reto: ASP.NET Core/.NET para la API y React con TypeScript/Vite para el frontend. Este stack permite separar las responsabilidades de Clean Architecture y usar xUnit para demostrar TDD.

### Clean Architecture proporcional

Se crearon proyectos separados para `Api`, `Application`, `Domain`, `Infrastructure` y `Tests`, pero se mantiene un solo host HTTP. No se añadieron CQRS, mediator, event sourcing ni microservicios.

### Herramientas locales

Se instaló .NET 10, Npgsql, Entity Framework Core, Swagger y `dotnet-ef`. PostgreSQL, API y frontend se ejecutan localmente, sin cuentas cloud.

### Alcance limitado al MVP

No se implementarán autenticación, colas, RAG, embeddings, almacenamiento de archivos ni streaming antes de terminar el MVP y dejar sus pruebas verdes.

### Expiración explícita

El cálculo automático de `expired` se omite en este MVP, como permite el brief. La transición explícita `available → expired` permanece protegida por el dominio.

### Mensajes como constantes locales

Los mensajes de logs, respuestas, excepciones y validaciones se declararán como constantes dentro de la clase que los utiliza. Se evita tanto el literal hardcoded en llamadas como una clase global de mensajes sin relación de dominio.

### Concurrencia optimista para reclamar

PostgreSQL y EF Core usan el estado persistido como token de concurrencia. El dominio rechaza un lote que ya no está `available` y el repositorio traduce una escritura concurrente a `ConflictException`; así un segundo reclamo nunca sobrescribe `claimedBy`.

### Cliente frontend centrado en el envelope

El cliente React comprueba simultáneamente el código HTTP y `succeeded`. Conserva el estado HTTP en `ApiError`, rechaza respuestas que no cumplan `{ succeeded, data, error }` y usa únicamente `VITE_API_URL` como base configurable.

## TDD Evidence

La prueba de plantilla `UnitTest1` fue eliminada porque no demostraba ningún requisito.

### Create_WhenValid_ReturnsAvailableLot

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenValid_ReturnsAvailableLot"`
- **Red result:** fallo esperado `CS0234`; `MealBridge.Domain.Donations` todavía no existe. La prueba no falló por configuración ni por una dependencia externa.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenValid_ReturnsAvailableLot"`
- **Green result:** 1 prueba superada; la suite completa también terminó con 1 prueba superada.
- **Production files:** `backend/MealBridge.Domain/Donations/DonationLot.cs`, `FoodCategory.cs`, `DonationUnit.cs` y `DonationStatus.cs`.
- **Refactor:** ninguno; se conservó la implementación mínima exigida por la prueba.

### Create_WhenQuantityLessThanOne_Rejects

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenQuantityLessThanOne_Rejects"`
- **Red result:** fallo esperado de xUnit: no se lanzó `InvalidArgumentException` para `quantity: 0`.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenQuantityLessThanOne_Rejects"`
- **Green result:** prueba enfocada superada; suite completa 2/2 y build sin advertencias.
- **Production files:** `backend/MealBridge.Domain/Donations/DonationLot.cs`.
- **Refactor:** ninguno; se añadió únicamente la validación exigida.

### DonationLot text validation

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenBusinessName|FullyQualifiedName~Create_WhenTitle|FullyQualifiedName~Create_WhenDescription|FullyQualifiedName~Create_WhenPickupAddress"`
- **Red result:** 7 fallos esperados; no se lanzó `InvalidArgumentException` para campos obligatorios en blanco ni para longitudes mayores a 120, 80, 500 y 200.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenBusinessName|FullyQualifiedName~Create_WhenTitle|FullyQualifiedName~Create_WhenDescription|FullyQualifiedName~Create_WhenPickupAddress"`
- **Green result:** 7/7 pruebas enfocadas superadas; la descripción opcional nula también fue comprobada y la suite terminó 10/10.
- **Production files:** `backend/MealBridge.Domain/Donations/DonationLot.cs`.
- **Refactor:** validaciones centralizadas en `ValidateRequiredText` y `ValidateOptionalText`; sin comportamiento adicional.

### DonationLot enum validation

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenFoodCategory|FullyQualifiedName~Create_WhenDonationUnit"`
- **Red result:** 2 fallos esperados; no se lanzó `InvalidArgumentException` para valores indefinidos de `FoodCategory` y `DonationUnit`.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenFoodCategory|FullyQualifiedName~Create_WhenDonationUnit"`
- **Green result:** 2/2 pruebas enfocadas y suite completa 12/12.
- **Production files:** `backend/MealBridge.Domain/Donations/DonationLot.cs`.
- **Refactor:** validación compartida en `ValidateDefinedEnum<TEnum>`.

### Create_WhenAvailableUntilIsNotAfterAvailableFrom_Rejects

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenAvailableUntilIsNotAfterAvailableFrom_Rejects"`
- **Red result:** fallo esperado; no se lanzó `InvalidArgumentException` cuando ambas fechas eran iguales.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenAvailableUntilIsNotAfterAvailableFrom_Rejects"`
- **Green result:** prueba enfocada verde y suite completa 13/13.
- **Production files:** `backend/MealBridge.Domain/Donations/DonationLot.cs`.
- **Refactor:** ninguno; se implementó únicamente la comparación exigida.

### Create_WhenAvailabilityHasOffset_StoresUtcValues

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenAvailabilityHasOffset_StoresUtcValues"`
- **Red result:** fallo esperado; se conservó el offset `-05:00` en lugar de almacenar UTC.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Create_WhenAvailabilityHasOffset_StoresUtcValues"`
- **Green result:** prueba enfocada verde y suite completa 14/14.
- **Production files:** `backend/MealBridge.Domain/Donations/DonationLot.cs`.
- **Refactor:** ninguno; se normalizaron ambos valores sin alterar el instante.

### Claim_WhenAvailable_SetsClaimedByAndStatus

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Claim_WhenAvailable_SetsClaimedByAndStatus"`
- **Red result:** fallo esperado `CS1061`; `DonationLot.Claim` todavía no existe.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Claim_WhenAvailable_SetsClaimedByAndStatus"`
- **Green result:** prueba enfocada verde y suite completa 15/15.
- **Production files:** `backend/MealBridge.Domain/Donations/DonationLot.cs`.
- **Refactor:** se garantizó que `updatedAt` avance al menos un tick.

### Claim coordinator name validation

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Claim_WhenCoordinatorName"`
- **Red result:** 2 fallos esperados; no se lanzó `InvalidArgumentException` para nombre en blanco ni mayor a 120 caracteres.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Claim_WhenCoordinatorName"`
- **Green result:** 2/2 pruebas enfocadas y suite completa 17/17.
- **Production files:** `backend/MealBridge.Domain/Donations/DonationLot.cs`.
- **Refactor:** se reutilizó `ValidateRequiredText`.

### Claim_WhenAlreadyClaimed_Conflicts

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Claim_WhenAlreadyClaimed_Conflicts"`
- **Red result:** fallo esperado; el segundo reclamo no lanzó `ConflictException` y el comportamiento aún permite sobrescribir el reclamo.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Claim_WhenAlreadyClaimed_Conflicts"`
- **Green result:** prueba enfocada verde y suite completa 18/18.
- **Production files:** `backend/MealBridge.Domain/Donations/DonationLot.cs`.
- **Refactor:** ninguno; se añadió únicamente la guarda de estado.

### DonationLot status transitions

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~DonationLotStatusTests"`
- **Red result:** fallo esperado `CS1061`; `DonationLot.ChangeStatus` todavía no existe.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~DonationLotStatusTests"`
- **Green result:** 5/5 pruebas enfocadas y suite completa 23/23.
- **Production files:** `DonationLot.cs` y `DonationStatusTransitions.cs`.
- **Refactor:** timestamp de mutación centralizado.

### Execute_WhenValid_PersistsAndReturnsCreatedLot

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Execute_WhenValid_PersistsAndReturnsCreatedLot"`
- **Red result:** fallo esperado `CS0234`/`CS0246`; todavía no existen los contratos ni el caso de uso de creación en `MealBridge.Application`.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~Execute_WhenValid_PersistsAndReturnsCreatedLot"`
- **Green result:** prueba enfocada verde; suite completa 24/24 y build sin advertencias.
- **Production files:** `IDonationRepository.cs`, `CreateDonationCommand.cs` y `CreateDonation.cs`.
- **Refactor:** ninguno; el caso de uso solo coordina dominio y persistencia.

### DonationQueryTests

- **Red commands:** cuatro ejecuciones enfocadas con filtros `List_WhenRepositoryIsEmpty_ReturnsEmptyList`, `List_WhenStatusAndCategoryAreProvided_CombinesFilters`, `Get_WhenDonationExists_ReturnsDonation` y `Get_WhenDonationDoesNotExist_ThrowsNotFound`.
- **Red result:** las cuatro ejecuciones fallaron por `CS0234`/`CS0246`; aún no existen `DonationFilters`, `ListDonations` ni `GetDonation`.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~DonationQueryTests"`
- **Green result:** 4/4 pruebas enfocadas; suite completa 28/28 y build sin advertencias.
- **Production files:** `DonationFilters.cs`, `ListDonations.cs`, `GetDonation.cs` e `IDonationRepository.cs`.
- **Refactor:** ninguno.

### DonationWorkflowTests

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~DonationWorkflowTests"`
- **Red result:** fallo esperado `CS0246`; `ClaimDonation` y `ChangeDonationStatus` todavía no existen.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~DonationWorkflowTests"`
- **Green result:** 6/6 pruebas enfocadas; suite completa 34/34 y build sin advertencias.
- **Production files:** `ClaimDonation.cs`, `ChangeDonationStatus.cs` e `IDonationRepository.cs`.
- **Refactor:** ninguno; los casos de uso delegan invariantes al dominio.

### PostgresDonationRepositoryTests

- **Red command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~PostgresDonationRepositoryTests"`
- **Red result:** después de corregir la firma de `IAsyncLifetime` en la prueba, el rojo válido fue `CS0234`/`CS0246` porque aún no existen `MealBridgeDbContext` ni `PostgresDonationRepository`.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~PostgresDonationRepositoryTests"`
- **Green result:** 3/3 pruebas contra PostgreSQL local; suite completa 37/37 y build sin advertencias.
- **Production files:** `MealBridgeDbContext.cs`, `DonationLotConfiguration.cs`, `PostgresDonationRepository.cs` y ajuste de versiones EF Core.
- **Refactor:** la comparación temporal de integración se ajustó a precisión de milisegundos por la precisión de PostgreSQL.

### DonationEndpointsTests

- **Red commands:** cinco ejecuciones enfocadas para crear, listar, consultar, reclamar y cambiar estado mediante `WebApplicationFactory`.
- **Red result:** las rutas devolvieron `404` o cuerpo vacío; después de sembrar directamente la base aislada para los workflows, claim y status fallaron específicamente porque sus rutas aún no existen.
- **Green command:** `& 'C:\Program Files\dotnet\dotnet.exe' test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~DonationEndpointsTests"`
- **Green result:** 5/5 pruebas HTTP reales en verde para las cinco rutas, envelope, camelCase, enums y códigos 201/200/400/404/409.
- **Production files:** DTOs bajo `Contracts/Donations`, `DonationsController.cs`, `ApiValidationResponseFactory.cs` y composición en `Program.cs`.
- **Refactor:** la fábrica de API reemplaza explícitamente el `DbContext` para aislar `mealbridge_api_tests`; se verificó que no elimina ni modifica la base principal.

## Compliance Checks

### domain-create-impl

- **Requisitos aplicables:** RF-01 (campos, UUID, estado inicial, reclamo nulo y timestamps), RNF-02, RNF-03, RNF-05 y alcance de creación del dominio.
- **Cumplimiento:** el corte implementado contiene exactamente los campos exigidos, genera UUID, crea `available`, mantiene `claimedBy`/`claimedAt` nulos y asigna timestamps UTC sin dependencias de HTTP o Infrastructure.
- **Evidencia:** build sin advertencias y `Create_WhenValid_ReturnsAvailableLot` verde; suite 1/1.
- **Pendiente en tareas posteriores:** validaciones, normalización UTC de la ventana, serialización exacta de enums, persistencia, API y frontend.
- **Alcance adicional:** ninguno.

### domain-quantity-validation-impl

- **Requisitos aplicables:** RF-01 (`quantity >= 1`), RNF-03, RNF-05, RNF-06 y alcance de validación del dominio.
- **Cumplimiento:** `DonationLot.Create` rechaza cantidades menores que uno mediante `InvalidArgumentException` y un mensaje constante en español.
- **Evidencia:** prueba enfocada verde, suite 2/2, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** las demás validaciones de texto, enums, ventana y UTC.
- **Alcance adicional:** ninguno.

### domain-text-validation-impl

- **Requisitos aplicables:** RF-01 para obligatoriedad, longitudes máximas y descripción opcional; RNF-03 y RNF-05.
- **Cumplimiento:** se rechazan blancos obligatorios y valores mayores a 120, 80, 500 y 200 mediante `InvalidArgumentException`; `description: null` continúa permitido.
- **Evidencia:** después del refactor, 8/8 pruebas de texto, suite 10/10, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** enums, ventana temporal y UTC.
- **Alcance adicional:** ninguno.

### domain-enum-validation-impl

- **Requisitos aplicables:** RF-01 para `foodCategory` y `unit`; RNF-03 y RNF-05.
- **Cumplimiento:** se rechazan valores fuera de ambos enums mediante `InvalidArgumentException`, con mensajes constantes en español.
- **Evidencia:** 2/2 pruebas enfocadas, suite 12/12, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** serialización JSON exacta, ventana temporal y UTC.
- **Alcance adicional:** ninguno.

### domain-window-validation-impl

- **Requisitos aplicables:** RF-01 para `availableUntil > availableFrom`; RNF-03 y RNF-05.
- **Cumplimiento:** ventanas iguales o invertidas se rechazan mediante `InvalidArgumentException` con mensaje constante en español.
- **Evidencia:** prueba enfocada verde, suite 13/13, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** normalización UTC.
- **Alcance adicional:** ninguno.

### domain-utc-impl

- **Requisitos aplicables:** RF-01 para fechas UTC; RNF-03 y RNF-05.
- **Cumplimiento:** ambas fechas se almacenan con offset cero conservando el instante original.
- **Evidencia:** prueba enfocada verde, suite 14/14, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** ninguno para la ventana de creación.
- **Alcance adicional:** ninguno.

### domain-claim-impl

- **Requisitos aplicables:** RF-05 para reclamo exitoso; RNF-03 y RNF-05.
- **Cumplimiento:** un lote disponible cambia a `claimed`, asigna `claimedBy`/`claimedAt` y actualiza `updatedAt`.
- **Evidencia:** prueba enfocada verde, suite 15/15, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** validación del coordinador y conflicto de segundo reclamo.
- **Alcance adicional:** ninguno.

### domain-claim-name-validation-impl

- **Requisitos aplicables:** RF-05 para `coordinatorName` obligatorio y 1–120 caracteres; RNF-03 y RNF-05.
- **Cumplimiento:** nombres en blanco o mayores a 120 se rechazan mediante `InvalidArgumentException` y mensajes constantes en español.
- **Evidencia:** 2/2 pruebas enfocadas, suite 17/17, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** conflicto de segundo reclamo.
- **Alcance adicional:** ninguno.

### domain-claim-conflict-impl

- **Requisitos aplicables:** RF-05 para segundo reclamo `409`; RNF-03 y RNF-05.
- **Cumplimiento:** un lote no disponible lanza `ConflictException` antes de modificar coordinador o timestamps.
- **Evidencia:** prueba enfocada verde, suite 18/18, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** atomicidad en PostgreSQL.
- **Alcance adicional:** ninguno.

### application-create-impl

- **Requisitos aplicables:** RF-01, RF-07, RNF-02, RNF-03 y RNF-05.
- **Cumplimiento:** Application crea mediante el dominio, persiste por un puerto interno y devuelve la misma entidad sin duplicar validaciones ni depender de Infrastructure.
- **Evidencia:** prueba enfocada verde, suite 24/24, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** consultas, workflows, PostgreSQL y HTTP.
- **Alcance adicional:** ninguno.

### application-query-impl

- **Requisitos aplicables:** RF-02, RF-03, RF-04, RNF-02 y RNF-05.
- **Cumplimiento:** Application delega filtros combinables al puerto, devuelve listas tipadas y traduce la ausencia de un lote a `NotFoundException` con mensaje constante en español.
- **Evidencia:** 4/4 pruebas enfocadas, suite 28/28, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** validación HTTP de enums e id malformado, implementación PostgreSQL.
- **Alcance adicional:** ninguno.

### application-workflow-impl

- **Requisitos aplicables:** RF-05, RF-06, RNF-02, RNF-03 y RNF-05.
- **Cumplimiento:** los casos de uso cargan el lote, lanzan `NotFoundException` si falta, invocan `Claim`/`ChangeStatus` del dominio y persisten el resultado.
- **Evidencia:** 6/6 pruebas enfocadas, suite 34/34, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** atomicidad por concurrencia en PostgreSQL y traducción HTTP.
- **Alcance adicional:** ninguno.

### persistence-impl

- **Requisitos aplicables:** RF-02, RF-03, RF-04, RF-05, RF-07, RNF-02, RNF-04 y RNF-07.
- **Cumplimiento:** EF Core/Npgsql persiste todos los campos, aplica ambos filtros con AND y usa `status` como token de concurrencia; un conflicto de escritura se traduce a `ConflictException`.
- **Evidencia:** 3/3 pruebas reales en PostgreSQL, incluida doble reclamación concurrente; suite 37/37, build sin advertencias y lint limpio.
- **Pendiente en tareas posteriores:** migración versionada, DI y contrato HTTP.
- **Alcance adicional:** base aislada `mealbridge_tests`, creada y eliminada por las pruebas.

### persistence-migration

- **Requisitos aplicables:** RF-07, RNF-01, RNF-02, RNF-04 y RNF-07.
- **Cumplimiento:** Infrastructure registra `MealBridgeDbContext` y el repositorio; se generó `InitialCreate` y se aplicó al PostgreSQL local sin guardar la conexión.
- **Evidencia:** `dotnet ef database update` aplicó `20260817231447_InitialCreate`; `dotnet ef migrations list` la mostró aplicada y el build terminó sin advertencias.
- **Pendiente en tareas posteriores:** composición de casos de uso y rutas HTTP.
- **Desviaciones corregidas:** se agregó `Microsoft.EntityFrameworkCore.Design` al host y se alineó EF Core a 10.0.4 con el proveedor Npgsql para eliminar un conflicto de ensamblados.

### api-contract-impl

- **Requisitos aplicables:** RF-01 a RF-07, RF-10, RNF-01 a RNF-04, RNF-07 y RNF-08.
- **Cumplimiento:** una sola API expone exactamente las cinco rutas; controllers delgados coordinan Application, todas las respuestas JSON usan envelope, los errores de negocio se traducen centralmente y enums/respuestas usan camelCase y snake_case exacto cuando corresponde.
- **Evidencia:** pruebas HTTP 5/5; `dotnet test backend/MealBridge.sln` 42/42; `dotnet build backend/MealBridge.sln` sin advertencias ni errores; lints limpios.
- **Persistencia:** `InitialCreate` aplicada a `mealbridge`; una segunda actualización informó que la base ya estaba al día después de ejecutar las pruebas aisladas.
- **Pendiente en tareas posteriores:** frontend y documentación, fuera del corte solicitado.
- **Alcance adicional:** ninguno.

### api-malformed-json-envelope

- **Requisito aplicable:** RF-10 y contrato §2.1: toda respuesta JSON usa `{ succeeded, data, error }`.
- **Comprobación:** `PostDonations_WhenJsonIsMalformed_Returns400Envelope`.
- **Resultado:** la prueba pasó desde su primera ejecución; `InvalidModelStateResponseFactory` ya cubría errores de deserialización, por lo que no se modificó producción.
- **Evidencia:** prueba enfocada verde, suite 43/43, build sin advertencias y lint limpio.
- **Alcance adicional:** ninguno.

### frontend-api-client

- **Requisitos aplicables:** RF-01 a RF-06 y RF-10; RNF-01 y RNF-08; cliente recomendado en §3.3 del reto.
- **Implementación:** tipos camelCase del contrato y cliente para las cinco rutas en `frontend/src/api/`, con filtros como query params y `VITE_API_URL`.
- **Validación del contrato:** se comprueban tanto el código HTTP como `succeeded`; las respuestas sin envelope o sin `data` exitoso se rechazan.
- **Errores:** `ApiError` conserva el estado HTTP y expone mensajes legibles en español; también cubre URL ausente, red fallida y JSON de respuesta inválido.
- **Evidencia:** `npm run build --prefix frontend` y `npm run lint --prefix frontend` finalizaron correctamente; diagnósticos del editor limpios.
- **Desviación corregida:** el primer build detectó `TS1294` por una propiedad de parámetro incompatible con `erasableSyntaxOnly`; se reemplazó por una propiedad explícita y el siguiente build quedó verde.
- **Pendiente de tareas posteriores:** consumir el cliente desde tablero, formulario y flujo de detalle; no se agregó UI en este corte.

### frontend-dashboard

- **Requisitos aplicables:** RF-02, RF-03 y RF-08; RNF-01 y RNF-09; frontend §3.1 y estados UX §3.2 del reto.
- **Implementación:** `App.tsx` ahora muestra un tablero React con componentes separados para filtros, lista y tarjetas bajo `frontend/src/features/donations/`.
- **Estados comprobados en código:** carga, lista vacía inicial, resultado vacío con filtros y error legible con acción de reintento.
- **Filtros:** `status` y `foodCategory` se combinan mediante el cliente existente, que los envía como query params al único endpoint de listado.
- **Desviación corregida:** el primer build no contenía `VITE_API_URL` porque Vite buscaba `.env` dentro de `frontend/`; se configuró `envDir` hacia la raíz y se confirmó que el artefacto final incorporó la URL local configurada.
- **Evidencia:** `npm run build --prefix frontend` y `npm run lint --prefix frontend` finalizaron correctamente; diagnósticos del editor limpios.
- **Pendiente de tareas posteriores:** formulario de creación, detalle y acciones de workflow, confirmaciones y estilos finales.
- **Alcance adicional:** ninguno; no se agregaron rutas, autenticación, polling ni acciones de estado.

### frontend-create

- **Requisitos aplicables:** RF-01 y RF-08; RNF-09; pantalla de creación y estados UX de §3.1–3.2 del reto.
- **Implementación:** formulario con los nueve campos de entrada permitidos, etiquetas, límites HTML, cantidad mínima y validación de la ventana temporal.
- **Contrato:** las fechas locales se convierten a ISO-8601 UTC, no se envía `status` y los errores del envelope `400` permanecen visibles.
- **Interacción:** el botón se deshabilita durante la solicitud; al crear, se limpia el formulario, se eliminan filtros y se recarga la lista para mostrar el lote nuevo.
- **Evidencia:** build y lint del frontend correctos; diagnósticos del editor limpios.
- **Pendiente:** la confirmación explícita de éxito y el estilo final corresponden a `frontend-feedback`.
- **Alcance adicional:** ninguno.

### frontend-workflow

- **Requisitos aplicables:** RF-04, RF-05, RF-06, RF-08 y RF-09; detalle/workflow de §3.1–3.2 del reto.
- **Implementación:** selección de una tarjeta, consulta del lote por UUID y panel de detalle persistente ante errores.
- **Acciones válidas:** `available` muestra únicamente el formulario de reclamo; `claimed` muestra únicamente recoger y cancelar; los estados terminales no muestran acciones.
- **Errores:** los mensajes `404` y `409` del envelope se muestran con su estado HTTP sin reemplazar la aplicación por una pantalla vacía.
- **Concurrencia UI:** los botones de reclamo y cambio de estado quedan deshabilitados mientras su solicitud está en curso.
- **Sincronización:** una mutación actualiza el detalle y la lista, y después vuelve a consultar el listado.
- **Evidencia:** build y lint correctos; diagnósticos del editor limpios.
- **Alcance adicional:** no se agregaron rutas, autenticación ni transiciones.

### frontend-feedback

- **Requisitos aplicables:** RF-08 y RF-09; RNF-09; estados UX de §3.2 del reto.
- **Confirmaciones:** el tablero muestra mensajes de éxito distintos después de publicar, reclamar, recoger o cancelar.
- **Estados ocupados:** los botones conservan etiquetas de progreso y permanecen deshabilitados durante cada solicitud.
- **Layout:** `App.css` define paneles, formularios, tarjetas, detalle lateral y breakpoints; a 1280 px usa un ancho máximo de 1200 px y el detalle se distribuye en una segunda columna.
- **Legibilidad:** se añadieron estados visuales de éxito/error, foco visible, badges de estado y adaptación a una columna en pantallas menores.
- **Evidencia:** `npm run build --prefix frontend` y `npm run lint --prefix frontend` finalizaron correctamente; diagnósticos del editor limpios.
- **Pendiente:** la ejecución manual del guion completo y la comprobación visual con API activa pertenecen a `verify`.
- **Alcance adicional:** no se incorporó design system, routing, animaciones ni pulido fuera del MVP.

### documentation

- **Requisitos aplicables:** RNF-07, RNF-10 y RNF-11; artefactos de entrega del reto.
- **README:** se eliminó el estado obsoleto de esqueleto y se documentaron `.env`, PostgreSQL, migración existente, API, frontend, pruebas, build, lint y alternativa de ruta completa para .NET en Windows.
- **Bitácora:** se completaron decisiones de concurrencia y cliente HTTP, prompts de backend/frontend y errores reales de TypeScript, Vite, procesos duplicados y JSON en PowerShell.
- **Evidencia:** búsqueda de frases obsoletas sin coincidencias, lint del frontend correcto y diagnósticos del editor limpios.
- **Pendiente:** resultados finales de suite, build y guion manual pertenecen exclusivamente a `verify`.
- **Alcance adicional:** ninguno.

### verify anterior (servicios y contrato HTTP)

- **PostgreSQL:** `tp-challenge-mealbridge` activo en `localhost:5455`.
- **Backend:** `dotnet test backend/MealBridge.sln` terminó 43/43; `dotnet build backend/MealBridge.sln` terminó con 0 advertencias y 0 errores.
- **Frontend:** `npm run build --prefix frontend` y `npm run lint --prefix frontend` terminaron correctamente.
- **Servicios:** API inició en `http://localhost:5080`, frontend en `http://localhost:5173`; ambos respondieron HTTP `200`.
- **Guion 1–3:** se comprobaron `AGENTS.md`, arquitectura, skills, invariantes y pruebas obligatorias; no faltó ninguno de los ocho artefactos de entrega verificados.
- **Flujo HTTP 4–6:** se iniciaron ambos servicios, se creó `Lote del guion final` con HTTP `201`/`available` y el filtro combinado `available + bakery` encontró su UUID.
- **Flujo HTTP 7–9:** el primer reclamo devolvió `200` y `claimedBy: Banco de Alimentos Uno`; el segundo devolvió `409`/`succeeded: false`; la transición a `picked_up` devolvió `200`.
- **Flujo HTTP 10:** se detuvo la API, se inició de nuevo y `GET` devolvió el mismo UUID en `picked_up` con el coordinador conservado.
- **Errores HTTP adicionales:** UUID malformado `400`, UUID inexistente `404` y transición desde `picked_up` `409`.
- **Límite de esta evidencia:** esta ejecución comprobó servicios y contrato HTTP, no interacción real del navegador; la verificación UI queda registrada únicamente cuando se ejecuta como tal.
- **Alcance adicional:** no se implementaron extensiones opcionales.

### PostDonations_WhenStatusIsProvided_Returns400Envelope

- **Red command:** `dotnet test backend/MealBridge.Tests/MealBridge.Tests.csproj --filter "FullyQualifiedName~PostDonations_WhenStatusIsProvided_Returns400Envelope"`.
- **Red result:** fallo esperado; la API devolvió `201 Created` porque `System.Text.Json` ignoró la propiedad `status`.
- **Requisito:** RF-01 exige rechazar que el cliente defina el estado de creación.
- **Green command:** el mismo filtro enfocado, seguido por `FullyQualifiedName~DonationEndpointsTests`.
- **Green result:** prueba enfocada 1/1 y suite API 7/7.
- **Production files:** `CreateDonationRequest.cs` captura propiedades adicionales y `DonationsController.cs` rechaza `status` mediante mensaje constante en español.

### Alineación final sin Git

- **Contrato corregido:** `POST /api/donations` con `status` devuelve `400`; filtros desconocidos devuelven `400`; `PATCH status: claimed` devuelve `409`. Todos conservan `{ succeeded, data, error }`.
- **Cobertura API:** las dos pruebas de regresión enfocadas pasaron 2/2 y `DonationEndpointsTests` pasó 9/9.
- **Limpieza:** se eliminaron tres `Class1.cs`, el archivo HTTP obsoleto y assets de plantilla sin referencias; `index.css`, idioma y título HTML quedaron propios de MealBridge.
- **Limpieza residual:** se eliminó `frontend/README.md` de Vite y el nombre de clase CSS sin regla `create-panel`. Se conservaron los briefs inglés y español porque son documentos fuente entregados por el usuario, no código duplicado del producto.
- **Documentación:** `README.md` enlaza el guion oficial de `MVP-SCOPE.md`; la evidencia anterior quedó identificada como flujo HTTP y no como interacción de navegador.
- **Backend final:** `dotnet test backend/MealBridge.sln` pasó 46/46; `dotnet build backend/MealBridge.sln --no-restore` terminó con 0 advertencias y 0 errores.
- **Frontend final:** `npm run build --prefix frontend` y `npm run lint --prefix frontend` terminaron correctamente.
- **Guion UI 4–9:** se automatizó Edge contra el frontend real en `localhost:5173` y la API real en `localhost:5080`. Se mostró un `400` de creación, se creó el lote, se filtró por `available` y `bakery`, dos páginas abrieron el mismo lote, el primer reclamo conservó `Banco de Alimentos Uno`, el segundo mostró `HTTP 409`, se marcó `picked_up` y desaparecieron las acciones ilegales.
- **Error 404 visible:** se verificó el renderizado `HTTP 404` en el detalle mediante una respuesta envelope interceptada; el `404` real de la API permanece cubierto por la prueba HTTP.
- **Guion UI 10:** se detuvo el proceso real de la API, se inició uno nuevo y Edge volvió a encontrar el mismo lote filtrado como `picked_up`, con `Banco de Alimentos Uno` y sin acciones ilegales.
- **Persistencia:** ningún paso del guion editó PostgreSQL manualmente.
- **Alcance:** la comparación final no encontró omisiones ni extensiones fuera de RF-01–RF-10, RNF-01–RNF-11 y `MVP-SCOPE.md`. La expiración automática continúa omitida y documentada según la opción permitida.
- **Exclusión solicitada:** no se inspeccionó ni validó estado, historial, autoría, remoto o entrega Git.

### Formulario con etiquetas flotantes

- **Solicitud:** reorganizar visualmente el formulario de creación y usar inputs con etiquetas flotantes.
- **Implementación:** `CreateDonationForm.tsx` conserva los mismos campos y contrato, añade etiquetas flotantes accesibles y organiza negocio/título, descripción, categoría/cantidad/unidad, dirección y fechas en grupos visuales.
- **Responsive:** `App.css` usa una grilla de seis columnas en escritorio y una columna en pantallas pequeñas, con estados hover, foco y campos persistentes para selects y fechas.
- **Evidencia:** `npm run build --prefix frontend` y `npm run lint --prefix frontend` finalizaron correctamente; la vista se comprobó en el navegador de Cursor con frontend y API activos.
- **Alineación:** el cambio es exclusivamente de presentación y no añade campos, endpoints ni comportamiento fuera de RF-03 y RNF-09.

### Diseño minimalista moderno

- **Solicitud:** reducir el peso visual y adoptar una estética minimalista moderna.
- **Implementación:** `index.css` define superficies neutras adaptables a claro/oscuro; `App.css` elimina el encabezado degradado, reduce sombras y radios, usa jerarquía tipográfica limpia y conserva un único acento verde.
- **Interacción:** foco, hover, feedback, estados y contraste permanecen visibles sin incorporar animaciones, librerías ni design system.
- **Evidencia:** build y lint del frontend correctos; comprobación visual en el navegador de Cursor con la aplicación activa.
- **Alineación:** refinamiento de presentación dentro de RNF-09; no modifica el alcance funcional.

### Paleta monocromática

- **Solicitud:** usar exclusivamente negro, blanco y grises.
- **Implementación:** `index.css` y `App.css` sustituyen verdes y colores semánticos por una escala neutral adaptable a modo claro y oscuro.
- **Legibilidad:** éxito, error y estados continúan diferenciándose mediante texto, contraste, bordes y densidad tonal, sin depender de color cromático.
- **Evidencia:** búsqueda de colores CSS, build, lint y comprobación visual en navegador confirmaron una paleta completamente monocromática.

### Fondo blanco con contraste

- **Solicitud:** usar fondo blanco y matices grises claramente visibles.
- **Implementación:** `index.css` fija `color-scheme: light`, fondo blanco, texto casi negro, superficies gris claro y bordes con mayor contraste; se eliminó la activación automática del modo oscuro.
- **Evidencia:** build, lint y comprobación visual en navegador correctos.

### Validación nativa en español

- **Solicitud:** mostrar en español las advertencias nativas de los inputs.
- **Implementación:** `formValidation.ts` centraliza mensajes constantes para campos requeridos, cantidad mínima, número, fecha y valor inválido; creación y reclamo limpian el mensaje al editar.
- **Evidencia:** build y lint correctos; al enviar el formulario vacío, `validationMessage` del primer campo devolvió `Completa este campo.` en el navegador.

### Jerarquía visual y secciones

- **Solicitud:** destacar títulos y botones y hacer distinguibles las secciones sin perder minimalismo, claridad ni funcionalidad.
- **Implementación:** creación, filtros, resultados y detalle usan jerarquía propia, bordes superiores, divisores y sombras sutiles; filtros y resultados incorporan títulos visibles mediante constantes.
- **Botones:** mayor contraste, peso tipográfico, sombra breve y estados hover/foco conservando la paleta monocromática.
- **Evidencia:** build y lint correctos; comprobación visual completa confirmó títulos, filtros, tarjetas y acciones claramente separados.
- **Alineación:** mejora RNF-09 y mantiene intactos flujo, contrato y responsive.

### Estados semánticos y detalle del lote

- **Solicitud:** resaltar estados por significado y hacer más reconocibles producto, subtítulos y secciones internas.
- **Estados:** disponible usa azul, reclamado ámbar, recogido verde, cancelado rojo y expirado gris; cada badge conserva texto, punto e identidad visual.
- **Tarjeta:** título del producto azul, más grande y con peso 800; negocio como subtítulo superior; descripción con contraste propio.
- **Datos:** categoría/cantidad, recogida, disponibilidad y coordinador se agrupan en bloques con fondo, borde, etiquetas en mayúsculas y valores destacados.
- **Evidencia:** build y lint correctos; navegador confirmó `picked_up` verde, regla roja de cancelado, título azul y bloques internos claros.
- **Alineación:** mejora de presentación de RF-04/RNF-09 sin modificar datos ni acciones.

### Título de producto negro

- **Solicitud:** reemplazar el azul del título del producto por negro.
- **Resultado:** se mantuvieron tamaño y peso destacados, usando `#09090b`; build, lint y color renderizado verificados.

## Prompts

### 1. Análisis del reto

- **Prompt:** “Analiza completamente el reto y dime de qué trata y qué hay que desarrollar”.
- **Resultado:** Se identificaron el producto MealBridge, MVP, contrato HTTP, arquitectura, TDD, artefactos de IA y guion de demostración.
- **Decisión:** Se aceptó mantener el alcance fijo y priorizar el corte vertical obligatorio.

### 2. Especificación de requisitos

- **Prompt:** “Crea archivos separados de requisitos funcionales, requisitos no funcionales y alcance solo del MVP, sin inventar”.
- **Resultado:** Se crearon `FUNCTIONAL-REQUIREMENTS.md`, `NON-FUNCTIONAL-REQUIREMENTS.md` y `MVP-SCOPE.md`.
- **Decisión:** Se conservaron únicamente requisitos presentes en el brief y se separaron las extensiones del MVP.

### 3. Selección de tecnologías

- **Prompt:** “Usar PostgreSQL; para las otras tecnologías está bien”.
- **Resultado:** Se mantuvieron React/TypeScript/Vite y ASP.NET Core/.NET, sustituyendo la recomendación inicial de SQLite por PostgreSQL.
- **Decisión:** Se aceptó PostgreSQL mediante el contenedor existente de Docker Desktop.

### 4. Preparación del proyecto

- **Prompt:** “Descarga dependencias y lo necesario para correr backend y frontend”.
- **Resultado:** Se creó la solución .NET con las cuatro responsabilidades, proyecto xUnit, frontend Vite, dependencias PostgreSQL/EF Core, Swagger y herramienta local `dotnet-ef`.
- **Decisión:** Se preparó solo el esqueleto ejecutable; no se implementó comportamiento de MealBridge antes de crear las skills y reglas exigidas.

### 5. Planeación precisa

- **Prompt:** “Genera el README, tareas puntuales en orden y una skill de planning con tareas bien especificadas para iterarlas”.
- **Resultado:** Se crearon `README.md`, `IMPLEMENTATION-PLAN.md` y `.agents/skills/planning/SKILL.md`.
- **Decisión:** Cada tarea debe nombrar rutas, una acción concreta, verificación y parejas `*-test-red`/`*-impl`.

### 6. Skill de TDD

- **Prompt:** “Crea la skill TDD”.
- **Resultado:** Se creó `.agents/skills/tdd/SKILL.md` con Red → Green → Refactor, pruebas obligatorias, comandos enfocados y anti-patrones.
- **Decisión:** Ningún comportamiento de Domain o Application se implementará sin un rojo válido registrado previamente.

### 7. Reglas de implementación

- **Prompt:** “Crea una skill de implementación; ningún mensaje hardcoded y todos los mensajes como constantes en la misma clase”.
- **Resultado:** Se creó `.agents/skills/implementation/SKILL.md` y se reemplazaron los literales de `GlobalExceptionHandler` por una constante privada reutilizada.
- **Decisión:** Cada clase será propietaria de sus mensajes y templates de logging.

### 8. Excepciones de negocio

- **Prompt:** “Crea un paquete con solo las excepciones de negocio necesarias e intercéptalas en el handler según las especificaciones”.
- **Resultado:** Se crearon excepciones para argumento inválido, recurso no encontrado y conflicto; el handler las traduce a `400`, `404` y `409`.
- **Decisión:** No se agregaron excepciones de autorización, duplicado o técnica porque no representan categorías adicionales necesarias para el MVP.

### 9. Implementación backend por TDD

- **Prompt:** “Avanza hasta `api-contract-impl`, sigue cada tarea, documenta cada paso y verifica alineación completa”.
- **Resultado:** Se implementaron dominio, casos de uso, repositorio PostgreSQL, migración y las cinco rutas mediante los ciclos registrados.
- **Decisión:** Cada corte se comparó por separado con brief, RF, RNF y alcance; no se afirmó conformidad completa antes de la verificación final.

### 10. Envelope para JSON malformado

- **Prompt:** “Asegurar el envelope del JSON malformado; toda respuesta JSON exitosa o fallida usa `{ succeeded, data, error }`”.
- **Resultado:** Se agregó una prueba HTTP enfocada que pasó con la fábrica de respuesta de validación ya configurada.
- **Decisión:** No se modificó producción porque la evidencia demostró que el comportamiento ya era correcto.

### 11. Implementación frontend

- **Prompt:** “Continúa desde `frontend-api-client` hasta `frontend-feedback`, documentando cada tarea”.
- **Resultado:** Se construyeron cliente tipado, tablero, filtros, formulario, detalle, reclamo, cambios de estado, feedback y layout adaptable.
- **Decisión:** Se mantuvo una sola página sin routing ni design system, como permite el MVP.

### 12. Ejecución local y datos de demostración

- **Prompt:** “Dame los comandos para correr la aplicación y genera cinco lotes sin reclamarlos ni cerrarlos”.
- **Resultado:** Se comprobó `/health` en `5080`, se corrigió una instancia duplicada y se crearon cinco lotes ficticios en estado `available`.
- **Decisión:** Los datos se insertaron por la API pública; no se editó PostgreSQL manualmente ni se aplicaron transiciones.

## What broke

### Uso de `&&` en PowerShell

- **Qué ocurrió:** El primer comando generado para crear backend y frontend encadenó instrucciones con `&&`.
- **Por qué falló:** La versión de PowerShell disponible no admite `&&` como separador.
- **Corrección:** Se reemplazó por comandos secuenciales y comprobaciones explícitas de `$LASTEXITCODE`.
- **Aprendizaje:** Los comandos del proyecto deben escribirse y verificarse para PowerShell, que es la shell declarada del entorno.

### PATH de .NET sin actualizar

- **Qué ocurrió:** `winget` indicó que .NET 10 estaba instalado, pero la terminal abierta no reconocía `dotnet`.
- **Corrección:** Se verificó la instalación mediante `C:\Program Files\dotnet\dotnet.exe`; el README indica abrir una terminal nueva si el PATH continúa desactualizado.

### Pruebas dependían de una variable heredada

- **Qué ocurrió:** después de eliminar `ConnectionStrings__MealBridge` del proceso, `dotnet test backend/MealBridge.sln --no-restore` terminó con 8 fallos y 34 pruebas superadas. `PostgresDonationRepositoryTests` y `DonationApiFactory` exigían que la terminal hubiese exportado la conexión.
- **Corrección:** se agregó DotNetEnv 3.2.0, la API carga el `.env` gitignored mediante `NoClobber().TraversePath()` y `TestEnvironment` centraliza la misma carga para pruebas sin duplicar credenciales ni mensajes.
- **Green command:** `dotnet test backend/MealBridge.sln --no-restore`.
- **Green result:** 42/42 pruebas superadas desde un proceso sin la variable predefinida.
- **Build command:** `dotnet build backend/MealBridge.sln --no-restore`.
- **Build result:** compilación correcta, 0 advertencias y 0 errores.
- **API local:** `dotnet run --no-restore --project backend/MealBridge.Api/MealBridge.Api.csproj` inició desde la raíz en `http://localhost:5080`; `/health` devolvió `succeeded: true`.
- **Migración:** la consulta enviada por stdin con `$query = 'SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";'; $query | docker exec -i tp-challenge-mealbridge psql -U postgres -d mealbridge -tA` devolvió `20260817231447_InitialCreate`.
- **Qué se corrigió durante la verificación:** pasar el SQL directamente con `psql -c` perdió las comillas de identificadores en PowerShell; enviar el SQL por stdin conservó el escape correcto.

### TypeScript `erasableSyntaxOnly`

- **Qué ocurrió:** el primer build del cliente falló con `TS1294` por declarar `public readonly status` como propiedad de parámetro.
- **Corrección:** `ApiError` declara `status` explícitamente y lo asigna en el constructor.
- **Resultado:** build y lint del frontend quedaron verdes.

### Vite no encontraba el `.env` raíz

- **Qué ocurrió:** el primer artefacto compilado no contenía `VITE_API_URL` porque Vite buscaba variables dentro de `frontend/`.
- **Corrección:** `vite.config.ts` usa `envDir: '..'`; solo las variables con prefijo `VITE_` se exponen al cliente.
- **Comprobación:** el build final incorporó `http://localhost:5080`.

### Ensamblados bloqueados por una API duplicada

- **Qué ocurrió:** una segunda ejecución de `dotnet run` no pudo copiar DLLs porque `MealBridge.Api` con PID `8020` ya estaba ejecutándose.
- **Diagnóstico:** `netstat` confirmó que ese PID escuchaba en `5080`; `8020` era un identificador de proceso, no un puerto.
- **Corrección:** se detuvo la instancia anterior, se confirmó que `5080` quedó libre y se inició una sola API.

### JSON enviado desde Windows PowerShell

- **Qué ocurrió:** los primeros intentos de crear datos de demostración devolvieron `400`; una llamada de `curl` contenía barras invertidas literales y el cuerpo de PowerShell no garantizaba UTF-8.
- **Corrección:** se serializó con `ConvertTo-Json` y se enviaron bytes UTF-8 con `application/json; charset=utf-8`.
- **Resultado:** los cinco lotes de demostración fueron creados por la API y permanecieron `available`.

### `HttpClient.PatchAsync` no disponible

- **Qué ocurrió:** Windows PowerShell cargó una versión de `HttpClient` sin el método auxiliar `PatchAsync` durante el guion final.
- **Corrección:** se construyó `HttpRequestMessage` con `HttpMethod("PATCH")` y se envió mediante `SendAsync`.
- **Resultado:** la transición final devolvió `200` y persistió `picked_up`.

### Origen distinto durante el guion de navegador

- **Qué ocurrió:** la primera automatización abrió `127.0.0.1:5173`, un origen distinto de `localhost:5173`; CORS bloqueó las llamadas y no llegó el `POST`.
- **Corrección:** el navegador usó el origen configurado `http://localhost:5173`.
- **Resultado:** el guion UI y la comprobación tras reiniciar la API finalizaron correctamente.

### Proceso hijo al reiniciar la API

- **Qué ocurrió:** detener el proceso de `dotnet run` no cerró inmediatamente el proceso hijo que escuchaba en `5080`, por lo que el primer reinicio encontró el puerto ocupado.
- **Corrección:** se identificó el PID que realmente escuchaba mediante `netstat`, se detuvo y se comprobó el puerto antes de iniciar una única API.
- **Resultado:** la nueva instancia inició en `5080` y el lote conservó `picked_up`.
