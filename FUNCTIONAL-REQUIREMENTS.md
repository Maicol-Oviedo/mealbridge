# Requisitos funcionales de MealBridge

Este documento extrae únicamente los requisitos funcionales del reto MealBridge. No incluye extensiones opcionales.

## RF-01 — Crear un lote de donación

El donante debe poder crear un lote desde la interfaz de usuario con:

- `businessName`: obligatorio, entre 1 y 120 caracteres.
- `title`: obligatorio, entre 1 y 80 caracteres.
- `description`: opcional, máximo 500 caracteres.
- `foodCategory`: `bakery`, `produce`, `dairy`, `prepared` u `other`.
- `quantity`: entero mayor o igual a 1.
- `unit`: `portions`, `kg`, `loaves` o `boxes`.
- `pickupAddress`: obligatorio, entre 1 y 200 caracteres.
- `availableFrom`: fecha y hora ISO-8601 en UTC.
- `availableUntil`: fecha y hora ISO-8601 en UTC, posterior a `availableFrom`.

El servidor debe:

- Generar `id` como UUID.
- Crear el lote con `status: "available"`.
- Establecer `claimedBy` y `claimedAt` en `null`.
- Establecer `createdAt` y `updatedAt`.
- Rechazar que el cliente defina `status`.
- Responder `201 Created` cuando la creación sea exitosa.
- Responder `400` cuando los datos sean inválidos.

## RF-02 — Listar lotes

El coordinador debe poder consultar todos los lotes. Una lista vacía es una respuesta válida.

## RF-03 — Filtrar lotes

El coordinador debe poder filtrar la lista por:

- `status`.
- `foodCategory`.

Cuando ambos filtros estén presentes, deben combinarse con AND. Un valor desconocido debe responder `400`.

## RF-04 — Consultar un lote

El sistema debe permitir consultar un lote por su UUID:

- Debe responder `200` cuando exista.
- Debe responder `404` cuando no exista.
- Debe responder `400` cuando el identificador esté mal formado.

## RF-05 — Reclamar un lote

El coordinador debe poder reclamar un lote `available` proporcionando `coordinatorName`, obligatorio y de 1 a 120 caracteres.

Una reclamación exitosa debe:

- Cambiar `status` a `claimed`.
- Establecer `claimedBy` con el nombre del coordinador.
- Establecer `claimedAt`.
- Actualizar `updatedAt`.

Un lote que no esté `available` debe responder `409`. Dos reclamaciones concurrentes no pueden ser exitosas ni sobrescribir `claimedBy`.

## RF-06 — Cambiar el estado de un lote

El coordinador debe poder aplicar únicamente estas transiciones:

```text
available → cancelled | expired
claimed   → picked_up | cancelled
```

La transición `available → claimed` solo puede realizarse mediante la operación de reclamación.

Los estados `picked_up`, `cancelled` y `expired` son terminales. Una transición no permitida debe responder `409`; un lote inexistente debe responder `404`.

El cálculo automático de `expired` puede omitirse en el MVP si se documenta en `AI-USE.md`.

## RF-07 — Persistir los lotes

Todos los campos de `DonationLot` deben persistirse en una base de datos. Los datos deben conservarse después de reiniciar el proceso de la API.

## RF-08 — Mostrar estados de la interfaz

La interfaz debe mostrar:

- Estado de carga mientras obtiene datos.
- Mensaje cuando todavía no existen lotes.
- Mensaje diferente cuando los filtros no encuentran resultados.
- Errores recibidos de la API.
- Confirmación después de crear, reclamar o cambiar un estado.
- Botones de acción deshabilitados mientras una solicitud está en curso.

## RF-09 — Mostrar acciones válidas

La interfaz debe:

- Mostrar la acción de reclamar cuando el lote esté `available`.
- Mostrar las acciones de marcar como `picked_up` y cancelar cuando esté `claimed`.
- Ocultar acciones ilegales.
- Mostrar los errores `400`, `404` y `409` sin dejar la página en blanco.

## RF-10 — Usar el contrato HTTP establecido

El sistema debe exponer una sola API con:

- `POST /api/donations`
- `GET /api/donations`
- `GET /api/donations/{id}`
- `POST /api/donations/{id}/claim`
- `PATCH /api/donations/{id}/status`

Todos los cuerpos JSON deben usar nombres `camelCase`.

Todas las respuestas JSON, exitosas o fallidas, deben usar este envelope:

```json
{
  "succeeded": true,
  "data": {},
  "error": null
}
```

En caso de error, `succeeded` debe ser `false`, `data` debe ser `null` y `error` debe contener un único mensaje legible. Los códigos HTTP deben conservar su significado: `200`, `201`, `400`, `404`, `409` y `500`.
