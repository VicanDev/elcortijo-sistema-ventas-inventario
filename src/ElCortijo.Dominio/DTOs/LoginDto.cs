using System.ComponentModel.DataAnnotations;

namespace ElCortijo.Dominio.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "El usuario o correo es obligatorio")]
    public string NombreUsuarioOCorreo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Clave { get; set; } = string.Empty;
}
