﻿using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ManagerFactory : IManagerFactory
	{
		public IArtifactGuidManager CreateArtifactGuidManager(IContextContainer contextContainer)
		{
			return new ArtifactGuidManager(CreateRepositoryFactory(contextContainer));
		}

		public IFieldManager CreateFieldManager(IContextContainer contextContainer)
		{
			return new FieldManager(CreateRepositoryFactory(contextContainer));
		}

		public IIntegrationPointManager CreateIntegrationPointManager(IContextContainer contextContainer)
		{
			return new IntegrationPointManager(CreateRepositoryFactory(contextContainer));
		}

		public IJobHistoryManager CreateJobHistoryManager(IContextContainer contextContainer)
		{
			return new JobHistoryManager(CreateRepositoryFactory(contextContainer));
		}

		public IJobHistoryErrorManager CreateJobHistoryErrorManager(IContextContainer contextContainer, int sourceWorkspaceArtifactId, string uniqueJobId)
		{
			return new JobHistoryErrorManager(CreateRepositoryFactory(contextContainer), sourceWorkspaceArtifactId, uniqueJobId);
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

		public ICancellationManager CreateCancellationManager(IContextContainer contextContainer, JobHistory jobHistory, Job job)
		{
			IRepositoryFactory repo = CreateRepositoryFactory(contextContainer);
			return new CancellationManager(repo, jobHistory, job);
		}

		public IStateManager CreateStateManager()
		{
			return new StateManager();
		}

		#region Private Helpers

		private IRepositoryFactory CreateRepositoryFactory(IContextContainer contextContainer)
		{
			return new RepositoryFactory(contextContainer.Helper);
		}

		#endregion

	}
}
