using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeRepositoryFactory : IRepositoryFactory
	{
		private readonly RelativityInstanceTest _db;
		private readonly IRepositoryFactory _repositoryFactory;

		public FakeRepositoryFactory(RelativityInstanceTest db, IRepositoryFactory repositoryFactory)
		{
			_db = db;
			_repositoryFactory = repositoryFactory;
		}

		public IArtifactGuidRepository GetArtifactGuidRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetArtifactGuidRepository(workspaceArtifactId);
		}

		public ICodeRepository GetCodeRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetCodeRepository(workspaceArtifactId);
		}

		public IDestinationProviderRepository GetDestinationProviderRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetDestinationProviderRepository(workspaceArtifactId);
		}

		public IDestinationWorkspaceRepository GetDestinationWorkspaceRepository(int sourceWorkspaceArtifactId)
		{
			return _repositoryFactory.GetDestinationWorkspaceRepository(sourceWorkspaceArtifactId);
		}

		public IDocumentRepository GetDocumentRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetDocumentRepository(workspaceArtifactId);
		}

		public IFieldQueryRepository GetFieldQueryRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetFieldQueryRepository(workspaceArtifactId);
		}

		public IFieldRepository GetFieldRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetFieldRepository(workspaceArtifactId);
		}

		public IJobHistoryRepository GetJobHistoryRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
		}

		public IJobHistoryErrorRepository GetJobHistoryErrorRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetJobHistoryErrorRepository(workspaceArtifactId);
		}

		public IObjectRepository GetObjectRepository(int workspaceArtifactId, int rdoArtifactId)
		{
			return _repositoryFactory.GetObjectRepository(workspaceArtifactId, rdoArtifactId);
		}

		public IObjectTypeRepository GetObjectTypeRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetObjectTypeRepository(workspaceArtifactId);
		}

		public IObjectTypeRepository GetDestinationObjectTypeRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetDestinationObjectTypeRepository(workspaceArtifactId);
		}

		public IPermissionRepository GetPermissionRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
		}

		public IQueueRepository GetQueueRepository()
		{
			return new FakeQueueRepository(_db.JobsInQueue);
		}

		public IScratchTableRepository GetScratchTableRepository(int workspaceArtifactID, string tablePrefix, string tableSuffix)
		{
			return new FakeScratchTableRepository(_db, workspaceArtifactID, tablePrefix, tableSuffix);
		}

		public ISourceJobRepository GetSourceJobRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetSourceJobRepository(workspaceArtifactId);
		}

		public ISourceProviderRepository GetSourceProviderRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
		}

		public ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetSourceWorkspaceRepository(workspaceArtifactId);
		}

		public ITabRepository GetTabRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetTabRepository(workspaceArtifactId);
		}

		public IWorkspaceRepository GetWorkspaceRepository()
		{
			return _repositoryFactory.GetWorkspaceRepository();
		}

		public IWorkspaceRepository GetSourceWorkspaceRepository()
		{
			return _repositoryFactory.GetSourceWorkspaceRepository();
		}

		public IErrorRepository GetErrorRepository()
		{
			return _repositoryFactory.GetErrorRepository();
		}

		public ISavedSearchRepository GetSavedSearchRepository(int workspaceArtifactId, int savedSearchArtifactId)
		{
			return _repositoryFactory.GetSavedSearchRepository(workspaceArtifactId, savedSearchArtifactId);
		}

		public IProductionRepository GetProductionRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetProductionRepository(workspaceArtifactId);
		}

		public ISavedSearchQueryRepository GetSavedSearchQueryRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetSavedSearchQueryRepository(workspaceArtifactId);
		}

		public IInstanceSettingRepository GetInstanceSettingRepository()
		{
			return _repositoryFactory.GetInstanceSettingRepository();
		}

		public IRelativityAuditRepository GetRelativityAuditRepository(int workspaceArtifactId)
		{
			return new FakeAuditRepository();
	}

		public IResourcePoolRepository GetResourcePoolRepository()
		{
			return _repositoryFactory.GetResourcePoolRepository();
		}

		public IKeywordSearchRepository GetKeywordSearchRepository()
		{
			return _repositoryFactory.GetKeywordSearchRepository();
		}

		public IQueryFieldLookupRepository GetQueryFieldLookupRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetQueryFieldLookupRepository(workspaceArtifactId);
		}

		public IAuditRepository GetAuditRepository(int workspaceArtifactId)
		{
			return _repositoryFactory.GetAuditRepository(workspaceArtifactId);
		}

		public ICaseRepository GetCaseRepository()
		{
			return _repositoryFactory.GetCaseRepository();
		}
    }
}