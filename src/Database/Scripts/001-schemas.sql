-- School ERP module schemas (SQL Server)
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'TenantManagement') EXEC('CREATE SCHEMA [TenantManagement]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Identity') EXEC('CREATE SCHEMA [Identity]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Billing') EXEC('CREATE SCHEMA [Billing]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Academics') EXEC('CREATE SCHEMA [Academics]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Students') EXEC('CREATE SCHEMA [Students]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Employees') EXEC('CREATE SCHEMA [Employees]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Attendance') EXEC('CREATE SCHEMA [Attendance]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Examinations') EXEC('CREATE SCHEMA [Examinations]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Fees') EXEC('CREATE SCHEMA [Fees]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Transport') EXEC('CREATE SCHEMA [Transport]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Communication') EXEC('CREATE SCHEMA [Communication]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Library') EXEC('CREATE SCHEMA [Library]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Inventory') EXEC('CREATE SCHEMA [Inventory]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Audit') EXEC('CREATE SCHEMA [Audit]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'infra') EXEC('CREATE SCHEMA [infra]');
GO
