using System.Data.SqlClient;
using System.Net;
using System.Security;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	public static class SqlHelper
	{
		public static SqlConnection CreateConnectionFromAppConfig(int workspaceArtifactId)
		{
			SecureString password = new NetworkCredential("", AppSettings.SqlPassword).SecurePassword;
			password.MakeReadOnly();
			SqlCredential credential = new SqlCredential(AppSettings.SqlUsername, password);

			return new SqlConnection(
				GetWorkspaceConnectionString(workspaceArtifactId),
				credential);
		}

		private static string GetWorkspaceConnectionString(int workspaceArtifactId) => $"Data Source={AppSettings.SqlServer};Initial Catalog=EDDS{workspaceArtifactId}";

	}
}
