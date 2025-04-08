using Microsoft.Data.SqlClient;

namespace dotnet_api_extensions.Sql
{
    public class SqlDbConnectionFactory
    {
        private readonly string _dbConnectionString;
        public SqlDbConnectionFactory(string connectionString)
        {
            _dbConnectionString = connectionString;

            if (string.IsNullOrEmpty(_dbConnectionString))
                throw new Exception("DB Connection string not specified.");
        }

        public SqlConnection GetDbConnection()
        {
            var sqlConnection = new SqlConnection(_dbConnectionString);
            sqlConnection.Open();
            return sqlConnection;
        }
    }
}
