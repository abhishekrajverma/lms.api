-- ============================================================================
-- Token Blacklist Table for Logout Support
-- Stores revoked tokens to prevent their use after logout
-- ============================================================================

USE [EMS_DB]
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TokenBlacklist')
BEGIN
    CREATE TABLE [dbo].[TokenBlacklist]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [TokenHash] NVARCHAR(256) NOT NULL UNIQUE,
        [ExpiresAt] DATETIME2 NOT NULL,
        [RevokedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [Reason] NVARCHAR(500) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL
    )
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TokenBlacklist_TokenHash')
    CREATE NONCLUSTERED INDEX [IX_TokenBlacklist_TokenHash] ON [dbo].[TokenBlacklist]([TokenHash])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TokenBlacklist_ExpiresAt')
    CREATE NONCLUSTERED INDEX [IX_TokenBlacklist_ExpiresAt] ON [dbo].[TokenBlacklist]([ExpiresAt])
GO

PRINT 'TokenBlacklist table created successfully!'
