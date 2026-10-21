using ElCortijo.Datos;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador,Almacenero")]
public class CatalogosController : Controller
{
    private readonly ElCortijoDbContext _context;
    public CatalogosController(ElCortijoDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        ViewBag.Categorias = await _context.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Marcas = await _context.Marcas.AsNoTracking().OrderBy(m => m.Nombre).ToListAsync();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> EditarCategoria(int id) => await EditarItemAsync<Categoria>(id, "Categoria");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCategoria(CatalogoItemViewModel modelo) =>
        await GuardarEdicionAsync<Categoria>(modelo, "Categoria", modelo.Id, x => x.Nombre, (x, nombre) => x.Nombre = nombre,
            (x, descripcion) => x.Descripcion = descripcion, "Categoría");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(CatalogoItemViewModel modelo)
    {
        var nombre = modelo.Nombre?.Trim() ?? string.Empty;
        if (await _context.Categorias.AnyAsync(x => x.Nombre == nombre))
            ModelState.AddModelError(nameof(modelo.Nombre), "Ya existe una categoría con ese nombre.");
        if (!ModelState.IsValid) return await IndexConErrorAsync("Categoria", modelo);
        _context.Categorias.Add(new Categoria { Nombre = nombre, Descripcion = modelo.Descripcion?.Trim(), Estado = true });
        await _context.SaveChangesAsync(); TempData["Exito"] = "Categoría creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoCategoria(int id) => await CambiarEstadoAsync<Categoria>(id, "Categoría");

    [HttpGet]
    public async Task<IActionResult> EditarMarca(int id) => await EditarItemAsync<Marca>(id, "Marca");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarMarca(CatalogoItemViewModel modelo) =>
        await GuardarEdicionAsync<Marca>(modelo, "Marca", modelo.Id, x => x.Nombre, (x, nombre) => x.Nombre = nombre,
            null, "Marca");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearMarca(CatalogoItemViewModel modelo)
    {
        var nombre = modelo.Nombre?.Trim() ?? string.Empty;
        if (await _context.Marcas.AnyAsync(x => x.Nombre == nombre))
            ModelState.AddModelError(nameof(modelo.Nombre), "Ya existe una marca con ese nombre.");
        if (!ModelState.IsValid) return await IndexConErrorAsync("Marca", modelo);
        _context.Marcas.Add(new Marca { Nombre = nombre, Estado = true });
        await _context.SaveChangesAsync(); TempData["Exito"] = "Marca creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoMarca(int id) => await CambiarEstadoAsync<Marca>(id, "Marca");

    private async Task<IActionResult> EditarItemAsync<T>(int id, string tipo) where T : class
    {
        var item = await _context.Set<T>().FindAsync(id);
        if (item is null) return NotFound();
        var modelo = item switch
        {
            Categoria c => new CatalogoItemViewModel { Id = c.IdCategoria, Nombre = c.Nombre, Descripcion = c.Descripcion, Estado = c.Estado },
            Marca m => new CatalogoItemViewModel { Id = m.IdMarca, Nombre = m.Nombre, Estado = m.Estado },
            _ => throw new InvalidOperationException()
        };
        ViewBag.Tipo = tipo;
        return View("Editar", modelo);
    }

    private async Task<IActionResult> GuardarEdicionAsync<T>(CatalogoItemViewModel modelo, string tipo, int id,
        System.Linq.Expressions.Expression<Func<T, string>> nombreSelector, Action<T, string> asignarNombre,
        Action<T, string?>? asignarDescripcion, string etiqueta) where T : class
    {
        var nombre = modelo.Nombre?.Trim() ?? string.Empty;
        if (await _context.Set<T>().AnyAsync(BuildNombreDuplicado<T>(nombreSelector, nombre, id)))
            ModelState.AddModelError(nameof(modelo.Nombre), $"Ya existe {etiqueta.ToLowerInvariant()} con ese nombre.");
        if (!ModelState.IsValid) { ViewBag.Tipo = tipo; return View("Editar", modelo); }
        var item = await _context.Set<T>().FindAsync(id);
        if (item is null) return NotFound();
        asignarNombre(item, nombre);
        asignarDescripcion?.Invoke(item, modelo.Descripcion?.Trim());
        switch (item) { case Categoria c: c.Estado = modelo.Estado; break; case Marca m: m.Estado = modelo.Estado; break; }
        await _context.SaveChangesAsync(); TempData["Exito"] = $"{etiqueta} actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private static Expression<Func<T, bool>> BuildNombreDuplicado<T>(Expression<Func<T, string>> selector, string nombre, int id)
    {
        var parameter = selector.Parameters[0];
        var nameMatch = Expression.Equal(selector.Body, Expression.Constant(nombre));
        var idProperty = typeof(T) == typeof(Categoria) ? "IdCategoria" : "IdMarca";
        var idMatch = Expression.NotEqual(Expression.Property(parameter, idProperty), Expression.Constant(id));
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(nameMatch, idMatch), parameter);
    }

    private async Task<IActionResult> CambiarEstadoAsync<T>(int id, string etiqueta) where T : class
    {
        var item = await _context.Set<T>().FindAsync(id);
        if (item is null) return NotFound();
        if (item is Categoria categoria)
        {
            if (categoria.Estado && await _context.Productos.AnyAsync(p => p.IdCategoria == id && p.Estado))
            { TempData["Error"] = "No se puede desactivar una categoría que tiene productos activos."; return RedirectToAction(nameof(Index)); }
            categoria.Estado = !categoria.Estado;
        }
        else if (item is Marca marca)
        {
            if (marca.Estado && await _context.Productos.AnyAsync(p => p.IdMarca == id && p.Estado))
            { TempData["Error"] = "No se puede desactivar una marca que tiene productos activos."; return RedirectToAction(nameof(Index)); }
            marca.Estado = !marca.Estado;
        }
        await _context.SaveChangesAsync(); TempData["Exito"] = $"{etiqueta} actualizado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> IndexConErrorAsync(string tipo, CatalogoItemViewModel modelo)
    {
        ViewBag.Categorias = await _context.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Marcas = await _context.Marcas.AsNoTracking().OrderBy(m => m.Nombre).ToListAsync();
        ViewBag.TipoError = tipo; ViewBag.ModeloError = modelo;
        return View("Index");
    }
}
