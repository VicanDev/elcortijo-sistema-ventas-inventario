using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Negocio.Interfaces;
using ElCortijo.Web.ViewModels;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public sealed class ComprobantesController : Controller
{
    private readonly IComprobanteService _comprobantes;
    private readonly IExportacionService _exportacion;

    public ComprobantesController(IComprobanteService comprobantes, IExportacionService exportacion)
    {
        _comprobantes = comprobantes;
        _exportacion = exportacion;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ComprobantesIndexViewModel filtro)
    {
        if (filtro.Desde.HasValue && filtro.Hasta.HasValue && filtro.Desde.Value.Date > filtro.Hasta.Value.Date)
            ModelState.AddModelError(nameof(filtro.Hasta), "La fecha final debe ser igual o posterior a la inicial.");
        if (filtro.Hasta?.Date == DateTime.MaxValue.Date)
            ModelState.AddModelError(nameof(filtro.Hasta), "La fecha final está fuera del rango admitido.");

        if (!ModelState.IsValid)
            return View(filtro);

        filtro.Comprobantes = await _comprobantes.BuscarAsync(filtro.Numero, filtro.Tipo, filtro.Desde, filtro.Hasta);
        return View(filtro);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        if (id <= 0) return BadRequest();
        var comprobante = await _comprobantes.ObtenerAsync(id);
        return comprobante is null ? NotFound() : View(comprobante);
    }

    [HttpGet]
    public async Task<IActionResult> Imprimir(int id)
    {
        if (id <= 0) return BadRequest();
        var comprobante = await _comprobantes.ObtenerAsync(id);
        return comprobante is null ? NotFound() : View(comprobante);
    }

    [HttpGet]
    public async Task<IActionResult> Pdf(int id)
    {
        if (id <= 0) return BadRequest();
        var comprobante = await _comprobantes.ObtenerAsync(id);
        if (comprobante is null) return NotFound();
        return File(_exportacion.ComprobantePdf(comprobante), "application/pdf", $"{SafeFileName(comprobante.NumeroComprobante)}.pdf");
    }

    private static string SafeFileName(string value) => string.Concat(value.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_'));
}
