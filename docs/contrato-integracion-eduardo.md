# Contrato de integración: Comprobantes, caja y reportes

Este documento registra los supuestos usados por el módulo de Eduardo y los puntos que debe confirmar el responsable de Ventas antes de asociar ventas nuevas a caja o definir la emisión.

## Ventas y comprobantes

- El módulo de Eduardo solo consulta filas existentes de `Ventas` y `DetalleVentas`; no crea ventas ni modifica importes.
- Para listarse como comprobante, `Venta.TipoComprobante` debe ser `Boleta` o `Factura` (comparación sin distinguir mayúsculas/minúsculas).
- `NumeroComprobante` debe ser único, persistido en la misma venta, y tener una regla de serie/correlativo acordada por Edson. El módulo de Eduardo no lo genera.
- `Venta.SubTotal`, `Venta.Igv` y `Venta.Total` se consideran valores históricos. Las vistas y exportaciones los imprimen sin recalcularlos. El texto “referencia 18%” no certifica que los datos históricos usen esa tasa.
- Para ventas nuevas, Edson debe acordar si el subtotal es base imponible antes del impuesto, el redondeo monetario, y cómo se calcula el total con IGV 18%. `ImpuestoCalculator` documenta el cálculo de referencia `base imponible * 0.18`, redondeado a dos decimales; no se aplica a ventas almacenadas.
- Una representación PDF de la venta no equivale a un comprobante electrónico validado por SUNAT.

## Caja y pagos

- La caja es por usuario; se impide que el mismo usuario tenga dos arqueos con estado `Abierta` mediante transacción serializable.
- Se consideran ventas de caja solo filas con `IdArqueo` igual al arqueo y `Estado = Completada`.
- Métodos reconocidos: `Efectivo` se contabiliza como efectivo; `Yape`, `Plin`, `Tarjeta`, `Transferencia` y `Digital` se contabilizan como digital (sin separación de tipos de tarjeta). Comparación sin distinguir mayúsculas/minúsculas.
- Cualquier método distinto aparece como `NoClasificado` y se excluye de ambos subtotales. Edson debe acordar y persistir un catálogo estable de `MetodoPago` antes de operar.
- Monto esperado físico = fondo inicial + ventas completadas clasificadas como Efectivo. Total digital se presenta aparte; no se inventan egresos porque el esquema no tiene tabla para ellos.
- El módulo de Eduardo no asigna `IdArqueo` a ventas automáticamente. Edson deberá asociar la venta a la caja abierta (o coordinar otro contrato) al confirmar venta. Hasta entonces una caja nueva no incluirá ventas que lleguen con `IdArqueo = NULL`.

## Reportes

- El reporte de ventas incluye el estado solicitado (por defecto, `Completada`); el resumen financiero suma solo las filas `Completada`, incluso si la tabla muestra `Todos`.
- El reporte de movimientos consulta `MovimientosInventario` y sus referencias a producto y usuario; no realiza operaciones de Kardex ni modifica inventario.
- Los reportes usan fechas de calendario locales del servidor y el rango incluye todo el día final.
