# 02 — Modelo de datos y entidades

> Este documento describe el modelo de datos **conceptual** derivado del legacy. Los nombres
> de tabla/campo del legacy se incluyen como referencia, pero la nueva app debe rediseñar el
> esquema con nombres limpios y sin las deudas señaladas (ver notas ⚠️ y el [documento 05](05-recomendaciones.md)).

## Convenciones del legacy

- Tablas con prefijo `t_` (p.ej. `t_product`).
- Cada columna lleva el prefijo de su tabla (p.ej. `pr_name` en `t_product`, `pr_` = product).
- PKs `int IDENTITY` autoincrementales.
- Booleanos como `bit`.
- Precios como `float` (⚠️ debe ser `decimal` en la nueva app).
- Fechas como `date` (⚠️ sin hora; conviene `datetime`/timestamp en la nueva app).

## Diagrama entidad-relación (conceptual)

```
                         ┌───────────┐
                         │   USER    │
                         └─────┬─────┘
             owner            │ owner (dueño de la transacción)
        ┌──────────────┬──────┼─────────────┬───────────────┐
        ▼              ▼      ▼             ▼               ▼
    ┌────────┐   ┌──────────┐ │        ┌────────┐     ┌────────┐
    │ ORDER  │   │  QUOTE   │ │        │PURCHASE│     │  SALE  │
    └────────┘   └──────────┘ │        └───┬────┘     └───┬────┘
     (encargo)   (cotización) │            │ 1:N          │ 1:N
                              │            ▼              ▼
                              │      ┌──────────────┐ ┌────────────┐
                              │      │PURCHASE_LINE │ │ SALE_LINE  │
                              │      └──────┬───────┘ └─────┬──────┘
   ┌──────────┐   ┌──────────┐│             │  N:1          │ N:1
   │ TRADEMARK│   │ CATEGORY ││             ▼               ▼
   └────┬─────┘   └────┬─────┘│         ┌────────────────────────┐
        │   N:1     N:1│      │         │        PRODUCT         │
        └──────────────┼──────┼────────►└───────────┬────────────┘
                       │      │              1:1     │  1:N
   ┌──────────┐        │      │                      ▼
   │ WARRANTY │────────┘      │                 ┌─────────┐
   └──────────┘   N:1         │                 │  STOCK  │
                              │                 └─────────┘
   ┌──────────┐               │
   │ SUPPLIER │◄──────────────┘ (proveedor de la compra)
   └──────────┘

   ┌──────────┐        ┌──────────┐
   │  CLIENT  │◄───────│   SALE   │  (cliente de la venta)
   └──────────┘  N:1   └──────────┘
   CLIENT.type = persona natural | empresa
```

---

## Entidades principales

### PRODUCTO (`t_product`)
El artículo que se compra y se vende. Núcleo del sistema.

| Campo legacy | Tipo | Significado / reglas |
|---|---|---|
| `pr_product` | PK int | Identificador. |
| `pr_name` | varchar(200) | Nombre del producto. Se guarda en MAYÚSCULAS. |
| `pr_part` | varchar(200) | Número de parte del fabricante. |
| `pr_description` | varchar(max) | Descripción. |
| `pr_serie` | varchar(100) | Número de serie. |
| `pr_trademark` | FK → trademark | Marca. |
| `pr_warranty` | FK → warranty | Garantía asociada. |
| `pr_category` | FK → category | Categoría. |
| `pr_price_bill` | float | **Precio de venta CON factura** (incluye impuesto). Nace en 0 y se fija en el módulo Precios. |
| `pr_price_no_bill` | float | **Precio de venta SIN factura** (neto). Nace en 0. |

> Regla: al **crear** un producto, ambos precios se inicializan en **0**; se definen luego en el módulo **Precios de venta**.

### MARCA (`t_trademark`)
| Campo | Tipo | Significado |
|---|---|---|
| `tr_trademark` | PK int | Id. |
| `tr_name` | varchar(100) | Nombre de la marca. |

