using System.ComponentModel.DataAnnotations;
using ElCortijo.Dominio.DTOs;

namespace ElCortijo.Web.ViewModels;

public sealed class ComprobantesIndexViewModel
{
    public string? Numero { get; set; }
    public string? Tipo { get; set; }
    [DataType(DataType.Date)] public DateTime? Desde { get; set; }
    [DataType(DataType.Date)] public DateTime? Hasta { get; set; }
    public IReadOnlyList<ComprobanteDto> Comprobantes { get; set; } = [];
}
