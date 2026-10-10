using System.ComponentModel.DataAnnotations;

namespace ElCortijo.Web.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingrese su nombre de usuario o correo")]
    [Display(Name = "Usuario o Correo")]
    public string NombreUsuarioOCorreo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese su contraseña")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Clave { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    public string? MensajeError { get; set; }
}
