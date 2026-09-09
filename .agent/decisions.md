# Decisiones de arquitectura

Por qué el sistema es como es. Cada entrada es una decisión **ya vigente en el código**, con el
contexto que la explica y lo que cuesta mantenerla.

> **Una decisión vigente no se edita para cambiar de opinión.** Si se revierte, se escribe una
> entrada nueva y la vieja pasa a *revertida*, apuntando a la que la reemplaza. El valor de este
> archivo no está en la lista de lo que hacemos, sino en poder leer por qué algo **no** se hizo de
> la otra forma — que es exactamente lo que una sesión nueva vuelve a proponer.

Los IDs `D-nn` son estables y **no se renumeran**. Vienen del registro común que vivía en
`ns-store-docs`; los huecos en la numeración son decisiones que solo aplican al SPA y viven en
`ns-store-ui/.agent/decisions.md`.

`D-01` a `D-12` y `D-14` fueron reconstruidas el 2026-08-15 leyendo el código y los `CLAUDE.md`.
Donde solo se pudo recuperar la regla y no su motivo, va marcado **⚠️ racional reconstruido**: una
razón inventada que suena bien es peor que un hueco declarado, porque nadie la vuelve a cuestionar.
La fecha real de casi todas es desconocida; se anota *anterior a 2026-08-15*.

