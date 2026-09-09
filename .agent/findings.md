# Hallazgos

Bugs, deuda y trampas de este repo que **no** se están arreglando ahora. Existe porque cada sesión
empieza sin memoria: sin este archivo, se vuelven a descubrir los mismos problemas y se vuelven a
perder los que alguien dejó deliberadamente fuera de alcance.

## Cómo usar este registro

- **Los IDs son estables.** `DT-nn` no se renumera nunca, ni cuando se cierra uno del medio. Un
  commit que dice "cierra DT-03" tiene que seguir significando lo mismo dentro de un año. El
  siguiente ID libre es el mayor que exista **en los dos repos** + 1.
- **Nada se borra.** Un hallazgo resuelto o descartado cambia de **estado**, no desaparece: saber
  que algo se decidió *no* arreglar vale tanto como la lista de lo pendiente.
- **Estados:** `abierto` · `en curso` · `resuelto` (con fecha y el commit que lo cerró) ·
  `descartado` (con el porqué).
- **Verifica contra el código antes de registrar.** Un hallazgo es una afirmación sobre el sistema:
  si entra sin verificar, contamina la priorización de todos los que vienen después.
- `/finding` hace los pasos anteriores.

Los IDs `DT-01`…`DT-10` vienen del registro común que vivía en `ns-store-docs`; los huecos son
hallazgos del SPA, que viven en `ns-store-ui/.agent/findings.md`. `DT-10` (la documentación del
esquema contradecía al código) se cerró el 2026-09-08 al borrar esa documentación: este archivo y
[`domain.md`](domain.md) la reemplazan.

