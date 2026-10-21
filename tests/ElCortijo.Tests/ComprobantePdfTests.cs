using System.Text;
using ElCortijo.Datos;
using ElCortijo.Dominio.Entidades;
using ElCortijo.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

namespace ElCortijo.Tests;

public sealed class ComprobantePdfTests
{
    [Fact]
    public async Task Pdf_ComprobanteExistente_DevuelveDocumentoPdfDescargable()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        var options = new DbContextOptionsBuilder<ElCortijoDbContext>()
            .UseSqlite(conexion)
            .Options;
        await using var contexto = new ElCortijoDbContext(options);
        await contexto.Database.EnsureCreatedAsync();

        var rol = new Rol { Nombre = "Vendedor", Estado = true };
        var categoria = new Categoria { Nombre = "Alimentos", Estado = true };
        var marca = new Marca { Nombre = "Marca de prueba", Estado = true };
        contexto.AddRange(rol, categoria, marca);
        await contexto.SaveChangesAsync();

        var usuario = new Usuario
        {
            IdRol = rol.IdRol,
            NombreCompleto = "Vendedor de prueba",
            Correo = "vendedor@prueba.pe",
            NombreUsuario = "vendedor",
            ClaveHash = "hash",
            Estado = true
        };
        var cliente = new Cliente
        {
            TipoDocumento = "DNI",
            NumeroDocumento = "00000001",
            NombreRazonSocial = "Cliente de prueba",
            Direccion = "Lima",
            Estado = true
        };
        var producto = new Producto
        {
            Codigo = "A-001",
            Nombre = "Alimento para perros",
            IdCategoria = categoria.IdCategoria,
            IdMarca = marca.IdMarca,
            PrecioCompra = 10m,
            PrecioVenta = 20m,
            StockActual = 5,
            StockMinimo = 1,
            UnidadMedida = "Bolsa",
            Estado = true
        };
        contexto.AddRange(usuario, cliente, producto);
        await contexto.SaveChangesAsync();

        var venta = new Venta
        {
            NumeroComprobante = "B001-000001",
            TipoComprobante = "Boleta",
            IdCliente = cliente.IdCliente,
            IdUsuario = usuario.IdUsuario,
            SubTotal = 20m,
            Igv = 3.6m,
            Total = 23.6m,
            MetodoPago = "Efectivo",
            MontoRecibido = 25m,
            Vuelto = 1.4m,
            Estado = "Completada",
            FechaRegistro = new DateTime(2026, 10, 19, 12, 30, 0)
        };
        contexto.Ventas.Add(venta);
        await contexto.SaveChangesAsync();
        contexto.DetalleVentas.Add(new DetalleVenta
        {
            IdVenta = venta.IdVenta,
            IdProducto = producto.IdProducto,
            Cantidad = 1,
            PrecioUnitario = 20m,
            SubTotal = 20m
        });
        await contexto.SaveChangesAsync();

        var controlador = new ComprobantesController(contexto);
        var resultado = Assert.IsType<FileContentResult>(await controlador.Pdf(venta.IdVenta));

        Assert.Equal("application/pdf", resultado.ContentType);
        Assert.Equal("comprobante-B001000001.pdf", resultado.FileDownloadName);
        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(resultado.FileContents, 0, 5));
        Assert.True(resultado.FileContents.Length > 1_000);

        var impresion = Assert.IsType<FileContentResult>(await controlador.Imprimir(venta.IdVenta));
        Assert.Equal("application/pdf", impresion.ContentType);
        Assert.Empty(impresion.FileDownloadName);
        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(impresion.FileContents, 0, 5));
    }
}
