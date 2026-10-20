using ElCortijo.Dominio.Entidades;

namespace ElCortijo.Negocio.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<Cliente>> ObtenerTodosAsync(bool soloActivos = false, string? buscar = null, string? tipoDocumento = null, bool? estado = null);
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<(bool Exito, string? Error)> CrearAsync(Cliente cliente);
    Task<(bool Exito, string? Error)> ActualizarAsync(Cliente cliente);
    Task<bool> CambiarEstadoAsync(int id);
}
