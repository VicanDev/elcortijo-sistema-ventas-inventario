using ElCortijo.Dominio.Entidades;

namespace ElCortijo.Web.ViewModels;

public class DashboardViewModel
{
    public decimal TotalVentasHoy { get; set; }
    public int CantidadVentasHoy { get; set; }
    public int TotalProductos { get; set; }
    public int ProductosBajoStock { get; set; }
    public List<Producto> ListaProductosAlerta { get; set; } = new List<Producto>();
    public List<Venta> UltimasVentas { get; set; } = new List<Venta>();
}
