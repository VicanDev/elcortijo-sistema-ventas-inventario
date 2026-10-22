using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public async Task<IActionResult> Index(string? buscar, int? idRol, string? estado)
    {
        bool? estadoActivo = estado switch { "activo" => true, "inactivo" => false, _ => null };
        var usuarios = await _usuarioService.ObtenerTodosAsync(buscar, idRol, estadoActivo);
        ViewBag.Buscar = buscar;
        ViewBag.IdRol = idRol;
        ViewBag.Estado = estado;
        ViewBag.RolesFiltro = await _usuarioService.ObtenerRolesAsync();
        return View(usuarios);
    }

    public async Task<IActionResult> Crear()
    {
        var roles = await _usuarioService.ObtenerRolesAsync();
        ViewBag.Roles = new SelectList(roles, "IdRol", "Nombre");
        return View(new Usuario());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Usuario usuario, string clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || clave.Length < 6)
        {
            ModelState.AddModelError("Clave", "La contraseña debe tener al menos 6 caracteres.");
        }

        if (ModelState.IsValid)
        {
            var creado = await _usuarioService.CrearAsync(usuario, clave);
            if (creado)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "No se pudo registrar el usuario. Verifique si el correo o usuario ya existen.");
        }

        var roles = await _usuarioService.ObtenerRolesAsync();
        ViewBag.Roles = new SelectList(roles, "IdRol", "Nombre", usuario.IdRol);
        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        await _usuarioService.CambiarEstadoAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
