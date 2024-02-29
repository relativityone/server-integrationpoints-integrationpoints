using System.Data.SqlClient;
using System.Net;
using System.Security;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
    public static class SqlHelper
    {
        public static SqlConnection CreateConnectionFromAppConfig(int workspaceArtifactId)
        {
            SecureString password = new NetworkCredential(string.Empty, AppSettings.SqlPassword).SecurePassword;
            password.MakeReadOnly();
            SqlCredential credential = new SqlCredential(AppSettings.SqlUsername, password);

            return new SqlConnection(
                GetConnectionString(workspaceArtifactId),
                credential);
        }

        public static SqlConnection CreateEddsConnectionFromAppConfig()
        {
            return CreateConnectionFromAppConfig(-1);
        }

        private static string GetConnectionString(int workspaceArtifactId) => workspaceArtifactId == -1
            ? AppSettings.ConnectionStringEDDS
            : AppSettings.ConnectionStringWorkspace(workspaceArtifactId);
    }
}
