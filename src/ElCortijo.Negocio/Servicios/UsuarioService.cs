using Microsoft.EntityFrameworkCore;
using ElCortijo.Datos;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Negocio.Servicios;

public class UsuarioService : IUsuarioService
{
    private readonly ElCortijoDbContext _context;
    private readonly IAuthService _authService;

    public UsuarioService(ElCortijoDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<IEnumerable<UsuarioDto>> ObtenerTodosAsync(string? buscar = null, int? idRol = null, bool? estado = null)
    {
        var consulta = _context.Usuarios.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            consulta = consulta.Where(u => u.NombreUsuario.Contains(buscar)
                || u.NombreCompleto.Contains(buscar)
                || u.Correo.Contains(buscar));
        }

        if (idRol.HasValue)
            consulta = consulta.Where(u => u.IdRol == idRol.Value);

        if (estado.HasValue)
            consulta = consulta.Where(u => u.Estado == estado.Value);

        return await consulta
            .Include(u => u.Rol)
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new UsuarioDto
            {
                IdUsuario = u.IdUsuario,
                IdRol = u.IdRol,
                RolNombre = u.Rol.Nombre,
                NombreCompleto = u.NombreCompleto,
                Correo = u.Correo,
                NombreUsuario = u.NombreUsuario,
                Telefono = u.Telefono,
                Estado = u.Estado,
                FechaRegistro = u.FechaRegistro
            })
            .ToListAsync();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == id);
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario.ToLower() == nombreUsuario.ToLower());
    }

    public async Task<bool> CrearAsync(Usuario usuario, string claveTextoPlano)
    {
        usuario.ClaveHash = _authService.HashearClave(claveTextoPlano);
        usuario.FechaRegistro = DateTime.Now;
        usuario.Estado = true;

        _context.Usuarios.Add(usuario);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ActualizarAsync(Usuario usuario, string? nuevaClaveTextoPlano = null)
    {
        var existente = await _context.Usuarios.FindAsync(usuario.IdUsuario);
        if (existente == null) return false;

        existente.NombreCompleto = usuario.NombreCompleto;
        existente.Correo = usuario.Correo;
        existente.Telefono = usuario.Telefono;
        existente.IdRol = usuario.IdRol;
        existente.Estado = usuario.Estado;

        if (!string.IsNullOrWhiteSpace(nuevaClaveTextoPlano))
        {
            existente.ClaveHash = _authService.HashearClave(nuevaClaveTextoPlano);
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> CambiarEstadoAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return false;

        usuario.Estado = !usuario.Estado;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Rol>> ObtenerRolesAsync()
    {
        return await _context.Roles.Where(r => r.Estado).ToListAsync();
    }
}
