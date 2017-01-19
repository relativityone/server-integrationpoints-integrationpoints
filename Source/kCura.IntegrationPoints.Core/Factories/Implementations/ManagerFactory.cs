using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
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

		public ManagerFactory(IHelper helper)
		{
			_helper = helper;
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

		public IJobStopManager CreateJobStopManager(IJobService jobService, IJobHistoryService jobHistoryService, Guid jobIdentifier, long jobId, bool isStoppableJob)
		{
			IJobStopManager manager;
			if (isStoppableJob)
			{
				manager = new JobStopManager(jobService, jobHistoryService, _helper, jobIdentifier, jobId);
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

		public IOAuthClientManager CreateOAuthClientManager(IContextContainer contextContainer)
		{
			IOAuthClientManager oAuthClientManager = new OAuthClientManager(CreateRepositoryFactory(contextContainer));

			return oAuthClientManager;
		}

		public IJobHistoryService CreateJobHistoryService(ICaseServiceContext caseServiceContext,
			IContextContainer targetContextContainer, ISerializer serializer)
		{
			IJobHistoryService jobHistoryService = 	new JobHistoryService(caseServiceContext, CreateRepositoryFactory(targetContextContainer), _helper, serializer);
			return jobHistoryService;
		}

		public IWorkspaceManager CreateWorkspaceManager(IContextContainer contextContainer)
		{
			IWorkspaceManager workspaceManager = new WorkspaceManager(CreateRepositoryFactory(contextContainer));
			return workspaceManager;
		}

		#region Private Helpers

		private IRepositoryFactory CreateRepositoryFactory(IContextContainer contextContainer)
		{
			return new RepositoryFactory(contextContainer.Helper, contextContainer.Helper.GetServicesManager());
		}

		#endregion
	}
}