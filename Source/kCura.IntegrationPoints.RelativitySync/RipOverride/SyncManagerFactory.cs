using System;
using System.Threading;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
	// This way we have an option to change only those behaviors we have to as almost everything in push originates here ;)
	internal sealed class SyncManagerFactory : IManagerFactory
	{
		private readonly ITagsCreator _tagsCreator;
		private readonly ITagSavedSearchManager _tagSavedSearchManager;
		private readonly ISourceWorkspaceTagCreator _sourceWorkspaceTagCreator;
		private readonly IManagerFactory _ordinaryManagerFactory;

		public SyncManagerFactory(ITagsCreator tagsCreator, ITagSavedSearchManager tagSavedSearchManager, ISourceWorkspaceTagCreator sourceWorkspaceTagCreator, IManagerFactory ordinaryManagerFactory)
		{
			_tagsCreator = tagsCreator;
			_tagSavedSearchManager = tagSavedSearchManager;
			_sourceWorkspaceTagCreator = sourceWorkspaceTagCreator;
			_ordinaryManagerFactory = ordinaryManagerFactory;
		}

		public ITagsCreator CreateTagsCreator(IContextContainer contextContainer)
		{
			return _tagsCreator;
		}

		public ITagSavedSearchManager CreateTaggingSavedSearchManager(IContextContainer contextContainer)
		{
			return _tagSavedSearchManager;
		}

		public ISourceWorkspaceTagCreator CreateSourceWorkspaceTagsCreator(IContextContainer contextContainer, IHelper targetHelper, SourceConfiguration sourceConfiguration)
		{
			return _sourceWorkspaceTagCreator;
		}

		#region Proxy only methods

		public IArtifactGuidManager CreateArtifactGuidManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateArtifactGuidManager(contextContainer);
		}

		public IFieldManager CreateFieldManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateFieldManager(contextContainer);
		}

		public IJobHistoryManager CreateJobHistoryManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateJobHistoryManager(contextContainer);
		}

		public IJobHistoryErrorManager CreateJobHistoryErrorManager(IContextContainer contextContainer, int sourceWorkspaceArtifactId, string uniqueJobId)
		{
			return _ordinaryManagerFactory.CreateJobHistoryErrorManager(contextContainer, sourceWorkspaceArtifactId, uniqueJobId);
		}

		public IObjectTypeManager CreateObjectTypeManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateObjectTypeManager(contextContainer);
		}

		public IQueueManager CreateQueueManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateQueueManager(contextContainer);
		}

		public IStateManager CreateStateManager()
		{
			return _ordinaryManagerFactory.CreateStateManager();
		}

		public ISourceProviderManager CreateSourceProviderManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateSourceProviderManager(contextContainer);
		}

		public IErrorManager CreateErrorManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateErrorManager(contextContainer);
		}

		public IJobStopManager CreateJobStopManager(IJobService jobService, IJobHistoryService jobHistoryService, Guid jobIdentifier, long jobId, bool isStoppableJob,
			CancellationTokenSource cancellationTokenSource = null)
		{
			return _ordinaryManagerFactory.CreateJobStopManager(jobService, jobHistoryService, jobIdentifier, jobId, isStoppableJob, cancellationTokenSource);
		}

		public IAuditManager CreateAuditManager(IContextContainer contextContainer, int workspaceArtifactId)
		{
			return _ordinaryManagerFactory.CreateAuditManager(contextContainer, workspaceArtifactId);
		}

		public IFederatedInstanceManager CreateFederatedInstanceManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateFederatedInstanceManager(contextContainer);
		}

		public IWorkspaceManager CreateWorkspaceManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateWorkspaceManager(contextContainer);
		}

		public IPermissionManager CreatePermissionManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreatePermissionManager(contextContainer);
		}

		public IInstanceSettingsManager CreateInstanceSettingsManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateInstanceSettingsManager(contextContainer);
		}

		public IProductionManager CreateProductionManager(IContextContainer contextContainer)
		{
			return _ordinaryManagerFactory.CreateProductionManager(contextContainer);
		}

		#endregion
	}
}