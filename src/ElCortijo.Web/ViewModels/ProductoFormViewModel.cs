using System.ComponentModel.DataAnnotations;

namespace ElCortijo.Web.ViewModels;

public class ProductoFormViewModel
{
    public int IdProducto { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]
    public int IdCategoria { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una marca.")]
    public int IdMarca { get; set; }

    [Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "El precio de compra debe ser mayor a cero.")]
    [Display(Name = "Precio de compra")]
    public decimal PrecioCompra { get; set; }

    [Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "El precio de venta debe ser mayor a cero.")]
    [Display(Name = "Precio de venta")]
    public decimal PrecioVenta { get; set; }

    [Range(0, 1000000)]
    [Display(Name = "Stock mínimo")]
    public int StockMinimo { get; set; } = 5;

    [Required, StringLength(50)]
    [Display(Name = "Unidad de medida")]
    public string UnidadMedida { get; set; } = "Unidad";

    [StringLength(500), Url]
    [Display(Name = "URL de imagen")]
    public string? ImagenUrl { get; set; }

    public bool Estado { get; set; } = true;
}
