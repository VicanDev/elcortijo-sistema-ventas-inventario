using System.ComponentModel.DataAnnotations;

namespace ElCortijo.Web.ViewModels;

public class CatalogoItemViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; } = true;
}
