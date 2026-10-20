using ElCortijo.Datos;
using ElCortijo.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ElCortijo.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public class ComprobantesController : Controller
{
    private readonly ElCortijoDbContext _context;

    public ComprobantesController(ElCortijoDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? buscar, string? tipo, DateTime? desde, DateTime? hasta)
    {
        var consulta = _context.Ventas.AsNoTracking().Include(v => v.Cliente).AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            consulta = consulta.Where(v => v.NumeroComprobante.Contains(buscar)
                || v.Cliente.NombreRazonSocial.Contains(buscar)
                || v.Cliente.NumeroDocumento.Contains(buscar));
        }

        if (!string.IsNullOrWhiteSpace(tipo))
            consulta = consulta.Where(v => v.TipoComprobante == tipo);

        if (desde.HasValue)
            consulta = consulta.Where(v => v.FechaRegistro >= desde.Value.Date);

        if (hasta.HasValue)
        {
            var limite = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(v => v.FechaRegistro < limite);
        }

        ViewBag.Buscar = buscar;
        ViewBag.Tipo = tipo;
        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
        return View(await consulta.OrderByDescending(v => v.FechaRegistro).ToListAsync());
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await ObtenerComprobante(id);

        return venta is null ? NotFound() : View(venta);
    }

    [HttpGet]
    public async Task<IActionResult> Pdf(int id)
    {
        var venta = await ObtenerComprobante(id);
        if (venta is null) return NotFound();

        var contenido = GenerarPdf(venta);
        var nombre = $"comprobante-{NombreSeguro(venta.NumeroComprobante)}.pdf";
        return File(contenido, "application/pdf", nombre);
    }

    [HttpGet]
    public async Task<IActionResult> Imprimir(int id)
    {
        var venta = await ObtenerComprobante(id);
        if (venta is null) return NotFound();

        // Sin nombre de descarga, el navegador abre el PDF para imprimirlo.
        return File(GenerarPdf(venta), "application/pdf");
    }

    private Task<Venta?> ObtenerComprobante(int id) => _context.Ventas
        .AsNoTracking()
        .Include(v => v.Cliente)
        .Include(v => v.Usuario)
        .Include(v => v.DetalleVentas)
            .ThenInclude(d => d.Producto)
        .FirstOrDefaultAsync(v => v.IdVenta == id);

    private static byte[] GenerarPdf(Venta venta) => Document.Create(document => document.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(36);
        page.DefaultTextStyle(style => style.FontSize(10).FontColor("#243247"));

        page.Header().Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("EL CORTIJO").FontSize(21).Bold().FontColor("#21409A");
                column.Item().Text("Distribuidora de alimentos para mascotas").FontColor(Colors.Grey.Darken1);
            });
            row.ConstantItem(190).Border(1).BorderColor("#D8E0EC").Padding(10).Column(column =>
            {
                column.Item().AlignCenter().Text(venta.TipoComprobante.ToUpperInvariant()).Bold().FontSize(12);
                column.Item().AlignCenter().Text(venta.NumeroComprobante).FontSize(14).FontColor("#21409A").Bold();
            });
        });

        page.Content().PaddingVertical(20).Column(column =>
        {
            column.Spacing(14);
            column.Item().Row(row =>
            {
                row.RelativeItem().Background("#F4F7FC").Padding(12).Column(customer =>
                {
                    customer.Item().Text("DATOS DEL CLIENTE").FontSize(9).Bold().FontColor("#21409A");
                    customer.Item().PaddingTop(5).Text(venta.Cliente.NombreRazonSocial).Bold();
                    customer.Item().Text($"{venta.Cliente.TipoDocumento}: {venta.Cliente.NumeroDocumento}");
                    if (!string.IsNullOrWhiteSpace(venta.Cliente.Direccion)) customer.Item().Text(venta.Cliente.Direccion);
                });
                row.ConstantItem(12);
                row.RelativeItem().Background("#F4F7FC").Padding(12).Column(info =>
                {
                    info.Item().Text("DATOS DE LA OPERACIÓN").FontSize(9).Bold().FontColor("#21409A");
                    info.Item().PaddingTop(5).Text($"Fecha: {venta.FechaRegistro:dd/MM/yyyy HH:mm}");
                    info.Item().Text($"Atendido por: {venta.Usuario.NombreCompleto}");
                    info.Item().Text($"Método de pago: {venta.MetodoPago}");
                });
            });

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.4f);
                });
                table.Header(header =>
                {
                    header.Cell().Background("#21409A").Padding(8).Text("PRODUCTO").FontColor(Colors.White).Bold().FontSize(8);
                    header.Cell().Background("#21409A").Padding(8).Text("CANTIDAD").FontColor(Colors.White).Bold().FontSize(8);
                    header.Cell().Background("#21409A").Padding(8).Text("PRECIO UNIT.").FontColor(Colors.White).Bold().FontSize(8);
                    header.Cell().Background("#21409A").Padding(8).Text("IMPORTE").FontColor(Colors.White).Bold().FontSize(8);
                });

                foreach (var detalle in venta.DetalleVentas.OrderBy(d => d.IdDetalleVenta))
                {
                    table.Cell().Element(BodyCell).Column(product =>
                    {
                        product.Item().Text(detalle.Producto.Nombre).Bold();
                        product.Item().Text(detalle.Producto.Codigo).FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                    table.Cell().Element(BodyCell).AlignRight().Text(detalle.Cantidad.ToString());
                    table.Cell().Element(BodyCell).AlignRight().Text($"S/ {detalle.PrecioUnitario:N2}");
                    table.Cell().Element(BodyCell).AlignRight().Text($"S/ {detalle.SubTotal:N2}");
                }
            });

            column.Item().AlignRight().Width(250).Column(totals =>
            {
                totals.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal");
                    row.ConstantItem(115).AlignRight().Text($"S/ {venta.SubTotal:N2}");
                });
                totals.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text("IGV (18%)");
                    row.ConstantItem(115).AlignRight().Text($"S/ {venta.Igv:N2}");
                });
                totals.Item().PaddingTop(7).BorderTop(1).BorderColor("#D8E0EC").PaddingTop(7).Row(row =>
                {
                    row.RelativeItem().Text("TOTAL").Bold().FontSize(13);
                    row.ConstantItem(115).AlignRight().Text($"S/ {venta.Total:N2}").Bold().FontSize(13).FontColor("#21409A");
                });
            });

            column.Item().PaddingTop(12).AlignCenter().Text("Gracias por su compra.").FontColor(Colors.Grey.Darken1);
            column.Item().AlignCenter().Text("Representación referencial generada por El Cortijo; no sustituye un comprobante electrónico emitido ante SUNAT.")
                .FontSize(8).FontColor(Colors.Grey.Darken1);
        });

        page.Footer().AlignCenter().Text(text =>
        {
            text.Span("Comprobante ");
            text.Span(venta.NumeroComprobante).Bold();
            text.Span("  •  Página ");
            text.CurrentPageNumber();
            text.Span(" de ");
            text.TotalPages();
        });
    })).GeneratePdf();

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(1).BorderColor("#E5E9F0").PaddingVertical(8).PaddingHorizontal(6);

    private static string NombreSeguro(string comprobante)
    {
        var nombre = new string(comprobante.Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrWhiteSpace(nombre) ? "venta" : nombre;
    }
}
