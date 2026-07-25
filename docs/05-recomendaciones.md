# 05 — Recomendaciones para la nueva aplicación web

> Deudas técnicas, bugs y decisiones de rediseño detectados durante la ingeniería inversa.
> El objetivo es **no arrastrar** los problemas del legacy a la nueva app.

---

## 1. Problemas críticos del legacy (NO replicar)

### 1.1 Seguridad
| Problema | Impacto | Solución en la nueva app |
|---|---|---|
| **Contraseñas en texto plano** (`us_password varchar(20)`) | Grave. | Hash con **bcrypt/argon2**; nunca almacenar ni loguear la contraseña. |
| **Cadena de conexión con usuario `sa` y contraseña en el código** (`DBServices.cs`) | Grave. Credenciales de admin de BD versionadas. | Secrets/variables de entorno; usuario de BD con privilegios mínimos; nunca en el repo. |
| Comparación de contraseña en cliente | El cliente decide el acceso. | Autenticación en el **backend**; tokens/sesiones (JWT o sesión server-side). |
| Sin control de permisos granular (solo master/normal) | Rígido. | Sistema de **roles y permisos** (RBAC). |
| Sin bloqueo de cuenta / auditoría de acceso | — | Rate limiting, intentos fallidos, auditoría. |

### 1.2 Integridad y consistencia de datos
| Problema | Solución |
|---|---|
| **Venta y descuento de stock sin transacción** (operaciones separadas) | Ejecutar venta + líneas + movimiento de stock en **una transacción atómica**. |
| **Sin control de concurrencia** en stock (riesgo de sobreventa) | Bloqueo optimista/pesimista o verificación de stock dentro de la transacción. |
| **Borrado físico** (`DELETE`) de productos/clientes/proveedores con dependencias | **Soft delete** o rechazo si hay ventas/compras asociadas. |
| **Al vender toda la existencia, se ELIMINA la fila de stock** | Mantener la fila en **0**; el stock es un dato permanente por producto. |
| `float` para precios y montos | Usar **`decimal`** (evita errores de redondeo monetario). |
| Fechas `date` sin hora | Usar **`datetime`/timestamp** con zona horaria. |
| **`catch(Exception){}` vacíos en todo el código** (fallos silenciosos) | Manejo de errores real, logging, y feedback al usuario. |

### 1.3 Modelo de datos redundante / muerto
| Elemento | Estado | Acción |
|---|---|---|
| `t_person`, `t_business` (+ FKs `cl_person`, `cl_business`) | **Sin uso** | Eliminar; modelar tipo de cliente con discriminador. |
| `t_sale_price` | Duplica precios de `t_product` | Eliminar; si se quiere historial de precios, tabla de historial con fecha. |
| Columnas monoproducto en `t_sale` (`sa_product`, `sa_unit_price`) y `t_purchase` (`pu_product`, `pu_unit_price`) | Legacy previo al carrito | Eliminar; el detalle vive en las tablas de líneas. |
| `t_sale_product.sp_total_price` declarado como **`bit`** en el script SQL | **Bug de tipo** | Debe ser `decimal`. |
| Script SQL del repo desactualizado (faltan `t_purchase_product`, acciones S10–S13, `sp_sale_product`, etc.) | El código C# es la fuente de verdad | Rediseñar esquema desde cero según esta documentación. |

### 1.4 Bugs de lógica detectados
- **`sp_sale` acción S3** hace `FROM t_sale, t_product, t_client` **sin cláusula de join** → producto cartesiano. No replicar; usar joins explícitos.
- Consultas con **subconsultas correlacionadas por fila** (nombres de FK) en vez de `JOIN` → ineficiente. Usar joins.
- **Reglas de negocio en la capa de presentación** (WPF code-behind): el descuento de stock, cálculo de totales, permisos, etc. están en las vistas. En la nueva app: **backend/servicios de dominio**, no en la UI.

---

## 2. Arquitectura sugerida para la nueva app

