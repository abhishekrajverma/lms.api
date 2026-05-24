namespace EMS.Infrastructure.Repositories;

using Dapper;
using EMS.Shared.Interfaces.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

/// <summary>
/// Stored procedure repository implementation
/// Executes complex database operations using SQL Server stored procedures
/// </summary>
public class StoredProcedureRepository : IStoredProcedureRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<StoredProcedureRepository> _logger;

    public StoredProcedureRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<StoredProcedureRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<T?> ExecuteScalarAsync<T>(
        string procedureName,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing stored procedure {ProcName} for scalar result", procedureName);

        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var result = await connection.QueryFirstOrDefaultAsync<T>(
            procedureName,
            parameters,
            commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(
        string procedureName,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing stored procedure {ProcName} returning multiple rows", procedureName);

        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var result = await connection.QueryAsync<T>(
            procedureName,
            parameters,
            commandType: CommandType.StoredProcedure);

        return result.ToList();
    }

//    public async Task<(T? Result, DynamicParameters OutputParameters)>
//ExecuteWithOutputAsync<T>(
//    string procedureName,
//    Action<DynamicParameters> parameterBuilder,
//    CancellationToken cancellationToken = default)
//    {
//        using var connection = await _connectionFactory.CreateConnectionAsync();

//        var parameters = new DynamicParameters();
//        parameterBuilder(parameters);

//        var result = await connection.ExecuteScalarAsync<T>(
//            new CommandDefinition(
//                procedureName,
//                parameters,
//                commandType: CommandType.StoredProcedure,
//                cancellationToken: cancellationToken
//            ));

//        return (result, parameters);
//    }


    public async Task ExecuteAsync(
        string procedureName,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing stored procedure {ProcName}", procedureName);

        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        await connection.ExecuteAsync(
            procedureName,
            parameters,
            commandType: CommandType.StoredProcedure);

        _logger.LogInformation("Successfully executed {ProcName}", procedureName);
    }

    /// <summary>
    /// Convert dynamic object to SQL parameters
    /// </summary>
    private static List<IDataParameter> ConvertToSqlParameters(object parameters)
    {
        var sqlParameters = new List<IDataParameter>();

        foreach (var prop in parameters.GetType().GetProperties())
        {
            var parameter = new SqlParameter
            {
                ParameterName = $"@{prop.Name}",
                Value = prop.GetValue(parameters) ?? DBNull.Value
            };
            sqlParameters.Add(parameter);
        }

        return sqlParameters;
    }
}
