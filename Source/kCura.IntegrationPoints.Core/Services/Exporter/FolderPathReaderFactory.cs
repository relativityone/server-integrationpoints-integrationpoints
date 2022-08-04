using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
    public class FolderPathReaderFactory : IFolderPathReaderFactory
    {
        private readonly IHelper _helper;

        public FolderPathReaderFactory(IHelper helper)
        {
            _helper = helper;
        }

        public IFolderPathReader Create(int workspaceArtifactID, bool useDynamicFolderPath)
        {
            if (useDynamicFolderPath)
            {
                IDBContext dbContext = _helper.GetDBContext(workspaceArtifactID);
                return new DynamicFolderPathReader(dbContext);
            }

            return new EmptyFolderPathReader();
        }
    }
}