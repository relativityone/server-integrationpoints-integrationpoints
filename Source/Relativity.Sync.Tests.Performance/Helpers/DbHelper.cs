using Relativity.Sync.Tests.System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Performance.Helpers
{
	public static class DbHelper
	{
		public static SqlConnection CreateConnectionFromAppConfig(int workspaceArtifactID = -1)
		{
			SecureString password = new NetworkCredential("", AppSettings.SqlPassword).SecurePassword;
			password.MakeReadOnly();
			SqlCredential credential = new SqlCredential(AppSettings.SqlUsername, password);

			var connection =  new SqlConnection(
				GetWorkspaceConnectionString(workspaceArtifactID),
				credential);

			connection.Open();
			return connection;
		}

		private static string GetWorkspaceConnectionString(int workspaceArtifactID) => $"Data Source={AppSettings.SqlServer};Initial Catalog=EDDS{workspaceArtifactID}";

	}
}
