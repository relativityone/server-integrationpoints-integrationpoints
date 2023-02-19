using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
    public class FolderPathInformation : IFolderPathInformation
    {
        private readonly IWorkspaceDBContext _dbContext;

        public FolderPathInformation(IWorkspaceDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string RetrieveName(string destinationConfiguration)
        {
            IntegrationPointDestinationConfiguration integrationPointDestinationConfiguration =
                JsonConvert.DeserializeObject<IntegrationPointDestinationConfiguration>(destinationConfiguration);

            string folderPathInformation = string.Empty;

            if ((integrationPointDestinationConfiguration.ImportOverwriteMode == ImportOverwriteModeEnum.AppendOnly) &&
                integrationPointDestinationConfiguration.UseFolderPathInformation)
            {
                var sqlString = $"SELECT TextIdentifier FROM Artifact WHERE ArtifactID = {integrationPointDestinationConfiguration.FolderPathSourceField}";
                folderPathInformation = _dbContext.ExecuteSqlStatementAsDataTable(sqlString).Rows[0].ItemArray[0].ToString();
            }

            return folderPathInformation;
        }

        internal class IntegrationPointDestinationConfiguration
        {
            public int FolderPathSourceField;
            public ImportOverwriteModeEnum ImportOverwriteMode;
            public bool UseFolderPathInformation;
        }
    }
}
