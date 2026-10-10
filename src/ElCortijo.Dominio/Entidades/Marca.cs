namespace ElCortijo.Dominio.Entidades;

public class Marca
{
    public int IdMarca { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
