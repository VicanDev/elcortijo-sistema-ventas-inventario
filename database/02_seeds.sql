USE BDElCortijo;
GO

INSERT INTO Roles (Nombre, Descripcion, Estado)
VALUES 
('Administrador', 'Control total del sistema, reportes, usuarios y configuracion general', 1),
('Vendedor', 'Registro de pedidos, emision de ventas y consulta de catalogo', 1),
('Almacenero', 'Control de stock, registro de ingresos de mercaderia y ajustes de almacen', 1);
GO

INSERT INTO Usuarios (IdRol, NombreCompleto, Correo, NombreUsuario, ClaveHash, Telefono, Estado)
VALUES
(1, 'Victor Andres Martinez Lazaro', 'admin@elcortijo.pe', 'admin', '$2b$11$/z5MmzMf6dQe9Jh6N5vxoOLeJEnRcV78H9w6Y5dZKpajyeg7zTX6y', '987654321', 1),
(2, 'Vendedor Principal', 'vendedor@elcortijo.pe', 'vendedor', '$2b$11$/z5MmzMf6dQe9Jh6N5vxoOLeJEnRcV78H9w6Y5dZKpajyeg7zTX6y', '987654322', 1),
(3, 'Almacenero Principal', 'almacen@elcortijo.pe', 'almacen', '$2b$11$/z5MmzMf6dQe9Jh6N5vxoOLeJEnRcV78H9w6Y5dZKpajyeg7zTX6y', '987654323', 1);
GO

INSERT INTO Categorias (Nombre, Descripcion, Estado)
VALUES
('Alimento Seco Perros', 'Croquetas y balanceados secos para perros de todas las edades', 1),
('Alimento Humedo Perros', 'Latas, sobres y patés nutritivos para perros', 1),
('Alimento Seco Gatos', 'Croquetas formuladas para gatos cachorros y adultos', 1),
('Alimento Humedo Gatos', 'Pouch y latas de alimento premium para felinos', 1),
('Snacks y Golosinas', 'Galletas, huesos masticables y bocaditos de entrenamiento', 1);
GO

INSERT INTO Marcas (Nombre, Estado)
VALUES
('Ricocan', 1),
('Mimaskot', 1),
('Canbo', 1),
('Purina Pro Plan', 1),
('Hills Science Diet', 1),
('Cat Chow', 1),
('Whiskas', 1),
('RicoCat', 1);
GO

INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, IdMarca, PrecioCompra, PrecioVenta, StockActual, StockMinimo, UnidadMedida, Estado)
VALUES
('PROD-001', 'Ricocan Adulto Carne y Cereales 15kg', 'Alimento balanceado completo para perros adultos de razas medianas y grandes', 1, 1, 95.00, 125.00, 45, 10, 'Saco 15 Kg', 1),
('PROD-002', 'Ricocan Cachorro Cordero y Arroz 15kg', 'Formula de alto desarrollo para cachorros hasta 12 meses', 1, 1, 105.00, 138.00, 30, 8, 'Saco 15 Kg', 1),
('PROD-003', 'Mimaskot Adulto Pollo y Verduras 15kg', 'Nutricion balanceada para mantenimiento de perros adultos', 1, 2, 88.00, 115.00, 50, 10, 'Saco 15 Kg', 1),
('PROD-004', 'Mimaskot Cachorro Carne y Leche 3kg', 'Alimento para cachorros en etapa de crecimiento inicial', 1, 2, 24.00, 32.00, 60, 15, 'Bolsa 3 Kg', 1),
('PROD-005', 'Canbo Adulto Raza Mediana Cordero 15kg', 'Alimento super premium sin granos de alto rendimiento', 1, 3, 175.00, 220.00, 25, 5, 'Saco 15 Kg', 1),
('PROD-006', 'Canbo Puppy Sensitive Salmon 15kg', 'Alimento hipoalergenico para cachorros con digestion sensible', 1, 3, 185.00, 235.00, 18, 5, 'Saco 15 Kg', 1),
('PROD-007', 'Pro Plan Adult Complete OptiHealth 15kg', 'Alimento avanzado con spirulina y antioxidantes naturales', 1, 4, 210.00, 265.00, 20, 5, 'Saco 15 Kg', 1),
('PROD-008', 'Ricocan Trozos de Carne en Salsa 300g', 'Lata de alimento humedo para perros adultos', 2, 1, 5.20, 7.50, 120, 24, 'Lata 300g', 1),
('PROD-009', 'Cat Chow Adulto Delicias de Pescado 8kg', 'Alimento seco con proteina marina y control de bolas de pelo', 3, 6, 72.00, 94.00, 35, 8, 'Saco 8 Kg', 1),
('PROD-010', 'Whiskas Adulto Carne 8kg', 'Formula equilibrada con taurina para la salud cardiaca y visual del gato', 3, 7, 68.00, 89.00, 40, 10, 'Saco 8 Kg', 1),
('PROD-011', 'RicoCat Adulto Atun y Sardina 15kg', 'Alimento economico y rendidor para colonias y gatos adultos', 3, 8, 90.00, 118.00, 28, 6, 'Saco 15 Kg', 1),
('PROD-012', 'Whiskas Pouch Salmon en Gelatina 85g', 'Sobre individual de alimento humedo irresistible para felinos', 4, 7, 2.50, 3.80, 200, 30, 'Sobre 85g', 1),
('PROD-013', 'Galletas Canbo Biscuits Cuidado Oral 200g', 'Snack crujiente que previene la formacion de sarro dental en perros', 5, 3, 9.50, 14.00, 80, 15, 'Caja 200g', 1);
GO

INSERT INTO Clientes (TipoDocumento, NumeroDocumento, NombreRazonSocial, Direccion, Telefono, Correo, Estado)
VALUES
('DNI', '00000000', 'CLIENTES VARIOS', 'Sin direccion', '000000000', 'ventas@elcortijo.pe', 1),
('DNI', '45892147', 'Juan Perez Lopez', 'Av. Las Palmeras 450, Lima', '991234567', 'juan.perez@gmail.com', 1),
('DNI', '72341908', 'Maria Gomez Ramirez', 'Jr. Los Cedros 123, Trujillo', '984512376', 'maria.gomez@hotmail.com', 1),
('RUC', '20601234567', 'VETERINARIA SAN MARTIN S.A.C.', 'Av. Proceres 890, Lima', '014567890', 'contacto@vetsanmartin.pe', 1),
('RUC', '20554321987', 'DISTRIBUCIONES PET LOVERS E.I.R.L.', 'Calle Las Gardenias 340, Lima', '016789123', 'pedidos@petlovers.com', 1);
GO
