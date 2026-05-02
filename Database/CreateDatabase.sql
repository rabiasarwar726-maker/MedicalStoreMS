IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'MedicalStoreDB')
    CREATE DATABASE MedicalStoreDB;
GO
USE MedicalStoreDB;
GO

IF OBJECT_ID('AuditLog','U')        IS NOT NULL DROP TABLE AuditLog;
IF OBJECT_ID('PurchaseDetails','U') IS NOT NULL DROP TABLE PurchaseDetails;
IF OBJECT_ID('Purchases','U')       IS NOT NULL DROP TABLE Purchases;
IF OBJECT_ID('InvoiceDetails','U')  IS NOT NULL DROP TABLE InvoiceDetails;
IF OBJECT_ID('Invoices','U')        IS NOT NULL DROP TABLE Invoices;
IF OBJECT_ID('Medicines','U')       IS NOT NULL DROP TABLE Medicines;
IF OBJECT_ID('Customers','U')       IS NOT NULL DROP TABLE Customers;
IF OBJECT_ID('Suppliers','U')       IS NOT NULL DROP TABLE Suppliers;
IF OBJECT_ID('Users','U')           IS NOT NULL DROP TABLE Users;
GO

CREATE TABLE Users (
    UserID       INT           IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    Role         NVARCHAR(20)  NOT NULL DEFAULT 'Staff',
    IsActive     BIT           NOT NULL DEFAULT 1,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Suppliers (
    SupplierID   INT           IDENTITY(1,1) PRIMARY KEY,
    SupplierName NVARCHAR(100) NOT NULL,
    ContactNo    NVARCHAR(20)  NULL,
    Address      NVARCHAR(200) NULL,
    Email        NVARCHAR(100) NULL,
    IsActive     BIT           NOT NULL DEFAULT 1
);

CREATE TABLE Customers (
    CustomerID   INT           IDENTITY(1,1) PRIMARY KEY,
    CustomerName NVARCHAR(100) NOT NULL,
    ContactNo    NVARCHAR(20)  NULL,
    Address      NVARCHAR(200) NULL,
    Email        NVARCHAR(100) NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Medicines (
    MedicineID    INT           IDENTITY(1,1) PRIMARY KEY,
    MedicineName  NVARCHAR(100) NOT NULL,
    BatchNo       NVARCHAR(50)  NULL,
    Category      NVARCHAR(50)  NULL,
    Description   NVARCHAR(500) NULL,
    ExpiryDate    DATE          NOT NULL,
    Quantity      INT           NOT NULL DEFAULT 0,
    UnitPrice     DECIMAL(10,2) NOT NULL,
    SupplierID    INT           NULL REFERENCES Suppliers(SupplierID),
    MinStockLevel INT           NOT NULL DEFAULT 10,
    IsActive      BIT           NOT NULL DEFAULT 1,
    CreatedAt     DATETIME      NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Invoices (
    InvoiceID       INT           IDENTITY(1,1) PRIMARY KEY,
    InvoiceDate     DATETIME      NOT NULL DEFAULT GETDATE(),
    CustomerID      INT           NULL REFERENCES Customers(CustomerID),
    TotalAmount     DECIMAL(10,2) NOT NULL,
    Discount        DECIMAL(10,2) NOT NULL DEFAULT 0,
    NetAmount       DECIMAL(10,2) NOT NULL,
    PaymentMode     NVARCHAR(20)  NOT NULL DEFAULT 'Cash',
    CreatedByUserID INT           NULL REFERENCES Users(UserID),
    Notes           NVARCHAR(500) NULL
);

CREATE TABLE InvoiceDetails (
    DetailID   INT           IDENTITY(1,1) PRIMARY KEY,
    InvoiceID  INT           NOT NULL REFERENCES Invoices(InvoiceID),
    MedicineID INT           NOT NULL REFERENCES Medicines(MedicineID),
    Quantity   INT           NOT NULL,
    UnitPrice  DECIMAL(10,2) NOT NULL,
    SubTotal   DECIMAL(10,2) NOT NULL
);

CREATE TABLE Purchases (
    PurchaseID      INT           IDENTITY(1,1) PRIMARY KEY,
    PurchaseDate    DATETIME      NOT NULL DEFAULT GETDATE(),
    SupplierID      INT           NULL REFERENCES Suppliers(SupplierID),
    TotalAmount     DECIMAL(10,2) NOT NULL,
    CreatedByUserID INT           NULL REFERENCES Users(UserID),
    Notes           NVARCHAR(500) NULL
);

CREATE TABLE PurchaseDetails (
    DetailID   INT           IDENTITY(1,1) PRIMARY KEY,
    PurchaseID INT           NOT NULL REFERENCES Purchases(PurchaseID),
    MedicineID INT           NOT NULL REFERENCES Medicines(MedicineID),
    Quantity   INT           NOT NULL,
    UnitCost   DECIMAL(10,2) NOT NULL,
    SubTotal   DECIMAL(10,2) NOT NULL
);

CREATE TABLE AuditLog (
    LogID     INT           IDENTITY(1,1) PRIMARY KEY,
    UserID    INT           NULL REFERENCES Users(UserID),
    Action    NVARCHAR(100) NOT NULL,
    Details   NVARCHAR(500) NULL,
    Timestamp DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- Admin password = Admin@123
INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive) VALUES
('admin','d27470375b2dc73a9a06228da0c356e997075b959c4f43310360b908a351abf4','Administrator','Admin',1);

INSERT INTO Suppliers (SupplierName, ContactNo, Address, Email) VALUES
('Medico Supplies','+92-300-1234567','Lahore, Pakistan','medico@supplies.pk'),
('PharmaTrade','+92-321-9876543','Karachi, Pakistan','info@pharmatrade.pk'),
('HealthPlus Dist','+92-311-5556677','Islamabad, Pakistan','hp@dist.pk');

INSERT INTO Customers (CustomerName, ContactNo) VALUES
('Walk-in Customer','0000000000'),
('Ahmad Raza','+92-333-1112222'),
('Fatima Malik','+92-312-9876543');

INSERT INTO Medicines (MedicineName,BatchNo,Category,Description,ExpiryDate,Quantity,UnitPrice,SupplierID,MinStockLevel) VALUES
('Paracetamol 500mg','B10001','Analgesic','Pain reliever','2027-06-30',500,12.50,1,20),
('Amoxicillin 250mg','B10002','Antibiotic','Broad spectrum antibiotic','2026-12-31',200,45.00,1,30),
('ORS Sachets','B10003','Hydration','Oral rehydration salts','2027-03-15',300,8.00,2,50),
('Metformin 500mg','B10004','Antidiabetic','Diabetes medication','2026-09-30',150,30.00,2,25),
('Cetirizine 10mg','B10005','Antihistamine','Allergy relief','2026-06-01',100,20.00,1,15),
('Omeprazole 20mg','B10006','Antacid','Acid reflux','2027-01-31',80,35.00,3,20),
('Vitamin C 500mg','B10007','Supplement','Immune support','2027-08-31',250,18.00,2,30),
('Ibuprofen 400mg','B10008','Analgesic','Anti-inflammatory','2026-11-30',60,25.00,1,20),
('Azithromycin 500mg','B10009','Antibiotic','Macrolide antibiotic','2026-08-31',40,85.00,3,10),
('Cough Syrup 100ml','B10010','Respiratory','Cough relief','2026-05-15',90,55.00,2,15);
GO

PRINT '================================================';
PRINT '  SUCCESS! MedicalStoreDB is ready.';
PRINT '  Username : admin';
PRINT '  Password : Admin@123';
PRINT '================================================';