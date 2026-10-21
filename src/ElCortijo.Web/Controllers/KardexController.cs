using ElCortijo.Datos;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        public async Task<IActionResult> RegistrarEntrada()
    {
        ViewBag.Productos = await _context.Productos
            .AsNoTracking()
            .Where(p => p.Estado)
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.IdProducto, p.Codigo, p.Nombre, p.StockActual })
            .ToListAsync();

        return View(new EntradaInventarioViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarEntrada(EntradaInventarioViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Productos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.Estado)
                .OrderBy(p => p.Nombre)
                .Select(p => new { p.IdProducto, p.Codigo, p.Nombre, p.StockActual })
                .ToListAsync();
            return View(modelo);
        }

        var producto = await _context.Productos.FindAsync(modelo.IdProducto);
        if (producto is null || !producto.Estado)
        {
            ModelState.AddModelError(nameof(modelo.IdProducto), "El producto no existe o está inactivo.");
            ViewBag.Productos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.Estado)
                .OrderBy(p => p.Nombre)
                .Select(p => new { p.IdProducto, p.Codigo, p.Nombre, p.StockActual })
                .ToListAsync();
            return View(modelo);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var stockAnterior = producto.StockActual;
            var stockPosterior = stockAnterior + modelo.Cantidad;

            producto.StockActual = stockPosterior;

            var movimiento = new MovimientoInventario
            {
                IdProducto = producto.IdProducto,
                IdUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                TipoMovimiento = "Entrada",
                Cantidad = modelo.Cantidad,
                StockAnterior = stockAnterior,
                StockPosterior = stockPosterior,
                Motivo = modelo.Motivo.Trim(),
                FechaRegistro = DateTime.Now
            };

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Exito"] = $"Entrada registrada. El stock de {producto.Nombre} ahora es {stockPosterior}.";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
