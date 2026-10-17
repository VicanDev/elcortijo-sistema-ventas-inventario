using ElCortijo.Dominio.DTOs;

namespace ElCortijo.Negocio.Interfaces;

public interface IReporteService
{
    Task<ReporteVentasDto> ObtenerVentasAsync(FiltroReporteDto filtro);
    Task<ReporteMovimientosDto> ObtenerMovimientosAsync(FiltroReporteDto filtro);
}
