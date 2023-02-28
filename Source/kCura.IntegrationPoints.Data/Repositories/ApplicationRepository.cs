using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Relativity.API;
using Relativity.Data;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IHelper _helper;

        public ApplicationRepository(IHelper helper)
        {
            _helper = helper;
        }

        public IList<int> GetWorkspaceArtifactIdsWhereApplicationInstalled(Guid applicationGuid)
        {
            SqlParameter appGuidParameter = new SqlParameter("@appGuid", SqlDbType.UniqueIdentifier) {Value = applicationGuid};
            SqlParameter[] parameters = {appGuidParameter};

            int[] validApplicationStatus =
            {
                (int) ApplicationInstall.StatusCode.Installed,
                (int) ApplicationInstall.StatusCode.Modified,
                (int) ApplicationInstall.StatusCode.OutOfDate
            };
            string applicationStatusCsv = $"{string.Join(",", validApplicationStatus)}";
            string sql = string.Format(_WORKSPACE_ARTIFACT_IDS_WHERE_APPLICATION_INSTALLED_SQL, applicationStatusCsv);

            using (IDataReader reader = _helper.GetDBContext(-1).ExecuteSqlStatementAsDbDataReader(sql, parameters))
            {
                IList<int> workspaceArtifactIds = new List<int>();
                while (reader.Read())
                {
                    int workspaceArtifactId = reader.GetInt32(0);
                    workspaceArtifactIds.Add(workspaceArtifactId);
                }
                return workspaceArtifactIds;
            }
        }

        private const string _WORKSPACE_ARTIFACT_IDS_WHERE_APPLICATION_INSTALLED_SQL = @"
            SELECT CA.[CaseArtifactId] FROM [eddsdbo].[ExtendedCaseApplication] as CA WITH(NOLOCK)
            INNER JOIN [eddsdbo].[ArtifactGuid] as AG WITH(NOLOCK)
            ON AG.[ArtifactID] = CA.[ApplicationArtifactID]
            WHERE AG.[ArtifactGuid] = @appGuid
            AND CA.[StatusCode] IN ({0})
            AND CA.[CaseArtifactId] <> -1";
    }
}
