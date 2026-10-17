namespace ElCortijo.Dominio.DTOs;

public sealed class CajaPanelDto
{
    public ArqueoCajaDto? CajaAbierta { get; init; }
    public IReadOnlyList<VentaCajaDto> Ventas { get; init; } = [];
    public IReadOnlyList<ArqueoCajaDto> Historial { get; init; } = [];
}

public sealed class ArqueoCajaDto
{
    public int IdArqueo { get; init; }
    public int IdUsuario { get; init; }
    public string Usuario { get; init; } = string.Empty;
    public DateTime FechaApertura { get; init; }
    public DateTime? FechaCierre { get; init; }
    public decimal MontoInicial { get; init; }
    public decimal TotalVentasEfectivo { get; init; }
    public decimal TotalVentasDigital { get; init; }
    public decimal TotalEsperado { get; init; }
    public decimal? TotalReal { get; init; }
    public decimal? Diferencia { get; init; }
    public string Estado { get; init; } = string.Empty;
}

public sealed class VentaCajaDto
{
    public int IdVenta { get; init; }
    public string NumeroComprobante { get; init; } = string.Empty;
    public DateTime FechaRegistro { get; init; }
    public string MetodoPago { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public string ClasificacionPago { get; init; } = string.Empty;
}

public sealed class ResultadoCajaDto
{
    public bool Exito { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public int? IdArqueo { get; init; }
}
