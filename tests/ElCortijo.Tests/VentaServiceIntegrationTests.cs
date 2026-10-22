using ElCortijo.Datos;
using ElCortijo.Dominio.DTOs;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Negocio.Servicios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ElCortijo.Tests;

public sealed class VentaServiceIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private ElCortijoDbContext _context = null!;
    private VentaService _service = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ElCortijoDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new ElCortijoDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        await SeedAsync();
        _service = new VentaService(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task RegistrarAsync_VentaValida_DescuentaStockYRegistraTrazabilidad()
    {
        var solicitud = CrearSolicitud((1, 2), (2, 1));
        var resultado = await _service.RegistrarAsync(solicitud, 1);

        Assert.True(resultado.Exito);
        Assert.Null(resultado.Error);
        Assert.NotNull(resultado.IdVenta);
        var venta = await _context.Ventas.Include(v => v.DetalleVentas)
            .SingleAsync(v => v.IdVenta == resultado.IdVenta);
        Assert.Equal(35m, venta.SubTotal);
        Assert.Equal(6.30m, venta.Igv);
        Assert.Equal(41.30m, venta.Total);
        Assert.Equal(1, venta.IdArqueo);
        Assert.Equal(58.70m, venta.Vuelto);
        Assert.Equal(2, venta.DetalleVentas.Count);
        Assert.Equal(8, (await _context.Productos.FindAsync(1))!.StockActual);
        Assert.Equal(4, (await _context.Productos.FindAsync(2))!.StockActual);

        var movimientos = await _context.MovimientosInventario.OrderBy(m => m.IdProducto).ToListAsync();
        Assert.Collection(movimientos,
            movimiento =>
            {
                Assert.Equal("Salida", movimiento.TipoMovimiento);
                Assert.Equal(10, movimiento.StockAnterior);
                Assert.Equal(8, movimiento.StockPosterior);
            },
            movimiento =>
            {
                Assert.Equal("Salida", movimiento.TipoMovimiento);
                Assert.Equal(5, movimiento.StockAnterior);
                Assert.Equal(4, movimiento.StockPosterior);
            });
    }

    [Fact]
    public async Task RegistrarAsync_SinCajaAbierta_RechazaVentaSinCambiarStock()
    {
        var caja = await _context.ArqueosCaja.SingleAsync();
        _context.ArqueosCaja.Remove(caja);
        await _context.SaveChangesAsync();

        var resultado = await _service.RegistrarAsync(CrearSolicitud((1, 1)), 1);

        Assert.False(resultado.Exito);
        Assert.Contains("Abra una caja", resultado.Error);
        Assert.Empty(_context.Ventas);
        Assert.Equal(10, (await _context.Productos.FindAsync(1))!.StockActual);
    }

    [Theory]
    [InlineData("Yape/Plin")]
    [InlineData("Tarjeta")]
    [InlineData("Transferencia")]
    public async Task RegistrarAsync_PagoDigital_AsignaTotalComoMontoRecibido(string metodoPago)
    {
        var solicitud = CrearSolicitud((1, 1));
        solicitud.MetodoPago = metodoPago;
        solicitud.MontoRecibido = 0;

        var resultado = await _service.RegistrarAsync(solicitud, 1);

        Assert.True(resultado.Exito);
        var venta = await _context.Ventas.SingleAsync();
        Assert.Equal(venta.Total, venta.MontoRecibido);
        Assert.Equal(0, venta.Vuelto);
        Assert.Equal(1, venta.IdArqueo);

        var cierre = await new CajaService(_context).CerrarAsync(1, 1, 100m);
        Assert.True(cierre.Exito);
        var caja = await _context.ArqueosCaja.SingleAsync();
        Assert.Equal(0m, caja.TotalVentasEfectivo);
        Assert.Equal(venta.Total, caja.TotalVentasDigital);
        Assert.Equal(100m, caja.TotalEsperado);
    }

    [Fact]
    public async Task RegistrarAsync_SinItems_NoCreaVenta()
    {
        var resultado = await _service.RegistrarAsync(CrearSolicitud(), 1);
        Assert.False(resultado.Exito);
        Assert.Contains("por lo menos", resultado.Error);
        Assert.Empty(_context.Ventas);
    }

    [Fact]
    public async Task RegistrarAsync_ProductoDuplicado_NoModificaStock()
    {
        var resultado = await _service.RegistrarAsync(CrearSolicitud((1, 1), (1, 1)), 1);
        Assert.False(resultado.Exito);
        Assert.Contains("no puede repetirse", resultado.Error);
        Assert.Equal(10, (await _context.Productos.FindAsync(1))!.StockActual);
    }

    [Theory]
    [InlineData("Criptomoneda", "Efectivo", "método de pago")]
    [InlineData("Efectivo", "Ticket", "comprobante")]
    public async Task RegistrarAsync_DatosDePagoInvalidos_RechazaVenta(
        string metodoPago, string tipoComprobante, string errorEsperado)
    {
        var solicitud = CrearSolicitud((1, 1));
        solicitud.MetodoPago = metodoPago;
        solicitud.TipoComprobante = tipoComprobante;
        var resultado = await _service.RegistrarAsync(solicitud, 1);
        Assert.False(resultado.Exito);
        Assert.Contains(errorEsperado, resultado.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegistrarAsync_ClienteInexistente_RechazaVenta()
    {
        var solicitud = CrearSolicitud((1, 1));
        solicitud.IdCliente = 999;
        var resultado = await _service.RegistrarAsync(solicitud, 1);
        Assert.False(resultado.Exito);
        Assert.Contains("cliente", resultado.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(_context.Ventas);
    }

    [Fact]
    public async Task RegistrarAsync_ClienteInactivo_RechazaVenta()
    {
        (await _context.Clientes.FindAsync(1))!.Estado = false;
        await _context.SaveChangesAsync();
        var resultado = await _service.RegistrarAsync(CrearSolicitud((1, 1)), 1);
        Assert.False(resultado.Exito);
        Assert.Contains("inactivo", resultado.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegistrarAsync_ProductoInactivo_RechazaVenta()
    {
        (await _context.Productos.FindAsync(1))!.Estado = false;
        await _context.SaveChangesAsync();
        var resultado = await _service.RegistrarAsync(CrearSolicitud((1, 1)), 1);
        Assert.False(resultado.Exito);
        Assert.Contains("producto", resultado.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(_context.MovimientosInventario);
    }

    [Fact]
    public async Task RegistrarAsync_CantidadCero_RechazaVenta()
    {
        var resultado = await _service.RegistrarAsync(CrearSolicitud((1, 0)), 1);
        Assert.False(resultado.Exito);
        Assert.Contains("mayores que cero", resultado.Error);
        Assert.Equal(10, (await _context.Productos.FindAsync(1))!.StockActual);
    }

    [Fact]
    public async Task RegistrarAsync_StockInsuficiente_NoGeneraCambiosParciales()
    {
        var resultado = await _service.RegistrarAsync(CrearSolicitud((1, 11)), 1);
        Assert.False(resultado.Exito);
        Assert.Contains("Stock insuficiente", resultado.Error);
        Assert.Equal(10, (await _context.Productos.FindAsync(1))!.StockActual);
        Assert.Empty(_context.Ventas);
        Assert.Empty(_context.MovimientosInventario);
    }

    [Fact]
    public async Task RegistrarAsync_EfectivoInsuficiente_NoDescuentaStock()
    {
        var solicitud = CrearSolicitud((1, 1));
        solicitud.MontoRecibido = 5m;
        var resultado = await _service.RegistrarAsync(solicitud, 1);
        Assert.False(resultado.Exito);
        Assert.Contains("no cubre", resultado.Error);
        Assert.Equal(10, (await _context.Productos.FindAsync(1))!.StockActual);
    }

    [Fact]
    public async Task ObtenerProductosDisponiblesAsync_FiltraSinStockEInactivosYOrdena()
    {
        var productos = (await _service.ObtenerProductosDisponiblesAsync()).ToList();
        Assert.Equal(new[] { "Alimento A", "Alimento B" }, productos.Select(p => p.Nombre));
        Assert.DoesNotContain(productos, p => p.IdProducto is 3 or 4);
    }

    [Fact]
    public async Task ObtenerTodasAsync_CargaClienteUsuarioDetallesYProducto()
    {
        var resultado = await _service.RegistrarAsync(CrearSolicitud((1, 1)), 1);
        Assert.True(resultado.Exito);
        var venta = Assert.Single(await _service.ObtenerTodasAsync());
        Assert.Equal("Cliente de prueba", venta.Cliente.NombreRazonSocial);
        Assert.Equal("vendedor", venta.Usuario.NombreUsuario);
        Assert.Equal("Alimento A", Assert.Single(venta.DetalleVentas).Producto.Nombre);
    }

    private static VentaRegistroViewModel CrearSolicitud(params (int IdProducto, int Cantidad)[] items) => new()
    {
        IdCliente = 1,
        MetodoPago = "Efectivo",
        TipoComprobante = "Boleta",
        MontoRecibido = 100m,
        Items = items.Select(item => new VentaItemViewModel
        {
            IdProducto = item.IdProducto,
            Cantidad = item.Cantidad
        }).ToList()
    };

    private async Task SeedAsync()
    {
        var rol = new Rol { Nombre = "Vendedor", Estado = true };
        var categoria = new Categoria { Nombre = "Alimentos", Estado = true };
        var marca = new Marca { Nombre = "Marca de prueba", Estado = true };
        _context.AddRange(rol, categoria, marca);
        await _context.SaveChangesAsync();
        _context.Usuarios.Add(new Usuario
        {
            IdUsuario = 1, IdRol = rol.IdRol, NombreCompleto = "Vendedor de prueba",
            Correo = "vendedor@prueba.pe", NombreUsuario = "vendedor", ClaveHash = "hash", Estado = true
        });
        _context.Clientes.Add(new Cliente
        {
            IdCliente = 1, TipoDocumento = "DNI", NumeroDocumento = "00000001",
            NombreRazonSocial = "Cliente de prueba", Estado = true
        });
        _context.Productos.AddRange(
            CrearProducto(1, "A-001", "Alimento A", 10m, 10, true, categoria.IdCategoria, marca.IdMarca),
            CrearProducto(2, "B-001", "Alimento B", 15m, 5, true, categoria.IdCategoria, marca.IdMarca),
            CrearProducto(3, "C-001", "Sin stock", 8m, 0, true, categoria.IdCategoria, marca.IdMarca),
            CrearProducto(4, "D-001", "Inactivo", 20m, 10, false, categoria.IdCategoria, marca.IdMarca));
        _context.ArqueosCaja.Add(new ArqueoCaja
        {
            IdUsuario = 1,
            MontoInicial = 100m,
            Estado = "Abierta",
            FechaApertura = DateTime.Now
        });
        await _context.SaveChangesAsync();
    }

    private static Producto CrearProducto(int id, string codigo, string nombre, decimal precio,
        int stock, bool estado, int idCategoria, int idMarca) => new()
    {
        IdProducto = id, Codigo = codigo, Nombre = nombre, IdCategoria = idCategoria, IdMarca = idMarca,
        PrecioCompra = precio / 2, PrecioVenta = precio, StockActual = stock, StockMinimo = 2,
        UnidadMedida = "Unidad", Estado = estado
    };
}
