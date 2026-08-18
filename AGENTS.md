# MealBridge Agent Guide

MealBridge es una aplicación local de coordinación de rescate de alimentos. Los donantes publican lotes de excedentes y los coordinadores los filtran, reclaman y actualizan hasta su recogida o cancelación.

## Fuente de verdad

Antes de implementar, leer:

1. `MVP-SCOPE.md`
2. `FUNCTIONAL-REQUIREMENTS.md`
3. `NON-FUNCTIONAL-REQUIREMENTS.md`
4. `IMPLEMENTATION-PLAN.md`

No agregar alcance que no aparezca en esos documentos.

## Stack

- Frontend: React, TypeScript y Vite en `frontend/`.
- Backend: ASP.NET Core y .NET 10 en `backend/`.
- Persistencia: PostgreSQL con Entity Framework Core y Npgsql.
- Pruebas: xUnit en `backend/MealBridge.Tests/`.
- API local: `http://localhost:5080`.
- Frontend local: `http://localhost:5173`.
- PostgreSQL local: `localhost:5455`.

## Arquitectura

```text
API / Presentation → Application → Domain
          └────────→ Infrastructure
```

- `backend/MealBridge.Api/`: HTTP, DTOs, CORS, envelope, códigos de estado y composición.
- `backend/MealBridge.Application/`: casos de uso, puertos, transacciones y mapeos.
- `backend/MealBridge.Domain/`: `DonationLot`, validaciones, reclamación, estados e invariantes.
- `backend/MealBridge.Infrastructure/`: EF Core, PostgreSQL, migraciones y repositorios.
- `backend/MealBridge.Tests/`: pruebas unitarias y de integración.

`Domain` no depende de HTTP, EF Core ni Infrastructure. `Application` depende de Domain. API e Infrastructure dependen de los contratos internos correspondientes.

## No negociables

- Mantener exactamente una API de backend.
- Usar el envelope `{ succeeded, data, error }` en toda respuesta JSON.
- Mantener nombres JSON y enums en camelCase según el contrato.
- Las invariantes de reclamación y transición pertenecen a Domain.
- Un segundo reclamo devuelve `409` y nunca sobrescribe `claimedBy`.
- Una transición ilegal devuelve `409`.
- Mapear `InvalidArgumentException`, `NotFoundException` y `ConflictException` a `400`, `404` y `409`; los errores inesperados usan un `500` genérico.
- La reclamación debe ser atómica frente a concurrencia.
- Los datos deben persistir después de reiniciar la API.
- No guardar secretos, contraseñas ni connection strings reales en Git.
- No implementar autenticación, colas, RAG, embeddings ni otras extensiones antes de completar el MVP.

## Flujo obligatorio de trabajo

### Planning

Para crear, dividir, ordenar o actualizar tareas, leer y aplicar:

- `.agents/skills/planning/SKILL.md`

Trabajar únicamente sobre la primera tarea `pending` válida de `IMPLEMENTATION-PLAN.md`. Mantener como máximo una tarea `in_progress`.

### TDD

Para cualquier cambio de comportamiento en Domain o Application, leer y aplicar:

- `.agents/skills/tdd/SKILL.md`

La secuencia obligatoria es:

```text
prueba enfocada en rojo → implementación mínima → prueba verde → refactor
```

No editar producción antes de confirmar el rojo por la razón correcta. Registrar comandos y resultados reales en `AI-USE.md`.

### Implementación

Antes de crear o modificar código de producción, leer y aplicar:

- `.agents/skills/implementation/SKILL.md`

Esta skill es obligatoria en toda tarea `*-impl`. Al terminar, comparar el corte implementado con `AI-AIDED-FULLSTACK-CHALLENGE.md`, RF, RNF, `MVP-SCOPE.md` y la tarea activa; no marcarlo completo si hace menos, hace más o inventa comportamiento.

Los mensajes de logs, respuestas, excepciones y validaciones deben estar en español y declararse como constantes en la misma clase que los utiliza; no deben escribirse directamente dentro de llamadas o retornos.
Los orígenes, URLs, puertos y listas configurables deben cargarse mediante opciones tipadas desde `appsettings.json` o variables de entorno; no deben escribirse directamente en código.

### Regla y comando del editor

- Regla persistente: `.cursor/rules/mealbridge-tdd.mdc`
- Comando reutilizable: `.cursor/commands/tdd-implement.md`

## Ejecutar PostgreSQL

El contenedor local esperado se llama `tp-challenge-mealbridge`.

```powershell
docker start tp-challenge-mealbridge
docker ps --filter "name=tp-challenge-mealbridge"
```

Crear `.env` gitignored en la raíz a partir de `.env.example`. La API y las
pruebas lo cargan automáticamente al ejecutarse desde la raíz o un
subdirectorio; una variable ya definida en el proceso tiene prioridad:

```powershell
$env:ConnectionStrings__MealBridge = "Host=localhost;Port=5455;Database=mealbridge;Username=your-local-user;Password=your-local-password"
```

## Ejecutar la API

```powershell
dotnet restore backend/MealBridge.sln
dotnet run --project backend/MealBridge.Api/MealBridge.Api.csproj
```

- Health: `http://localhost:5080/health`
- Swagger: `http://localhost:5080/swagger`

## Ejecutar el frontend

```powershell
npm install --prefix frontend
npm run dev --prefix frontend
```

## Ejecutar pruebas y verificaciones

Prueba xUnit enfocada:

```powershell
dotnet test backend/MealBridge.Tests/MealBridge.Tests.csproj `
  --filter "FullyQualifiedName~NombreDeLaPrueba"
```

Suite y compilaciones:

```powershell
dotnet test backend/MealBridge.sln
dotnet build backend/MealBridge.sln
npm run build --prefix frontend
npm run lint --prefix frontend
```

## Definición de terminado

Una tarea solo se marca `completed` cuando:

1. Su resultado existe en la ruta indicada.
2. Se ejecutó la verificación descrita.
3. No introdujo errores de compilación o lint.
4. La evidencia TDD fue registrada cuando aplica.
5. `IMPLEMENTATION-PLAN.md` refleja el estado real.
