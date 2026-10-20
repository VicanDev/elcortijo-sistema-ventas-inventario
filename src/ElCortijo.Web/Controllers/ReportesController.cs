using ElCortijo.Datos;
using ElCortijo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class ReportesController : Controller
{
    private readonly ElCortijoDbContext _context;

    public ReportesController(ElCortijoDbContext context) => _context = context;

    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, string? metodoPago, string? tipoComprobante)
    {
        var (fechaDesde, fechaHasta) = NormalizarFechas(desde, hasta);
        var ventas = await ConsultarVentas(fechaDesde, fechaHasta, metodoPago, tipoComprobante);

        var modelo = new ReporteResumenViewModel
        {
            Desde = fechaDesde,
            Hasta = fechaHasta,
            CantidadVentas = ventas.Count,
            TotalVentas = ventas.Sum(v => v.Total),
            TotalEfectivo = ventas.Where(v => v.MetodoPago == "Efectivo").Sum(v => v.Total),
            TotalDigital = ventas.Where(v => v.MetodoPago != "Efectivo").Sum(v => v.Total),
            UltimasVentas = ventas.Take(10).ToList()
        };

        ViewBag.MetodoPago = metodoPago;
        ViewBag.TipoComprobante = tipoComprobante;

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Excel(DateTime? desde, DateTime? hasta, string? metodoPago, string? tipoComprobante)
    {
        var (fechaDesde, fechaHasta) = NormalizarFechas(desde, hasta);
        var ventas = await ConsultarVentas(fechaDesde, fechaHasta, metodoPago, tipoComprobante);
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Ventas");
        hoja.Cell(1, 1).Value = "REPORTE DE VENTAS - EL CORTIJO";
        hoja.Range(1, 1, 1, 6).Merge();
        hoja.Cell(2, 1).Value = $"Periodo: {fechaDesde:dd/MM/yyyy} al {fechaHasta:dd/MM/yyyy}";
        hoja.Range(2, 1, 2, 6).Merge();
        var encabezados = new[] { "Comprobante", "Tipo", "Fecha", "Método de pago", "Subtotal (S/)", "IGV (S/)", "Total (S/)" };
        for (var col = 0; col < encabezados.Length; col++) hoja.Cell(4, col + 1).Value = encabezados[col];
        var encabezado = hoja.Range(4, 1, 4, encabezados.Length);
        encabezado.Style.Font.Bold = true;
        encabezado.Style.Font.FontColor = XLColor.White;
        encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#21409A");
        for (var i = 0; i < ventas.Count; i++)
        {
            var v = ventas[i]; var row = i + 5;
            hoja.Cell(row, 1).Value = v.NumeroComprobante;
            hoja.Cell(row, 2).Value = v.TipoComprobante;
            hoja.Cell(row, 3).Value = v.FechaRegistro;
            hoja.Cell(row, 3).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
            hoja.Cell(row, 4).Value = v.MetodoPago;
            hoja.Cell(row, 5).Value = (double)v.SubTotal;
            hoja.Cell(row, 6).Value = (double)v.Igv;
            hoja.Cell(row, 7).Value = (double)v.Total;
        }
        var totalRow = ventas.Count + 5;
        hoja.Cell(totalRow, 1).Value = "TOTAL";
        hoja.Range(totalRow, 1, totalRow, 4).Merge();
        if (ventas.Count > 0)
        {
            hoja.Cell(totalRow, 5).FormulaA1 = $"SUM(E5:E{totalRow - 1})";
            hoja.Cell(totalRow, 6).FormulaA1 = $"SUM(F5:F{totalRow - 1})";
            hoja.Cell(totalRow, 7).FormulaA1 = $"SUM(G5:G{totalRow - 1})";
        }
        else
        {
            hoja.Cell(totalRow, 5).Value = 0;
            hoja.Cell(totalRow, 6).Value = 0;
            hoja.Cell(totalRow, 7).Value = 0;
        }
        hoja.Range(totalRow, 1, totalRow, 7).Style.Font.Bold = true;
        hoja.Range(5, 5, totalRow, 7).Style.NumberFormat.Format = "S/ #,##0.00";
        hoja.Columns().AdjustToContents();
        hoja.SheetView.FreezeRows(4);
        using var stream = new MemoryStream();
        libro.SaveAs(stream);
        var nombre = $"reporte-ventas-{fechaDesde:yyyyMMdd}-{fechaHasta:yyyyMMdd}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombre);
    }

    [HttpGet]
    public async Task<IActionResult> Pdf(DateTime? desde, DateTime? hasta, string? metodoPago, string? tipoComprobante)
    {
        var (fechaDesde, fechaHasta) = NormalizarFechas(desde, hasta);
        var ventas = await ConsultarVentas(fechaDesde, fechaHasta, metodoPago, tipoComprobante);
        var total = ventas.Sum(v => v.Total);
        var pdf = Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(28);
            page.DefaultTextStyle(style => style.FontSize(9));
            page.Header().Column(column =>
            {
                column.Item().Text("DISTRIBUIDORA EL CORTIJO").FontSize(18).Bold().FontColor("#21409A");
                column.Item().Text("Reporte de ventas").FontSize(13).SemiBold();
                column.Item().Text($"Periodo: {fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}    |    Método: {metodoPago ?? "Todos"}    |    Comprobante: {tipoComprobante ?? "Todos"}").FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(5).Text($"Operaciones: {ventas.Count}     Total vendido: S/ {total:N2}").Bold();
            });
            page.Content().PaddingVertical(12).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.5f); columns.RelativeColumn(1); columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.2f); columns.RelativeColumn(1); columns.RelativeColumn(1); columns.RelativeColumn(1);
                });
                table.Header(header =>
                {
                    foreach (var title in new[] { "Comprobante", "Tipo", "Fecha", "Método de pago", "Subtotal", "IGV", "Total" })
                        header.Cell().Background("#21409A").Padding(5).Text(title).FontColor(Colors.White).Bold();
                });
                foreach (var v in ventas)
                {
                    table.Cell().Element(Cell).Text(v.NumeroComprobante);
                    table.Cell().Element(Cell).Text(v.TipoComprobante);
                    table.Cell().Element(Cell).Text(v.FechaRegistro.ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Element(Cell).Text(v.MetodoPago);
                    table.Cell().Element(Cell).Text($"S/ {v.SubTotal:N2}");
                    table.Cell().Element(Cell).Text($"S/ {v.Igv:N2}");
                    table.Cell().Element(Cell).Text($"S/ {v.Total:N2}").Bold();
                }
            });
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Generado el "); text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                text.Span("     •     Página "); text.CurrentPageNumber(); text.Span(" de "); text.TotalPages();
            });
        })).GeneratePdf();
        return File(pdf, "application/pdf", $"reporte-ventas-{fechaDesde:yyyyMMdd}-{fechaHasta:yyyyMMdd}.pdf");
    }

    private async Task<List<ElCortijo.Dominio.Entidades.Venta>> ConsultarVentas(DateTime desde, DateTime hasta, string? metodoPago, string? tipoComprobante)
    {
        var limiteHasta = hasta.AddDays(1);
        var consulta = _context.Ventas.AsNoTracking()
            .Where(v => v.FechaRegistro >= desde && v.FechaRegistro < limiteHasta && v.Estado == "Completada");
        if (!string.IsNullOrWhiteSpace(metodoPago)) consulta = consulta.Where(v => v.MetodoPago == metodoPago);
        if (!string.IsNullOrWhiteSpace(tipoComprobante)) consulta = consulta.Where(v => v.TipoComprobante == tipoComprobante);
        return await consulta.OrderByDescending(v => v.FechaRegistro).ToListAsync();
    }

    private static (DateTime Desde, DateTime Hasta) NormalizarFechas(DateTime? desde, DateTime? hasta)
    {
        var fechaDesde = (desde ?? DateTime.Today.AddDays(-29)).Date;
        var fechaHasta = (hasta ?? DateTime.Today).Date;
        return fechaHasta < fechaDesde ? (fechaHasta, fechaDesde) : (fechaDesde, fechaHasta);
    }

    private static QuestPDF.Infrastructure.IContainer Cell(QuestPDF.Infrastructure.IContainer container) =>
        container.BorderBottom(1).BorderColor("#E5E7EB").Padding(5);
}
