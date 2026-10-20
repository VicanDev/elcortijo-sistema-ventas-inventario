using ElCortijo.Dominio.Entidades;
using ElCortijo.Dominio.DTOs;

namespace ElCortijo.Negocio.Interfaces;

public interface IVentaService
{
    Task<IEnumerable<Venta>> ObtenerTodasAsync(string? buscar = null, string? tipoComprobante = null, string? metodoPago = null, DateTime? desde = null, DateTime? hasta = null);
    Task<IEnumerable<Producto>> ObtenerProductosDisponiblesAsync();
    Task<(bool Exito, string? Error, int? IdVenta)> RegistrarAsync(VentaRegistroViewModel solicitud, int idUsuario);
}
