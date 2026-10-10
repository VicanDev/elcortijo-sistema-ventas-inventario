USE BDElCortijo;
GO

CREATE OR ALTER PROCEDURE sp_RegistrarMovimientoInventario
    @IdProducto INT,
    @IdUsuario INT,
    @TipoMovimiento VARCHAR(20),
    @Cantidad INT,
    @Motivo VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @StockActual INT;
        DECLARE @NuevoStock INT;

        SELECT @StockActual = StockActual FROM Productos WHERE IdProducto = @IdProducto;

        IF @StockActual IS NULL
        BEGIN
            THROW 50001, 'El producto especificado no existe.', 1;
        END

        IF @TipoMovimiento = 'Entrada'
        BEGIN
            SET @NuevoStock = @StockActual + @Cantidad;
        END
        ELSE IF @TipoMovimiento = 'Salida'
        BEGIN
            IF @StockActual < @Cantidad
            BEGIN
                THROW 50002, 'Stock insuficiente para realizar la salida.', 1;
            END
            SET @NuevoStock = @StockActual - @Cantidad;
        END
        ELSE IF @TipoMovimiento = 'Ajuste'
        BEGIN
            SET @NuevoStock = @Cantidad;
            SET @Cantidad = ABS(@NuevoStock - @StockActual);
        END
        ELSE
        BEGIN
            THROW 50003, 'Tipo de movimiento no valido.', 1;
        END

        UPDATE Productos 
        SET StockActual = @NuevoStock 
        WHERE IdProducto = @IdProducto;

        INSERT INTO MovimientosInventario (IdProducto, IdUsuario, TipoMovimiento, Cantidad, StockAnterior, StockPosterior, Motivo, FechaRegistro)
        VALUES (@IdProducto, @IdUsuario, @TipoMovimiento, @Cantidad, @StockActual, @NuevoStock, @Motivo, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_ObtenerResumenDashboard
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalVentasHoy DECIMAL(10,2);
    DECLARE @CantidadVentasHoy INT;
    DECLARE @TotalProductos INT;
    DECLARE @ProductosBajoStock INT;

    SELECT 
        @TotalVentasHoy = ISNULL(SUM(Total), 0),
        @CantidadVentasHoy = COUNT(1)
    FROM Ventas 
    WHERE CAST(FechaRegistro AS DATE) = CAST(GETDATE() AS DATE) 
      AND Estado = 'Completada';

    SELECT @TotalProductos = COUNT(1) FROM Productos WHERE Estado = 1;
    SELECT @ProductosBajoStock = COUNT(1) FROM Productos WHERE StockActual <= StockMinimo AND Estado = 1;

    SELECT 
        @TotalVentasHoy AS TotalVentasHoy,
        @CantidadVentasHoy AS CantidadVentasHoy,
        @TotalProductos AS TotalProductos,
        @ProductosBajoStock AS ProductosBajoStock;
END;
GO
