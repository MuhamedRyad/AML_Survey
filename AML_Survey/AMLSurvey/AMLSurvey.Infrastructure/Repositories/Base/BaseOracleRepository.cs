using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using AMLSurvey.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;

namespace AMLSurvey.Infrastructure.Repositories.Base
{

    //1-get connection string from appsettings.json
    //2-Open and close connection with "using" automatically
    //3-Execute stored procedure with Dapper
    public abstract class BaseOracleRepository
    {
        protected readonly string _connectionString;

        protected BaseOracleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        //1-ExecuteQueryAsync returning single result set
        //2-ExecuteQueryMultiple2Async returning 2 result sets
        //3-ExecuteQueryMultiple3Async returning 3 result sets
        //4-ExecuteQueryMultiple4Async returning 4 result sets
        //5-ExecuteScalarAsync for single value return (e.g., count, sum)
        //6-ExecuteAsync for Insert, Update, Delete


        //1- For procedures returning single result set
        protected async Task<IEnumerable<T>> ExecuteQueryAsync<T>(
            string procedureName,
            DynamicParameters parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);

            // ✅ مش محتاج OpenAsync - Dapper بيعملها
            using var multi = await connection.QueryMultipleAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return await multi.ReadAsync<T>();
        }
        //2- For procedures returning 2 result sets
        protected async Task<(IEnumerable<T1>, IEnumerable<T2>)> ExecuteQueryMultiple2Async<T1, T2>(
            string procedureName,
            DynamicParameters parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);

            using var multi = await connection.QueryMultipleAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var result1 = await multi.ReadAsync<T1>();
            var result2 = await multi.ReadAsync<T2>();

            return (result1, result2);
        }

        //3- For procedures returning 3 result sets
        protected async Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)>
            ExecuteQueryMultiple3Async<T1, T2, T3>(
                string procedureName,
                DynamicParameters parameters = null,
                CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);
            using var multi = await connection.QueryMultipleAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );
            var result1 = await multi.ReadAsync<T1>();
            var result2 = await multi.ReadAsync<T2>();
            var result3 = await multi.ReadAsync<T3>();
            return (result1, result2, result3);
        }

        //4- For procedures returning 4 result sets
        protected async Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>)>
            ExecuteQueryMultiple4Async<T1, T2, T3, T4>(
                string procedureName,
                DynamicParameters parameters = null,
                CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);

            using var multi = await connection.QueryMultipleAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var result1 = await multi.ReadAsync<T1>();
            var result2 = await multi.ReadAsync<T2>();
            var result3 = await multi.ReadAsync<T3>();
            var result4 = await multi.ReadAsync<T4>();

            return (result1, result2, result3, result4);
        }

        //5- ExecuteScalarAsync for single value return (e.g., count, sum)
        protected async Task<T?> ExecuteScalarAsync<T>(
            string procedureName,
            DynamicParameters parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);

            return await connection.ExecuteScalarAsync<T>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        //6- ExecuteAsync for Insert, Update, Delete
        protected async Task<int> ExecuteAsync(
            string procedureName,
            DynamicParameters parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = new OracleConnection(_connectionString);

            return await connection.ExecuteAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        protected static OracleParametersBuilder CreateParametersBuilder() => new();
    }
}