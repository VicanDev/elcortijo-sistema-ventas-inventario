using Microsoft.EntityFrameworkCore;
using ElCortijo.Datos;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Negocio.Servicios;

public sealed class ComprobanteService : IComprobanteService
{
    private readonly ElCortijoDbContext _context;

    public ComprobanteService(ElCortijoDbContext context) => _context = context;

    public async Task<IReadOnlyList<ComprobanteDto>> BuscarAsync(string? numero, string? tipo, DateTime? desde, DateTime? hasta)
    {
        var query = _context.Ventas.AsNoTracking()
            .Where(v => v.TipoComprobante.ToLower() == "boleta" || v.TipoComprobante.ToLower() == "factura");

        if (!string.IsNullOrWhiteSpace(numero))
        {
            var term = numero.Trim();
            query = query.Where(v => EF.Functions.Like(v.NumeroComprobante, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var normalizedType = tipo.Trim().ToLower();
            if (normalizedType is "boleta" or "factura")
                query = query.Where(v => v.TipoComprobante.ToLower() == normalizedType);
        }

        if (desde.HasValue)
            query = query.Where(v => v.FechaRegistro >= desde.Value.Date);
        if (hasta.HasValue)
        {
            var exclusiveEnd = hasta.Value.Date.AddDays(1);
            query = query.Where(v => v.FechaRegistro < exclusiveEnd);
        }

        var ventas = await query
            .OrderByDescending(v => v.FechaRegistro)
            .Take(500)
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .ToListAsync();

        return ventas.Select(ToDto).ToList();
    }

    public async Task<ComprobanteDto?> ObtenerAsync(int idVenta)
    {
        var venta = await _context.Ventas.AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.DetalleVentas)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta &&
                (v.TipoComprobante.ToLower() == "boleta" || v.TipoComprobante.ToLower() == "factura"));

        return venta is null ? null : ToDto(venta);
    }

    private static ComprobanteDto ToDto(Venta venta) => new()
    {
        IdVenta = venta.IdVenta,
        NumeroComprobante = venta.NumeroComprobante,
        TipoComprobante = venta.TipoComprobante,
        FechaRegistro = venta.FechaRegistro,
        Estado = venta.Estado,
        ClienteNombre = venta.Cliente?.NombreRazonSocial,
        ClienteTipoDocumento = venta.Cliente?.TipoDocumento,
        ClienteNumeroDocumento = venta.Cliente?.NumeroDocumento,
        ClienteDireccion = venta.Cliente?.Direccion,
        Vendedor = venta.Usuario?.NombreCompleto,
        MetodoPago = venta.MetodoPago,
        // These are persisted historical amounts. Do not recompute or overwrite them here.
        SubTotal = venta.SubTotal,
        Igv = venta.Igv,
        Total = venta.Total,
        Detalles = venta.DetalleVentas.Select(d => new ComprobanteDetalleDto
        {
            Producto = d.Producto?.Nombre ?? "Producto no disponible",
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            SubTotal = d.SubTotal
        }).ToList()
    };
}
