# Alcance del MVP de MealBridge

## Objetivo

Entregar una aplicación fullstack local y demostrable para coordinar donaciones de excedentes de alimentos. Un donante crea lotes y un coordinador los consulta, filtra, reclama y actualiza hasta su recogida o cancelación.

## Incluido en el MVP

### Actores

- **Donante:** crea lotes de donación.
- **Coordinador:** lista, filtra, consulta, reclama y actualiza lotes.
- Se usan nombres ficticios; no se requiere autenticación real.

### Frontend

- Un frontend ejecutable localmente.
- Tablero o lista de lotes.
- Filtros por `status` y `foodCategory`.
- Formulario para crear un lote.
- Detalle de un lote.
- Acción para reclamar un lote `available`.
- Acciones para marcar un lote `claimed` como `picked_up` o `cancelled`.
- Estados de carga, vacío, error y éxito.
- Botones deshabilitados mientras una operación está en curso.
- Visualización de errores `400`, `404` y `409`.
- Ocultamiento de acciones que no sean válidas para el estado actual.

### Backend

- Una sola API HTTP ejecutable localmente.
- Responsabilidades separadas de `API/Presentation`, `Application`, `Domain` e `Infrastructure`.
- Reglas de reclamación y transición de estados dentro del dominio.
- Los siguientes endpoints:
  - `POST /api/donations`
  - `GET /api/donations`
  - `GET /api/donations/{id}`
  - `POST /api/donations/{id}/claim`
  - `PATCH /api/donations/{id}/status`
- JSON en `camelCase`.
- Envelope común para todas las respuestas JSON.
- Validación y códigos HTTP `200`, `201`, `400`, `404`, `409` y `500`.
- CORS para el frontend local cuando sea necesario.

### Datos

- Persistencia de todos los campos de `DonationLot`.
- UUID generado por el servidor.
- Timestamps de creación, reclamación y actualización.
- Base de datos persistente; los datos sobreviven al reinicio de la API.
- Protección atómica contra reclamaciones dobles.

### Flujo de estados

```text
available → claimed | cancelled | expired
claimed   → picked_up | cancelled
picked_up → ninguna transición
cancelled → ninguna transición
expired   → ninguna transición
```

- La creación siempre produce `available`.
- `claimed` solo se alcanza mediante la operación de reclamación.
- Las transiciones ilegales producen `409`.
- El cálculo automático de `expired` puede omitirse si se documenta en `AI-USE.md`.

### Pruebas y proceso

- TDD estricto para comportamiento de dominio y aplicación.
- Evidencia de una prueba en rojo antes de su implementación.
- Pruebas unitarias para:
  - Creación válida.
  - Entrada inválida.
  - Reclamación exitosa.
  - Segundo reclamo en conflicto.
  - Transición ilegal en conflicto.
- Ejecución final de la suite y registro del resultado en `AI-USE.md`.

### Artefactos de entrega

- Código fuente del frontend, una API y soporte de base de datos.
- `AGENTS.md`.
- Una regla persistente del editor.
- Un comando o prompt reutilizable.
- `.agents/skills/tdd/SKILL.md`.
- `.agents/skills/planning/SKILL.md`.
- `AI-USE.md`.
- `README.md` con instrucciones de ejecución.
- `.env.example`.

## Fuera del alcance del MVP

- Autenticación real, JWT, OAuth o autorización por roles.
- Pagos, facturas o impuestos.
- Mapas reales, GPS o cálculo de rutas.
- Multi-tenancy.
- Correos electrónicos o mensajes SMS.
- Despliegue en una nube de producción.
- Múltiples APIs o microservicios.
- Pulido visual perfecto, design system o auditoría completa de accesibilidad.
- Certificación sanitaria o ciencia real de perecederos.
- Colas y workers.
- Carga de fotos o recibos y almacenamiento de objetos.
- SSE o actualizaciones en tiempo real.
- Servidor MCP.
- RAG.
- Búsqueda mediante embeddings.
- Agente de planificación de recogidas.

## Criterio de finalización

El MVP se considera terminado cuando:

1. El frontend, una API y una base de datos persistente funcionan localmente.
2. Se puede crear un lote desde la interfaz y verlo en la lista.
3. Se puede filtrar por `status` y `foodCategory`.
4. Se puede reclamar un lote disponible y ver `claimedBy`.
5. Se puede marcar un lote reclamado como `picked_up` o `cancelled`.
6. Los errores `400`, `404` y `409` son visibles en la interfaz.
7. Las pruebas unitarias obligatorias existen, fueron desarrolladas con TDD y pasan.
8. El backend conserva las responsabilidades de Arquitectura Limpia y las invariantes viven en el dominio.
9. Están presentes los artefactos obligatorios del espacio de trabajo de IA.
10. El guion de demostración puede completarse sin editar la base de datos manualmente.
11. Después de reiniciar la API, el lote conserva su último estado.

## Guion de demostración del MVP

1. Mostrar `AGENTS.md`, la arquitectura y las skills de TDD y planning.
2. Mostrar las invariantes en el dominio.
3. Ejecutar las pruebas unitarias obligatorias.
4. Iniciar la API y el frontend.
5. Crear el lote del ejemplo proporcionado en el reto.
6. Filtrar por `available` y `bakery`.
7. Reclamar el lote con el coordinador del ejemplo.
8. Intentar reclamarlo nuevamente y mostrar el error `409`.
9. Marcarlo como `picked_up` y comprobar que no aparecen acciones ilegales.
10. Reiniciar la API y comprobar que el lote continúa en `picked_up`.
