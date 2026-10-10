namespace ElCortijo.Dominio.Entidades;

public class DetalleVenta
{
    public int IdDetalleVenta { get; set; }
    public int IdVenta { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubTotal { get; set; }

    public virtual Venta Venta { get; set; } = null!;
    public virtual Producto Producto { get; set; } = null!;
}
