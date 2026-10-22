using ElCortijo.Datos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElCortijo.Web.Controllers;

[ApiController]
[Route("api/chatbot")]
public class ChatbotController : ControllerBase
{
    private readonly ElCortijoDbContext _context;

    public ChatbotController(ElCortijoDbContext context) => _context = context;

    [HttpGet("productos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsultarProductos([FromQuery] string? buscar)
    {
        var termino = buscar?.Trim();
        var consulta = _context.Productos
            .AsNoTracking()
            .Where(p => p.Estado);

        if (!string.IsNullOrWhiteSpace(termino))
        {
            var palabras = termino.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var palabra in palabras)
                consulta = consulta.Where(p => p.Nombre.Contains(palabra)
                    || p.Codigo.Contains(palabra)
                    || p.Marca.Nombre.Contains(palabra));
        }

        var productos = await consulta
            .OrderBy(p => p.Marca.Nombre)
            .ThenBy(p => p.Nombre)
            .Take(50)
            .Select(p => new
            {
                p.Codigo,
                p.Nombre,
                Marca = p.Marca.Nombre,
                Precio = p.PrecioVenta,
                Stock = p.StockActual,
                p.UnidadMedida,
                Disponible = p.StockActual > 0
            })
            .ToListAsync();

        return Ok(new { consulta = termino, encontrados = productos.Count, productos });
    }

    [HttpGet("marcas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsultarMarcas()
    {
        var marcas = await _context.Productos
            .AsNoTracking()
            .Where(p => p.Estado)
            .Select(p => p.Marca.Nombre)
            .Distinct()
            .OrderBy(nombre => nombre)
            .ToListAsync();

        return Ok(new { marcas });
    }
}
