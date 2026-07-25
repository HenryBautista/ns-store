# 03 — Lógica de negocio y flujos

> Este documento captura las **reglas de negocio** y **workflows** reales, extraídos del
> comportamiento del código legacy (no de los stored procedures, que son solo CRUD). Es la
> parte más importante para reconstruir el sistema fielmente.

---

## 1. Autenticación y sesión

**Flujo de login:**
1. El usuario ingresa `login` + `password`.
2. Se busca el usuario por `login`.
3. El acceso se concede **solo si**: existe, la contraseña coincide **exactamente**, y `us_enable = true`.
4. Al entrar, se guarda `CurrentUser` en sesión y se muestra el dashboard.

**Reglas:**
- Contraseña comparada en texto plano (⚠️ reemplazar por hash + verificación segura).
- Si el usuario es **normal** (`us_master = 0`), el botón/módulo **Usuarios** se oculta.
- **Cerrar sesión** limpia el usuario actual y vuelve al login.

---

## 2. Modelo de doble precio: con factura / sin factura

Cada producto tiene **dos precios de venta**:
- `pr_price_no_bill` — precio **sin factura** (neto, sin comprobante fiscal).
- `pr_price_bill` — precio **con factura** (incluye impuesto).

En la **venta**, un checkbox "factura" decide qué precio se aplica a **toda** la venta.
En la **compra**, también se distingue compra con/sin factura (`pu_type_purchase`).

### Sugerencia automática de precio (módulo Precios de venta)

Cuando se fija el precio de un producto, el sistema **sugiere** precios a partir del
**precio de la última compra** del producto (`purchase_product` más reciente):

```
precioUltimaCompra           = último pu/pp unit_price del producto
gananciaMargen (30%)         = precioUltimaCompra * 0.30
precioSinFactura             = precioUltimaCompra + gananciaMargen        (= última compra + 30%)
precioConFactura             = precioSinFactura + (precioSinFactura * 0.16) (= +16% IVA)
```

**Reglas:**
- Constantes legacy: **margen 30%**, **IVA 16%**. *(Parametrizar en la nueva app; confirmar tasa de IVA vigente.)*
- Si el producto **aún no tiene precio** (`pr_price_bill = 0`) y hay compras previas → se **sugieren** ambos precios (editables) y se muestra el precio de última compra.
- Si no hay compras previas → "no se puede sugerir precio".
- Si el producto **ya tiene precio** → se muestran los precios actuales y, como referencia, la sugerencia basada en la última compra.
- El usuario puede aceptar la sugerencia o escribir precios manualmente. Al guardar se actualizan `pr_price_bill` y `pr_price_no_bill`.

---

## 3. Gestión automática de stock

El stock **no se edita a mano** de forma habitual: se mueve como efecto de compras y ventas.

### Entrada de stock (por COMPRA)
Por cada línea de la compra:
```
si el producto YA tiene fila en stock:
    stock.cantidad += cantidadComprada     (UPDATE)
si NO tiene fila:
    crear fila de stock con cantidadComprada   (INSERT)
en ambos casos: registrar fecha de modificación = hoy
```

### Salida de stock (por VENTA)
Por cada línea de la venta:
```
si cantidadVendida == cantidadEnStock:
    ELIMINAR la fila de stock            ⚠️ (legacy) — en la nueva app: dejar en 0
si cantidadVendida <  cantidadEnStock:
    insertar un delta negativo → efectivamente stock.cantidad -= cantidadVendida
```
> El mecanismo legacy reutiliza la misma rutina de "entrada" pasando cantidad **negativa**.
> En la nueva app conviene un movimiento de inventario explícito (ver más abajo).

### Validación de existencias antes de vender
- Antes de agregar/confirmar una venta se comprueba **"¿hay suficiente de este producto?"**
  (`cantidadEnStock >= cantidadRequerida`).
- En la pantalla de venta, al cambiar la cantidad se muestra la **cantidad disponible** y una
  **advertencia** si no alcanza.
- El botón **Vender** bloquea la operación si no hay stock suficiente.

> ⚠️ **Sin transaccionalidad ni control de concurrencia**: registrar venta y descontar stock
> son operaciones separadas, sin transacción. Riesgo de inconsistencia (venta registrada sin
> descontar stock, o sobreventa por concurrencia). **La nueva app debe hacer venta+stock en
> una transacción atómica y con control de concurrencia.**

