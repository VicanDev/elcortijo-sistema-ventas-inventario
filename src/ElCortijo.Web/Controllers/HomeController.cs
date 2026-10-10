using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        var viewModel = new DashboardViewModel
        {
            TotalVentasHoy = totalVentasHoy,
            CantidadVentasHoy = cantidadVentasHoy,
            TotalProductos = totalProductos,
            ProductosBajoStock = productosBajoStock.Count,
            ListaProductosAlerta = productosBajoStock,
            UltimasVentas = ultimasVentas
        };

        return View(viewModel);
    }
}