### CATEGORÍA (`t_category`)
| Campo | Tipo | Significado |
|---|---|---|
| `ca_category` | PK int | Id. |
| `ca_name` | varchar(100) | Nombre de la categoría. |

### GARANTÍA (`t_warranty`)
Catálogo de plazos de garantía (p.ej. "6 MESES", "1 AÑO", "SIN GARANTÍA").
| Campo | Tipo | Significado |
|---|---|---|
| `wa_warranty` | PK int | Id. |
| `wa_description` | varchar(100) | Texto de la garantía. Se guarda en MAYÚSCULAS. |

### PROVEEDOR (`t_supplier`)
| Campo | Tipo | Significado |
|---|---|---|
| `su_supplier` | PK int | Id. |
| `su_name` | varchar(100) | Nombre del proveedor. |
| `su_phone` | varchar(100) | Teléfono. |
| `su_mail` | varchar(100) | Correo. |

### STOCK (`t_stock`)
Existencias por producto. **Una fila por producto** (relación 1:1 lógica con producto).
| Campo | Tipo | Significado / reglas |
|---|---|---|
| `st_stock` | PK int | Id. |
| `st_quantity` | int | Cantidad disponible. |
| `st_stock_modification` | date | Fecha de la última modificación de stock. |
| `st_product` | FK → product | Producto. |

> Reglas clave (ver [documento 03](03-logica-negocio.md)):
> - Al **comprar**, se **suma** cantidad; si el producto no tenía fila de stock, se **crea**.
> - Al **vender**, se **resta**; si la cantidad vendida iguala la existencia, el legacy **elimina** la fila de stock (⚠️ en la nueva app: dejar en 0, no borrar).
> - Antes de vender se **valida** que haya cantidad suficiente.

---

## Clientes

### CLIENTE (`t_client`)
Un cliente es **persona natural** o **empresa**, distinguido por `cl_type`. Ambos tipos
comparten la misma tabla; según el tipo se llenan unos campos u otros.

| Campo | Tipo | Aplica a | Significado |
|---|---|---|---|
| `cl_client` | PK int | ambos | Id. |
| `cl_type` | bit | ambos | **0 = persona natural, 1 = empresa**. |
| `cl_name` | varchar(50) | ambos | Persona: nombre(s). Empresa: **razón social**. |
| `cl_last_name` | varchar(50) | persona | Apellido paterno. |
| `cl_mother_last_name` | varchar(50) | persona | Apellido materno. |
| `cl_ci` | varchar(15) | persona | Carnet de identidad. |
| `cl_nit` | varchar(100) | ambos | NIT (fiscal). |
| `cl_phone` | varchar(50) | ambos | Teléfono. |
| `cl_mail` | varchar(100) | ambos | Correo. |
| `cl_city` | varchar(50) | empresa | Ciudad. |
| `cl_address` | varchar(max) | empresa | Dirección. |
| `cl_contact` | varchar(100) | empresa | Persona de contacto. |
| `cl_person` | FK → person | — | ⚠️ **Sin uso** (legacy huérfano). |
| `cl_business` | FK → business | — | ⚠️ **Sin uso** (legacy huérfano). |

> El SP expone un campo calculado `cl_complete_name = name + ' ' + last_name + ' ' + mother_last_name`.

> ⚠️ **Deuda de diseño:** en el legacy existen tablas `t_person` y `t_business` con FKs desde
> `t_client`, pero **no se usan**: el cliente almacena sus propios campos de persona/empresa
> directamente. En la nueva app, modelar el tipo con un discriminador o dos subtipos, y
> **eliminar** `t_person`/`t_business`.

### PERSONA (`t_person`) — ⚠️ LEGACY SIN USO
`pe_person` PK, `pe_name`, `pe_last_name`, `pe_mother_last_name`, `pe_ci`. No se referencia en el código activo.