### Kardex (resumen por producto)
Para cada producto:
```
total_vendido   = Σ cantidades en ventas del producto
total_comprado  = Σ cantidades en compras del producto
disponible      = cantidad actual en stock
```
Se puede filtrar por nombre de producto.

---

## 4. Flujo de COMPRA

**Objetivo:** registrar mercadería que ingresa desde un proveedor y actualizar stock.

**Pasos:**
1. El usuario abre Compras y selecciona un **proveedor** (desde el módulo Proveedores).
2. Selecciona un **producto** (desde el catálogo) e ingresa **precio de compra** y **cantidad**.
3. Agrega la línea al **carrito** (puede repetir para varios productos).
4. Elige **tipo** (con/sin factura) y **estado** (pagada/crédito).
5. Confirma la compra:
   - Se crea la **cabecera** de compra (`t_purchase`) con fecha, cantidad total, total, tipo, estado, proveedor y usuario.
   - Se crea una **línea** (`t_purchase_product`) por cada ítem del carrito, con `total_linea = cantidad × precioUnitario`.
   - Se **suma al stock** cada producto del carrito.
6. Mensaje de éxito y se refresca la lista de compras.

**Reglas:**
- Debe existir al menos un ítem en el carrito y un proveedor seleccionado.
- El total de la compra = Σ (cantidad × precioUnitario) de las líneas.
- La compra queda asociada al **usuario** que la registró.
- El **precio de última compra** alimenta la sugerencia de precio de venta.

---

## 5. Flujo de VENTA (POS) — el más importante

**Objetivo:** vender productos a un cliente, descontar stock, registrar cobro (contado o crédito) y emitir nota de garantía.

**Pasos:**
1. El usuario abre Ventas y selecciona un **cliente** (desde el módulo Clientes). Los datos del
   cliente (persona o empresa) se cargan en la pestaña correspondiente.
2. Selecciona un **producto**. Se muestran sus precios (con/sin factura) y la **cantidad disponible**.
3. Marca o no el checkbox **"factura"** (define qué precio se usa en toda la venta).
4. Ingresa **cantidad** (se valida contra stock) y agrega al **carrito**.
5. El sistema calcula:
   - `total = Σ (cantidad × precioSegunFactura)` de las líneas del carrito.
   - `cantidadTotal = Σ cantidades`.
6. Elige el **estado**: contado (pagado) o crédito.
7. Ingresa **monto pagado** (`total_paid`). El sistema calcula el **saldo** = total − pagado.
8. Al confirmar **Vender** (con stock suficiente, carrito no vacío y cliente seleccionado):
   - Se crea la **cabecera** de venta (`t_sale`): cantidad total, total, pagado, estado, cliente, fecha, usuario.
   - Se crea una **línea** (`t_sale_product`) por producto, con el precio unitario según con/sin factura.
   - Se **descuenta el stock** de cada producto.
   - Se **genera e imprime** la nota de garantía / comprobante (ver §7).
   - Mensaje de éxito y cierre de la pantalla.

**Reglas:**
- No se puede vender sin **cliente** y sin **al menos un producto** en el carrito.
- No se puede vender más de lo que hay en **stock**.
- El precio (con/sin factura) se aplica **a toda la venta**, no por línea.
- La venta queda asociada al **usuario** (vendedor).

---

## 6. Ventas a crédito y cobranza (deudas)

Una venta con `state = 0` (crédito) queda como **deuda**: `saldo = total − pagado > 0`.

**Vista de Deudas / No pagadas (en el dashboard):**
- Lista todas las ventas con estado crédito (no pagadas), mostrando cliente, producto, total, pagado y saldo (`rest`).
- Se puede **buscar por nombre de cliente**.

**Operaciones de cobranza:**
1. **Pago total (`PaidSale`)** — marca la venta como pagada (`state = 1`). Deja de aparecer en Deudas.
2. **Pago parcial / cambio de estado (`ChangePaidAndState`)** — desde el detalle de la venta se
   actualiza el **monto pagado** y el **estado**; recalcula el saldo. (estado > 0 ⇒ pagado.)

**Reglas:**
- El saldo pendiente se calcula siempre como `total_price − total_paid`.
- Al saldar completamente, la venta sale del listado de deudas.

> ⚠️ El legacy **no registra cada abono** como transacción independiente (solo actualiza el
> acumulado `total_paid`). Para trazabilidad, la nueva app debería tener una tabla de **pagos/abonos**.

