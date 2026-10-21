using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ElCortijo.Datos;
using ElCortijo.Web.ViewModels;

namespace ElCortijo.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ElCortijoDbContext _context;

    public HomeController(ElCortijoDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;

        var ventasHoy = await _context.Ventas
            .Where(v => v.FechaRegistro >= hoy && v.Estado == "Completada")
            .ToListAsync();

        var totalVentasHoy = ventasHoy.Sum(v => v.Total);
        var cantidadVentasHoy = ventasHoy.Count;

        var totalProductos = await _context.Productos.CountAsync(p => p.Estado);

        var productosBajoStock = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Where(p => p.Estado && p.StockActual <= p.StockMinimo)
            .OrderBy(p => p.StockActual)
            .Take(5)
            .ToListAsync();

        var ultimasVentas = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .OrderByDescending(v => v.FechaRegistro)
            .Take(5)
            .ToListAsync();

        var inicioSemana = hoy.AddDays(-6);
        var ventasSemana = await _context.Ventas.AsNoTracking()
            .Where(v => v.FechaRegistro >= inicioSemana && v.FechaRegistro < hoy.AddDays(1) && v.Estado == "Completada")
            .Select(v => new { v.FechaRegistro, v.Total })
            .ToListAsync();

        var graficoVentasSemana = Enumerable.Range(0, 7)
            .Select(dia =>
            {
                var fecha = inicioSemana.AddDays(dia);
                var delDia = ventasSemana.Where(v => v.FechaRegistro.Date == fecha).ToList();
                return new VentasPorDiaViewModel
                {
                    Fecha = fecha,
                    Etiqueta = fecha.ToString("ddd", new CultureInfo("es-PE")),
                    Total = delDia.Sum(v => v.Total),
                    Cantidad = delDia.Count,
                    X = 54 + dia * 99,
                    Y = 185
                };
            })
            .ToList();

        var maxVentaDia = Math.Max(1m, graficoVentasSemana.Max(p => p.Total));
        foreach (var punto in graficoVentasSemana)
            punto.Y = 185 - (double)(punto.Total / maxVentaDia) * 145;

        var puntosSvg = string.Join(" ", graficoVentasSemana.Select(p => FormattableString.Invariant($"{p.X:0.##},{p.Y:0.##}")));
        var areaSvg = "M54,185 L" + string.Join(" L", graficoVentasSemana.Select(p => FormattableString.Invariant($"{p.X:0.##},{p.Y:0.##}"))) + " L648,185 Z";

        var inicioMes = hoy.AddDays(-29);
        var productosMasVendidos = await _context.DetalleVentas.AsNoTracking()
            .Where(d => d.Venta.FechaRegistro >= inicioMes && d.Venta.FechaRegistro < hoy.AddDays(1) && d.Venta.Estado == "Completada")
            .GroupBy(d => d.Producto.Nombre)
            .Select(g => new ProductoMasVendidoViewModel
            {
                Nombre = g.Key,
                Cantidad = g.Sum(d => d.Cantidad),
                TotalVendido = g.Sum(d => d.SubTotal)
            })
            .OrderByDescending(p => p.Cantidad)
            .Take(5)
            .ToListAsync();

        var maxUnidades = Math.Max(1, productosMasVendidos.Select(p => p.Cantidad).DefaultIfEmpty().Max());
        foreach (var producto in productosMasVendidos)
            producto.Porcentaje = Math.Max(8, producto.Cantidad * 100d / maxUnidades);

        var ventasMes = await _context.Ventas.AsNoTracking()
            .Where(v => v.FechaRegistro >= inicioMes && v.FechaRegistro < hoy.AddDays(1) && v.Estado == "Completada")
            .Select(v => new { v.MetodoPago, v.Total })
            .ToListAsync();

        var ventasPorMetodo = ventasMes
            .GroupBy(v => string.IsNullOrWhiteSpace(v.MetodoPago) ? "Otro" : v.MetodoPago)
            .Select(g => new MetodoPagoResumenViewModel
            {
                Metodo = g.Key,
                Operaciones = g.Count(),
                Total = g.Sum(v => v.Total)
            })
            .OrderByDescending(m => m.Total)
            .ToList();

        var totalMetodosPago = ventasPorMetodo.Sum(m => m.Total);
        foreach (var metodo in ventasPorMetodo)
            metodo.Porcentaje = totalMetodosPago == 0 ? 0 : (double)(metodo.Total / totalMetodosPago) * 100;

        var viewModel = new DashboardViewModel
        {
            TotalVentasHoy = totalVentasHoy,
            CantidadVentasHoy = cantidadVentasHoy,
            TotalProductos = totalProductos,
            ProductosBajoStock = productosBajoStock.Count,
            ListaProductosAlerta = productosBajoStock,
            UltimasVentas = ultimasVentas,
            VentasUltimosSieteDias = graficoVentasSemana,
            VentasSemanaPuntosSvg = puntosSvg,
            VentasSemanaAreaSvg = areaSvg,
            ProductosMasVendidos = productosMasVendidos,
            VentasPorMetodoPago = ventasPorMetodo
        };

        return View(viewModel);
    }
}
