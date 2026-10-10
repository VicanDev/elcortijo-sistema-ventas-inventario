namespace ElCortijo.Dominio.Entidades;

public class Usuario
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string ClaveHash { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public virtual Rol Rol { get; set; } = null!;
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public virtual ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    public virtual ICollection<ArqueoCaja> Arqueos { get; set; } = new List<ArqueoCaja>();
}
