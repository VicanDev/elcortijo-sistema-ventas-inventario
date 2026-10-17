namespace ElCortijo.Negocio.Servicios;

/// <summary>Reference calculation for future tax-exclusive amounts; never used to alter stored sales.</summary>
public static class ImpuestoCalculator
{
    public const decimal TasaIgv = 0.18m;

    public static decimal CalcularIgvSobreBaseImponible(decimal baseImponible)
    {
        if (baseImponible < 0)
            throw new ArgumentOutOfRangeException(nameof(baseImponible), "La base imponible no puede ser negativa.");

        return decimal.Round(baseImponible * TasaIgv, 2, MidpointRounding.AwayFromZero);
    }
}
