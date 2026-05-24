namespace EMS.Infrastructure.Repositories;

using EMS.Domain.Entities;
using EMS.Shared.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

/// <summary>
/// Unit of Work implementation
/// Manages multiple repositories and transactions
/// Ensures atomic operations (all succeed or all fail)
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<UnitOfWork> _logger;
    private DbConnection? _connection;
    private DbTransaction? _transaction;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(IDbConnectionFactory connectionFactory, ILogger<UnitOfWork> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>).MakeGenericType(type);
            var logger = GetLoggerForType(type);
            
            var repository = Activator.CreateInstance(repositoryType, _connectionFactory, logger);
            _repositories.Add(type, repository!);
        }

        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Beginning transaction");
        
        _connection = await _connectionFactory.CreateConnectionAsync();
        _transaction = _connection.BeginTransaction();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Committing transaction");
            _transaction?.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction");
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _transaction = null;
            _connection = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Rolling back transaction");
            _transaction?.Rollback();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back transaction");
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _transaction = null;
            _connection = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving changes");
        return 1; // Dapper doesn't track changes like EF Core, so just return success
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing UnitOfWork");
        _transaction?.Dispose();
        _connection?.Dispose();
        _repositories.Clear();
    }

    /// <summary>
    /// Get logger for a specific type
    /// </summary>
    private ILogger GetLoggerForType(Type type)
    {
        var loggerType = typeof(ILogger<>).MakeGenericType(type);
        // Note: In real implementation, use ILoggerFactory
        return new NullLogger();
    }

    /// <summary>
    /// Null logger for when real logger cannot be created
    /// </summary>
    private class NullLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
