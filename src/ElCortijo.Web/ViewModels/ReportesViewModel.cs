using System.ComponentModel.DataAnnotations;
using ElCortijo.Dominio.DTOs;

namespace ElCortijo.Web.ViewModels;

public sealed class ReportesViewModel
{
    [Required, DataType(DataType.Date)] public DateTime? Desde { get; set; }
    [Required, DataType(DataType.Date)] public DateTime? Hasta { get; set; }
    public string TipoReporte { get; set; } = "Ventas";
    public string? EstadoVenta { get; set; } = "Completada";
    public string? TipoComprobante { get; set; }
    public string? MetodoPago { get; set; }
    public string? TipoMovimiento { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "El producto debe ser un identificador válido.")]
    public int? IdProducto { get; set; }
    public ReporteVentasDto? Ventas { get; set; }
    public ReporteMovimientosDto? Movimientos { get; set; }
}
