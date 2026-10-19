using Microsoft.EntityFrameworkCore;
using ElCortijo.Datos;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Negocio.Interfaces;

namespace ElCortijo.Negocio.Servicios;

public class ClienteService : IClienteService
{
    private readonly ElCortijoDbContext _context;

    public ClienteService(ElCortijoDbContext context) => _context = context;

    public async Task<IEnumerable<Cliente>> ObtenerTodosAsync(bool soloActivos = false)
    {
        var consulta = _context.Clientes.AsNoTracking();
        if (soloActivos) consulta = consulta.Where(c => c.Estado);
        return await consulta.OrderBy(c => c.NombreRazonSocial).ToListAsync();
    }

    public Task<Cliente?> ObtenerPorIdAsync(int id) => _context.Clientes.FindAsync(id).AsTask();

    public async Task<(bool Exito, string? Error)> CrearAsync(Cliente cliente)
    {
        var error = await ValidarAsync(cliente);
        if (error != null) return (false, error);
        cliente.NumeroDocumento = cliente.NumeroDocumento.Trim();
        cliente.NombreRazonSocial = cliente.NombreRazonSocial.Trim();
        cliente.FechaRegistro = DateTime.Now;
        cliente.Estado = true;
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Exito, string? Error)> ActualizarAsync(Cliente cliente)
    {
        var existente = await _context.Clientes.FindAsync(cliente.IdCliente);
        if (existente == null) return (false, "El cliente solicitado no existe.");
        var error = await ValidarAsync(cliente);
        if (error != null) return (false, error);

        existente.TipoDocumento = cliente.TipoDocumento;
        existente.NumeroDocumento = cliente.NumeroDocumento.Trim();
        existente.NombreRazonSocial = cliente.NombreRazonSocial.Trim();
        existente.Direccion = cliente.Direccion?.Trim();
        existente.Telefono = cliente.Telefono?.Trim();
        existente.Correo = cliente.Correo?.Trim();
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> CambiarEstadoAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return false;
        cliente.Estado = !cliente.Estado;
        return await _context.SaveChangesAsync() > 0;
    }

    private async Task<string?> ValidarAsync(Cliente cliente)
    {
        cliente.TipoDocumento = cliente.TipoDocumento.Trim().ToUpperInvariant();
        cliente.NumeroDocumento = cliente.NumeroDocumento.Trim();
        if (cliente.TipoDocumento is not ("DNI" or "RUC")) return "El tipo de documento debe ser DNI o RUC.";
        if (cliente.TipoDocumento == "DNI" && !EsDniValido(cliente.NumeroDocumento)) return "El DNI debe tener exactamente 8 dígitos.";
        if (cliente.TipoDocumento == "RUC" && !EsRucValido(cliente.NumeroDocumento)) return "El RUC debe tener 11 dígitos y un dígito verificador válido.";
        var existe = await _context.Clientes.AnyAsync(c => c.NumeroDocumento == cliente.NumeroDocumento && c.IdCliente != cliente.IdCliente);
        return existe ? "Ya existe un cliente registrado con ese número de documento." : null;
    }

    private static bool EsDniValido(string documento) => documento.Length == 8 && documento.All(char.IsDigit);

    private static bool EsRucValido(string ruc)
    {
        if (ruc.Length != 11 || !ruc.All(char.IsDigit)) return false;
        var pesos = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        var suma = pesos.Select((peso, i) => peso * (ruc[i] - '0')).Sum();
        var resto = 11 - suma % 11;
        var verificador = resto is 10 or 11 ? 0 : resto;
        return ruc[10] - '0' == verificador;
    }
}
