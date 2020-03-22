﻿using Relativity.Sync.Tests.System;
using System;
using System.Collections.Generic;
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
		public static SqlConnection CreateConnectionFromAppConfig(int workspaceArtifactID)
		{
			SecureString password = new NetworkCredential("", AppSettings.SqlPassword).SecurePassword;
			password.MakeReadOnly();
			SqlCredential credential = new SqlCredential(AppSettings.SqlUsername, password);

			return new SqlConnection(
				GetWorkspaceConnectionString(workspaceArtifactID),
				credential);
		}

		private static string GetWorkspaceConnectionString(int workspaceArtifactID) => $"Data Source={AppSettings.SqlServer};Initial Catalog=EDDS{workspaceArtifactID}";

	}
}
