using NServiceBus;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Common.Data.Access
{
    public class SqlHelper
    {
        public static void ExecuteSql(string connectionString, string query)
        {
            EnsureDatabaseExists(connectionString);

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        public static bool EnsureDatabaseExists(string connectionString)
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

        public static void CreateSchema(string connectionString, string schema)
        {
            var sql = $@"
                if not exists (select  *
                               from    sys.schemas
                               where   name = N'{schema}')
                    exec('create schema {schema}');";

            ExecuteSql(connectionString, sql);
        }
    }
}
