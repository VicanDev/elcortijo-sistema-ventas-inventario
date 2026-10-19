using System.ComponentModel.DataAnnotations;

namespace ElCortijo.Dominio.DTOs;

public class VentaRegistroViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente.")]
    public int IdCliente { get; set; }
    [Required]
    public string MetodoPago { get; set; } = "Efectivo";
    [Required]
    public string TipoComprobante { get; set; } = "Boleta";
    [Range(0, 99999999, ErrorMessage = "El monto recibido no es válido.")]
    public decimal MontoRecibido { get; set; }
    public List<VentaItemViewModel> Items { get; set; } = [];
}

public class VentaItemViewModel
{
    [Range(1, int.MaxValue)]
    public int IdProducto { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { get; set; }
}