### NEGOCIO/EMPRESA (`t_business`) — ⚠️ LEGACY SIN USO
`bu_business` PK, `bu_name`, `bu_contact`. No se referencia en el código activo.

---

## Compras

### COMPRA — cabecera (`t_purchase`)
Registro de una compra a un proveedor. Puede tener **varios productos** (detalle en
`t_purchase_product`).

| Campo | Tipo | Significado / reglas |
|---|---|---|
| `pu_purchase` | PK int | Id. |
| `pu_purchase_date` | date | Fecha de la compra. |
| `pu_quantity` | int | Cantidad total de artículos de la compra. |
| `pu_unit_price` | float | (legacy monoproducto) Precio unitario. |
| `pu_total_price` | float | Total de la compra. |
| `pu_type_purchase` | bit | **Tipo: con factura / sin factura.** |
| `pu_state` | bit | **Estado: pagada / a crédito.** |
| `pu_supplier` | FK → supplier | Proveedor. |
| `pu_product` | FK → product | ⚠️ (legacy monoproducto) — hoy el detalle está en `t_purchase_product`. |
| `pu_user` | FK → user | Usuario que registró la compra. |

### COMPRA — detalle / línea (`t_purchase_product`)
> ⚠️ **No aparece en el script SQL** del repo, pero el código C# (`sp_purchase_product`, columnas `pp_*`) demuestra que **existe en la BD real**. El script SQL está desactualizado.

| Campo (inferido) | Tipo | Significado |
|---|---|---|
| `pp_purchase_product` | PK int | Id. |
| `pp_product` | FK → product | Producto comprado. |
| `pp_purchase` | FK → purchase | Compra a la que pertenece. |
| `pp_quantity` | int | Cantidad. |
| `pp_unit_price` | decimal | Precio unitario de compra. |
| `pp_total_price` | decimal | Subtotal de la línea. |

---

## Ventas

### VENTA — cabecera (`t_sale`)
Una venta a un cliente. Multi-producto (detalle en `t_sale_product`).

| Campo | Tipo | Significado / reglas |
|---|---|---|
| `sa_sale` | PK int | Id. |
| `sa_sale_date` | date | Fecha de la venta. |
| `sa_client` | FK → client | Cliente. |
| `sa_user` | FK → user | Usuario/vendedor. |
| `sa_total_quantity` | int | Cantidad total de artículos (columna nueva usada por el código; el script viejo la llama `sa_quantity`). |
| `sa_total_price` | float | **Total de la venta.** |
| `sa_total_paid` | float | **Monto pagado** por el cliente. |
| `sa_state` | bit | **0 = crédito/deuda, 1 = pagado (contado).** |
| `sa_product` | FK → product | ⚠️ (legacy monoproducto) — el detalle real está en `t_sale_product`. |
| `sa_unit_price` | float | ⚠️ (legacy monoproducto). |

> Campo calculado por el SP: `sa_rest = sa_total_price − sa_total_paid` (**saldo/deuda pendiente**).

### VENTA — detalle / línea (`t_sale_product`)
El "carrito" de la venta.

| Campo | Tipo | Significado |
|---|---|---|
| `sp_sale_product` | PK int | Id. |
| `sp_product` | FK → product | Producto vendido. |
| `sp_sale` | FK → sale | Venta a la que pertenece. |
| `sp_quantity` | int | Cantidad. |
| `sp_unit_price` | float | Precio unitario aplicado (según con/sin factura). |
| `sp_total_price` | ⚠️ bit en el script | Subtotal de la línea. ⚠️ En el script SQL el tipo es `bit` — **es un bug**; debe ser `decimal`. |

### PRECIO DE VENTA (`t_sale_price`) — ⚠️ LEGACY REDUNDANTE
| Campo | Tipo | Significado |
|---|---|---|
| `sp_sale_price` | PK int | Id. |
| `sp_sale_bill` | float | Precio con factura. |
| `sp_sale_no_bill` | float | Precio sin factura. |
| `sp_product` | FK → product | Producto. |

