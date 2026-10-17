using ElCortijo.Dominio.DTOs;

namespace ElCortijo.Negocio.Interfaces;

public interface IComprobanteService
{
    Task<IReadOnlyList<ComprobanteDto>> BuscarAsync(string? numero, string? tipo, DateTime? desde, DateTime? hasta);
    Task<ComprobanteDto?> ObtenerAsync(int idVenta);
}
