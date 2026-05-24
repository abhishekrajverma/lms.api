-- ============================================================================
-- Refresh Tokens Table for JWT Token Management
-- Stores refresh tokens for session extension
-- ============================================================================

USE [EMS_DB]
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RefreshTokens')
BEGIN
    CREATE TABLE [dbo].[RefreshTokens]
    (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [UserId] INT NOT NULL,
        [Token] NVARCHAR(MAX) NOT NULL,
        [ExpiresAt] DATETIME2 NOT NULL,
        [IsRevoked] BIT NOT NULL DEFAULT 0,
        [RevokedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedByIp] NVARCHAR(50) NULL,
        [ReplacedByTokenId] INT NULL,
        [ReplacedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [RowVersion] ROWVERSION NULL,
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
        FOREIGN KEY ([ReplacedByTokenId]) REFERENCES [dbo].[RefreshTokens]([Id])
    )
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RefreshTokens_UserId')
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId] ON [dbo].[RefreshTokens]([UserId])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RefreshTokens_Token')
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_Token] ON [dbo].[RefreshTokens]([Token])
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RefreshTokens_ExpiresAt')
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_ExpiresAt] ON [dbo].[RefreshTokens]([ExpiresAt])
GO

PRINT 'RefreshTokens table created successfully!'
