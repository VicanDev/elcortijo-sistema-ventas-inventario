namespace ElCortijo.Dominio.DTOs;

public sealed class ComprobanteDto
{
    public int IdVenta { get; init; }
    public string NumeroComprobante { get; init; } = string.Empty;
    public string TipoComprobante { get; init; } = string.Empty;
    public DateTime FechaRegistro { get; init; }
    public string Estado { get; init; } = string.Empty;
    public string? ClienteNombre { get; init; }
    public string? ClienteTipoDocumento { get; init; }
    public string? ClienteNumeroDocumento { get; init; }
    public string? ClienteDireccion { get; init; }
    public string? Vendedor { get; init; }
    public string MetodoPago { get; init; } = string.Empty;
    public decimal SubTotal { get; init; }
    public decimal Igv { get; init; }
    public decimal Total { get; init; }
    public IReadOnlyList<ComprobanteDetalleDto> Detalles { get; init; } = [];
}

public sealed class ComprobanteDetalleDto
{
    public string Producto { get; init; } = "Producto no disponible";
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal SubTotal { get; init; }
}
