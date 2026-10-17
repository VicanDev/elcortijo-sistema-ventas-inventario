using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ElCortijo.Datos;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Negocio.Servicios;

namespace ElCortijo.Tests;

public sealed class CajaServiceTests
{
    [Fact]
    public async Task AbrirAsync_AbreCajaYRechazaSegundaCajaAbiertaDelUsuario()
    {
        await using var fixture = await TestDatabase.CreateAsync();
        var service = new CajaService(fixture.Context);

        var opened = await service.AbrirAsync(fixture.UserId, 80m);
        var duplicate = await service.AbrirAsync(fixture.UserId, 30m);

        Assert.True(opened.Exito);
        Assert.NotNull(opened.IdArqueo);
        Assert.False(duplicate.Exito);
        Assert.Contains("Ya tienes una caja abierta", duplicate.Mensaje);
        Assert.Equal(1, await fixture.Context.ArqueosCaja.CountAsync(a => a.IdUsuario == fixture.UserId));
    }

    [Fact]
    public async Task CerrarAsync_CalculaTotalesDiferenciaYRechazaCierreDuplicado()
    {
        await using var fixture = await TestDatabase.CreateAsync();
        var caja = new ArqueoCaja { IdUsuario = fixture.UserId, MontoInicial = 100m, Estado = "Abierta", FechaApertura = DateTime.Now };
        fixture.Context.ArqueosCaja.Add(caja);
        await fixture.Context.SaveChangesAsync();
        fixture.Context.Ventas.AddRange(
            CreateSale(fixture.UserId, fixture.ClientId, caja.IdArqueo, "Efectivo", 50m, "B001-1"),
            CreateSale(fixture.UserId, fixture.ClientId, caja.IdArqueo, "Yape", 20m, "B001-2"),
            CreateSale(fixture.UserId, fixture.ClientId, caja.IdArqueo, "Otro", 5m, "B001-3"),
            CreateSale(fixture.UserId, fixture.ClientId, caja.IdArqueo, "Efectivo", 500m, "B001-4", "Anulada"));
        await fixture.Context.SaveChangesAsync();
        var service = new CajaService(fixture.Context);

        var closed = await service.CerrarAsync(fixture.UserId, caja.IdArqueo, 140m);
        var duplicateClose = await service.CerrarAsync(fixture.UserId, caja.IdArqueo, 140m);

        Assert.True(closed.Exito);
        Assert.False(duplicateClose.Exito);
        var saved = await fixture.Context.ArqueosCaja.SingleAsync(a => a.IdArqueo == caja.IdArqueo);
        Assert.Equal(50m, saved.TotalVentasEfectivo);
        Assert.Equal(20m, saved.TotalVentasDigital);
        Assert.Equal(150m, saved.TotalEsperado);
        Assert.Equal(140m, saved.TotalReal);
        Assert.Equal(-10m, saved.Diferencia);
        Assert.Equal("Cerrada", saved.Estado);
        Assert.NotNull(saved.FechaCierre);
    }

    [Fact]
    public async Task CerrarAsync_NoPermiteCerrarCajaDeOtroUsuario()
    {
        await using var fixture = await TestDatabase.CreateAsync();
        var caja = new ArqueoCaja { IdUsuario = fixture.UserId, Estado = "Abierta" };
        fixture.Context.ArqueosCaja.Add(caja);
        await fixture.Context.SaveChangesAsync();

        var result = await new CajaService(fixture.Context).CerrarAsync(fixture.UserId + 1, caja.IdArqueo, 0m);

        Assert.False(result.Exito);
        Assert.Equal("Abierta", (await fixture.Context.ArqueosCaja.FindAsync(caja.IdArqueo))!.Estado);
    }

    private static Venta CreateSale(int userId, int clientId, int cajaId, string method, decimal total, string number, string state = "Completada") => new()
    {
        IdUsuario = userId, IdCliente = clientId, IdArqueo = cajaId, NumeroComprobante = number,
        TipoComprobante = "Boleta", MetodoPago = method, Total = total, Estado = state,
        SubTotal = total, Igv = 0m, MontoRecibido = total, Vuelto = 0m, FechaRegistro = DateTime.Now
    };

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        public ElCortijoDbContext Context { get; }
        public int UserId { get; private set; }
        public int ClientId { get; private set; }

        private TestDatabase(SqliteConnection connection, ElCortijoDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<ElCortijoDbContext>().UseSqlite(connection).Options;
            var context = new ElCortijoDbContext(options);
            await context.Database.EnsureCreatedAsync();
            var role = new Rol { Nombre = "Vendedor", Estado = true };
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            var user = new Usuario { IdRol = role.IdRol, NombreCompleto = "Vendedor Test", Correo = "test@example.com", NombreUsuario = "test", ClaveHash = "hash", Estado = true };
            var client = new Cliente { TipoDocumento = "DNI", NumeroDocumento = "11111111", NombreRazonSocial = "Cliente Test", Estado = true };
            context.AddRange(user, client);
            await context.SaveChangesAsync();
            return new TestDatabase(connection, context) { UserId = user.IdUsuario, ClientId = client.IdCliente };
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
