using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.DbContext
{
    public class GetArtifactForMassAction
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public GetArtifactForMassAction(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public List<int> GetArtifactsToBeDeleted(IWorkspaceDBContext workspaceContext, string tempTableName, int workspaceID)
        {
            IScratchTableRepository scratchTableRepository = _repositoryFactory.GetScratchTableRepository(workspaceID, string.Empty, string.Empty);

            string sql = string.Format("SELECT [ArtifactID] FROM {0}.[{1}]", scratchTableRepository.GetResourceDBPrepend(), tempTableName);

            return workspaceContext.ExecuteSqlStatementAsDataTable(sql).Rows.Cast<System.Data.DataRow>().Select(dr => Convert.ToInt32(dr["ArtifactID"])).ToList();
        }
    }
}
