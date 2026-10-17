using ElCortijo.Negocio.Servicios;

namespace ElCortijo.Tests;

public sealed class ReglasNegocioTests
{
    [Theory]
    [InlineData(100, 18)]
    [InlineData(0.05, 0.01)]
    [InlineData(0, 0)]
    public void CalcularIgvSobreBaseImponible_UsaTasa18YRedondeaADosDecimales(decimal baseImponible, decimal expected)
    {
        Assert.Equal(expected, ImpuestoCalculator.CalcularIgvSobreBaseImponible(baseImponible));
    }

    [Theory]
    [InlineData("Efectivo", ClasificacionPago.Efectivo)]
    [InlineData(" efectivo ", ClasificacionPago.Efectivo)]
    [InlineData("Yape", ClasificacionPago.Digital)]
    [InlineData("PLIN", ClasificacionPago.Digital)]
    [InlineData("Tarjeta", ClasificacionPago.Digital)]
    [InlineData("Transferencia", ClasificacionPago.Digital)]
    [InlineData("Otro", ClasificacionPago.NoClasificado)]
    [InlineData(null, ClasificacionPago.NoClasificado)]
    public void ClasificarMetodoPago_UsaCatalogoDocumentado(string? method, ClasificacionPago expected)
    {
        Assert.Equal(expected, ClasificadorMetodoPago.Clasificar(method));
    }

    [Fact]
    public void CalcularIgvSobreBaseImponible_RechazaBaseNegativa()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ImpuestoCalculator.CalcularIgvSobreBaseImponible(-1m));
    }
}
