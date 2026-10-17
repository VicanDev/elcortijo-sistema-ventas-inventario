using ElCortijo.Dominio.DTOs;

namespace ElCortijo.Negocio.Interfaces;

public interface ICajaService
{
    Task<CajaPanelDto> ObtenerPanelAsync(int idUsuario);
    Task<ResultadoCajaDto> AbrirAsync(int idUsuario, decimal montoInicial);
    Task<ResultadoCajaDto> CerrarAsync(int idUsuario, int idArqueo, decimal totalReal);
}
