using Microsoft.EntityFrameworkCore;
using ElCortijo.Datos;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Negocio.Servicios;

public class VentaService : IVentaService
{
    private readonly ElCortijoDbContext _context;

    public VentaService(ElCortijoDbContext context) => _context = context;

    public async Task<IEnumerable<Venta>> ObtenerTodasAsync(
        string? buscar = null,
        string? tipoComprobante = null,
        string? metodoPago = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var consulta = _context.Ventas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            consulta = consulta.Where(v => v.NumeroComprobante.Contains(buscar)
                || v.Cliente.NombreRazonSocial.Contains(buscar)
                || v.Cliente.NumeroDocumento.Contains(buscar)
                || v.DetalleVentas.Any(d => d.Producto.Nombre.Contains(buscar)));
        }

        if (!string.IsNullOrWhiteSpace(tipoComprobante))
            consulta = consulta.Where(v => v.TipoComprobante == tipoComprobante);

        if (!string.IsNullOrWhiteSpace(metodoPago))
            consulta = consulta.Where(v => v.MetodoPago == metodoPago);

        if (desde.HasValue)
            consulta = consulta.Where(v => v.FechaRegistro >= desde.Value.Date);

        if (hasta.HasValue)
        {
            var limiteHasta = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(v => v.FechaRegistro < limiteHasta);
        }

        return await consulta
            .Include(v => v.Cliente).Include(v => v.Usuario)
            .Include(v => v.DetalleVentas).ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.FechaRegistro)
            .ToListAsync();
    }

    public async Task<IEnumerable<Producto>> ObtenerProductosDisponiblesAsync() => await _context.Productos
        .AsNoTracking().Where(p => p.Estado && p.StockActual > 0).OrderBy(p => p.Nombre).ToListAsync();

    public async Task<(bool Exito, string? Error, int? IdVenta)> RegistrarAsync(VentaRegistroViewModel solicitud, int idUsuario)
    {
        if (solicitud.Items.Count == 0) return (false, "Agregue por lo menos un producto al pedido.", null);
        if (solicitud.Items.GroupBy(i => i.IdProducto).Any(g => g.Count() > 1)) return (false, "Un producto no puede repetirse en el pedido.", null);
        if (solicitud.MetodoPago is not ("Efectivo" or "Yape/Plin" or "Tarjeta" or "Transferencia")) return (false, "Seleccione un método de pago válido.", null);
        if (solicitud.TipoComprobante is not ("Boleta" or "Factura")) return (false, "Seleccione un comprobante válido.", null);

        await using var transaccion = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            Cliente? cliente = null;
            var nombreManual = solicitud.NombreClienteManual?.Trim();
            if (solicitud.IdCliente > 0)
            {
                cliente = await _context.Clientes.SingleOrDefaultAsync(c => c.IdCliente == solicitud.IdCliente && c.Estado);
                if (cliente == null) return (false, "El cliente seleccionado no existe o está inactivo.", null);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(nombreManual)) return (false, "Escriba el nombre del cliente.", null);
                if (solicitud.TipoComprobante == "Factura") return (false, "Para emitir una factura, seleccione un cliente registrado con RUC.", null);
            }

            var ids = solicitud.Items.Select(i => i.IdProducto).ToList();
            var productos = await _context.Productos.Where(p => ids.Contains(p.IdProducto) && p.Estado).ToDictionaryAsync(p => p.IdProducto);
            if (productos.Count != ids.Count) return (false, "Uno de los productos no existe o está inactivo.", null);
            foreach (var item in solicitud.Items)
            {
                if (item.Cantidad < 1) return (false, "Las cantidades deben ser mayores que cero.", null);
                if (productos[item.IdProducto].StockActual < item.Cantidad)
                    return (false, $"Stock insuficiente para {productos[item.IdProducto].Nombre}. Disponible: {productos[item.IdProducto].StockActual}.", null);
            }

            var subtotal = solicitud.Items.Sum(i => productos[i.IdProducto].PrecioVenta * i.Cantidad);
            var igv = Math.Round(subtotal * 0.18m, 2);
            var total = subtotal + igv;
            if (solicitud.MetodoPago == "Efectivo" && solicitud.MontoRecibido < total)
                return (false, "El monto recibido no cubre el total de la venta.", null);

            if (solicitud.TipoComprobante == "Factura" && cliente?.TipoDocumento != "RUC")
                return (false, "Para emitir una factura, el cliente debe tener un RUC registrado.", null);

            if (cliente == null)
            {
                cliente = new Cliente
                {
                    TipoDocumento = "SIN DOCUMENTO",
                    NumeroDocumento = $"CF{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
                    NombreRazonSocial = nombreManual!,
                    Estado = true,
                    FechaRegistro = DateTime.Now
                };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }

            var venta = new Venta
            {
                NumeroComprobante = $"V{DateTime.Now:yyMMddHHmmssfff}",
                TipoComprobante = solicitud.TipoComprobante,
                IdCliente = cliente.IdCliente,
                IdUsuario = idUsuario,
                SubTotal = subtotal,
                Igv = igv,
                Total = total,
                MetodoPago = solicitud.MetodoPago,
                MontoRecibido = solicitud.MetodoPago == "Efectivo" ? solicitud.MontoRecibido : total,
                Vuelto = solicitud.MetodoPago == "Efectivo" ? solicitud.MontoRecibido - total : 0,
                FechaRegistro = DateTime.Now
            };
            _context.Ventas.Add(venta);

            foreach (var item in solicitud.Items)
            {
                var producto = productos[item.IdProducto];
                var stockAnterior = producto.StockActual;
                producto.StockActual -= item.Cantidad;
                venta.DetalleVentas.Add(new DetalleVenta
                {
                    IdProducto = producto.IdProducto,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.PrecioVenta,
                    SubTotal = producto.PrecioVenta * item.Cantidad
                });
                _context.MovimientosInventario.Add(new MovimientoInventario
                {
                    IdProducto = producto.IdProducto,
                    IdUsuario = idUsuario,
                    TipoMovimiento = "Salida",
                    Cantidad = item.Cantidad,
                    StockAnterior = stockAnterior,
                    StockPosterior = producto.StockActual,
                    Motivo = $"Venta {venta.NumeroComprobante}",
                    FechaRegistro = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();
            return (true, null, venta.IdVenta);
        }
        catch (Exception)
        {
            await transaccion.RollbackAsync();
            return (false, "No se pudo registrar la venta. Intente nuevamente.", null);
        }
    }
}