| ID | Decisión | Estado |
|----|----------|--------|
| [D-01](#d-01) | Reescritura desde cero; ningún código legacy se reutiliza | vigente |
| [D-02](#d-02) | Código en inglés, interfaz en español, API sin idioma | vigente |
| [D-03](#d-03) | Servicios planos: sin MediatR, sin repositorios | vigente |
| [D-04](#d-04) | PostgreSQL con enums nativos y `snake_case` | vigente |
| [D-05](#d-05) | Soft delete global por filtro de consulta | vigente |
| [D-06](#d-06) | `InventoryMovement` es el libro mayor; `StockLevel` es caché | vigente |
| [D-07](#d-07) | Bloqueo pesimista de stock, ids en orden, más token de versión | vigente |
| [D-08](#d-08) | `TimeProvider` inyectado en vez de `DateTime.UtcNow` | vigente |
| [D-09](#d-09) | Access token en memoria, refresh en cookie httpOnly | vigente |
| [D-12](#d-12) | Tests de aplicación sobre SQLite en memoria | vigente, con costo abierto |
| [D-14](#d-14) | Se prefiere la cifra calculada en el servidor | vigente |
| [D-15](#d-15) | El arnés vive en cada repo; `ns-store-docs` se archiva | vigente |

---

<a id="d-01"></a>
## D-01 — Reescritura desde cero; ningún código legacy se reutiliza

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente

**Contexto.** El sistema original es una aplicación de escritorio WPF sobre SQL Server, con la
lógica repartida entre el code-behind de cada vista y stored procedures `sp_<entidad>`
multiplexados por un parámetro `@i_accion` (`S1`, `I1`, `U1`, `D1`…). No hay capa de dominio que
rescatar.

**Decisión.** Construir una aplicación web nueva. Del legacy se conserva **qué hace y por qué**,
nunca el cómo.

**Consecuencias.** Cualquier duda funcional se resuelve contra `.agent/domain.md`; cualquier duda
técnica, contra el código. La especificación completa del sistema viejo quedó congelada en el repo
archivado `ns-store-docs`, y es especificación histórica: no describe cómo está implementado nada
hoy.

---

<a id="d-02"></a>
## D-02 — Código en inglés, interfaz en español, API sin idioma

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente · declarada *no negociable*

**Contexto.** El dominio es boliviano y se piensa en español (factura, cotización, encargo,
kardex, NIT, CI), pero el código, el esquema y las herramientas son en inglés.

**Decisión.** Tres capas separadas: identificadores, esquema, comentarios y mensajes de commit en
**inglés**; la API devuelve códigos, enums, ids y números y **nunca copy en español**; todo texto
visible es español `es-BO` y vive solo en el SPA, vía i18n.

**Consecuencias.** Cada término de negocio necesita su equivalente en inglés fijado de antemano —
por eso existe el glosario de `.agent/domain.md`. Un `errorCode` nuevo aquí está **incompleto**
hasta que `ns-store-ui` tiene su mensaje; eso lo verifica `just check` en ese repo. La API puede
servir otra interfaz en otro idioma sin tocarse.

---

<a id="d-03"></a>
## D-03 — Servicios planos: sin MediatR, sin repositorios

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente

**Contexto.** El patrón por defecto en ASP.NET Core para una app de este tamaño suele ser
MediatR + CQRS + repositorios sobre EF.

**Decisión.** Una carpeta por feature en `Application/Features/`, cada una con su `XService.cs`,
sus DTOs y sus validadores. Los servicios son clases normales registradas en
`Application/DependencyInjection.cs` que dependen solo de puertos de `Common/Interfaces`. **Las
proyecciones `IQueryable` de EF son la capa de acceso a datos**; no hay repositorios encima.

**Consecuencias.** Menos ceremonia y menos indirección para leer un caso de uso completo. A cambio
la disciplina no la impone el framework: nada impide inyectar `AppDbContext` donde debía ir un
puerto, ni escribir una consulta que se salte el filtro de soft delete (`D-05`). El olvido típico
—registrar el servicio y sumarlo a `TestHarness`— hoy lo atrapan los tests de
`tests/NsStore.Architecture.Tests`.

⚠️ **Racional reconstruido.** No consta si se evaluó MediatR y se descartó, o si nunca se
consideró. Confirmar.

---

<a id="d-04"></a>
## D-04 — PostgreSQL con enums nativos y `snake_case`

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente

**Decisión.** PostgreSQL 17 con `UseSnakeCaseNamingConvention`, y los enums del dominio mapeados
como **tipos enum nativos de PostgreSQL**, registrados en `Infrastructure/DependencyInjection.cs`.
El JSON de la API sale en camelCase con los enums como cadenas camelCase.

**Consecuencias.** La base se lee bien desde `psql` y los valores inválidos los rechaza el motor.
El costo es un paso fácil de olvidar: **agregar un enum exige registrarlo en `DependencyInjection`
y generar migración**. Lo primero lo atrapa un test de arquitectura, lo segundo `just check`.
Además el borde HTTP ya cobró una pieza por este tema: el binding es sensible a mayúsculas y la
API llegó a rechazar la grafía que ella misma emite, por lo que los enums de query se parsean con
`QueryEnum.Parse` y `Enum.TryParse` está prohibido en `NsStore.Api`.

---

<a id="d-05"></a>
## D-05 — Soft delete global por filtro de consulta

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente

**Decisión.** Casi toda entidad hereda `AuditableEntity` y lleva
`HasQueryFilter(x => x.DeletedAt == null)`. Nada se borra físicamente.

**Consecuencias.** Un registro eliminado sigue existiendo para la historia: una venta vieja no
pierde su cliente. El costo está en las tablas hijas — `SaleItem` y similares **no** tienen filtro
propio, así que una consulta que entre directo a la hija ve filas de padres borrados. Toda consulta
a una hija tiene que pasar por el padre filtrado; la proyección de kardex en `InventoryService` es
el ejemplo a copiar.

---

<a id="d-06"></a>
## D-06 — `InventoryMovement` es el libro mayor; `StockLevel` es caché

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente

**Contexto.** El stock se puede guardar como un número por producto, o derivar siempre de los
movimientos. Lo primero es rápido pero pierde la historia; lo segundo es exacto pero caro.

**Decisión.** Las dos cosas, con jerarquía explícita: **cada cambio de stock escribe un
`InventoryMovement`, que es la fuente de verdad**, y además actualiza el `StockLevel`, que es
caché. El `StockLevel` nunca se borra y puede quedarse en 0. Solo se modifica llamando a `Apply`,
que impone stock no negativo y sube el token `Version`.

**Consecuencias.** El kardex y la auditoría salen del libro mayor; las pantallas rápidas leen el
caché. La regla dura es que **nadie toca `StockLevel.Quantity` directamente**: hacerlo desincroniza
las dos representaciones en silencio y la caché miente sin que nada falle. El stock solo se mueve
por compras, ventas y ajustes de admin.

---

<a id="d-07"></a>
## D-07 — Bloqueo pesimista de stock, ids en orden, más token de versión

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente

**Contexto.** Dos cajas vendiendo el último artículo a la vez.

**Decisión.** La venta es una sola unidad de trabajo: `ExecuteInTransactionAsync` envolviendo
`IStockLockService.LockAsync`, que hace `SELECT … FOR UPDATE` sobre `stock_levels` **tomando los
ids en orden ascendente**, más la comprobación optimista de `Version`. La sobreventa sale como
`409 INSUFFICIENT_STOCK`.

**Consecuencias.** El orden de los ids no es un detalle de estilo: es lo que evita el interbloqueo
entre dos transacciones que quieren los mismos dos productos en orden distinto. La transacción es
segura de anidar y **se reintenta entera** bajo la estrategia de ejecución de Npgsql, así que la
acción envuelta tiene que ser idempotente. Este comportamiento es exclusivo de PostgreSQL y la
suite corre en SQLite con un `NoOpStockLock`: **no está cubierto por ningún test** (ver `D-12` y
[`findings.md`](findings.md#trampas-transversales)).

---

<a id="d-08"></a>
## D-08 — `TimeProvider` inyectado en vez de `DateTime.UtcNow`

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente

**Decisión.** Nadie lee el reloj del sistema directamente; se inyecta `TimeProvider`.
`CreatedAt`/`UpdatedAt`/`CreatedBy` los pone `AuditInterceptor`, nunca el código a mano.

**Consecuencias.** Los tests sustituyen un reloj falso —`TestHarness` lo fija en 2026-07-24— y
prueban vencimientos, deuda atrasada y cortes por fecha de forma determinista. Un solo
`DateTime.UtcNow` colado vuelve ese test dependiente del día en que se corre, así que hoy
`DateTime.UtcNow` y `DateTime.Now` están **prohibidos por el compilador** en `src/`
(`BannedApiAnalyzers`, `src/BannedSymbols.txt`).

---

<a id="d-09"></a>
## D-09 — Access token en memoria, refresh en cookie httpOnly

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente

**Contexto.** Guardar tokens en `localStorage` los deja al alcance de cualquier XSS.

**Decisión.** El access token JWT vive **solo en memoria del SPA**, nunca en `localStorage`. El
refresh token es una cookie httpOnly rotatoria, acotada a `/api/v1/auth`, que el JavaScript nunca
lee (`AuthCookies`).

**Consecuencias.** Del lado de esta API, el alcance de la cookie es parte del contrato: cambiarlo
rompe la sesión del SPA. Y tiene una consecuencia que parece de configuración pero es de
seguridad: el dev server del SPA **tiene que proxear `/api`** en vez de apuntar a `localhost:5080`,
porque la cookie debe quedar en el mismo origen.

---

<a id="d-12"></a>
## D-12 — Tests de aplicación sobre SQLite en memoria

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente, con costo abierto

**Decisión.** `NsStore.Application.Tests` corre los servicios reales contra **SQLite en memoria**
vía `TestHarness`, que cablea el `AppDbContext` real, un usuario falso, un reloj fijo, un
`NoOpStockLock` y una semilla mínima. Sin Testcontainers.

**Consecuencias.** La suite es rápida, no necesita Docker y prueba servicios de verdad en vez de
mocks. Pero deja fuera todo lo específico de PostgreSQL —bloqueo de filas (`D-07`), enums nativos
(`D-04`)— y además **distorsiona lo que se puede probar**: EF guarda `decimal` como TEXT en SQLite,
así que un check constraint sobre dinero compara cadenas. Ver
[`findings.md`](findings.md#trampas-transversales).

---

<a id="d-14"></a>
## D-14 — Se prefiere la cifra calculada en el servidor

**Fecha:** anterior a 2026-08-15 · **Estado:** vigente

**Contexto.** Varios totales se derivaban en el navegador a partir de la lista recibida.

**Decisión.** Cuando un número depende de una regla de negocio lo calcula **esta API** y el
frontend lo renderiza sin recalcular: `/stock` manda `lastCost` e `inventoryValue`,
`/reports/stock` manda `totalValue`, `/kardex` manda `totalAdjusted` y `totalSoldAmount`,
`/purchases` manda `lineCount`. El IVA y el margen se leen de `app_settings` en los dos lados,
nunca como constantes.

**Consecuencias.** No hay dos copias de una regla que puedan separarse con el tiempo. La razón de
fondo es que agregar en el cliente sobre una lista paginada da un total que miente en silencio,
porque el cliente solo tiene la página. Cada número nuevo que dependa de una regla implica
**ampliar el DTO** en vez de resolverlo en el componente.

---

<a id="d-15"></a>
## D-15 — El arnés vive en cada repo; `ns-store-docs` se archiva

**Fecha:** 2026-09-08 · **Estado:** vigente · **revierte a `D-13`**

**Contexto.** `D-13` sacó `docs/` de este repo y convirtió la carpeta paraguas en el repo
`ns-store-docs`, con los tres registros (hallazgos, bitácora, decisiones) compartidos por los dos
repos de código. En tres semanas y media acumuló tres commits, todos sobre sí mismo y ninguno sobre
una entrega real. El riesgo que `D-13` declaraba —que nada obliga a actualizar los documentos
cuando cambia el código— se materializó: `DT-10` quedó abierto con documentación afirmando que el
sistema *no* es multi-sucursal cuando sí lo es.

**Decisión.** El conocimiento durable baja a cada repo, junto al código que describe:
`.agent/domain.md`, `.agent/decisions.md`, `.agent/findings.md`. Lo que puede ser una comprobación
deja de ser prosa y pasa a `just check`. `ns-store-docs` se archiva en solo lectura con la
especificación histórica del legacy.

**Alternativas descartadas.**
- *Mantener el paraguas como `.agent/` compartido:* es lo que evita duplicar el glosario, pero
  obliga a que los dos repos se clonen dentro de una carpeta concreta para que el arnés cargue —
  cosa que el propio `CLAUDE.md` tenía que advertir— y deja la documentación fuera del PR que
  cambia el código.
- *Submódulos:* el puntero fijo habría que actualizarlo en cada avance de rama.

**Consecuencias.** El glosario y cuatro decisiones (`D-01`, `D-02`, `D-09`, `D-14`) quedan
**escritas en los dos repos**, y pueden derivar. Es el costo aceptado a cambio de que cada repo
sea autocontenido. Lo que sí queda verificado por máquina es la parte del contrato que más
duele: `ns-store-ui` comprueba en cada `check` que todo `errorCode` de esta API tiene mensaje en
español. La bitácora desaparece; su función la cumplen la descripción del PR y el `git log`.
