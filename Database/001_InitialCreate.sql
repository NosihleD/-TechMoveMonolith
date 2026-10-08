/* TechMove - SQL Server database migration script */
IF DB_ID('TechMoveDb') IS NULL CREATE DATABASE TechMoveDb;
GO
USE TechMoveDb;
GO
IF OBJECT_ID('dbo.Clients','U') IS NULL
BEGIN
CREATE TABLE dbo.Clients (
 ClientId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clients PRIMARY KEY,
 Name nvarchar(150) NOT NULL,
 ContactDetails nvarchar(250) NOT NULL,
 Region nvarchar(100) NOT NULL
);
END
GO
IF OBJECT_ID('dbo.Contracts','U') IS NULL
BEGIN
CREATE TABLE dbo.Contracts (
 ContractId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Contracts PRIMARY KEY,
 ClientId int NOT NULL,
 StartDate datetime2 NOT NULL,
 EndDate datetime2 NOT NULL,
 Status nvarchar(20) NOT NULL,
 ServiceLevel nvarchar(100) NOT NULL,
 SignedAgreementPath nvarchar(500) NULL,
 CONSTRAINT FK_Contracts_Clients FOREIGN KEY (ClientId) REFERENCES dbo.Clients(ClientId)
);
END
GO
IF OBJECT_ID('dbo.ServiceRequests','U') IS NULL
BEGIN
CREATE TABLE dbo.ServiceRequests (
 ServiceRequestId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ServiceRequests PRIMARY KEY,
 ContractId int NOT NULL,
 Description nvarchar(500) NOT NULL,
 CostUsd decimal(18,2) NOT NULL,
 ExchangeRate decimal(18,6) NOT NULL,
 CostZar decimal(18,2) NOT NULL,
 Status nvarchar(30) NOT NULL,
 CONSTRAINT FK_ServiceRequests_Contracts FOREIGN KEY (ContractId) REFERENCES dbo.Contracts(ContractId) ON DELETE CASCADE
);
END
GO
