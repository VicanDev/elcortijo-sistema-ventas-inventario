using Microsoft.EntityFrameworkCore;
using ElCortijo.Datos;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Negocio.Servicios;

public class AuthService : IAuthService
{
    private readonly ElCortijoDbContext _context;

    public AuthService(ElCortijoDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string usuarioOCorreo, string clave)
    {
        var termino = usuarioOCorreo.Trim().ToLower();

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => 
                (u.NombreUsuario.ToLower() == termino || u.Correo.ToLower() == termino) 
                && u.Estado);

        if (usuario == null)
        {
            return null;
        }

        bool claveValida = VerificarClave(clave, usuario.ClaveHash);
        return claveValida ? usuario : null;
    }

    public string HashearClave(string clave)
    {
        return BCrypt.Net.BCrypt.HashPassword(clave, 11);
    }

    public bool VerificarClave(string clave, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(clave, hash);
        }
        catch
        {
            return false;
        }
    }
}
