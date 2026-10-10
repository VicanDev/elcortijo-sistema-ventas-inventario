namespace ElCortijo.Dominio.Entidades;

public class MovimientoInventario
{
    public int IdMovimiento { get; set; }
    public int IdProducto { get; set; }
    public int IdUsuario { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int StockAnterior { get; set; }
    public int StockPosterior { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public virtual Producto Producto { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
}
