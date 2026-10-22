using System.ComponentModel.DataAnnotations;
using ElCortijo.Dominio.DTOs;

namespace ElCortijo.Web.ViewModels;

public sealed class CajaViewModel
{
    public CajaPanelDto Panel { get; set; } = new();
    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "El monto inicial debe estar entre cero y S/ 99,999,999.99.")]
    public decimal MontoInicial { get; set; }
    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "El monto contado debe estar entre cero y S/ 99,999,999.99.")]
    public decimal TotalReal { get; set; }
}
