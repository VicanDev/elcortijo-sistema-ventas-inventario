using ElCortijo.Dominio.Entidades;

namespace ElCortijo.Web.ViewModels;

public class ReporteResumenViewModel
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int CantidadVentas { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal TotalDigital { get; set; }
    public IReadOnlyList<Venta> UltimasVentas { get; set; } = [];
}
