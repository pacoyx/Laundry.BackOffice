/*
  Seed de datos ficticios para desarrollo local.
  Pensado para correr contra la LocalDB de desarrollo (Db24Dexter_Dev), NO contra UAT/prod.

  Es reejecutable: borra primero los datos que este script controla y los vuelve a insertar,
  para que puedas "resetear" tu entorno cuando quieras. Solo toca tablas maestras/catálogo
  (Users, Companies, BranchSales, BrachSalesUsers, Customers, CatServices, ProdServices,
  UnitMeasurements, CategoryProds, Suppliers, Products) — NO toca datos transaccionales
  (WorkGuideMain, Tickets, CashBox, ExpenseBox, etc.), esos los generas usando la app/API
  una vez que tengas login y catálogo.

  Usa SCOPE_IDENTITY() para encadenar las FKs en vez de IDs fijos, así es seguro
  re-ejecutarlo sin importar en qué quedaron los contadores de identidad de una corrida anterior.

  Uso (el -f 65001 evita que se corrompan los acentos/ñ al insertar):
    (el -I activa QUOTED_IDENTIFIER; sin el, los DELETE fallan con "Msg 1934" por los indices filtrados)
    sqlcmd -S "(localdb)\mssqllocaldb" -d Db24Dexter_Dev -C -I -f 65001 -i Scripts\seed-dev-data.sql

  Usuarios creados (para loguear via POST /api/login):
    admin    / Admin123!
    cajero1  / Vendedor123!
*/

SET NOCOUNT ON;

-- 1) Limpieza (orden inverso de dependencias FK)
DELETE FROM InvoicePayments;
DELETE FROM InvoiceDetails;
DELETE FROM Invoices;
DELETE FROM CollectionSheets;
DELETE FROM PaymentMethods;
DELETE FROM NumbersDocuments;
DELETE FROM TypeDocuments;
DELETE FROM BrachSalesUsers;
DELETE FROM Products;
DELETE FROM ProdServices;
DELETE FROM Suppliers;
DELETE FROM CategoryProds;
DELETE FROM UnitMeasurements;
DELETE FROM CatServices;
DELETE FROM Customers;
DELETE FROM Companies;
DELETE FROM BranchSales;
DELETE FROM Users;

DECLARE @adminId INT, @cajeroId INT;
DECLARE @sucMirafloresId INT, @sucSanIsidroId INT;
DECLARE @catServLavadoId INT, @catServPlanchadoId INT, @catServTintoreriaId INT;
DECLARE @unitUndId INT, @unitKgId INT, @unitLtId INT;
DECLARE @catProdDetergentesId INT, @catProdSuavizantesId INT, @catProdInsumosId INT;
DECLARE @provQuimicaId INT, @provLimpiezaId INT;

-- 2) Usuarios
-- Hash generado con el mismo esquema que EncryptService.cs (PBKDF2-SHA256, 10000 iteraciones, salt 16 bytes)
INSERT INTO Users (Name, UserName, Password, Role, Status, RefreshToken, HashPassword, Email)
VALUES ('Administrador General', 'admin', 'd5kC8/ehz0H+c8zBa+OG6V98hlNSWesuA9lchMQbpck=', 'Admin', 'A', '', '5M5hPwqH9dzTlvvxfpCPlA==', 'admin@dexterlavanderia.test');
SET @adminId = SCOPE_IDENTITY();

INSERT INTO Users (Name, UserName, Password, Role, Status, RefreshToken, HashPassword, Email)
VALUES ('Maria Cajero', 'cajero1', 'V92/TveM0+gZlQeCxVkYTs58+qQLjzkQtFjlS5ONPEY=', 'Cajero', 'A', '', '9hQpiMfRSGZts9wxxXp9CQ==', 'cajero1@dexterlavanderia.test');
SET @cajeroId = SCOPE_IDENTITY();

-- 3) Empresa
INSERT INTO Companies (NameComercial, NameCompany, DocumentType, NumberType, Phone, Email, Logo, WebSite, Description, Address, Status, UsuarioId, Facebook, Twitter, Instagram)
VALUES ('Dexter Lavandería', 'Dexter Comercial S.A.C.', 'RUC', '20601234567', '014567890', 'contacto@dexterlavanderia.test', NULL, 'https://dexterlavanderia.test', 'Lavandería comercial y recepción de prendas', 'Av. Siempre Viva 123, Lima', 'A', @adminId, NULL, NULL, NULL);

-- 4) Sucursales
INSERT INTO BranchSales (Description, Address, Status) VALUES ('Sucursal Miraflores', 'Av. Larco 123, Miraflores', 'A');
SET @sucMirafloresId = SCOPE_IDENTITY();
INSERT INTO BranchSales (Description, Address, Status) VALUES ('Sucursal San Isidro', 'Av. Javier Prado 456, San Isidro', 'A');
SET @sucSanIsidroId = SCOPE_IDENTITY();

