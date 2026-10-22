using Microsoft.EntityFrameworkCore;
using ElCortijo.Datos;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Negocio.Servicios;

public sealed class ReporteService : IReporteService
{
    private const string Completada = "Completada";
    private readonly ElCortijoDbContext _context;

    public ReporteService(ElCortijoDbContext context) => _context = context;

    public async Task<ReporteVentasDto> ObtenerVentasAsync(FiltroReporteDto filtro)
    {
        ValidarRango(filtro.Desde, filtro.Hasta);
        var desde = filtro.Desde.Date;
        var hastaExclusivo = filtro.Hasta.Date.AddDays(1);
        var query = _context.Ventas.AsNoTracking().Where(v => v.FechaRegistro >= desde && v.FechaRegistro < hastaExclusivo);

        if (!string.IsNullOrWhiteSpace(filtro.EstadoVenta) && !filtro.EstadoVenta.Equals("Todos", StringComparison.OrdinalIgnoreCase))
            query = query.Where(v => v.Estado.ToLower() == filtro.EstadoVenta.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(filtro.TipoComprobante))
            query = query.Where(v => v.TipoComprobante.ToLower() == filtro.TipoComprobante.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(filtro.MetodoPago))
            query = query.Where(v => v.MetodoPago.ToLower() == filtro.MetodoPago.Trim().ToLower());

        var filas = await query.OrderByDescending(v => v.FechaRegistro)
            .Select(v => new ReporteVentaFilaDto
            {
                IdVenta = v.IdVenta,
                Fecha = v.FechaRegistro,
                NumeroComprobante = v.NumeroComprobante,
                TipoComprobante = v.TipoComprobante,
                Cliente = v.Cliente == null ? "Cliente no disponible" : v.Cliente.NombreRazonSocial,
                MetodoPago = v.MetodoPago,
                Estado = v.Estado,
                SubTotal = v.SubTotal,
                Igv = v.Igv,
                Total = v.Total
            }).ToListAsync();

        var completadas = filas.Where(v => v.Estado.Equals(Completada, StringComparison.OrdinalIgnoreCase)).ToList();
        return new ReporteVentasDto
        {
            Desde = desde,
            Hasta = filtro.Hasta.Date,
            CantidadVentas = completadas.Count,
            TotalSubTotalCompletadas = completadas.Sum(v => v.SubTotal),
            TotalIgvCompletadas = completadas.Sum(v => v.Igv),
            TotalVentasCompletadas = completadas.Sum(v => v.Total),
            Filas = filas
        };
    }

    public async Task<ReporteMovimientosDto> ObtenerMovimientosAsync(FiltroReporteDto filtro)
    {
        ValidarRango(filtro.Desde, filtro.Hasta);
        var desde = filtro.Desde.Date;
        var hastaExclusivo = filtro.Hasta.Date.AddDays(1);
        var query = _context.MovimientosInventario.AsNoTracking()
            .Where(m => m.FechaRegistro >= desde && m.FechaRegistro < hastaExclusivo);

        if (!string.IsNullOrWhiteSpace(filtro.TipoMovimiento))
            query = query.Where(m => m.TipoMovimiento.ToLower() == filtro.TipoMovimiento.Trim().ToLower());
        if (filtro.IdProducto.HasValue && filtro.IdProducto.Value > 0)
            query = query.Where(m => m.IdProducto == filtro.IdProducto.Value);

        var filas = await query.OrderByDescending(m => m.FechaRegistro)
            .Select(m => new ReporteMovimientoFilaDto
            {
                Fecha = m.FechaRegistro,
                Producto = m.Producto == null ? "Producto no disponible" : m.Producto.Nombre,
                CodigoProducto = m.Producto == null ? string.Empty : m.Producto.Codigo,
                TipoMovimiento = m.TipoMovimiento,
                Cantidad = m.Cantidad,
                StockAnterior = m.StockAnterior,
                StockPosterior = m.StockPosterior,
                Motivo = m.Motivo,
                Usuario = m.Usuario == null ? "Usuario no disponible" : m.Usuario.NombreCompleto
            }).ToListAsync();

        return new ReporteMovimientosDto
        {
            Desde = desde,
            Hasta = filtro.Hasta.Date,
            CantidadMovimientos = filas.Count,
            TotalUnidadesEntrada = filas.Where(m => m.TipoMovimiento.Equals("Entrada", StringComparison.OrdinalIgnoreCase)).Sum(m => m.Cantidad),
            TotalUnidadesSalida = filas.Where(m => m.TipoMovimiento.Equals("Salida", StringComparison.OrdinalIgnoreCase)).Sum(m => m.Cantidad),
            Filas = filas
        };
    }

    private static void ValidarRango(DateTime desde, DateTime hasta)
    {
        if (desde == default || hasta == default || desde.Date > hasta.Date)
            throw new ArgumentException("El rango de fechas no es válido.");
        if (hasta.Date == DateTime.MaxValue.Date)
            throw new ArgumentException("La fecha final está fuera del rango admitido.");
    }
}
