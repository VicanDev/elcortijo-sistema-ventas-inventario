using QuestPDF.Infrastructure;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Negocio.Servicios;

namespace ElCortijo.Tests;

public sealed class ExportacionServiceTests
{
    static ExportacionServiceTests() => QuestPDF.Settings.License = LicenseType.Community;

    [Fact]
    public void ExportarComprobantePdf_ProducePdfConImportesPersistidos()
    {
        var comprobante = new ComprobanteDto
        {
            IdVenta = 1, NumeroComprobante = "B001-0001", TipoComprobante = "Boleta",
            ClienteNombre = "Cliente de prueba", MetodoPago = "Efectivo", SubTotal = 100m, Igv = 18m, Total = 118m,
            Detalles = [new ComprobanteDetalleDto { Producto = "Producto de prueba", Cantidad = 1, PrecioUnitario = 118m, SubTotal = 118m }]
        };

        var bytes = new ExportacionService().ComprobantePdf(comprobante);

        Assert.True(bytes.Length > 100);
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(bytes, 0, 5));
    }

    [Fact]
    public void ExportacionesExcel_ProducenArchivosValidosSinFilas()
    {
        var exporter = new ExportacionService();
        var sales = exporter.ReporteVentasExcel(new ReporteVentasDto { Desde = DateTime.Today, Hasta = DateTime.Today });
        var movements = exporter.ReporteMovimientosExcel(new ReporteMovimientosDto { Desde = DateTime.Today, Hasta = DateTime.Today });

        Assert.StartsWith("PK", System.Text.Encoding.ASCII.GetString(sales, 0, 2));
        Assert.StartsWith("PK", System.Text.Encoding.ASCII.GetString(movements, 0, 2));
    }

    [Fact]
    public void ExportacionesPdf_ProducenPdfSinFilas()
    {
        var exporter = new ExportacionService();
        var sales = exporter.ReporteVentasPdf(new ReporteVentasDto { Desde = DateTime.Today, Hasta = DateTime.Today });
        var movements = exporter.ReporteMovimientosPdf(new ReporteMovimientosDto { Desde = DateTime.Today, Hasta = DateTime.Today });

        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(sales, 0, 5));
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(movements, 0, 5));
    }
}
