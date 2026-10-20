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
    public List<VentasPorDiaViewModel> VentasUltimosSieteDias { get; set; } = [];
    public string VentasSemanaPuntosSvg { get; set; } = string.Empty;
    public string VentasSemanaAreaSvg { get; set; } = string.Empty;
    public List<ProductoMasVendidoViewModel> ProductosMasVendidos { get; set; } = [];
    public List<MetodoPagoResumenViewModel> VentasPorMetodoPago { get; set; } = [];
}

public class VentasPorDiaViewModel
{
    public DateTime Fecha { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Cantidad { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}

public class ProductoMasVendidoViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal TotalVendido { get; set; }
    public double Porcentaje { get; set; }
}

public class MetodoPagoResumenViewModel
{
    public string Metodo { get; set; } = string.Empty;
    public int Operaciones { get; set; }
    public decimal Total { get; set; }
    public double Porcentaje { get; set; }
}
