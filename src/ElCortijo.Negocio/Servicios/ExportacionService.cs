using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Negocio.Servicios;

public sealed class ExportacionService : IExportacionService
{
    public byte[] ComprobantePdf(ComprobanteDto comprobante) => CrearPdf(document => document.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(36);
        page.DefaultTextStyle(s => s.FontSize(10));
        page.Content().Column(column =>
        {
            column.Item().Text("EL CORTIJO").FontSize(20).Bold();
            column.Item().Text("REPRESENTACIÓN IMPRESA DE COMPROBANTE REGISTRADO").FontSize(11).Bold();
            column.Item().Text($"{comprobante.TipoComprobante} {comprobante.NumeroComprobante}").FontSize(16).Bold();
            column.Item().Text($"Fecha: {comprobante.FechaRegistro:dd/MM/yyyy HH:mm}    Estado: {comprobante.Estado}");
            column.Item().PaddingTop(8).Text($"Cliente: {comprobante.ClienteNombre ?? "Datos de cliente no disponibles"}");
            column.Item().Text($"Documento: {comprobante.ClienteTipoDocumento ?? "-"} {comprobante.ClienteNumeroDocumento ?? "-"}");
            column.Item().Text($"Dirección: {comprobante.ClienteDireccion ?? "-"}");
            column.Item().PaddingTop(14).Table(table =>
            {
                table.ColumnsDefinition(columns => { columns.RelativeColumn(5); columns.RelativeColumn(); columns.RelativeColumn(1.5f); columns.RelativeColumn(1.5f); });
                table.Header(header =>
                {
                    HeaderCell(header, "Producto"); HeaderCell(header, "Cant."); HeaderCell(header, "P. unitario"); HeaderCell(header, "Subtotal");
                });
                foreach (var detail in comprobante.Detalles)
                {
                    table.Cell().Element(Cell).Text(detail.Producto);
                    table.Cell().Element(Cell).AlignRight().Text(detail.Cantidad.ToString());
                    table.Cell().Element(Cell).AlignRight().Text(detail.PrecioUnitario.ToString("N2"));
                    table.Cell().Element(Cell).AlignRight().Text(detail.SubTotal.ToString("N2"));
                }
                if (comprobante.Detalles.Count == 0)
                {
                    table.Cell().ColumnSpan(4).Element(Cell).Text("No hay detalles de productos disponibles.");
                }
            });
            column.Item().PaddingTop(12).AlignRight().Column(totals =>
            {
                totals.Item().Text($"Subtotal registrado: S/ {comprobante.SubTotal:N2}");
                totals.Item().Text($"IGV registrado (referencia 18%): S/ {comprobante.Igv:N2}");
                totals.Item().Text($"Total registrado: S/ {comprobante.Total:N2}").Bold();
            });
            column.Item().PaddingTop(6).Text($"Pago: {comprobante.MetodoPago}    Vendedor: {comprobante.Vendedor ?? "-"}");
            column.Item().PaddingTop(16).Text("Documento generado desde los datos de la venta. No es un comprobante electrónico validado por SUNAT.").FontSize(8);
        });
        page.Footer().AlignCenter().Text(text => { text.Span("Página "); text.CurrentPageNumber(); });
    }));

    public byte[] ReporteVentasPdf(ReporteVentasDto reporte) => CrearPdf(document => document.Page(page =>
    {
        page.Size(PageSizes.A4.Landscape()); page.Margin(28); page.DefaultTextStyle(s => s.FontSize(8));
        page.Content().Column(column =>
        {
            column.Item().Text("Reporte de ventas — El Cortijo").FontSize(16).Bold();
            column.Item().Text($"Periodo: {reporte.Desde:dd/MM/yyyy} al {reporte.Hasta:dd/MM/yyyy}");
            column.Item().Text($"Ventas completadas: {reporte.CantidadVentas} | Total: S/ {reporte.TotalVentasCompletadas:N2} | IGV registrado: S/ {reporte.TotalIgvCompletadas:N2}");
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(1.2f); c.RelativeColumn(1.2f); c.RelativeColumn(1.2f); c.RelativeColumn(2.5f); c.RelativeColumn(1.5f); c.RelativeColumn(1.1f); c.RelativeColumn(1.1f); c.RelativeColumn(1.1f); });
                table.Header(h => { HeaderCell(h,"Fecha"); HeaderCell(h,"Comprobante"); HeaderCell(h,"Tipo"); HeaderCell(h,"Cliente"); HeaderCell(h,"Pago"); HeaderCell(h,"Estado"); HeaderCell(h,"IGV"); HeaderCell(h,"Total"); });
                foreach (var row in reporte.Filas)
                {
                    table.Cell().Element(Cell).Text(row.Fecha.ToString("dd/MM/yyyy HH:mm")); table.Cell().Element(Cell).Text(row.NumeroComprobante);
                    table.Cell().Element(Cell).Text(row.TipoComprobante); table.Cell().Element(Cell).Text(row.Cliente); table.Cell().Element(Cell).Text(row.MetodoPago);
                    table.Cell().Element(Cell).Text(row.Estado); table.Cell().Element(Cell).AlignRight().Text(row.Igv.ToString("N2")); table.Cell().Element(Cell).AlignRight().Text(row.Total.ToString("N2"));
                }
            });
            if (reporte.Filas.Count == 0) column.Item().PaddingTop(10).Text("No hay ventas para el periodo seleccionado.");
        });
        page.Footer().AlignCenter().Text(text => { text.Span("Página "); text.CurrentPageNumber(); });
    }));

    public byte[] ReporteMovimientosPdf(ReporteMovimientosDto reporte) => CrearPdf(document => document.Page(page =>
    {
        page.Size(PageSizes.A4.Landscape()); page.Margin(28); page.DefaultTextStyle(s => s.FontSize(8));
        page.Content().Column(column =>
        {
            column.Item().Text("Reporte de movimientos de inventario — El Cortijo").FontSize(16).Bold();
            column.Item().Text($"Periodo: {reporte.Desde:dd/MM/yyyy} al {reporte.Hasta:dd/MM/yyyy} | Movimientos: {reporte.CantidadMovimientos} | Unidades de entrada: {reporte.TotalUnidadesEntrada} | Unidades de salida: {reporte.TotalUnidadesSalida}");
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(1.3f); c.RelativeColumn(2.4f); c.RelativeColumn(1.2f); c.RelativeColumn(1.2f); c.RelativeColumn(1f); c.RelativeColumn(1f); c.RelativeColumn(1f); c.RelativeColumn(2f); c.RelativeColumn(1.8f); });
                table.Header(h => { HeaderCell(h,"Fecha"); HeaderCell(h,"Producto"); HeaderCell(h,"Código"); HeaderCell(h,"Tipo"); HeaderCell(h,"Cantidad"); HeaderCell(h,"Stock ant."); HeaderCell(h,"Stock post."); HeaderCell(h,"Motivo"); HeaderCell(h,"Usuario"); });
                foreach (var row in reporte.Filas)
                {
                    table.Cell().Element(Cell).Text(row.Fecha.ToString("dd/MM/yyyy HH:mm")); table.Cell().Element(Cell).Text(row.Producto); table.Cell().Element(Cell).Text(row.CodigoProducto);
                    table.Cell().Element(Cell).Text(row.TipoMovimiento); table.Cell().Element(Cell).AlignRight().Text(row.Cantidad.ToString()); table.Cell().Element(Cell).AlignRight().Text(row.StockAnterior.ToString());
                    table.Cell().Element(Cell).AlignRight().Text(row.StockPosterior.ToString()); table.Cell().Element(Cell).Text(row.Motivo); table.Cell().Element(Cell).Text(row.Usuario);
                }
            });
            if (reporte.Filas.Count == 0) column.Item().PaddingTop(10).Text("No hay movimientos para el periodo seleccionado.");
        });
        page.Footer().AlignCenter().Text(text => { text.Span("Página "); text.CurrentPageNumber(); });
    }));

    public byte[] ReporteVentasExcel(ReporteVentasDto reporte) => CreateWorkbook("Ventas", sheet =>
    {
        sheet.Cell(1, 1).Value = "Reporte de ventas — El Cortijo";
        sheet.Cell(2, 1).Value = $"Periodo: {reporte.Desde:dd/MM/yyyy} al {reporte.Hasta:dd/MM/yyyy}";
        sheet.Cell(3, 1).Value = "Ventas completadas"; sheet.Cell(3, 2).Value = reporte.CantidadVentas;
        sheet.Cell(4, 1).Value = "Subtotal completadas"; sheet.Cell(4, 2).Value = reporte.TotalSubTotalCompletadas;
        sheet.Cell(5, 1).Value = "IGV registrado completadas"; sheet.Cell(5, 2).Value = reporte.TotalIgvCompletadas;
        sheet.Cell(6, 1).Value = "Total completadas"; sheet.Cell(6, 2).Value = reporte.TotalVentasCompletadas;
        var headers = new[] { "Fecha", "Número", "Tipo", "Cliente", "Método de pago", "Estado", "Subtotal", "IGV registrado", "Total" };
        SetHeaders(sheet, 8, headers);
        for (var i = 0; i < reporte.Filas.Count; i++)
        {
            var row = i + 9; var item = reporte.Filas[i];
            sheet.Cell(row, 1).Value = item.Fecha; sheet.Cell(row, 2).Value = item.NumeroComprobante; sheet.Cell(row, 3).Value = item.TipoComprobante;
            sheet.Cell(row, 4).Value = item.Cliente; sheet.Cell(row, 5).Value = item.MetodoPago; sheet.Cell(row, 6).Value = item.Estado;
            sheet.Cell(row, 7).Value = item.SubTotal; sheet.Cell(row, 8).Value = item.Igv; sheet.Cell(row, 9).Value = item.Total;
        }
        FormatTable(sheet, 8, reporte.Filas.Count + 8, headers.Length);
        sheet.Column(1).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
        sheet.Columns(7, 9).Style.NumberFormat.Format = "S/ #,##0.00";
    });

    public byte[] ReporteMovimientosExcel(ReporteMovimientosDto reporte) => CreateWorkbook("Movimientos", sheet =>
    {
        sheet.Cell(1, 1).Value = "Reporte de movimientos de inventario — El Cortijo";
        sheet.Cell(2, 1).Value = $"Periodo: {reporte.Desde:dd/MM/yyyy} al {reporte.Hasta:dd/MM/yyyy}";
        sheet.Cell(3, 1).Value = "Movimientos"; sheet.Cell(3, 2).Value = reporte.CantidadMovimientos;
        sheet.Cell(4, 1).Value = "Unidades de entrada"; sheet.Cell(4, 2).Value = reporte.TotalUnidadesEntrada;
        sheet.Cell(5, 1).Value = "Unidades de salida"; sheet.Cell(5, 2).Value = reporte.TotalUnidadesSalida;
        var headers = new[] { "Fecha", "Producto", "Código", "Tipo", "Cantidad", "Stock anterior", "Stock posterior", "Motivo", "Usuario" };
        SetHeaders(sheet, 7, headers);
        for (var i = 0; i < reporte.Filas.Count; i++)
        {
            var row = i + 8; var item = reporte.Filas[i];
            sheet.Cell(row, 1).Value = item.Fecha; sheet.Cell(row, 2).Value = item.Producto; sheet.Cell(row, 3).Value = item.CodigoProducto;
            sheet.Cell(row, 4).Value = item.TipoMovimiento; sheet.Cell(row, 5).Value = item.Cantidad; sheet.Cell(row, 6).Value = item.StockAnterior;
            sheet.Cell(row, 7).Value = item.StockPosterior; sheet.Cell(row, 8).Value = item.Motivo; sheet.Cell(row, 9).Value = item.Usuario;
        }
        FormatTable(sheet, 7, reporte.Filas.Count + 7, headers.Length);
        sheet.Column(1).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
    });

    private static byte[] CrearPdf(Action<IDocumentContainer> compose)
    {
        var document = Document.Create(compose);
        return document.GeneratePdf();
    }

    private static byte[] CreateWorkbook(string name, Action<IXLWorksheet> fill)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(name);
        fill(sheet);
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void SetHeaders(IXLWorksheet sheet, int row, IReadOnlyList<string> headers)
    {
        for (var column = 0; column < headers.Count; column++) sheet.Cell(row, column + 1).Value = headers[column];
    }

    private static void FormatTable(IXLWorksheet sheet, int headerRow, int lastRow, int lastColumn)
    {
        var range = sheet.Range(headerRow, 1, Math.Max(lastRow, headerRow), lastColumn);
        range.CreateTable();
        sheet.Row(headerRow).Style.Font.Bold = true;
        sheet.SheetView.FreezeRows(headerRow);
    }

    private static void HeaderCell(TableCellDescriptor header, string text) => header.Cell().Element(Cell).Background(Colors.Grey.Lighten2).Text(text).Bold();
    private static IContainer Cell(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingHorizontal(3);
}
