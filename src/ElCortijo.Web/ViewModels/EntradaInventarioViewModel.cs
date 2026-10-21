using System.ComponentModel.DataAnnotations;

namespace ElCortijo.Web.ViewModels;

public class EntradaInventarioViewModel
{
    [Required(ErrorMessage = "Selecciona un producto.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un producto.")]
    [Display(Name = "Producto")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "Ingresa la cantidad.")]
    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    [Display(Name = "Cantidad")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "Ingresa el motivo.")]
    [StringLength(200)]
    [Display(Name = "Motivo")]
    public string Motivo { get; set; } = "Compra de mercadería";
}