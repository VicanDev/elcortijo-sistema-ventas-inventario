using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public class ClientesController : Controller
{
    private readonly IClienteService _clienteService;
    public ClientesController(IClienteService clienteService) => _clienteService = clienteService;

    public async Task<IActionResult> Index(string? buscar, string? tipoDocumento, string? estado)
    {
        bool? estadoActivo = estado switch { "activo" => true, "inactivo" => false, _ => null };
        ViewBag.Buscar = buscar;
        ViewBag.TipoDocumento = tipoDocumento;
        ViewBag.Estado = estado;
        return View(await _clienteService.ObtenerTodosAsync(false, buscar, tipoDocumento, estadoActivo));
    }
    public IActionResult Crear() => View(new Cliente());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _clienteService.CrearAsync(cliente);
            if (resultado.Exito) { TempData["Exito"] = "Cliente registrado correctamente."; return RedirectToAction(nameof(Index)); }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }
        return View(cliente);
    }

    public async Task<IActionResult> Editar(int id)
    {
        var cliente = await _clienteService.ObtenerPorIdAsync(id);
        return cliente == null ? NotFound() : View(cliente);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _clienteService.ActualizarAsync(cliente);
            if (resultado.Exito) { TempData["Exito"] = "Cliente actualizado correctamente."; return RedirectToAction(nameof(Index)); }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }
        return View(cliente);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        await _clienteService.CambiarEstadoAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
