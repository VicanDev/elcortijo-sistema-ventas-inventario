using System.ComponentModel.DataAnnotations;

namespace ElCortijo.Dominio.Entidades;

public class Cliente
{
    public int IdCliente { get; set; }
    [Required(ErrorMessage = "Seleccione el tipo de documento.")]
    public string TipoDocumento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese el número de documento.")]
    [StringLength(20)]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese el nombre o razón social.")]
    [StringLength(150)]
    public string NombreRazonSocial { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    public string? Correo { get; set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
