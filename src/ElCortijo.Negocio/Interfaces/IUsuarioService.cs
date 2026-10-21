using ElCortijo.Dominio.DTOs;
using ElCortijo.Dominio.Entidades;

namespace ElCortijo.Negocio.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> ObtenerTodosAsync(string? buscar = null, int? idRol = null, bool? estado = null);
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
    Task<bool> CrearAsync(Usuario usuario, string claveTextoPlano);
    Task<bool> ActualizarAsync(Usuario usuario, string? nuevaClaveTextoPlano = null);
    Task<bool> CambiarEstadoAsync(int id);
    Task<IEnumerable<Rol>> ObtenerRolesAsync();
}
