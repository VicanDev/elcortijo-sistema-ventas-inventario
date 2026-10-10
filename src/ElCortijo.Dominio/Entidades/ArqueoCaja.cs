namespace ElCortijo.Dominio.Entidades;

public class ArqueoCaja
{
    public int IdArqueo { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaApertura { get; set; } = DateTime.Now;
    public DateTime? FechaCierre { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal TotalVentasEfectivo { get; set; }
    public decimal TotalVentasDigital { get; set; }
    public decimal TotalEsperado { get; set; }
    public decimal? TotalReal { get; set; }
    public decimal? Diferencia { get; set; }
    public string Estado { get; set; } = "Abierta";

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
