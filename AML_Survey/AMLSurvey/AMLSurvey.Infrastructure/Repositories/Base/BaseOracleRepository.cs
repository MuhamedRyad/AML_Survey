using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace AMLSurvey.Infrastructure.Repositories.Base
{
    public abstract class BaseOracleRepository
    {
        protected readonly string _connectionString;

        protected BaseOracleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        protected async Task<T?> ExecuteScalarAsync<T>(
            string procedureName, 
            DynamicParameters parameters = null, 
            CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            var result = await connection.ExecuteScalarAsync<T>(
                procedureName, 
                parameters, 
                commandType: CommandType.StoredProcedure
            );
            
            return result;
        }

        protected async Task<IEnumerable<T>> ExecuteQueryAsync<T>(
            string procedureName,
            DynamicParameters parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            return await connection.QueryAsync<T>(
                procedureName, 
                parameters, 
                commandType: CommandType.StoredProcedure
            );
        }

        protected async Task<int> ExecuteAsync(
            string procedureName,
            DynamicParameters parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            return await connection.ExecuteAsync(
                procedureName, 
                parameters, 
                commandType: CommandType.StoredProcedure
            );
        }

        protected async Task<T?> ExecuteWithOutputAsync<T>(
            string procedureName,
            DynamicParameters parameters,
            string outputParameterName,
            CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            await connection.ExecuteAsync(
                procedureName, 
                parameters, 
                commandType: CommandType.StoredProcedure
            );
            
            return parameters.Get<T>(outputParameterName);
        }

        protected async Task<(T1, T2)> ExecuteWithMultipleOutputAsync<T1, T2>(
            string procedureName,
            DynamicParameters parameters,
            string outputParam1,
            string outputParam2,
            CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            await connection.ExecuteAsync(
                procedureName, 
                parameters, 
                commandType: CommandType.StoredProcedure
            );
            
            return (parameters.Get<T1>(outputParam1), parameters.Get<T2>(outputParam2));
        }

        // Helper method to create OracleParametersBuilder
        protected static OracleParametersBuilder CreateParametersBuilder() => new();
    }
}