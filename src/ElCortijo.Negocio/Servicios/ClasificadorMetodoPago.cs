namespace ElCortijo.Negocio.Servicios;

public enum ClasificacionPago
{
    NoClasificado,
    Efectivo,
    Digital
}

public static class ClasificadorMetodoPago
{
    private static readonly HashSet<string> MetodosDigitales = new(StringComparer.OrdinalIgnoreCase)
    {
        "Yape", "Plin", "Tarjeta", "Transferencia", "Digital"
    };

    public static ClasificacionPago Clasificar(string? metodoPago)
    {
        var metodo = metodoPago?.Trim();
        if (string.Equals(metodo, "Efectivo", StringComparison.OrdinalIgnoreCase))
            return ClasificacionPago.Efectivo;
        return metodo is not null && MetodosDigitales.Contains(metodo)
            ? ClasificacionPago.Digital
            : ClasificacionPago.NoClasificado;
    }
}
