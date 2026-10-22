# Contrato de integración: Comprobantes, caja y reportes

Este documento registra los supuestos usados por el módulo de Eduardo y los puntos que debe confirmar el responsable de Ventas antes de asociar ventas nuevas a caja o definir la emisión.

## Ventas y comprobantes

- El módulo de Eduardo solo consulta filas existentes de `Ventas` y `DetalleVentas`; no crea ventas ni modifica importes.
- Para listarse como comprobante, `Venta.TipoComprobante` debe ser `Boleta` o `Factura` (comparación sin distinguir mayúsculas/minúsculas).
- `NumeroComprobante` debe ser único, persistido en la misma venta, y tener una regla de serie/correlativo acordada por Edson. El módulo de Eduardo no lo genera.
- `Venta.SubTotal`, `Venta.Igv` y `Venta.Total` se consideran valores históricos. Las vistas y exportaciones los imprimen sin recalcularlos. El texto “referencia 18%” no certifica que los datos históricos usen esa tasa.
- El flujo actual de ventas trata `Producto.PrecioVenta` como base antes del IGV: suma los precios por cantidad una sola vez, calcula IGV al 18% de esa base con redondeo monetario a dos decimales y suma el impuesto al total. La interfaz de caja de ventas usa la misma fórmula. El esquema no tiene una marca que indique si el precio incluye IGV; Edson debe confirmar este contrato comercial antes de usar precios tax-inclusive. El módulo no recalcula importes ya persistidos.
- Una representación PDF de la venta no equivale a un comprobante electrónico validado por SUNAT.

## Caja y pagos

- La caja es por usuario; se impide que el mismo usuario tenga dos arqueos con estado `Abierta` mediante transacción serializable. El registro de una venta requiere una caja abierta del usuario autenticado y guarda su `IdArqueo` en esa venta dentro de la transacción.
- Se consideran ventas de caja solo filas con `IdArqueo` igual al arqueo y `Estado = Completada`.
- Métodos reconocidos: `Efectivo` se contabiliza como efectivo; `Yape/Plin`, `Yape`, `Plin`, `Tarjeta`, `Transferencia` y `Digital` se contabilizan como digital (sin separación de tipos de tarjeta). Comparación sin distinguir mayúsculas/minúsculas.
- Cualquier método distinto aparece como `NoClasificado` y se excluye de ambos subtotales. Edson debe acordar y persistir un catálogo estable de `MetodoPago` antes de operar.
- Monto esperado físico = fondo inicial + ventas completadas clasificadas como Efectivo. Total digital se presenta aparte; no se inventan egresos porque el esquema no tiene tabla para ellos.
- Las ventas nuevas no pueden confirmarse sin una caja abierta y quedan asociadas al arqueo del usuario. Las ventas históricas cuyo `IdArqueo` sea `NULL` no se asignan retrospectivamente a una caja.

## Reportes

- El reporte de ventas incluye el estado solicitado (por defecto, `Completada`); el resumen financiero suma solo las filas `Completada`, incluso si la tabla muestra `Todos`.
- El reporte de movimientos consulta `MovimientosInventario` y sus referencias a producto y usuario; no realiza operaciones de Kardex ni modifica inventario.
- Los reportes usan fechas de calendario locales del servidor y el rango incluye todo el día final.
