using ElCortijo.Dominio.DTOs;
using ElCortijo.Dominio.Entidades;

namespace ElCortijo.Negocio.Interfaces;

public interface IAuthService
{
    Task<Usuario?> ValidarCredencialesAsync(string usuarioOCorreo, string clave);
    string HashearClave(string clave);
    bool VerificarClave(string clave, string hash);
}
