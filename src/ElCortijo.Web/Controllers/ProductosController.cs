using ElCortijo.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Web.ViewModels;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador,Almacenero")]
public class ProductosController : Controller
{
    private readonly ElCortijoDbContext _context;

    public ProductosController(ElCortijoDbContext context) => _context = context;

    public async Task<IActionResult> Index(
        string? buscar,
        int? idCategoria,
        int? idMarca,
        string? estado,
        bool soloBajoStock = false)
    {
        var consulta = _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            consulta = consulta.Where(p => p.Codigo.Contains(buscar) || p.Nombre.Contains(buscar));
        }

        if (soloBajoStock)
            consulta = consulta.Where(p => p.StockActual <= p.StockMinimo);

        if (idCategoria.HasValue)
            consulta = consulta.Where(p => p.IdCategoria == idCategoria.Value);

        if (idMarca.HasValue)
            consulta = consulta.Where(p => p.IdMarca == idMarca.Value);

        if (estado == "activo")
            consulta = consulta.Where(p => p.Estado);
        else if (estado == "inactivo")
            consulta = consulta.Where(p => !p.Estado);

        ViewBag.Buscar = buscar;
        ViewBag.IdCategoria = idCategoria;
        ViewBag.IdMarca = idMarca;
        ViewBag.Estado = estado;
        ViewBag.SoloBajoStock = soloBajoStock;
        ViewBag.Categorias = await _context.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Marcas = await _context.Marcas.AsNoTracking().OrderBy(m => m.Nombre).ToListAsync();
        return View(await consulta.OrderBy(p => p.Nombre).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        await CargarCatalogosAsync();
        return View("Formulario", new ProductoFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProductoFormViewModel modelo)
    {
        var codigo = modelo.Codigo?.Trim() ?? string.Empty;
        if (await _context.Productos.AnyAsync(p => p.Codigo == codigo))
            ModelState.AddModelError(nameof(modelo.Codigo), "Ya existe un producto con ese código.");
        await ValidarReferenciasAsync(modelo);
        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync();
            return View("Formulario", modelo);
        }

        var producto = new Producto
        {
            Codigo = codigo, Nombre = modelo.Nombre.Trim(), Descripcion = modelo.Descripcion?.Trim(),
            IdCategoria = modelo.IdCategoria, IdMarca = modelo.IdMarca, PrecioCompra = modelo.PrecioCompra,
            PrecioVenta = modelo.PrecioVenta, StockActual = 0, StockMinimo = modelo.StockMinimo,
            UnidadMedida = modelo.UnidadMedida.Trim(), ImagenUrl = modelo.ImagenUrl?.Trim(), Estado = true
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Producto creado. El stock inicial queda en cero; registra sus entradas desde Kardex.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var producto = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducto == id);
        if (producto is null) return NotFound();
        await CargarCatalogosAsync();
        return View("Formulario", new ProductoFormViewModel
        {
            IdProducto = producto.IdProducto, Codigo = producto.Codigo, Nombre = producto.Nombre,
            Descripcion = producto.Descripcion, IdCategoria = producto.IdCategoria, IdMarca = producto.IdMarca,
            PrecioCompra = producto.PrecioCompra, PrecioVenta = producto.PrecioVenta,
            StockMinimo = producto.StockMinimo, UnidadMedida = producto.UnidadMedida,
            ImagenUrl = producto.ImagenUrl, Estado = producto.Estado
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ProductoFormViewModel modelo)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == modelo.IdProducto);
        if (producto is null) return NotFound();
        var codigo = modelo.Codigo?.Trim() ?? string.Empty;
        if (await _context.Productos.AnyAsync(p => p.Codigo == codigo && p.IdProducto != modelo.IdProducto))
            ModelState.AddModelError(nameof(modelo.Codigo), "Ya existe otro producto con ese código.");
        await ValidarReferenciasAsync(modelo);
        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync();
            return View("Formulario", modelo);
        }

        producto.Codigo = codigo; producto.Nombre = modelo.Nombre.Trim();
        producto.Descripcion = modelo.Descripcion?.Trim(); producto.IdCategoria = modelo.IdCategoria;
        producto.IdMarca = modelo.IdMarca; producto.PrecioCompra = modelo.PrecioCompra;
        producto.PrecioVenta = modelo.PrecioVenta; producto.StockMinimo = modelo.StockMinimo;
        producto.UnidadMedida = modelo.UnidadMedida.Trim(); producto.ImagenUrl = modelo.ImagenUrl?.Trim();
        producto.Estado = modelo.Estado;
        await _context.SaveChangesAsync();
        TempData["Exito"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound();
        producto.Estado = !producto.Estado;
        await _context.SaveChangesAsync();
        TempData["Exito"] = producto.Estado ? "Producto activado." : "Producto desactivado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCatalogosAsync()
    {
        ViewBag.Categorias = await _context.Categorias.AsNoTracking().Where(c => c.Estado).OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Marcas = await _context.Marcas.AsNoTracking().Where(m => m.Estado).OrderBy(m => m.Nombre).ToListAsync();
    }

    private async Task ValidarReferenciasAsync(ProductoFormViewModel modelo)
    {
        if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == modelo.IdCategoria && c.Estado))
            ModelState.AddModelError(nameof(modelo.IdCategoria), "La categoría seleccionada no está disponible.");
        if (!await _context.Marcas.AnyAsync(m => m.IdMarca == modelo.IdMarca && m.Estado))
            ModelState.AddModelError(nameof(modelo.IdMarca), "La marca seleccionada no está disponible.");
    }
}
