using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public class VentasController : Controller
{
    private readonly IVentaService _ventaService;
    private readonly IClienteService _clienteService;
    public VentasController(IVentaService ventaService, IClienteService clienteService) => (_ventaService, _clienteService) = (ventaService, clienteService);

    public async Task<IActionResult> Index(string? buscar, string? tipoComprobante, string? metodoPago, DateTime? desde, DateTime? hasta)
    {
        ViewBag.Buscar = buscar;
        ViewBag.TipoComprobante = tipoComprobante;
        ViewBag.MetodoPago = metodoPago;
        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
        return View(await _ventaService.ObtenerTodasAsync(buscar, tipoComprobante, metodoPago, desde, hasta));
    }

    public async Task<IActionResult> Crear()
    {
        await CargarListasAsync();
        return View(new VentaRegistroViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(VentaRegistroViewModel solicitud)
    {
        if (ModelState.IsValid)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var resultado = await _ventaService.RegistrarAsync(solicitud, idUsuario);
            if (resultado.Exito) { TempData["Exito"] = $"Venta {resultado.IdVenta} registrada y stock actualizado."; return RedirectToAction(nameof(Index)); }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }
        else
        {
            var errores = ModelState
                .Where(campo => campo.Value is { Errors.Count: > 0 })
                .SelectMany(campo => campo.Value!.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage) ? $"{campo.Key}: dato inválido." : $"{campo.Key}: {error.ErrorMessage}"));
            ModelState.AddModelError(string.Empty, string.Join(" ", errores));
        }
        await CargarListasAsync();
        return View(solicitud);
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Clientes = await _clienteService.ObtenerTodosAsync(true);
        ViewBag.Productos = await _ventaService.ObtenerProductosDisponiblesAsync();
    }
}
