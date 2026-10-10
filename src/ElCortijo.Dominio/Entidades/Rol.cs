namespace ElCortijo.Dominio.Entidades;

public class Rol
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Estado { get; set; } = true;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
