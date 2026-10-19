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

    public async Task<IActionResult> Index() => View(await _ventaService.ObtenerTodasAsync());

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
        await CargarListasAsync();
        return View(solicitud);
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Clientes = new SelectList(await _clienteService.ObtenerTodosAsync(true), "IdCliente", "NombreRazonSocial");
        ViewBag.Productos = await _ventaService.ObtenerProductosDisponiblesAsync();
    }
}
