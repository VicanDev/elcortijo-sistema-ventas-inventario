namespace ElCortijo.Dominio.Entidades;

public class Venta
{
    public int IdVenta { get; set; }
    public string NumeroComprobante { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = "Boleta";
    public int IdCliente { get; set; }
    public int IdUsuario { get; set; }
    public int? IdArqueo { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = "Efectivo";
    public decimal MontoRecibido { get; set; }
    public decimal Vuelto { get; set; }
    public string Estado { get; set; } = "Completada";
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public virtual Cliente Cliente { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ArqueoCaja? Arqueo { get; set; }
    public virtual ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
}
