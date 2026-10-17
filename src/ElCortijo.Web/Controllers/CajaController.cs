using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElCortijo.Negocio.Interfaces;
using ElCortijo.Web.ViewModels;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public sealed class CajaController : Controller
{
    private readonly ICajaService _caja;

    public CajaController(ICajaService caja) => _caja = caja;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario is null) return Forbid();
        return View(new CajaViewModel { Panel = await _caja.ObtenerPanelAsync(idUsuario.Value) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Abrir(CajaViewModel model)
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario is null) return Forbid();
        if (!ModelState.IsValid)
        {
            model.Panel = await _caja.ObtenerPanelAsync(idUsuario.Value);
            return View("Index", model);
        }

        var resultado = await _caja.AbrirAsync(idUsuario.Value, model.MontoInicial);
        TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cerrar(int idArqueo, CajaViewModel model)
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario is null) return Forbid();
        if (idArqueo <= 0 || !ModelState.IsValid)
        {
            model.Panel = await _caja.ObtenerPanelAsync(idUsuario.Value);
            if (idArqueo <= 0) ModelState.AddModelError(string.Empty, "La caja indicada no es válida.");
            return View("Index", model);
        }

        var resultado = await _caja.CerrarAsync(idUsuario.Value, idArqueo, model.TotalReal);
        TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    private int? ObtenerIdUsuario() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
