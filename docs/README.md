# NS_Store — Documentación funcional y de negocio

> Documentación de ingeniería inversa del sistema legacy **NS_Store**, elaborada como
> especificación de referencia para construir una **nueva aplicación web moderna desde cero**.
> Ningún código legacy será reutilizado; este documento captura únicamente **qué hace** el
> sistema y **por qué**, no cómo está implementado.

## ¿Qué es NS_Store?

NS_Store es un sistema de **gestión de tienda / punto de venta (POS) e inventario** para un
comercio de venta de artículos (por el modelo de datos —número de parte, serie, marca,
garantía— corresponde a una **tienda de computación / electrónica**). Es un producto de un
solo local (single-store), monoempresa.

El contexto es **boliviano**: se manejan conceptos como **NIT**, **CI (Carnet de Identidad)**,
ventas **con factura / sin factura**, y **cotizaciones/pedidos** en español.

### Tecnología legacy (solo como referencia, NO se reutiliza)

| Capa | Tecnología legacy |
|------|-------------------|
| Cliente | Aplicación de escritorio **WPF** (.NET Framework, C#), Visual Studio 2015/2017 |
| Backend | Lógica en el propio cliente + **Stored Procedures** en SQL Server |
| Base de datos | **SQL Server** (`ns_store`) |
| Reportes | **RDLC** (Microsoft ReportViewer) |

El patrón legacy es: cada vista (`*_View.xaml.cs`) llama a una clase de servicio
(`*Services.cs`), que ejecuta un stored procedure `sp_<entidad>` con un parámetro
`@i_accion` (`S1`, `S2`, `I1`, `U1`, `D1`, `F1`…) que multiplexa Select/Insert/Update/Delete/Find.

## Índice de la documentación

| Documento | Contenido |
|-----------|-----------|
| [01 — Visión general del negocio](01-vision-general.md) | Dominio, actores, glosario, mapa de módulos |
| [02 — Modelo de datos y entidades](02-modelo-datos.md) | Todas las entidades, campos, tipos, relaciones, diagrama ER |
| [03 — Lógica de negocio y flujos](03-logica-negocio.md) | Reglas de negocio, workflows (venta, compra, stock, crédito, precios) |
| [04 — Especificación por módulo](04-modulos.md) | Cada pantalla/módulo y sus casos de uso funcionales |
| [05 — Recomendaciones para la nueva app](05-recomendaciones.md) | Deudas técnicas, bugs detectados, decisiones para el rediseño |

## Resumen ejecutivo del alcance funcional

El sistema cubre estos **módulos**:

1. **Autenticación y usuarios** — login, roles (master/normal), habilitar/deshabilitar.
2. **Catálogo de productos** — productos con marca, categoría, garantía, nº de parte, serie.
3. **Catálogos auxiliares** — marcas, categorías, garantías, proveedores.
4. **Clientes** — persona natural o empresa (razón social/NIT).
5. **Compras** — registro de compras a proveedores (multi-producto, con/sin factura, crédito).
6. **Inventario / Stock** — cantidades por producto, actualización automática con compras/ventas.
7. **Precios de venta** — doble precio (con/sin factura), sugerencia de precio por margen.
8. **Ventas / POS** — venta multi-producto tipo carrito, con/sin factura, contado o crédito.
9. **Cobros / Deudas** — seguimiento de ventas a crédito y registro de pagos.
10. **Pedidos (encargos)** — solicitudes de artículos con anticipo.
11. **Cotizaciones** — presupuestos a clientes.
12. **Kardex** — resumen por producto: comprado, vendido, disponible.
13. **Reportes** — factura/nota de garantía, ventas, compras, stock, deudas, pedidos, cotizaciones, lista de precios.

Detalle completo en los documentos siguientes.
