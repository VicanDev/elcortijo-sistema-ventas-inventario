namespace ElCortijo.Dominio.DTOs;

public sealed class FiltroReporteDto
{
    public DateTime Desde { get; init; }
    public DateTime Hasta { get; init; }
    public string? EstadoVenta { get; init; }
    public string? TipoComprobante { get; init; }
    public string? MetodoPago { get; init; }
    public string? TipoMovimiento { get; init; }
    public int? IdProducto { get; init; }
}

public sealed class ReporteVentasDto
{
    public DateTime Desde { get; init; }
    public DateTime Hasta { get; init; }
    public int CantidadVentas { get; init; }
    public decimal TotalSubTotalCompletadas { get; init; }
    public decimal TotalIgvCompletadas { get; init; }
    public decimal TotalVentasCompletadas { get; init; }
    public IReadOnlyList<ReporteVentaFilaDto> Filas { get; init; } = [];
}

public sealed class ReporteVentaFilaDto
{
    public int IdVenta { get; init; }
    public DateTime Fecha { get; init; }
    public string NumeroComprobante { get; init; } = string.Empty;
    public string TipoComprobante { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public string MetodoPago { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public decimal SubTotal { get; init; }
    public decimal Igv { get; init; }
    public decimal Total { get; init; }
}

public sealed class ReporteMovimientosDto
{
    public DateTime Desde { get; init; }
    public DateTime Hasta { get; init; }
    public int CantidadMovimientos { get; init; }
    public int TotalUnidadesEntrada { get; init; }
    public int TotalUnidadesSalida { get; init; }
    public IReadOnlyList<ReporteMovimientoFilaDto> Filas { get; init; } = [];
}

public sealed class ReporteMovimientoFilaDto
{
    public DateTime Fecha { get; init; }
    public string Producto { get; init; } = string.Empty;
    public string CodigoProducto { get; init; } = string.Empty;
    public string TipoMovimiento { get; init; } = string.Empty;
    public int Cantidad { get; init; }
    public int StockAnterior { get; init; }
    public int StockPosterior { get; init; }
    public string Motivo { get; init; } = string.Empty;
    public string Usuario { get; init; } = string.Empty;
}
