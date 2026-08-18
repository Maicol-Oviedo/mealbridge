# MealBridge

MealBridge es una aplicación fullstack local para coordinar donaciones de excedentes de alimentos. Los donantes publican lotes y los coordinadores los filtran, reclaman y marcan como recogidos o cancelados.

## Estado actual

El corte funcional del MVP está implementado: creación, listado, filtros, detalle, reclamo, cambios de estado, persistencia PostgreSQL y manejo uniforme de errores. El guion oficial de demostración está en `MVP-SCOPE.md`.

## Tecnologías

- Frontend: React, TypeScript y Vite.
- Backend: ASP.NET Core sobre .NET 10.
- Persistencia: PostgreSQL y Entity Framework Core con Npgsql.
- Pruebas: xUnit.
- API: Swagger/OpenAPI.

## Estructura

```text
backend/
  MealBridge.Api/             HTTP, CORS, envelope y composición
  MealBridge.Application/     casos de uso y puertos
  MealBridge.Domain/          entidad, estados e invariantes
  MealBridge.Infrastructure/  EF Core, PostgreSQL y repositorios
  MealBridge.Tests/           pruebas unitarias
frontend/                     aplicación React
```

La dirección de dependencias es:

```text
API / Presentation → Application → Domain
          └────────→ Infrastructure
```

## Requisitos previos

- .NET SDK 10.
- Node.js y npm.
- Docker Desktop.
- Git.

## PostgreSQL

El entorno local usa el contenedor `tp-challenge-mealbridge`, con PostgreSQL publicado en el puerto `5455`.

Comprobar que está activo:

```powershell
docker ps --filter "name=tp-challenge-mealbridge"
```

Iniciar un contenedor ya creado y detenido:

```powershell
docker start tp-challenge-mealbridge
```

Para crear otro entorno, defina credenciales locales y ejecute:

```powershell
$env:POSTGRES_USER = "your-local-user"
$env:POSTGRES_PASSWORD = "your-local-password"
$env:POSTGRES_DB = "mealbridge"

docker run --name tp-challenge-mealbridge `
  -e POSTGRES_USER=$env:POSTGRES_USER `
  -e POSTGRES_PASSWORD=$env:POSTGRES_PASSWORD `
  -e POSTGRES_DB=$env:POSTGRES_DB `
  -p 5455:5432 `
  -d postgres:latest
```

No guarde credenciales reales en Git.

## Variables de entorno

Copie `.env.example` como `.env` en la raíz y complete solo sus valores
locales. `.env` está ignorado por Git.

```powershell
Copy-Item .env.example .env
```

La API y las pruebas buscan `.env` desde el directorio de ejecución hacia la
raíz. Una variable ya definida en el proceso tiene prioridad. Vite también
carga el `.env` de la raíz mediante `envDir`.

El frontend usa:

```text
VITE_API_URL=http://localhost:5080
```

## Ejecutar el backend

Desde la raíz del repositorio:

```powershell
dotnet restore backend/MealBridge.sln
dotnet run --project backend/MealBridge.Api/MealBridge.Api.csproj
```

Si la terminal de Windows aún no reconoce `dotnet` después de instalar el
SDK, use la ruta comprobada:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" run `
  --project .\backend\MealBridge.Api\MealBridge.Api.csproj
```

Servicios locales:

- API: `http://localhost:5080`
- Health: `http://localhost:5080/health`
- Swagger: `http://localhost:5080/swagger`

No inicie una segunda instancia de la API: bloqueará los ensamblados de
`bin/Debug`. Compruebe primero:

```powershell
Invoke-RestMethod http://localhost:5080/health
```

## Ejecutar el frontend

```powershell
npm install --prefix frontend
npm run dev --prefix frontend
```

Frontend: `http://localhost:5173`

## Demostración

Con PostgreSQL, API y frontend activos, complete en el navegador los diez pasos
de `MVP-SCOPE.md`: crear, filtrar, reclamar desde dos vistas para mostrar el
segundo reclamo `409`, marcar como recogido y comprobar la persistencia después
de reiniciar la API. No edite la base de datos manualmente durante el guion.

## Ejecutar pruebas

```powershell
dotnet test backend/MealBridge.sln
```

La suite incluye pruebas unitarias de Domain/Application y pruebas de
integración contra PostgreSQL y las rutas HTTP reales. Las bases aisladas de
prueba no modifican la base principal `mealbridge`.

## Compilar y validar

```powershell
dotnet build backend/MealBridge.sln
npm run build --prefix frontend
npm run lint --prefix frontend
```

## Migraciones

La migración versionada existente es
`20260817231447_InitialCreate`. Para aplicarla:

```powershell
dotnet tool restore

dotnet ef database update `
  --project backend/MealBridge.Infrastructure/MealBridge.Infrastructure.csproj `
  --startup-project backend/MealBridge.Api/MealBridge.Api.csproj
```

Para comprobar las migraciones conocidas y aplicadas:

```powershell
dotnet ef migrations list `
  --project backend/MealBridge.Infrastructure/MealBridge.Infrastructure.csproj `
  --startup-project backend/MealBridge.Api/MealBridge.Api.csproj
```

## Documentos

- `FUNCTIONAL-REQUIREMENTS.md`: requisitos funcionales.
- `NON-FUNCTIONAL-REQUIREMENTS.md`: requisitos no funcionales.
- `MVP-SCOPE.md`: alcance exclusivo del MVP.
- `IMPLEMENTATION-PLAN.md`: tareas ordenadas de implementación.
- `AI-USE.md`: decisiones, prompts, errores y evidencia real de los ciclos TDD.
