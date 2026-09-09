# Dominio: vocabulario e invariantes de negocio

El negocio se piensa y se habla en **español**; el código es **íntegramente en inglés**
(ver [`D-02`](decisions.md#d-02)). Esta tabla es el puente, y es de uso
obligatorio: sin un equivalente fijado de antemano, dos sesiones bautizan lo mismo distinto y
quedan dos nombres para un solo concepto.

**Término de negocio nuevo ⇒ fila nueva aquí, antes de escribir el identificador.**

El paréntesis en la primera columna es el nombre del sistema legacy (WPF + SQL Server), que ya
no se usa en código pero sí aparece cuando el negocio habla.

## Glosario

| Término | En código | Significado |
|---------|-----------|-------------|
| **Con factura / Sin factura** | `InvoiceType.WithInvoice` / `.WithoutInvoice` · `Product.PriceWithInvoice` / `.PriceWithoutInvoice` | Modalidad de precio y venta. "Con factura" incluye IVA; "sin factura" es el precio neto sin comprobante fiscal. Cada producto tiene **ambos precios** |
| **NIT** | `Client.Nit` | Número de Identificación Tributaria (identificador fiscal de empresa en Bolivia) |
| **CI** | `Client.Ci` | Carnet de Identidad (documento de persona natural) |
| **Anticipo** (`anticipe`) | `Order.AdvanceAmount` | Adelanto de dinero que deja un cliente al hacer un pedido |
| **Saldo / Resto** (`rest`) | `Sale.Balance` · `Order.Balance` | Pendiente de pago = total − pagado |
| **Crédito / Deuda** | `PaymentStatus.Credit` | Venta no pagada totalmente. Aparece en la vista de deudas |
| **Contado / Pagado** | `PaymentStatus.Paid` | Venta cancelada completamente |
| **Pedido / Encargo** (`order`) | `Order` | Artículo que el cliente encarga y puede no existir aún en stock, con anticipo |
| **Cotización** (`quote`) | `Quote` | Presupuesto / proforma que se entrega a un cliente |
| **Kardex** | `Kardex` · `GET /kardex` | Movimientos por producto: total comprado, total vendido, disponible |
| **Nota de garantía** | `WarrantyNoteDto` · `GET /reports/warranty-note` | Comprobante que se imprime al vender. Hace también de recibo |
| **Marca** (`trademark`) | `Trademark` | Fabricante del producto (HP, Samsung, …) |
| **Categoría** | `Category` | Agrupación de productos |
| **Garantía (plazo)** | `WarrantyTerm` | Plazo como texto libre: "6 MESES", "1 AÑO", "SIN GARANTÍA" |
| **Proveedor** | `Supplier` | A quién se le compra mercadería |
| **Número de parte** (`part`) | `Product.PartNumber` | Código de parte del fabricante |
| **Serie** (`serie`) | `ProductSerial` · `Product.IsSerialized` | Seguimiento por unidad, opcional por producto |
| **Movimiento de inventario** | `InventoryMovement` · `MovementType` | El libro mayor del stock: toda entrada o salida deja una fila. Es la fuente de verdad |
| **Existencia / Nivel de stock** | `StockLevel` | Caché de cantidad por producto **y sucursal**. Solo se modifica vía `Apply` |
| **Ajuste de stock** | `MovementType.Adjustment` · `POST /stock/adjustments` | Corrección manual de inventario, solo admin |
| **Cobro** | `Payment` · `POST /sales/collections` | Pago de una venta a crédito |
| **Recibo de cobro** | `PaymentReceipt` · `Payment.ReceiptId` | Comprobante numerado y reimprimible de un cobro |
| **Vendedor** | `UserRole.Seller` | Usuario normal. Edita solo lo suyo, no borra |
| **Administrador** | `UserRole.Admin` | Ve el módulo de usuarios; edita y borra cualquier cosa |
| **Sucursal** | `Branch` | Local físico. El sistema **es** multi-sucursal: `Product.StockLevels` tiene una fila por sucursal, y los documentos se numeran por sucursal |
| **Transferencia de stock** | `StockTransfer` · `StockTransferItem` | Movimiento de mercadería entre sucursales |
| **Configuración** | `AppSetting` · `GET /settings` | IVA, margen por defecto y moneda. Nunca constantes en el código |

## Invariantes de negocio

Lo que no se deduce leyendo el código en 30 segundos, o que se deduce mal.

### Precio dual

Cada producto guarda **dos** precios de venta. El `InvoiceType` de la venta decide cuál se
aplica, y se aplica a **toda** la venta — no línea por línea.

### Sugerencia de precio

```
precioSinFactura = costoUltimaCompra × (1 + margen)
precioConFactura = precioSinFactura  × (1 + iva)
```

El legacy tenía el margen y el IVA como constantes (30 % y 16 %). **En la app nueva no son
constantes**: viven en `app_settings` y se leen de ahí, sembrados con esos mismos valores por
continuidad. Hardcodearlos vuelve a introducir el defecto que se estaba arreglando.

Si el producto no tiene compras previas, no se puede sugerir precio — no es un error, es un
estado válido (`NO_PURCHASE_HISTORY`).

### Anticipos

El anticipo de un pedido **no puede superar** el precio (`ADVANCE_EXCEEDS_PRICE`). El saldo es
precio − anticipo.

### Nota de garantía

Tiene dos variantes según el estado de la venta: pagada emite la nota estándar; a crédito emite
la variante que refleja el saldo. Se puede reimprimir desde ventas registradas.

### Convenciones heredadas del legacy que siguen vivas

- **Moneda única**, boliviano (Bs). No hay tipos de cambio.
- El legacy guardaba todo el texto en **MAYÚSCULAS** (`ToUpper()` al insertar). Los datos
  migrados lo reflejan; la app nueva no lo impone.

## Lo que el legacy hacía mal y no se replica

El sistema anterior (WPF + SQL Server) borraba la fila de stock al llegar a cero, guardaba
dinero en `float`, no envolvía venta y descuento de stock en una transacción, y almacenaba las
contraseñas en texto plano. Cada una de esas tiene hoy su decisión explícita en
[`decisions.md`](decisions.md). El detalle histórico completo quedó en el repo archivado
`ns-store-docs`.