> ⚠️ Esta tabla duplica `pr_price_bill` / `pr_price_no_bill` de `t_product`. El código
> **activo** usa las columnas de `t_product`, no esta tabla. En la nueva app: **no incluirla**
> (o, si se quiere historial de precios, rediseñarla como tabla de historial con fecha).

---

## Pedidos y cotizaciones

### PEDIDO / ENCARGO (`t_orders`)
Solicitud de un artículo que el cliente encarga. **No referencia producto ni cliente por FK**:
guarda los datos como texto libre (es un encargo, el producto puede no existir en catálogo).

| Campo | Tipo | Significado |
|---|---|---|
| `or_order` | PK int | Id. (⚠️ el IDENTITY arranca en **1665**, indicio de datos migrados). |
| `or_date` | date | Fecha del pedido. |
| `or_client_name` | varchar(200) | Nombre del cliente (texto libre, MAYÚSCULAS). |
| `or_phone` | varchar(20) | Teléfono. |
| `or_product_description` | varchar(max) | Descripción de lo encargado (MAYÚSCULAS). |
| `or_price` | float | Precio acordado. |
| `or_anticipe` | float | **Anticipo/adelanto** entregado. |
| `or_observation` | varchar(max) | Observaciones. |
| `or_state` | bit | Estado (p.ej. pendiente/entregado). |
| `or_user_owner` | FK → user | Usuario dueño del pedido (define permisos de edición). |

> Saldo del pedido = `or_price − or_anticipe` (calculado en UI).

### COTIZACIÓN (`t_quotes`)
Presupuesto/proforma entregado a un cliente.

| Campo | Tipo | Significado |
|---|---|---|
| `qu_quote` | PK int | Id. |
| `qu_date` | date | Fecha. |
| `qu_client_name` | varchar(200) | Cliente (texto libre, MAYÚSCULAS). |
| `qu_phone` | varchar(20) | Teléfono. |
| `qu_detail` | varchar(max) | Detalle de lo cotizado (MAYÚSCULAS). |
| `qu_price` | float | Precio cotizado. |
| `qu_supplier` | varchar(200) | Proveedor **como texto libre** (no FK). |
| `qu_user_owner` | FK → user | Usuario dueño de la cotización. |

---

## Usuarios y seguridad

### USUARIO (`t_users`)
| Campo | Tipo | Significado / reglas |
|---|---|---|
| `us_user` | PK int | Id. |
| `us_login` | varchar(20) | Nombre de usuario (único; se valida que no exista al crear). |
| `us_password` | varchar(20) | ⚠️ **Contraseña en TEXTO PLANO** (grave). En la nueva app: **hash** (bcrypt/argon2). |
| `us_name` | varchar(50) | Nombre. |
| `us_last_name` | varchar(50) | Apellido paterno. |
| `us_mother_last_name` | varchar(50) | Apellido materno. |
| `us_master` | bit | **1 = administrador.** Se crea siempre en **0** desde la app (el master se define directamente en BD). |
| `us_enable` | bit | **1 = habilitado.** Se crea en **1**. Un usuario en 0 no puede iniciar sesión. |

---

## Resumen de relaciones (FKs)

| Desde | Campo | Hacia |
|---|---|---|
| product | pr_trademark | trademark |
| product | pr_category | category |
| product | pr_warranty | warranty |
| stock | st_product | product |
| sale | sa_client | client |
| sale | sa_user | user |
| sale_product | sp_sale | sale |
| sale_product | sp_product | product |
| purchase | pu_supplier | supplier |
| purchase | pu_user | user |
| purchase_product | pp_purchase | purchase |
| purchase_product | pp_product | product |
| orders | or_user_owner | user |
| quotes | qu_user_owner | user |
| client | cl_person / cl_business | ⚠️ person / business (sin uso) |
