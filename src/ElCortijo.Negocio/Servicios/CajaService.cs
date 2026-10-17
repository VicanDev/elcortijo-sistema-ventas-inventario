using System.Data;
using Microsoft.EntityFrameworkCore;
using ElCortijo.Datos;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Negocio.Servicios;

public sealed class CajaService : ICajaService
{
    private const decimal MaximoMonto = 99999999.99m;
    private const string Abierta = "Abierta";
    private const string Cerrada = "Cerrada";
    private const string Completada = "Completada";
    private readonly ElCortijoDbContext _context;

    public CajaService(ElCortijoDbContext context) => _context = context;

    public async Task<CajaPanelDto> ObtenerPanelAsync(int idUsuario)
    {
        var abierta = await _context.ArqueosCaja.AsNoTracking()
            .Include(a => a.Usuario)
            .Where(a => a.IdUsuario == idUsuario && a.Estado == Abierta)
            .OrderByDescending(a => a.FechaApertura)
            .FirstOrDefaultAsync();

        var ventas = abierta is null
            ? new List<VentaCajaDto>()
            : (await _context.Ventas.AsNoTracking()
                .Where(v => v.IdArqueo == abierta.IdArqueo && v.Estado == Completada)
                .OrderByDescending(v => v.FechaRegistro)
                .Select(v => new { v.IdVenta, v.NumeroComprobante, v.FechaRegistro, v.MetodoPago, v.Total })
                .ToListAsync())
                .Select(v => new VentaCajaDto
                {
                    IdVenta = v.IdVenta,
                    NumeroComprobante = v.NumeroComprobante,
                    FechaRegistro = v.FechaRegistro,
                    MetodoPago = v.MetodoPago,
                    Total = v.Total,
                    ClasificacionPago = ClasificadorMetodoPago.Clasificar(v.MetodoPago).ToString()
                }).ToList();

        var historial = await _context.ArqueosCaja.AsNoTracking()
            .Include(a => a.Usuario)
            .Where(a => a.IdUsuario == idUsuario)
            .OrderByDescending(a => a.FechaApertura)
            .Take(50)
            .ToListAsync();

        return new CajaPanelDto
        {
            CajaAbierta = abierta is null ? null : Map(abierta),
            Ventas = ventas,
            Historial = historial.Select(Map).ToList()
        };
    }

    public async Task<ResultadoCajaDto> AbrirAsync(int idUsuario, decimal montoInicial)
    {
        if (idUsuario <= 0)
            return new() { Mensaje = "No se pudo identificar al usuario." };
        if (montoInicial < 0 || montoInicial > MaximoMonto)
            return new() { Mensaje = "El monto inicial debe estar entre S/ 0.00 y S/ 99,999,999.99." };

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var yaAbierta = await _context.ArqueosCaja.AnyAsync(a => a.IdUsuario == idUsuario && a.Estado == Abierta);
        if (yaAbierta)
        {
            await transaction.RollbackAsync();
            return new() { Mensaje = "Ya tienes una caja abierta. Debes cerrarla antes de abrir otra." };
        }

        var arqueo = new ArqueoCaja
        {
            IdUsuario = idUsuario,
            FechaApertura = DateTime.Now,
            MontoInicial = decimal.Round(montoInicial, 2, MidpointRounding.AwayFromZero),
            Estado = Abierta
        };
        _context.ArqueosCaja.Add(arqueo);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new() { Exito = true, IdArqueo = arqueo.IdArqueo, Mensaje = "Caja abierta correctamente." };
    }

    public async Task<ResultadoCajaDto> CerrarAsync(int idUsuario, int idArqueo, decimal totalReal)
    {
        if (idUsuario <= 0 || idArqueo <= 0)
            return new() { Mensaje = "No se pudo identificar la caja o el usuario." };
        if (totalReal < 0 || totalReal > MaximoMonto)
            return new() { Mensaje = "El monto contado debe estar entre S/ 0.00 y S/ 99,999,999.99." };

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var arqueo = await _context.ArqueosCaja
            .SingleOrDefaultAsync(a => a.IdArqueo == idArqueo && a.IdUsuario == idUsuario);

        if (arqueo is null)
        {
            await transaction.RollbackAsync();
            return new() { Mensaje = "No se encontró una caja de este usuario." };
        }
        if (arqueo.Estado != Abierta || arqueo.FechaCierre.HasValue)
        {
            await transaction.RollbackAsync();
            return new() { Mensaje = "La caja ya está cerrada." };
        }

        var pagos = await _context.Ventas.AsNoTracking()
            .Where(v => v.IdArqueo == idArqueo && v.Estado == Completada)
            .Select(v => new { v.MetodoPago, v.Total })
            .ToListAsync();

        var efectivo = pagos.Where(p => ClasificadorMetodoPago.Clasificar(p.MetodoPago) == ClasificacionPago.Efectivo).Sum(p => p.Total);
        var digital = pagos.Where(p => ClasificadorMetodoPago.Clasificar(p.MetodoPago) == ClasificacionPago.Digital).Sum(p => p.Total);

        arqueo.TotalVentasEfectivo = efectivo;
        arqueo.TotalVentasDigital = digital;
        arqueo.TotalEsperado = arqueo.MontoInicial + efectivo;
        arqueo.TotalReal = decimal.Round(totalReal, 2, MidpointRounding.AwayFromZero);
        arqueo.Diferencia = arqueo.TotalReal.Value - arqueo.TotalEsperado;
        arqueo.FechaCierre = DateTime.Now;
        arqueo.Estado = Cerrada;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return new() { Exito = true, IdArqueo = arqueo.IdArqueo, Mensaje = "Caja cerrada correctamente." };
    }

    private static ArqueoCajaDto Map(ArqueoCaja a) => new()
    {
        IdArqueo = a.IdArqueo,
        IdUsuario = a.IdUsuario,
        Usuario = a.Usuario?.NombreCompleto ?? "Usuario no disponible",
        FechaApertura = a.FechaApertura,
        FechaCierre = a.FechaCierre,
        MontoInicial = a.MontoInicial,
        TotalVentasEfectivo = a.TotalVentasEfectivo,
        TotalVentasDigital = a.TotalVentasDigital,
        TotalEsperado = a.TotalEsperado,
        TotalReal = a.TotalReal,
        Diferencia = a.Diferencia,
        Estado = a.Estado
    };
}
