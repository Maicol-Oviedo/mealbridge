# Requisitos no funcionales de MealBridge

Este documento extrae únicamente los requisitos no funcionales del reto MealBridge. No incluye extensiones opcionales.

## RNF-01 — Ejecución local

El frontend, la API y la base de datos deben poder ejecutarse localmente. No se requiere despliegue en la nube ni una cuenta de servicios cloud.

## RNF-02 — Arquitectura

El backend debe utilizar una Arquitectura Limpia simplificada con estas responsabilidades:

- `API/Presentation`: transporte HTTP, DTOs, CORS, envelope y códigos de estado.
- `Application`: casos de uso, coordinación, interfaces, transacciones y mapeos.
- `Domain`: entidad `DonationLot`, estados, transiciones e invariantes.
- `Infrastructure`: persistencia, ORM, migraciones y repositorios.

Las dependencias deben apuntar hacia adentro: `Domain` no depende de HTTP ni de `Infrastructure`; `Application` depende de `Domain`; la composición se realiza en la entrada de la API.

## RNF-03 — Dominio rico

Las reglas de reclamación y transición de estado deben pertenecer al dominio y ser comprobables sin HTTP, base de datos ni servicios de IA. Los controladores y la infraestructura no deben redefinir esas reglas.

## RNF-04 — Persistencia

No se permite un almacenamiento exclusivamente en memoria. La reclamación debe ser atómica respecto al estado del lote.

## RNF-05 — TDD estricto

Los cambios de comportamiento de dominio o aplicación deben seguir:

1. Escribir una prueba unitaria enfocada.
2. Ejecutarla y confirmar que falla por la razón correcta.
3. Implementar el mínimo código necesario.
4. Ejecutar la prueba hasta dejarla verde.
5. Refactorizar manteniendo la suite verde.

La evidencia de rojo y verde debe registrarse en `AI-USE.md`.

## RNF-06 — Pruebas mínimas

Deben existir y pasar pruebas unitarias de dominio o aplicación para:

- Creación válida con estado `available`, identificador y timestamps.
- Rechazo de una entrada inválida.
- Reclamación exitosa de un lote disponible.
- Conflicto ante un segundo reclamo.
- Conflicto ante una transición ilegal.

## RNF-07 — Seguridad de configuración y datos

- No deben almacenarse secretos en Git.
- La configuración sensible debe recibirse por variables de entorno, user secrets o un `.env` ignorado.
- Debe entregarse `.env.example`.
- Solo deben utilizarse negocios, direcciones y personas ficticias.
- Los errores `500` no deben revelar secretos.

## RNF-08 — Interoperabilidad entre frontend y API

La API debe permitir el origen local del frontend para `GET`, `POST`, `PATCH` y `OPTIONS`, salvo que ambas aplicaciones utilicen un reverse proxy.

## RNF-09 — Usabilidad mínima

- La interfaz debe ser utilizable aproximadamente a 1280 px de ancho.
- Cada campo del formulario debe tener una etiqueta.
- Los formularios deben poder enviarse con teclado.

## RNF-10 — Artefactos del espacio de trabajo de IA

El repositorio debe incluir:

- `AGENTS.md`.
- Al menos una regla persistente del editor.
- Al menos un comando o prompt reutilizable.
- `.agents/skills/tdd/SKILL.md`.
- `.agents/skills/planning/SKILL.md`.
- `AI-USE.md`.

Estos artefactos deben estar escritos específicamente para MealBridge. `AGENTS.md` debe enlazar las dos skills obligatorias.

## RNF-11 — Documentación y entrega

El repositorio debe incluir:

- `README.md` con instrucciones para ejecutar API, frontend, base de datos y pruebas.
- `.env.example`.
- Evidencia del uso de IA y TDD en `AI-USE.md`.
- Instrucciones y puertos suficientes para ejecutar el guion de demostración.

## RNF-12 — Restricción de tiempo

El reto debe completarse individualmente dentro de un límite de tres horas. Un MVP completo, probado y demostrable tiene prioridad sobre extensiones o pulido visual.
