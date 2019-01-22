using System;
using System.Threading;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ManagerFactory : IManagerFactory
	{
		private readonly IHelper _helper;
		private readonly IServiceManagerProvider _serviceManagerProvider;

		public ManagerFactory(IHelper helper, IServiceManagerProvider serviceManagerProvider)
		{
			_helper = helper;
			_serviceManagerProvider = serviceManagerProvider;
		}

		public IArtifactGuidManager CreateArtifactGuidManager(IContextContainer contextContainer)
		{
			return new ArtifactGuidManager(CreateRepositoryFactory(contextContainer));
		}

		public IFieldManager CreateFieldManager(IContextContainer contextContainer)
		{
			return new FieldManager(CreateRepositoryFactory(contextContainer));
		}

		public IJobHistoryManager CreateJobHistoryManager(IContextContainer contextContainer)
		{
			return new JobHistoryManager(CreateRepositoryFactory(contextContainer), _helper);
		}

		public IJobHistoryErrorManager CreateJobHistoryErrorManager(IContextContainer contextContainer, int sourceWorkspaceArtifactId, string uniqueJobId)
		{
			return new JobHistoryErrorManager(CreateRepositoryFactory(contextContainer), _helper, sourceWorkspaceArtifactId, uniqueJobId);
		}

		public IObjectTypeManager CreateObjectTypeManager(IContextContainer contextContainer)
		{
			return new ObjectTypeManager(CreateRepositoryFactory(contextContainer));
		}

		public IQueueManager CreateQueueManager(IContextContainer contextContainer)
		{
			return new QueueManager(CreateRepositoryFactory(contextContainer));
		}

		public ISourceProviderManager CreateSourceProviderManager(IContextContainer contextContainer)
		{
			return new SourceProviderManager(CreateRepositoryFactory(contextContainer));
		}

		public IErrorManager CreateErrorManager(IContextContainer contextContainer)
		{
			return new ErrorManager(CreateRepositoryFactory(contextContainer));
		}

		public IJobStopManager CreateJobStopManager(IJobService jobService, IJobHistoryService jobHistoryService, Guid jobIdentifier, long jobId, bool isStoppableJob, CancellationTokenSource cancellationTokenSource = null)
		{
			IJobStopManager manager;
			if (isStoppableJob)
			{
				manager = new JobStopManager(jobService, jobHistoryService, _helper, jobIdentifier, jobId, cancellationTokenSource ?? new CancellationTokenSource());
			}
			else
			{
				manager = new NullableStopJobManager();
			}
			return manager;
		}

		public IAuditManager CreateAuditManager(IContextContainer contextContainer, int workspaceArtifactId)
		{
			IRepositoryFactory repositoryFactory = CreateRepositoryFactory(contextContainer);
			IRelativityAuditRepository relativityAuditRepository = repositoryFactory.GetRelativityAuditRepository(workspaceArtifactId);
			return new AuditManager(relativityAuditRepository);
		}

		public IStateManager CreateStateManager()
		{
			return new StateManager();
		}

		public IFederatedInstanceManager CreateFederatedInstanceManager(IContextContainer contextContainer)
		{
			IFederatedInstanceManager manager = new FederatedInstanceManager(CreateRepositoryFactory(contextContainer));

			return manager;
		}

		public IWorkspaceManager CreateWorkspaceManager(IContextContainer contextContainer)
		{
			IWorkspaceManager workspaceManager = new WorkspaceManager(CreateRepositoryFactory(contextContainer));
			return workspaceManager;
		}

		public IPermissionManager CreatePermissionManager(IContextContainer contextContainer)
		{
			IPermissionManager permissionManager = new PermissionManager(CreateRepositoryFactory(contextContainer));
			return permissionManager;
		}

		public ITagsCreator CreateTagsCreator(IContextContainer contextContainer)
		{
			var repositoryFactory = CreateRepositoryFactory(contextContainer);
			ISourceJobManager sourceJobManager = new SourceJobManager(repositoryFactory, _helper);
			ISourceWorkspaceManager sourceWorkspaceManager = new SourceWorkspaceManager(repositoryFactory, _helper);
			var relativitySourceRdoHelpersFactory = new RelativitySourceRdoHelpersFactory(repositoryFactory);
			IRelativitySourceJobRdoInitializer sourceJobRdoInitializer = new RelativitySourceJobRdoInitializer(_helper, repositoryFactory, relativitySourceRdoHelpersFactory);
			IRelativitySourceWorkspaceRdoInitializer sourceWorkspaceRdoInitializer = new RelativitySourceWorkspaceRdoInitializer(_helper, repositoryFactory, relativitySourceRdoHelpersFactory);
			return new TagsCreator(sourceJobManager, sourceWorkspaceManager, sourceJobRdoInitializer, sourceWorkspaceRdoInitializer, _helper);
		}

		public IInstanceSettingsManager CreateInstanceSettingsManager(IContextContainer contextContainer)
		{
			IRepositoryFactory repositoryFactory = CreateRepositoryFactory(contextContainer);
			return new InstanceSettingsManager(repositoryFactory);
		}

		public IProductionManager CreateProductionManager(IContextContainer contextContainer)
		{
			IRepositoryFactory repositoryFactory = CreateRepositoryFactory(contextContainer);
			IFederatedInstanceManager federatedInstanceManager = CreateFederatedInstanceManager(contextContainer);
			IAPILog logger = _helper.GetLoggerFactory().GetLogger();
			return new ProductionManager(logger, repositoryFactory, _serviceManagerProvider, federatedInstanceManager);
		}

		public ITagSavedSearchManager CreateTaggingSavedSearchManager(IContextContainer contextContainer)
		{
			IRepositoryFactory repositoryFactory = CreateRepositoryFactory(contextContainer);
			ITagSavedSearch tagSavedSearch = new TagSavedSearch(repositoryFactory, new MultiObjectSavedSearchCondition(), _helper);
			ITagSavedSearchFolder tagSavedSearchFolder = new TagSavedSearchFolder(repositoryFactory, _helper);
			return new TagSavedSearchManager(tagSavedSearch, tagSavedSearchFolder);
		}

		#region Private Helpers

		private IRepositoryFactory CreateRepositoryFactory(IContextContainer contextContainer)
		{
			return new RepositoryFactory(contextContainer.Helper, contextContainer.ServicesMgr);
		}

		#endregion
	}
}