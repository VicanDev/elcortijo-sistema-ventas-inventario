using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Negocio.Interfaces;
using ElCortijo.Web.ViewModels;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador")]
public sealed class ReportesController : Controller
{
    private readonly IReporteService _reportes;
    private readonly IExportacionService _exportacion;

    public ReportesController(IReporteService reportes, IExportacionService exportacion)
    {
        _reportes = reportes;
        _exportacion = exportacion;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ReportesViewModel filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro.TipoReporte)) filtro.TipoReporte = "Ventas";
        if (!filtro.Desde.HasValue) filtro.Desde = DateTime.Today.AddDays(-30);
        if (!filtro.Hasta.HasValue) filtro.Hasta = DateTime.Today;
        if (!Validar(filtro)) return View(filtro);
        try
        {
            var dto = CrearFiltro(filtro);
            if (string.Equals(filtro.TipoReporte, "Movimientos", StringComparison.OrdinalIgnoreCase))
                filtro.Movimientos = await _reportes.ObtenerMovimientosAsync(dto);
            else
                filtro.Ventas = await _reportes.ObtenerVentasAsync(dto);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View(filtro);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarExcel(ReportesViewModel filtro)
    {
        if (!Validar(filtro)) return BadRequest(ModelState);
        var dto = CrearFiltro(filtro);
        if (string.Equals(filtro.TipoReporte, "Movimientos", StringComparison.OrdinalIgnoreCase))
        {
            var reporte = await _reportes.ObtenerMovimientosAsync(dto);
            return File(_exportacion.ReporteMovimientosExcel(reporte), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "movimientos-inventario.xlsx");
        }
        var ventas = await _reportes.ObtenerVentasAsync(dto);
        return File(_exportacion.ReporteVentasExcel(ventas), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "reporte-ventas.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPdf(ReportesViewModel filtro)
    {
        if (!Validar(filtro)) return BadRequest(ModelState);
        var dto = CrearFiltro(filtro);
        if (string.Equals(filtro.TipoReporte, "Movimientos", StringComparison.OrdinalIgnoreCase))
        {
            var reporte = await _reportes.ObtenerMovimientosAsync(dto);
            return File(_exportacion.ReporteMovimientosPdf(reporte), "application/pdf", "movimientos-inventario.pdf");
        }
        var ventas = await _reportes.ObtenerVentasAsync(dto);
        return File(_exportacion.ReporteVentasPdf(ventas), "application/pdf", "reporte-ventas.pdf");
    }

    private bool Validar(ReportesViewModel filtro)
    {
        if (!ModelState.IsValid) return false;
        if (!filtro.Desde.HasValue || !filtro.Hasta.HasValue || filtro.Desde.Value.Date > filtro.Hasta.Value.Date)
            ModelState.AddModelError(string.Empty, "Selecciona un rango de fechas válido.");
        if (!string.Equals(filtro.TipoReporte, "Ventas", StringComparison.OrdinalIgnoreCase) && !string.Equals(filtro.TipoReporte, "Movimientos", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(filtro.TipoReporte), "El tipo de reporte no es válido.");
        if (filtro.Hasta?.Date == DateTime.MaxValue.Date)
            ModelState.AddModelError(nameof(filtro.Hasta), "La fecha final está fuera del rango admitido.");
        return ModelState.IsValid;
    }

    private static FiltroReporteDto CrearFiltro(ReportesViewModel filtro) => new()
    {
        Desde = filtro.Desde!.Value,
        Hasta = filtro.Hasta!.Value,
        EstadoVenta = filtro.EstadoVenta,
        TipoComprobante = filtro.TipoComprobante,
        MetodoPago = filtro.MetodoPago,
        TipoMovimiento = filtro.TipoMovimiento,
        IdProducto = filtro.IdProducto
    };
}