-- 5) Asignación usuario <-> sucursal (requerida para poder loguear, ver LoginUserEndpoint.cs)
INSERT INTO BrachSalesUsers (BranchSalesId, UserId, Status) VALUES (@sucMirafloresId, @adminId, 'A');
INSERT INTO BrachSalesUsers (BranchSalesId, UserId, Status) VALUES (@sucMirafloresId, @cajeroId, 'A');
INSERT INTO BrachSalesUsers (BranchSalesId, UserId, Status) VALUES (@sucSanIsidroId, @adminId, 'A');

-- 6) Clientes
INSERT INTO Customers (FirtsName, LastName, Address, Phone, Email, DocPersonal, Status) VALUES ('Carlos', 'Bazán', 'Calle Las Flores 200, Lima', '987654321', 'carlos.bazan@example.com', '45678912', 'A');
INSERT INTO Customers (FirtsName, LastName, Address, Phone, Email, DocPersonal, Status) VALUES ('Lucía', 'Fernández', 'Jr. Los Pinos 55, Surco', '976543210', 'lucia.fernandez@example.com', '41234567', 'A');
INSERT INTO Customers (FirtsName, LastName, Address, Phone, Email, DocPersonal, Status) VALUES ('Jorge', 'Ramírez', 'Av. Benavides 800, Miraflores', '965432109', 'jorge.ramirez@example.com', '43219876', 'A');
INSERT INTO Customers (FirtsName, LastName, Address, Phone, Email, DocPersonal, Status) VALUES ('Empresa ABC', 'S.A.C.', 'Av. Industrial 100, Ate', '014448888', 'contacto@empresaabc.test', '20509876543', 'A');

-- 7) Catálogo de servicios (lavandería)
INSERT INTO CatServices (Name, Description, Icon) VALUES ('Lavado', 'Servicios de lavado de prendas', 'wash');
SET @catServLavadoId = SCOPE_IDENTITY();
INSERT INTO CatServices (Name, Description, Icon) VALUES ('Planchado', 'Servicios de planchado', 'iron');
SET @catServPlanchadoId = SCOPE_IDENTITY();
INSERT INTO CatServices (Name, Description, Icon) VALUES ('Tintorería', 'Limpieza en seco', 'dry-cleaning');
SET @catServTintoreriaId = SCOPE_IDENTITY();

-- 8) Servicios específicos
INSERT INTO ProdServices (Name, Description, Price, Status, CatServiceId, IsPeso, IsLavado, Created, CreatedBy) VALUES ('Lavado por Kilo', 'Lavado estándar cobrado por peso', 12.50, 'A', @catServLavadoId, 'S', 'S', GETUTCDATE(), 'seed');
INSERT INTO ProdServices (Name, Description, Price, Status, CatServiceId, IsPeso, IsLavado, Created, CreatedBy) VALUES ('Lavado de Edredón', 'Lavado especial para edredones', 35.00, 'A', @catServLavadoId, 'N', 'S', GETUTCDATE(), 'seed');
INSERT INTO ProdServices (Name, Description, Price, Status, CatServiceId, IsPeso, IsLavado, Created, CreatedBy) VALUES ('Planchado de Camisa', 'Planchado por unidad', 4.50, 'A', @catServPlanchadoId, 'N', 'N', GETUTCDATE(), 'seed');
INSERT INTO ProdServices (Name, Description, Price, Status, CatServiceId, IsPeso, IsLavado, Created, CreatedBy) VALUES ('Terno Tintorería', 'Limpieza en seco de terno completo', 28.00, 'A', @catServTintoreriaId, 'N', 'N', GETUTCDATE(), 'seed');

-- 9) Unidades de medida
INSERT INTO UnitMeasurements (CodeUm, Name, Description, Status, CodeSunat, Abbreviation, Symbol) VALUES ('UND', 'Unidad', 'Unidad simple', 'A', 'NIU', 'und', 'u');
SET @unitUndId = SCOPE_IDENTITY();
INSERT INTO UnitMeasurements (CodeUm, Name, Description, Status, CodeSunat, Abbreviation, Symbol) VALUES ('KG', 'Kilogramo', 'Kilogramos', 'A', 'KGM', 'kg', 'kg');
SET @unitKgId = SCOPE_IDENTITY();
INSERT INTO UnitMeasurements (CodeUm, Name, Description, Status, CodeSunat, Abbreviation, Symbol) VALUES ('LT', 'Litro', 'Litros', 'A', 'LTR', 'lt', 'L');
SET @unitLtId = SCOPE_IDENTITY();

-- 10) Categorías de producto (insumos de lavandería)
INSERT INTO CategoryProds (Name, Description, Status) VALUES ('Detergentes', 'Detergentes y jabones', 'A');
SET @catProdDetergentesId = SCOPE_IDENTITY();
INSERT INTO CategoryProds (Name, Description, Status) VALUES ('Suavizantes', 'Suavizantes de telas', 'A');
SET @catProdSuavizantesId = SCOPE_IDENTITY();
INSERT INTO CategoryProds (Name, Description, Status) VALUES ('Insumos Tintorería', 'Solventes y químicos de limpieza en seco', 'A');
SET @catProdInsumosId = SCOPE_IDENTITY();

