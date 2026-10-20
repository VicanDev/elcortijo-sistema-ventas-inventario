using ElCortijo.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public class CajaController : Controller
{
    private readonly ElCortijoDbContext _context;

    public CajaController(ElCortijoDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? idUsuario, string? estado, DateTime? desde, DateTime? hasta)
    {
        var consulta = _context.ArqueosCaja.AsNoTracking().Include(a => a.Usuario).AsQueryable();

        if (idUsuario.HasValue)
            consulta = consulta.Where(a => a.IdUsuario == idUsuario.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(a => a.Estado == estado);

        if (desde.HasValue)
            consulta = consulta.Where(a => a.FechaApertura >= desde.Value.Date);

        if (hasta.HasValue)
        {
            var limite = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(a => a.FechaApertura < limite);
        }

        ViewBag.IdUsuario = idUsuario;
        ViewBag.Estado = estado;
        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
        ViewBag.Usuarios = await _context.Usuarios.AsNoTracking().OrderBy(u => u.NombreCompleto).ToListAsync();
        return View(await consulta.OrderByDescending(a => a.FechaApertura).Take(150).ToListAsync());
    }

    // TODO (Eduardo): implementar apertura, vinculacion de ventas al arqueo
    // abierto, calculo por metodo de pago y cierre con diferencia de caja.
}
