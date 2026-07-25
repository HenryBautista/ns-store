# 01 — Visión general del negocio

## 1. Dominio

NS_Store gestiona la operación de una **tienda minorista de electrónica/computación**:
compra mercadería a proveedores, la mantiene en inventario, le fija precios y la vende a
clientes (al contado o a crédito), además de gestionar pedidos especiales y cotizaciones.

El eje del sistema es el **producto** y su **stock**, alrededor del cual giran los dos
movimientos económicos principales:

```
        COMPRA (entra stock)                VENTA (sale stock)
Proveedor ─────────────────► [ STOCK ] ─────────────────► Cliente
             +cantidad          por         −cantidad
                              producto
```

## 2. Actores / Roles

| Rol | En el sistema | Permisos |
|-----|---------------|----------|
| **Usuario master** (`us_master = 1`) | Administrador | Todo. Ve el módulo de **Usuarios**. Puede **eliminar** y **editar** pedidos/cotizaciones de cualquier usuario. |
| **Usuario normal** (`us_master = 0`) | Vendedor / operador | Opera ventas, compras, clientes, stock, etc. **No** ve Usuarios. Solo puede **editar sus propios** pedidos/cotizaciones (los que creó). **No** puede eliminar pedidos/cotizaciones. |

- No hay auto-registro: los usuarios los crea un master desde el módulo Usuarios.
- Un usuario puede estar **habilitado o deshabilitado** (`us_enable`); un usuario deshabilitado no puede iniciar sesión.
- La sesión guarda el **usuario actual** (`SessionData.CurrentUser`), que queda como "dueño" (owner) de ventas, compras, pedidos y cotizaciones que registra.

> ⚠️ En el legacy **no existe granularidad de permisos** más allá de master/normal. Para la
> nueva app conviene un modelo de roles/permisos explícito (ver [documento 05](05-recomendaciones.md)).

## 3. Glosario de negocio

| Término | Significado |
|---------|-------------|
| **Con factura / Sin factura** | Modalidad de precio y venta. "Con factura" incluye impuesto (IVA); "sin factura" es el precio neto sin comprobante fiscal. Cada producto tiene **ambos precios**. |
| **NIT** | Número de Identificación Tributaria (identificador fiscal del cliente/empresa en Bolivia). |
| **CI** | Carnet de Identidad (documento de persona natural). |
| **Anticipo (`anticipe`)** | Adelanto de dinero que deja un cliente al hacer un **pedido/encargo**. |
| **Saldo / Resto (`rest`)** | Monto pendiente de pago = total − pagado. Aplica a ventas a crédito y a pedidos. |
| **Crédito / Deuda** | Venta no pagada totalmente (`sa_state = 0`). Aparece en la vista de **Deudas / No pagadas**. |
| **Contado / Pagado** | Venta cancelada completamente (`sa_state = 1`). |
| **Pedido / Encargo (`order`)** | Solicitud de un artículo que el cliente encarga (puede no existir aún en stock), con anticipo. |
| **Cotización (`quote`)** | Presupuesto/proforma que se entrega a un cliente. |
| **Kardex** | Reporte de movimientos por producto: total comprado, total vendido, disponible. |
| **Nota de garantía** | Comprobante que se imprime al vender, indicando la garantía del/los producto(s). Sirve también como factura/recibo. |
| **Marca (`trademark`)** | Fabricante/marca del producto (HP, Samsung, etc.). |
| **Número de parte (`part`)** | Código de parte del fabricante. |
| **Serie (`serie`)** | Número de serie del artículo. |

## 4. Mapa de módulos y navegación

Tras iniciar sesión, el usuario llega a la **pantalla principal (dashboard)** que muestra
tableros de: ventas por fecha, compras, stock, lista de precios, deudas (no pagadas),
pedidos y cotizaciones; y botones que abren cada módulo.

```
Login
  └── Main (Dashboard)
        ├── Catálogos
        │     ├── Productos ──(al seleccionar, alimenta Venta/Compra)
        │     ├── Marcas (Trademark)
        │     ├── Categorías
        │     ├── Garantías (Warranty)
        │     └── Proveedores (Supplier)
        ├── Clientes
        ├── Precios de venta (SalePrice)
        ├── Compras (Purchase)  ── usa Producto + Proveedor
        ├── Ventas / POS (Sale) ── usa Producto + Cliente ──► genera Nota de garantía
        ├── Ventas registradas (Sales) ── cobros / pagos de crédito
        ├── Ventas por cliente (Sales_Client)
        ├── Stock
        ├── Kardex
        ├── Pedidos y Cotizaciones (Orders_Quotes)
        ├── Usuarios (solo master)
        └── Reportes (Report) — invocado desde los módulos anteriores
```

### Dependencias entre módulos (para orden de construcción)

Un producto no puede venderse ni comprarse sin existir; el precio depende de la compra;
el stock depende de compras y ventas. Orden lógico de dependencias:

```
Usuarios ─┐
Marcas    ─┤
Categorías├─► Productos ─► Compras ─► Stock ─► Precios ─► Ventas ─► Cobros
Garantías ─┘                  │                              │
Proveedores ──────────────────┘                              │
Clientes ────────────────────────────────────────────────────┘
Pedidos / Cotizaciones (independientes, solo requieren Usuario)
```

## 5. Consideraciones de contexto

- **Un solo local, una sola empresa.** No hay multi-sucursal ni multi-tenant.
- **Moneda única** (no se especifica; se asume Bs. bolivianos). No hay manejo de tipos de cambio.
- **Idioma:** español. Los datos de texto se guardan en **MAYÚSCULAS** (el legacy hace `ToUpper()` al insertar nombres, descripciones, etc.).
- **Impuesto:** el legacy aplica **16%** como IVA en la sugerencia de precio con factura (ver [documento 03](03-logica-negocio.md)). *Nota: verificar la tasa vigente con el negocio para la nueva app.*
- **Margen de ganancia por defecto:** **30%** sobre el precio de última compra (sugerencia automática).
