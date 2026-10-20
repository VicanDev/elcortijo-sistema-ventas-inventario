using ElCortijo.Datos;
using ElCortijo.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElCortijo.Web.Controllers;

public class InicioController : Controller
{
    private readonly ElCortijoDbContext _context;

    public InicioController(ElCortijoDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var model = new LandingViewModel
        {
            ProductosDestacados = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .Where(p => p.Estado && p.StockActual > 0)
                .OrderBy(p => p.IdProducto)
                .Take(6)
                .ToListAsync(),
            TotalProductos = await _context.Productos.CountAsync(p => p.Estado),
            TotalCategorias = await _context.Categorias.CountAsync(c => c.Estado),
            TotalMarcas = await _context.Marcas.CountAsync(m => m.Estado)
        };

        return View(model);
    }
}
