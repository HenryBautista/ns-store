# 04 — Especificación funcional por módulo

> Casos de uso por pantalla del legacy. Sirve como checklist de funcionalidad para la nueva app.
> Cada vista WPF (`*_View`) se mapea a una pantalla/ruta web equivalente.

## Índice de pantallas legacy → web

| Vista legacy | Módulo | Ruta web sugerida |
|---|---|---|
| `Login_View` | Login | `/login` |
| `Main_View` | Dashboard | `/` |
| `Product_View` | Productos | `/productos` |
| `Trademark_View` | Marcas | `/marcas` |
| `Category_View` | Categorías | `/categorias` |
| `Warranty_View` | Garantías | `/garantias` |
| `Supplier_View` | Proveedores | `/proveedores` |
| `Client_View` | Clientes | `/clientes` |
| `SalePrice_View` | Precios de venta | `/precios` |
| `Purchase_View` | Compras | `/compras` |
| `Stock_View` | Stock | `/stock` |
| `Sale_View` | Punto de venta (POS) | `/ventas/nueva` |
| `Sales_View` | Ventas registradas / cobros | `/ventas` |
| `Sales_Client_View` | Ventas por cliente | `/ventas/por-cliente` |
| `Kardex_View` | Kardex | `/kardex` |
| `Orders_Quotes_View` | Pedidos y cotizaciones | `/pedidos`, `/cotizaciones` |
| `User_View` | Usuarios (solo master) | `/usuarios` |
| `Report_View` | Visor de reportes | (modales / PDF) |

---

## 1. Login (`/login`)
- **Entrada:** login, contraseña.
- **Acción:** autenticar; si es válido y el usuario está habilitado, ir al dashboard.
- **Errores:** "usuario o contraseña incorrecta"; "Ingrese un usuario y contraseña".

## 2. Dashboard (`/`)
Pantalla principal con múltiples tableros y accesos. Muestra:
- **Usuario actual** (nombre). Oculta "Usuarios" si no es master.
- **Deudas / No pagadas:** lista de ventas a crédito; buscar por cliente; **registrar pago total**; ver **detalle** (abre venta); **reporte** de deudas.
- **Ventas por fecha:** lista; filtrar por **rango de fechas**; **reporte** de ventas.
- **Compras:** lista; **reporte** de compras.
- **Stock:** lista; buscar; **reporte** de stock.
- **Lista de precios:** productos con precios de venta; buscar; **imprimir** lista de precios.
- **Pedidos:** lista; buscar (cliente/producto/fecha); **reporte** de pedido.
- **Cotizaciones:** lista; buscar (cliente/fecha); **reporte** de cotización.
- **Accesos** a todos los módulos y **cerrar sesión**.

## 3. Productos (`/productos`)
- **Listar** todos; **buscar** por nombre.
- **Crear/editar:** nombre, parte, descripción, serie + selección de **marca**, **garantía**, **categoría**.
- **Eliminar** (con confirmación).
- **Modo selección:** cuando se invoca desde Venta o Compra, permite elegir un producto y devolverlo.
- Precios NO se editan aquí (nacen en 0; se fijan en Precios).

## 4. Marcas / 5. Categorías / 6. Garantías / 7. Proveedores
ABM simple (catálogos):
- **Marcas:** nombre.
- **Categorías:** nombre.
- **Garantías:** descripción (texto del plazo).
- **Proveedores:** nombre, teléfono, correo.
- Todos: listar, crear, editar, eliminar, buscar por nombre/descripción.

## 8. Clientes (`/clientes`)
- **Listar** todos; **buscar** por nombre/apellidos.
- **Crear/editar** con selector de tipo:
  - **Persona:** nombre, apellido paterno, apellido materno, CI, NIT, teléfono, correo.
  - **Empresa:** razón social, NIT, teléfono, correo, ciudad, dirección, contacto.
- **Eliminar**.
- **Modo selección** desde la venta (devuelve el cliente elegido al POS).

## 9. Precios de venta (`/precios`)
- **Listar** productos; **buscar**.
- Al seleccionar un producto: mostrar datos + **sugerencia de precios** basada en la última compra
  (última compra +30% = sin factura; +16% = con factura).
