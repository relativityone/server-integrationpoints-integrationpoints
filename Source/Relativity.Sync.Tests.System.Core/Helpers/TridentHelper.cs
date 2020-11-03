using System;
using System.IO;
using System.Net;
using System.Security;
using System.Data.SqlClient;
using System.Linq;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	internal static class TridentHelper
	{
		public static void UpdateFilePathToLocalIfNeeded(int workspaceArtifactId, Dataset dataSet)
		{
			if (AppSettings.IsSettingsFileSet)
			{
				#region Hopper Instance workaround explanation

				//This workaround was provided to omit Hopper Instance restrictions. IAPI which is executing on agent can't access file based on file location in database like '\\emttest\DefaultFileRepository\...'.
				//Hopper is closed for outside traffic so there is no possibility to access fileshare from Trident Agent. Jira related to this https://jira.kcura.com/browse/DEVOPS-70257.
				//If we decouple Sync from RIP and move it to RAP problem probably disappears. Right now as workaround we change on database this relative Fileshare path to local,
				//where out test data are stored. So we assume in testing that push is working correctly, but whole flow (metadata, etc.) is under tests.

				#endregion
				using (SqlConnection connection = CreateConnectionFromAppConfig(workspaceArtifactId))
				{
					connection.Open();

					if (dataSet.ImportType == ImportType.Native)
					{
						string localFolderPath = Path.Combine(dataSet.FolderPath, "NATIVES");

						const string sqlStatement =
							@"UPDATE [File] SET Location = CONCAT(@LocalFilePath, '\', [Filename])";
						
						SqlCommand command = new SqlCommand(sqlStatement, connection);
						command.Parameters.AddWithValue("LocalFilePath", localFolderPath);

						command.ExecuteNonQuery();
					}
					else
					{
						SqlParameter[] fileNames = dataSet.GetFiles()
							.Select((x, idx) => new SqlParameter($"File{idx}", x.Name)).ToArray();
						string fileParameterNames = string.Join(",", fileNames.Select(x => $"@{x.ParameterName}"));

						string sqlStatement =
							$@"UPDATE [File] SET Location = CONCAT(@LocalFilePath, '\', [Filename]) WHERE [TYPE] = @FileType AND [Filename] IN ({fileParameterNames})";

						SqlCommand command = new SqlCommand(sqlStatement, connection);
						command.Parameters.AddWithValue("LocalFilePath", dataSet.FolderPath);
						command.Parameters.AddWithValue("FileType", (int)dataSet.ImportType);
						command.Parameters.AddRange(fileNames);

						command.ExecuteNonQuery();
					}
				}
			}
		}

		private static SqlConnection CreateConnectionFromAppConfig(int workspaceArtifactId)
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
