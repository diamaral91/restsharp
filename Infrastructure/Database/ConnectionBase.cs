using Dapper;
using System.Data.SqlClient;

namespace DepositAccount.APITest.Infrastructure.database
{
    public class ConnectionBase
    {
        private readonly string _connectionString;

        public ConnectionBase()
        {
            _connectionString = Configuration.GetConfigs().GetSection("DB:ConnectionString").Value;
            SqlMapper.Settings.CommandTimeout = 90;
        }

        public ConnectionBase(string connectionString)
        {
            _connectionString = Configuration.GetConfigs().GetSection("DB:" + connectionString).Value;
        }

        public async Task<IEnumerable<T>> Get<T>(string sql)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                return await con.QueryAsync<T>(sql, commandTimeout: 90);
            }
        }

        public async Task<IEnumerable<T>> Get<T>(string sql, object parameters)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                return await con.QueryAsync<T>(sql, parameters, commandTimeout: 90);
            }
        }

        public async Task<T> GetFirst<T>(string sql, object parameters)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                return await con.QueryFirstAsync<T>(sql, parameters, commandTimeout: 90);
            }
        }

        public async Task UpdateDepositAccount(string sql, object parameters)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.ExecuteAsync(sql, parameters, commandTimeout: 90);
            }
        }
    }
}