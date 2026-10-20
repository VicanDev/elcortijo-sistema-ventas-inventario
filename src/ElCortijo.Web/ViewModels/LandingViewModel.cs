using ElCortijo.Dominio.Entidades;

namespace ElCortijo.Web.ViewModels;

public class LandingViewModel
{
    public IReadOnlyList<Producto> ProductosDestacados { get; init; } = [];
    public int TotalProductos { get; init; }
    public int TotalCategorias { get; init; }
    public int TotalMarcas { get; init; }
}