- **Backend API** (REST o GraphQL) con la lógica de negocio en servicios de dominio; la BD solo persiste.
- **Frontend web** SPA (React/Vue/Angular) o SSR (Next/Nuxt), responsive.
- **BD relacional** (PostgreSQL/SQL Server) con esquema limpio, FKs, constraints e índices.
- **Transacciones** para operaciones compuestas (venta, compra, cobro).
- **Migraciones** versionadas del esquema.
- **Autenticación** con hash de contraseñas + tokens; **RBAC** para permisos.
- **Reportes** como exportación PDF y vistas imprimibles.
- **Auditoría**: registrar quién y cuándo creó/modificó transacciones.

---

## 3. Mejoras funcionales recomendadas (más allá de la paridad)

| Área | Mejora |
|---|---|
| **Inventario** | Tabla de **movimientos de inventario** (entrada/salida/ajuste) con motivo y referencia, en vez de mutar solo un contador. Da kardex real e historial. |
| **Cobros** | Tabla de **abonos/pagos** por venta (fecha, monto, usuario) para trazabilidad de crédito. |
| **Precios** | Historial de precios; margen e IVA **parametrizables** (no hardcodear 30%/16%). Confirmar la **tasa de IVA vigente** (el legacy usa 16%). |
| **Productos** | Código de barras/SKU; imágenes; unidades de medida. |
| **Clientes/Proveedores** | Estado de cuenta; múltiples contactos; validación de NIT/CI. |
| **Pedidos** | Vincular pedido → venta cuando se concreta; convertir cotización → venta. |
| **Multi-usuario** | Concurrencia real; sesiones; auditoría. |
| **Reportes** | Dashboard con KPIs (ventas del día, deudas totales, stock bajo, más vendidos). |
| **Alertas** | Aviso de **stock mínimo/bajo**; deudas vencidas. |
| **Datos** | Normalización opcional (no forzar MAYÚSCULAS); soporte de zona horaria y moneda explícita. |

---

## 4. Checklist de paridad funcional (mínimo para reemplazar el legacy)

- [ ] Login con roles (admin / vendedor) y habilitar/deshabilitar usuarios.
- [ ] ABM de productos, marcas, categorías, garantías, proveedores, clientes (persona/empresa).
- [ ] Fijar precios con/sin factura, con sugerencia por última compra + margen + IVA.
- [ ] Registrar compras multi-producto (con/sin factura, pagada/crédito) que **suman stock**.
- [ ] Registrar ventas multi-producto (POS) que **validan y descuentan stock**, con/sin factura, contado/crédito.
- [ ] Emitir **nota de garantía** (normal y de crédito) al vender; reimprimir.
- [ ] Gestión de **deudas** y **cobros** (pago total y parcial).
- [ ] Consulta de ventas por fecha y por cliente.
- [ ] **Kardex** por producto (comprado / vendido / disponible).
- [ ] **Pedidos** con anticipo y saldo, permisos por dueño.
- [ ] **Cotizaciones**, permisos por dueño.
- [ ] Reportes: ventas, compras, stock, deudas, lista de precios, pedido, cotización.
- [ ] Dashboard con los tableros del `Main_View`.

---

## 5. Datos a confirmar con el negocio antes de construir

1. **Tasa de IVA** correcta (el legacy usa 16%; verificar la vigente).
2. **Margen de ganancia** por defecto (legacy 30%) — ¿fijo, por categoría, por producto?
3. ¿Se mantiene el modelo **con factura / sin factura** tal cual?
4. **Moneda** y si se requiere multi-moneda / tipo de cambio.
5. ¿Se necesita **multi-sucursal** a futuro? (afecta el modelo de datos desde el inicio).
6. Política de **numeración** de comprobantes/facturas (¿requisitos fiscales?).
7. ¿Migración de **datos históricos** del SQL Server actual? (los IDs de pedidos empiezan en 1665, hay datos productivos).
