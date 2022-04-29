using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Security;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers
{
    public static class SqlHelper
    {
        public static SqlConnection CreateConnectionFromAppConfig(int workspaceArtifactId)
        {
            SecureString password = new NetworkCredential("", TestConfig.SqlPassword).SecurePassword;
            password.MakeReadOnly();
            SqlCredential credential = new SqlCredential(TestConfig.SqlUsername, password);

            return new SqlConnection(
                GetConnectionString(workspaceArtifactId),
                credential);
        }

        public static SqlConnection CreateEddsConnectionFromAppConfig()
        {
            return CreateConnectionFromAppConfig(-1);
        }

        private static string GetConnectionString(int workspaceArtifactId) => workspaceArtifactId == -1
            ? TestConfig.ConnectionStringEDDS
            : TestConfig.ConnectionStringWorkspace(workspaceArtifactId);

        public static DataTable ExecuteSqlStatementAsDataTable(int workspaceArtifactId, string sqlQuery)
        {
            SqlConnection connection = CreateConnectionFromAppConfig(workspaceArtifactId);
            DataTable dataTable = new DataTable();

            connection.Open();
            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
            {                             
                dataTable.Load(command.ExecuteReader());                
            }
            connection.Close();
            return dataTable;
        }
    }
}
