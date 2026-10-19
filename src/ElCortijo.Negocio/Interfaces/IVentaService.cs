using ElCortijo.Dominio.Entidades;
using ElCortijo.Dominio.DTOs;

namespace ElCortijo.Negocio.Interfaces;

public interface IVentaService
{
    Task<IEnumerable<Venta>> ObtenerTodasAsync();
    Task<IEnumerable<Producto>> ObtenerProductosDisponiblesAsync();
    Task<(bool Exito, string? Error, int? IdVenta)> RegistrarAsync(VentaRegistroViewModel solicitud, int idUsuario);
}
