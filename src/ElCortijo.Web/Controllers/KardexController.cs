using ElCortijo.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador,Almacenero")]
public class KardexController : Controller
{
    private readonly ElCortijoDbContext _context;

    public KardexController(ElCortijoDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? idProducto, string? producto, string? tipo, DateTime? desde, DateTime? hasta)
    {
        var consulta = _context.MovimientosInventario
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .AsQueryable();

        if (idProducto.HasValue)
            consulta = consulta.Where(m => m.IdProducto == idProducto.Value);

        if (!string.IsNullOrWhiteSpace(tipo))
            consulta = consulta.Where(m => m.TipoMovimiento == tipo);

        if (desde.HasValue)
            consulta = consulta.Where(m => m.FechaRegistro >= desde.Value.Date);

        if (hasta.HasValue)
        {
            var limite = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(m => m.FechaRegistro < limite);
        }

        ViewBag.IdProducto = idProducto;
        ViewBag.Producto = producto;
        ViewBag.Tipo = tipo;
        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
        ViewBag.Productos = await _context.Productos.AsNoTracking()
            .Where(p => p.Estado && (!idProducto.HasValue || p.IdProducto == idProducto.Value))
            .Select(p => new { p.IdProducto, p.Codigo, p.Nombre })
            .ToListAsync();

        return View(await consulta.OrderByDescending(m => m.FechaRegistro).Take(150).ToListAsync());
    }

    // TODO (Mellany): implementar el formulario transaccional para registrar
    // entradas de mercaderia y ajustes de inventario.
}
