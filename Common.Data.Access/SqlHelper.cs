using System;
using System.Data.SqlClient;

namespace Common.Data.Access
{
    public class SqlHelper
    {
        public static void ExecuteQuery(string query, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        public static bool VerifyDbExists(string connectionString)
        {
            var isExistingDb = false;
            var builder = new SqlConnectionStringBuilder(connectionString);
            var database = builder.InitialCatalog;
            var masterConnection = connectionString.Replace(builder.InitialCatalog, "master");

            using var connection = new SqlConnection(masterConnection);

            try
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = $@"if(db_id('{database}') is null) create database [{database}]";
                var result = command.ExecuteNonQuery();
                isExistingDb = result > 0;

            }
            catch (SqlException ex)
            {
                throw new Exception($"{ex.Message}; {ex.InnerException}");
            }

            return isExistingDb;
        }
    }
}