---

## 7. Comprobantes y notas de garantía

Al concretar una venta se emite un reporte que depende del **estado de la venta**:
- Venta **pagada** (`state = 1`) → **Nota de garantía** (comprobante estándar).
- Venta a **crédito** (`state = 0`) → **Nota de garantía de crédito** (variante que refleja el saldo).

El comprobante incluye la cabecera de la venta + el detalle (carrito). La nota de garantía
lista las garantías de los productos vendidos (por eso el producto referencia una garantía).

Además, desde "Ventas registradas" se puede **reimprimir** la nota de garantía de una venta.

---

## 8. Flujo de PEDIDOS (encargos)

**Objetivo:** registrar un artículo que el cliente **encarga** (puede no estar en catálogo/stock), dejando un anticipo.

**Pasos y reglas:**
1. Se registra: fecha, nombre de cliente (texto), teléfono, descripción del producto, precio, anticipo, observación, estado.
2. El pedido queda asociado al **usuario dueño** (`user_owner`).
3. **Validación de anticipo:** el anticipo **no puede superar** el precio; si se intenta, se
   resetea a 0 y se avisa "Saldo superior al precio". El **saldo** = precio − anticipo.
4. **Permisos:**
   - Un **master** puede editar/eliminar **cualquier** pedido.
   - Un **usuario normal** solo puede **editar sus propios** pedidos y **no puede eliminar**.
5. **Búsqueda** por nombre de cliente, descripción de producto y/o fecha (combinable).
6. Se puede **imprimir** un reporte del pedido.

---

## 9. Flujo de COTIZACIONES

**Objetivo:** generar un presupuesto/proforma para un cliente.

**Pasos y reglas:**
1. Se registra: fecha, nombre de cliente (texto), teléfono, detalle, precio, proveedor (texto libre).
2. Queda asociada al **usuario dueño**.
3. **Permisos** idénticos a pedidos: master edita/elimina todo; normal solo edita las propias y no elimina.
4. **Búsqueda** por nombre de cliente y/o fecha.
5. Se puede **imprimir** un reporte de la cotización.

---

## 10. Gestión de clientes

- Un cliente es **persona natural** o **empresa** (pestaña/selector define el tipo).
- **Persona:** nombre, apellido paterno, apellido materno, CI, NIT, teléfono, correo.
- **Empresa:** razón social (name), NIT, teléfono, correo, ciudad, dirección, contacto.
- Todos los textos se guardan en **MAYÚSCULAS**.
- **Búsqueda** por nombre / apellido paterno / apellido materno (coincidencia parcial).
- El cliente seleccionado se puede "enviar" a la pantalla de venta.

---

## 11. Gestión de productos y catálogos

- **Producto:** se crea con nombre, parte, descripción, serie y se eligen **marca, garantía y
  categoría** de sus catálogos. Precios inician en **0** (se fijan en Precios de venta).
- **Marcas, categorías, garantías, proveedores:** ABM simple (alta, edición, borrado, búsqueda).
- El mismo módulo de Productos se reutiliza en **modo selección** cuando se invoca desde Venta o
  Compra (al elegir un producto, lo devuelve a la pantalla que lo llamó).
- Eliminar un producto pide **confirmación**.

> ⚠️ **Integridad referencial en borrados:** el legacy borra con `DELETE` directo. Borrar un
> producto/cliente/proveedor con ventas o compras asociadas rompería integridad. La nueva app
> debe usar **borrado lógico** (soft delete) o impedir el borrado si hay dependencias.

---

## 12. Reglas transversales y constantes

| Regla / constante | Valor legacy | Nota para la nueva app |
|---|---|---|
| Margen de ganancia sugerido | **30%** | Parametrizable por producto/categoría. |
| IVA para precio con factura | **16%** | Confirmar tasa fiscal vigente; parametrizar. |
| Normalización de texto | `MAYÚSCULAS` al guardar | Decidir si se mantiene o se preserva capitalización. |
| Estado de venta | `0 = crédito`, `1 = pagado` | Usar enum explícito. |
| Estado/tipo de compra | `bit` con/sin factura y pagada/crédito | Usar enums explícitos. |
| Fecha de transacciones | `date` (sin hora) | Usar `datetime`/timestamp con zona horaria. |
| Precios | `float` | Usar `decimal`. |
| Rol | `master` / normal | Sistema de roles y permisos granular. |
