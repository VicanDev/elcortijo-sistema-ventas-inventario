namespace ElCortijo.Dominio.Entidades;

public class Cliente
{
    public int IdCliente { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string NombreRazonSocial { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
