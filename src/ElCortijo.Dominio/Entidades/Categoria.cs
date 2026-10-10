namespace ElCortijo.Dominio.Entidades;

public class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Estado { get; set; } = true;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
