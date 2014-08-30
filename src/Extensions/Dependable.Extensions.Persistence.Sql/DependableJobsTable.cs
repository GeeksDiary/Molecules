using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace Dependable.Extensions.Persistence.Sql
{
    public static class DependableJobsTable
    {
        public static void Create(string connectionString)
        {
            var stream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream("Dependable.Extensions.Persistence.Sql.DependableJobsTable.sql");

            if(stream == null)
                throw new InvalidOperationException("Unable to read DependableJobsTable resource.");

            using(var reader = new StreamReader(stream))
            using(var connection = new SqlConnection(connectionString))
            {
                connection.InfoMessage += connection_InfoMessage;
                var sql = reader.ReadToEnd();

                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        static void connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}