namespace ElCortijo.Dominio.DTOs;

public class UsuarioDto
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
}
