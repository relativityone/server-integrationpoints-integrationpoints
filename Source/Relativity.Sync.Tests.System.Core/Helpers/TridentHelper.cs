using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
    internal static class TridentHelper
    {
        public static void UpdateFilePathToLocalIfNeeded(int workspaceArtifactId, Dataset dataSet, bool wereNativesImported = true)
        {
            if (AppSettings.IsSettingsFileSet && wereNativesImported)
            {
                #region Hopper Instance workaround explanation

                // This workaround was provided to omit Hopper Instance restrictions. IAPI which is executing on agent can't access file based on file location in database like '\\emttest\DefaultFileRepository\...'.
                // Hopper is closed for outside traffic so there is no possibility to access fileshare from Trident Agent. Jira related to this https://jira.kcura.com/browse/DEVOPS-70257.
                // If we decouple Sync from RIP and move it to RAP problem probably disappears. Right now as workaround we change on database this relative Fileshare path to local,
                // where out test data are stored. So we assume in testing that push is working correctly, but whole flow (metadata, etc.) is under tests.
                #endregion
                using (SqlConnection connection = SqlHelper.CreateConnectionFromAppConfig(workspaceArtifactId))
                {
                    connection.Open();

                    string localFolderPath = dataSet.ImportType == ImportType.Native
                        ? Path.Combine(dataSet.FolderPath, "NATIVES")
                        : dataSet.FolderPath;

                    SqlParameter[] fileNames = Directory.GetFiles(localFolderPath)
                        .Select((x, idx) => new SqlParameter($"File{idx}", new FileInfo(x).Name)).ToArray();
                    string fileParameterNames = string.Join(",", fileNames.Select(x => $"@{x.ParameterName}"));

                    string sqlStatement =
                        $@"UPDATE [File] SET Location = CONCAT(@LocalFilePath, '\', [Filename]) WHERE [TYPE] = @FileType AND [Filename] IN ({fileParameterNames})";

                    SqlCommand command = new SqlCommand(sqlStatement, connection);
                    command.Parameters.AddWithValue("LocalFilePath", localFolderPath);
                    command.Parameters.AddWithValue("FileType", (int)dataSet.ImportType);
                    command.Parameters.AddRange(fileNames);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
