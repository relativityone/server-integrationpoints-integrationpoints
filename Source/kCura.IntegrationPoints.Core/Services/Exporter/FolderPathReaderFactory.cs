using kCura.IntegrationPoints.Data.DbContext;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
    public class FolderPathReaderFactory : IFolderPathReaderFactory
    {
        private readonly IHelper _helper;
        private readonly IDbContextFactory _dbContextFactory;

        public FolderPathReaderFactory(IHelper helper)
        {
            _helper = helper;
            _dbContextFactory = new DbContextFactory(_helper);
        }

        public IFolderPathReader Create(int workspaceArtifactID, bool useDynamicFolderPath)
        {
            if (useDynamicFolderPath)
            {
                IWorkspaceDBContext dbContext = _dbContextFactory.CreateWorkspaceDbContext(workspaceArtifactID); //_helper.GetDBContext(workspaceArtifactID);
                return new DynamicFolderPathReader(dbContext);
            }

            return new EmptyFolderPathReader();
        }
    }
}