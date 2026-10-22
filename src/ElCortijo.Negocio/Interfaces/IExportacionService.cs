using ElCortijo.Dominio.DTOs;

namespace ElCortijo.Negocio.Interfaces;

public interface IExportacionService
{
    byte[] ComprobantePdf(ComprobanteDto comprobante);
    byte[] ReporteVentasExcel(ReporteVentasDto reporte);
    byte[] ReporteVentasPdf(ReporteVentasDto reporte);
    byte[] ReporteMovimientosExcel(ReporteMovimientosDto reporte);
    byte[] ReporteMovimientosPdf(ReporteMovimientosDto reporte);
}
