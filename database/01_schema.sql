CREATE DATABASE BDElCortijo;
GO

USE BDElCortijo;
GO

CREATE TABLE Roles (
    IdRol INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion VARCHAR(200) NULL,
    Estado BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Usuarios (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    IdRol INT NOT NULL,
    NombreCompleto VARCHAR(150) NOT NULL,
    Correo VARCHAR(100) NOT NULL UNIQUE,
    NombreUsuario VARCHAR(50) NOT NULL UNIQUE,
    ClaveHash VARCHAR(255) NOT NULL,
    Telefono VARCHAR(20) NULL,
    Estado BIT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (IdRol) REFERENCES Roles(IdRol)
);
GO

CREATE TABLE Categorias (
    IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL UNIQUE,
    Descripcion VARCHAR(250) NULL,
    Estado BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Marcas (
    IdMarca INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL UNIQUE,
    Estado BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Productos (
    IdProducto INT IDENTITY(1,1) PRIMARY KEY,
    Codigo VARCHAR(50) NOT NULL UNIQUE,
    Nombre VARCHAR(150) NOT NULL,
    Descripcion VARCHAR(500) NULL,
    IdCategoria INT NOT NULL,
    IdMarca INT NOT NULL,
    PrecioCompra DECIMAL(10,2) NOT NULL,
    PrecioVenta DECIMAL(10,2) NOT NULL,
    StockActual INT NOT NULL DEFAULT 0,
    StockMinimo INT NOT NULL DEFAULT 5,
    UnidadMedida VARCHAR(50) NOT NULL,
    ImagenUrl VARCHAR(500) NULL,
    Estado BIT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (IdCategoria) REFERENCES Categorias(IdCategoria),
    CONSTRAINT FK_Productos_Marcas FOREIGN KEY (IdMarca) REFERENCES Marcas(IdMarca)
);
GO

CREATE TABLE Clientes (
    IdCliente INT IDENTITY(1,1) PRIMARY KEY,
    TipoDocumento VARCHAR(20) NOT NULL,
    NumeroDocumento VARCHAR(20) NOT NULL UNIQUE,
    NombreRazonSocial VARCHAR(150) NOT NULL,
    Direccion VARCHAR(200) NULL,
    Telefono VARCHAR(20) NULL,
    Correo VARCHAR(100) NULL,
    Estado BIT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE ArqueosCaja (
    IdArqueo INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,
    FechaApertura DATETIME NOT NULL DEFAULT GETDATE(),
    FechaCierre DATETIME NULL,
    MontoInicial DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    TotalVentasEfectivo DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    TotalVentasDigital DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    TotalEsperado DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    TotalReal DECIMAL(10,2) NULL,
    Diferencia DECIMAL(10,2) NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Abierta',
    CONSTRAINT FK_Arqueos_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO

CREATE TABLE Ventas (
    IdVenta INT IDENTITY(1,1) PRIMARY KEY,
    NumeroComprobante VARCHAR(20) NOT NULL UNIQUE,
    TipoComprobante VARCHAR(20) NOT NULL,
    IdCliente INT NOT NULL,
    IdUsuario INT NOT NULL,
    IdArqueo INT NULL,
    SubTotal DECIMAL(10,2) NOT NULL,
    Igv DECIMAL(10,2) NOT NULL,
    Total DECIMAL(10,2) NOT NULL,
    MetodoPago VARCHAR(50) NOT NULL,
    MontoRecibido DECIMAL(10,2) NOT NULL,
    Vuelto DECIMAL(10,2) NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Completada',
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Ventas_Clientes FOREIGN KEY (IdCliente) REFERENCES Clientes(IdCliente),
    CONSTRAINT FK_Ventas_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario),
    CONSTRAINT FK_Ventas_Arqueos FOREIGN KEY (IdArqueo) REFERENCES ArqueosCaja(IdArqueo)
);
GO

CREATE TABLE DetalleVentas (
    IdDetalleVenta INT IDENTITY(1,1) PRIMARY KEY,
    IdVenta INT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(10,2) NOT NULL,
    SubTotal DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_DetalleVentas_Ventas FOREIGN KEY (IdVenta) REFERENCES Ventas(IdVenta) ON DELETE CASCADE,
    CONSTRAINT FK_DetalleVentas_Productos FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto)
);
GO

CREATE TABLE MovimientosInventario (
    IdMovimiento INT IDENTITY(1,1) PRIMARY KEY,
    IdProducto INT NOT NULL,
    IdUsuario INT NOT NULL,
    TipoMovimiento VARCHAR(20) NOT NULL,
    Cantidad INT NOT NULL,
    StockAnterior INT NOT NULL,
    StockPosterior INT NOT NULL,
    Motivo VARCHAR(200) NOT NULL,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Movimientos_Productos FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto),
    CONSTRAINT FK_Movimientos_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO

CREATE INDEX IX_Productos_Codigo ON Productos(Codigo);
CREATE INDEX IX_Productos_IdCategoria ON Productos(IdCategoria);
CREATE INDEX IX_Ventas_NumeroComprobante ON Ventas(NumeroComprobante);
CREATE INDEX IX_Ventas_FechaRegistro ON Ventas(FechaRegistro);
CREATE INDEX IX_Movimientos_FechaRegistro ON MovimientosInventario(FechaRegistro);
GO
