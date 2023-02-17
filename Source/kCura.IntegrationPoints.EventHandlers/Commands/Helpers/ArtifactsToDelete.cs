using System.Collections.Generic;
using System.Data.Common;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Helpers
{
    public class ArtifactsToDelete : IArtifactsToDelete
    {
        private const string _SQL = "SELECT ArtifactID FROM {0}.[{1}]";
        private readonly IEHContext _context;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IDbContextFactory _dbContextFactory;

        public ArtifactsToDelete(IEHContext context, IRepositoryFactory repositoryFactory)
        {
            _context = context;
            _repositoryFactory = repositoryFactory;
            _dbContextFactory = new DbContextFactory(_context.Helper);
        }

        public List<int> GetIds()
        {
            List<int> artifactIds = new List<int>();
            using (DbDataReader reader = GetArtifactsToBeDeleted())
            {
                while (reader.Read())
                {
                    artifactIds.Add(reader.GetInt32(0));
                }
            }

            return artifactIds;
        }

        private DbDataReader GetArtifactsToBeDeleted()
        {
            IWorkspaceDBContext dbContext = GetWorkspaceDbContext();
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(_context.Helper.GetActiveCaseID(), string.Empty, string.Empty);

            string sql = string.Format(_SQL, scratchTableRepository.GetResourceDBPrepend(), _context.TempTableNameWithParentArtifactsToDelete);
            return dbContext.ExecuteSqlStatementAsDbDataReader(sql);
        }

        private IWorkspaceDBContext GetWorkspaceDbContext()
        {
            return _dbContextFactory.CreateWorkspaceDbContext(_context.Helper.GetActiveCaseID());
        }
    }
}