- **Fijar** precio con factura y sin factura (editable). Guardar actualiza el producto.

## 10. Compras (`/compras`)
- **Listar** compras (con nombres de proveedor/producto/usuario).
- **Registrar compra:** elegir **proveedor** y **producto**, ingresar precio de compra y cantidad,
  **agregar al carrito** (multi-producto), elegir **tipo** (con/sin factura) y **estado**
  (pagada/crédito), confirmar. Efecto: crea compra + líneas y **suma stock**.
- Quitar líneas del carrito. Cálculo automático de total y cantidad total.

## 11. Stock (`/stock`)
- **Listar** existencias por producto (nombre, parte, descripción, serie, cantidad, marca, fecha mod.).
- **Buscar** por nombre.
- **Reporte** de stock.
- Ver **detalle** de un producto (ventas y compras del producto — ver Detalle).

## 12. Punto de venta / POS (`/ventas/nueva`)
- Elegir **cliente** y **producto(s)**.
- Checkbox **factura** (define precio con/sin factura).
- Ingresar **cantidad** (validada vs stock, muestra disponible y advertencia).
- **Agregar al carrito**; quitar ítems. Cálculo de total, cantidad total.
- Elegir **estado** (contado/crédito) y **monto pagado**; muestra **saldo**.
- **Vender:** valida stock + cliente + carrito; registra venta + líneas; **descuenta stock**;
  **genera nota de garantía** (normal o de crédito).

## 13. Ventas registradas / cobros (`/ventas`)
- **Listar** ventas con datos completos (cliente, producto, cantidades, montos, saldo, estado, vendedor).
- **Buscar** por cliente.
- Ver **detalle** de una venta + su **carrito**.
- **Cobrar:** actualizar **monto pagado** y **estado** (pago parcial o total) → recalcula saldo.
- **Reimprimir** nota de garantía.

## 14. Ventas por cliente (`/ventas/por-cliente`)
- Consultar todas las ventas de un cliente específico.

## 15. Kardex (`/kardex`)
- **Listar** por producto: total comprado, total vendido, disponible.
- **Buscar** por nombre de producto.

## 16. Pedidos y cotizaciones (`/pedidos`, `/cotizaciones`)
Pantalla con dos secciones:
- **Pedidos (encargos):** crear/editar/eliminar con fecha, cliente (texto), teléfono,
  descripción, precio, **anticipo** (validado ≤ precio), observación, estado; muestra saldo;
  buscar; imprimir. Permisos por dueño (ver [doc 03 §8](03-logica-negocio.md)).
- **Cotizaciones:** crear/editar/eliminar con fecha, cliente (texto), teléfono, detalle, precio,
  proveedor (texto); buscar; imprimir. Permisos por dueño.

## 17. Usuarios (`/usuarios`, solo master)
- **Listar** usuarios.
- **Crear:** nombre, apellidos, login, contraseña. Valida que el **login no exista**. Nace como
  no-master y habilitado.
- **Editar:** datos y credenciales.
- **Habilitar / Deshabilitar** usuario.

## 18. Reportes (RDLC → PDF/impresión)
Tipos de reporte que el sistema genera:

| Tipo | Contenido |
|---|---|
| **Nota de garantía** (`sale_invoice` estado pagado) | Comprobante de venta al contado con garantías. |
| **Nota de garantía de crédito** (`sale_invoice` estado crédito) | Comprobante de venta a crédito (refleja saldo). |
| **Reporte de ventas** (`sales_report`) | Ventas por rango de fechas. |
| **Reporte de compras** (`purchases_report`) | Compras. |
| **Reporte de stock** (`stock_report`) | Existencias. |
| **Reporte de deudas** (`no_paid_report`) | Ventas no pagadas. |
| **Reporte de pedido** (`order_report`) | Detalle de un pedido. |
| **Reporte de cotización** (`quote_report`) | Detalle de una cotización. |
| **Lista de precios** (`sale_prices`) | Productos con precios de venta. |

> En la nueva app estos reportes se implementan como **exportación PDF** y/o vistas imprimibles.