-- 11) Proveedores
INSERT INTO Suppliers (Name, Ruc, Address, Phone, Email, Status, ContactName, ContactPhone) VALUES ('Química Industrial SAC', '20512345678', 'Av. Argentina 1500, Callao', '014412233', 'ventas@quimicaindustrial.test', 'A', 'Pedro Salas', '987001122');
SET @provQuimicaId = SCOPE_IDENTITY();
INSERT INTO Suppliers (Name, Ruc, Address, Phone, Email, Status, ContactName, ContactPhone) VALUES ('Distribuidora Limpieza Total', '20598765432', 'Jr. Comercio 300, Lima', '014455667', 'contacto@limpiezatotal.test', 'A', 'Ana Gómez', '987223344');
SET @provLimpiezaId = SCOPE_IDENTITY();

-- 12) Productos (insumos de bodega)
INSERT INTO Products (Name, Description, Price, Stock, UnitMeasurementId, CategoryProdId, Status, SupplierId) VALUES ('Detergente Industrial 20L', 'Detergente concentrado para lavado industrial', 145.00, 40, @unitLtId, @catProdDetergentesId, 'A', @provQuimicaId);
INSERT INTO Products (Name, Description, Price, Stock, UnitMeasurementId, CategoryProdId, Status, SupplierId) VALUES ('Suavizante Concentrado 5L', 'Suavizante de telas concentrado', 55.00, 60, @unitLtId, @catProdSuavizantesId, 'A', @provLimpiezaId);
INSERT INTO Products (Name, Description, Price, Stock, UnitMeasurementId, CategoryProdId, Status, SupplierId) VALUES ('Percloroetileno 1L', 'Solvente para tintorería', 38.00, 25, @unitLtId, @catProdInsumosId, 'A', @provQuimicaId);
INSERT INTO Products (Name, Description, Price, Stock, UnitMeasurementId, CategoryProdId, Status, SupplierId) VALUES ('Bolsas Plásticas x100', 'Bolsas para empaque de prendas', 18.00, 200, @unitUndId, @catProdDetergentesId, 'A', @provLimpiezaId);

-- 13) Tipos de documento (catálogo, ver Models\TypeDocument.cs)
INSERT INTO TypeDocuments (Name, Description, Status, DocId) VALUES ('Factura', 'Factura electrónica', 'A', '01');
INSERT INTO TypeDocuments (Name, Description, Status, DocId) VALUES ('Boleta', 'Boleta de venta electrónica', 'A', '03');
INSERT INTO TypeDocuments (Name, Description, Status, DocId) VALUES ('Nota de Crédito', 'Nota de crédito', 'A', '07');
INSERT INTO TypeDocuments (Name, Description, Status, DocId) VALUES ('Guía de Remisión', 'Guía de remisión remitente', 'A', '09');
INSERT INTO TypeDocuments (Name, Description, Status, DocId) VALUES ('Ticket', 'Comprobante interno no fiscal', 'A', 'TK');

-- 14) Numeración de documentos por sucursal (correlativos)
-- TypeProcess = 'RECEP' es OBLIGATORIO: CreateWorkGuideMainEndpoint.cs busca el NumbersDocument
-- de la sucursal con ese TypeProcess para asignar el correlativo de la guía; sin esta fila
-- POST /api/workGuideMain devuelve 400 "No se encontró correlativo de guia de servicio".
-- Ajusta TypeDoc/SerieDoc/NumberDoc inicial según la numeración real que quieras usar.
INSERT INTO NumbersDocuments (BranchId, TypeDoc, SerieDoc, NumberDoc, Status, TypeProcess) VALUES (@sucMirafloresId, '03', 'B001', 1, 'A', 'RECEP');
INSERT INTO NumbersDocuments (BranchId, TypeDoc, SerieDoc, NumberDoc, Status, TypeProcess) VALUES (@sucSanIsidroId, '03', 'B002', 1, 'A', 'RECEP');

-- 15) Métodos de pago (ver Models\Invoices\PaymentMethod.cs)
INSERT INTO PaymentMethods (Name, IsActive) VALUES ('Efectivo', 1);
INSERT INTO PaymentMethods (Name, IsActive) VALUES ('Tarjeta', 1);
INSERT INTO PaymentMethods (Name, IsActive) VALUES ('Transferencia', 1);
INSERT INTO PaymentMethods (Name, IsActive) VALUES ('Depósito', 1);
INSERT INTO PaymentMethods (Name, IsActive) VALUES ('Yape/Plin (QR)', 1);

PRINT 'Seed completado: 2 usuarios, 1 empresa, 2 sucursales, 3 asignaciones sucursal-usuario, 4 clientes, 3 categorías de servicio, 4 servicios, 3 unidades de medida, 3 categorías de producto, 2 proveedores, 4 productos, 5 tipos de documento, 2 numeraciones de documento, 5 métodos de pago.';
