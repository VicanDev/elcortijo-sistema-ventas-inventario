using Microsoft.EntityFrameworkCore;
using ElCortijo.Dominio.Entidades;

namespace ElCortijo.Datos;

public class ElCortijoDbContext : DbContext
{
    public ElCortijoDbContext(DbContextOptions<ElCortijoDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ArqueoCaja> ArqueosCaja => Set<ArqueoCaja>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.IdRol);
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.Estado).HasDefaultValue(true);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.NombreCompleto).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Correo).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Correo).IsUnique();
            entity.Property(e => e.NombreUsuario).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.NombreUsuario).IsUnique();
            entity.Property(e => e.ClaveHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

            entity.HasOne(d => d.Rol)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias");
            entity.HasKey(e => e.IdCategoria);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.ToTable("Marcas");
            entity.HasKey(e => e.IdMarca);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Estado).HasDefaultValue(true);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Productos");
            entity.HasKey(e => e.IdProducto);
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.PrecioCompra).HasPrecision(10, 2);
            entity.Property(e => e.PrecioVenta).HasPrecision(10, 2);
            entity.Property(e => e.StockActual).HasDefaultValue(0);
            entity.Property(e => e.StockMinimo).HasDefaultValue(5);
            entity.Property(e => e.UnidadMedida).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ImagenUrl).HasMaxLength(500);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

            entity.HasOne(d => d.Categoria)
                .WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Marca)
                .WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdMarca)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(e => e.IdCliente);
            entity.Property(e => e.TipoDocumento).HasMaxLength(20).IsRequired();
            entity.Property(e => e.NumeroDocumento).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.NumeroDocumento).IsUnique();
            entity.Property(e => e.NombreRazonSocial).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<ArqueoCaja>(entity =>
        {
            entity.ToTable("ArqueosCaja");
            entity.HasKey(e => e.IdArqueo);
            entity.Property(e => e.MontoInicial).HasPrecision(10, 2);
            entity.Property(e => e.TotalVentasEfectivo).HasPrecision(10, 2);
            entity.Property(e => e.TotalVentasDigital).HasPrecision(10, 2);
            entity.Property(e => e.TotalEsperado).HasPrecision(10, 2);
            entity.Property(e => e.TotalReal).HasPrecision(10, 2);
            entity.Property(e => e.Diferencia).HasPrecision(10, 2);
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Abierta");
            entity.Property(e => e.FechaApertura).HasDefaultValueSql("GETDATE()");

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Arqueos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.ToTable("Ventas");
            entity.HasKey(e => e.IdVenta);
            entity.Property(e => e.NumeroComprobante).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.NumeroComprobante).IsUnique();
            entity.Property(e => e.TipoComprobante).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SubTotal).HasPrecision(10, 2);
            entity.Property(e => e.Igv).HasPrecision(10, 2);
            entity.Property(e => e.Total).HasPrecision(10, 2);
            entity.Property(e => e.MetodoPago).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MontoRecibido).HasPrecision(10, 2);
            entity.Property(e => e.Vuelto).HasPrecision(10, 2);
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Completada");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

            entity.HasOne(d => d.Cliente)
                .WithMany(p => p.Ventas)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Ventas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Arqueo)
                .WithMany(p => p.Ventas)
                .HasForeignKey(d => d.IdArqueo)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.ToTable("DetalleVentas");
            entity.HasKey(e => e.IdDetalleVenta);
            entity.Property(e => e.PrecioUnitario).HasPrecision(10, 2);
            entity.Property(e => e.SubTotal).HasPrecision(10, 2);

            entity.HasOne(d => d.Venta)
                .WithMany(p => p.DetalleVentas)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Producto)
                .WithMany(p => p.DetalleVentas)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.ToTable("MovimientosInventario");
            entity.HasKey(e => e.IdMovimiento);
            entity.Property(e => e.TipoMovimiento).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Motivo).HasMaxLength(200).IsRequired();
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

            entity.HasOne(d => d.Producto)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