| ID | Hallazgo | Tipo | Severidad | Estado |
|---|---|---|---|---|
| [DT-01](#dt-01) | Los saldos de pedidos no se cuentan como deuda | Bug | **Alta** — plata sin seguimiento | abierto |
| [DT-02](#dt-02) | Las compras a crédito no tienen libro de pagos | Mejora | **Alta** — plata sin seguimiento | abierto |
| [DT-03](#dt-03) | Los reportes imprimen como máximo 200 filas | Bug | Media | abierto |
| [DT-06](#dt-06) | `GET /clients/{id}/sales` ignora la búsqueda | Bug | Baja | abierto |
| [DT-07](#dt-07) | No todo cobro emite recibo | Mejora | Baja | abierto |
| [DT-09](#dt-09) | Nada prueba el contrato HTTP | Deuda | **Alta** — ya dejó pasar un 500 | abierto |

**Prioridad sugerida.** `DT-01` y `DT-02` primero: son los únicos donde el sistema pierde de vista
dinero real. Después `DT-09` (y la trampa de SQLite de más abajo), que habilitan probar bien lo que
se construya para los dos anteriores y cierran el hueco por el que ya se coló un 500 en producción.
`DT-03` va junto con `DT-04` del SPA, que son el mismo defecto visto desde los dos lados. `DT-06` y
`DT-07` son arreglos acotados y sin dependencias.

---

<a id="dt-01"></a>
## DT-01 — Los saldos de pedidos no se cuentan como deuda

**Tipo:** Bug · **Severidad:** Alta · **Estado:** abierto · **Dónde:** `src/NsStore.Domain/Entities/Order.cs`

Un pedido tiene precio y anticipo, y la entidad ya calcula el saldo:

```csharp
public decimal Balance => Price - AdvanceAmount;
```

Ese saldo es **plata que el cliente debe** y que no aparece en ningún total del sistema: ni en el
dashboard, ni en cuentas por cobrar, ni en cobranzas, ni en el estado de cuenta. Si un cliente
encarga algo de Bs 2.000 y deja Bs 500, los Bs 1.500 restantes son invisibles.

**Por qué quedó fuera:** `Order.ClientName` es texto libre, no una referencia al catálogo. Sin
`ClientId` no hay forma de juntar el pedido de "Juan Perez" con las ventas del cliente #14 sin
adivinar por nombre, y adivinar sobre deuda es exactamente lo que no se debe hacer.

**Arreglo propuesto:** agregar `ClientId` **nullable** a `Order` (migración), cambiar el alta para
elegir del catálogo dejando el texto libre como respaldo de los históricos, y sumar el saldo al
agregado de cobranzas. Los pedidos viejos sin `ClientId` conviene mostrarlos aparte, no repartirlos
por coincidencia de nombre.

---

<a id="dt-02"></a>
## DT-02 — Las compras a crédito no tienen libro de pagos

**Tipo:** Mejora · **Severidad:** Alta · **Estado:** abierto · **Dónde:** `src/NsStore.Domain/Entities/Purchase.cs`

`Purchase` tiene `PaymentStatus` —que puede ser `Credit`— pero, a diferencia de `Sale`, **no**
tiene `TotalPaid`, ni `Balance`, ni tabla de pagos asociada.

| | `Sale` | `Purchase` |
|---|---|---|
| `PaymentStatus` | ✅ | ✅ |
| `TotalPaid` | ✅ | ❌ |
| `Balance` | ✅ | ❌ |
| Pagos (`Payment`) | ✅ | ❌ |

El sistema puede marcar una compra como "a crédito" pero **no puede registrar cuánto se le pagó al
proveedor ni cuánto se le debe**. Es una bandera sin contabilidad detrás.

**Arreglo propuesto:** o espejar el modelo de `Sale` (`TotalPaid`, `Balance`, pagos a proveedor y
una pantalla de cuentas por pagar), o —si el negocio no lleva crédito con proveedores— **quitar
`PaymentStatus` de `Purchase`**, porque hoy promete algo que no cumple.

---

<a id="dt-03"></a>
## DT-03 — Los reportes imprimen como máximo 200 filas

**Tipo:** Bug · **Severidad:** Media · **Estado:** abierto · **Dónde:** `src/NsStore.Application/Features/Reports/ReportService.cs`

```csharp
private const int ReportPageSize = PageRequest.MaxPageSize;   // = 200
```

El trabajo de cobranzas corrigió los **totales**, que antes sumaban solo la página traída. Pero las
**filas** siguen cortadas en 200: un reporte de 347 ventas imprime 200 filas con totales correctos
de 347. Los números ya no mienten, pero la hoja sigue incompleta y no lo dice.

**Arreglo propuesto:** paginar la impresión (varias hojas A4 encadenadas), levantar el tope para
reportes, o —mínimo— imprimir una advertencia visible cuando `SaleCount > Sales.Count`.

Relacionado: `DT-04` en `ns-store-ui`.

---

<a id="dt-06"></a>
## DT-06 — `GET /clients/{id}/sales` ignora la búsqueda

**Tipo:** Bug · **Severidad:** Baja · **Estado:** abierto · **Dónde:** `src/NsStore.Api/Endpoints/ClientEndpoints.cs`

```csharp
Results.Ok(await sales.ListByClientAsync(id, new PageRequest(null, page ?? 1, pageSize ?? 25), ct)));
//                                                            ↑ search hardcodeado
```

`PageRequest` acepta `search` y `ListByClientAsync` lo pasaría al filtro, pero la ruta manda `null`
siempre. Es un parámetro muerto: quien consuma el endpoint esperando filtrar no obtiene error, solo
resultados sin filtrar.

**Arreglo propuesto:** exponer `search` en la ruta, o documentar explícitamente que no se soporta.

---

<a id="dt-07"></a>
## DT-07 — No todo cobro emite recibo

**Tipo:** Mejora · **Severidad:** Baja · **Estado:** abierto · **Dónde:** `src/NsStore.Application/Features/Sales/SaleService.cs`

Hay tres caminos para cobrar y solo uno deja respaldo:

| Camino | Emite recibo |
|---|---|
| `POST /sales/collections` (pantalla de cobranzas) | ✅ numerado y reimprimible |
| `RegisterPaymentAsync` (panel de una venta en `/sales`) | ❌ |
| Pago inicial al crear una venta en el POS | ❌ |

Fue una decisión consciente para no tocar un flujo que ya funcionaba, y `Payment.ReceiptId` es
nullable justamente por eso. Pero deja un hueco: un cobro hecho desde el panel de la venta no le
puede dar nada al cliente.

**Arreglo propuesto:** que `RegisterPaymentAsync` cree también su `PaymentReceipt` de un solo
renglón. El modelo ya lo soporta.

---

<a id="dt-09"></a>
## DT-09 — Nada prueba el contrato HTTP

**Tipo:** Deuda · **Severidad:** Alta · **Estado:** abierto · **Dónde:** `tests/` — no existe proyecto de tests de API · Relacionado: [`D-04`](decisions.md#d-04)

La suite prueba **servicios**, invocándolos en C# directamente. Entre el `SaleQuery` que recibe un
servicio y la URL que manda el navegador no hay ninguna prueba: ruteo, autorización, serialización
y **binding de parámetros** quedan fuera.

Ya cobró una pieza. Los minimal APIs enlazan enums de query string con la sobrecarga **sensible a
mayúsculas** `Enum.TryParse(string, out T)`, mientras las respuestas salen en camelCase. Es decir:
la API rechazaba exactamente la grafía que ella misma emite. `GET /sales?status=credit` respondía
**500** desde que existe, y nadie lo detectó porque `SaleServiceTests` le pasa `PaymentStatus.Credit`
ya tipado. Se arregló con `QueryEnum.Parse`, que además convierte un valor desconocido en 400 en vez
de 500 — y hoy `Enum.TryParse` está prohibido por el compilador en `NsStore.Api` para que no
vuelva a pasar. Pero el arreglo sigue sin test que lo proteja.

**Arreglo propuesto:** un proyecto `NsStore.Api.Tests` con `WebApplicationFactory` sobre la base
SQLite del `TestHarness`, cubriendo al menos un caso por endpoint: código de estado, forma del JSON
y cada parámetro de query.

---

# Trampas transversales

Buzón de las trampas que no son de un módulo, no las causa un sistema externo y todavía no se
pudieron convertir en un test. **No es un archivo histórico.** Toda entrada lleva fecha y **línea de
salida**: el test que la eliminaría. Si una lleva meses aquí, o baja un escalón o se borra porque
dejó de importar. Cada una es en realidad una pieza de herramienta que falta.

## SQLite guarda `decimal` como TEXT y rompe los check constraints de dinero

2026-08-15 · antes `DT-08` · relacionado con [`D-12`](decisions.md#d-12)

La suite corre sobre SQLite, donde EF guarda `decimal` en columnas TEXT. El check
`ck_sales_total_paid_within_total` compara entonces **cadenas**: una venta perfectamente válida de
260,00 con 30,00 pagados es rechazada porque `'30' > '260'`.

Consecuencia práctica: **todo test que involucre un pago parcial tiene que elegir montos cuyo orden
lexicográfico coincida con el numérico.** `CollectionTests` lo hace y lo explica. No se puede
escribir un test con cifras representativas del negocio, y un error de imputación con montos
"normales" (1.234,56 sobre 987,00) no sería detectado. `DemoDataSeeder` directamente no puede
correr en la suite.

**Salida:** tests de integración con Testcontainers sobre PostgreSQL real. Cubriría de paso el
bloqueo de filas de [`D-07`](decisions.md#d-07) y los enums nativos de [`D-04`](decisions.md#d-04),
que hoy tampoco tienen cobertura.
