﻿using System;
using System.Threading;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Factories
{
	/// <summary>
	/// Manager factory is responsible for creation of specific managers.
	/// </summary>
	public interface IManagerFactory
	{
		/// <summary>
		/// Creates Artifact GUID manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Artifact GUID manager</returns>
		IArtifactGuidManager CreateArtifactGuidManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates field manager.
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Field manager</returns>
		IFieldManager CreateFieldManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates Job History manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Job History manager</returns>
		IJobHistoryManager CreateJobHistoryManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates Job History Error manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <param name="sourceWorkspaceArtifactId">Artifact id of the source workspace</param>
		/// <param name="uniqueJobId">Unique job id created using the id of the job and the guid identifier</param>
		/// <returns>Job History Error manager</returns>
		IJobHistoryErrorManager CreateJobHistoryErrorManager(IContextContainer contextContainer, int sourceWorkspaceArtifactId, string uniqueJobId);

		/// <summary>
		/// Creates Object Type manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Object Type manager</returns>
		IObjectTypeManager CreateObjectTypeManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates Queue Manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>A queue manager</returns>
		IQueueManager CreateQueueManager(IContextContainer contextContainer);
		
		/// <summary>
		/// Create State manager.
		/// </summary>
		/// <returns>State manager (for console buttons)</returns>
		IStateManager CreateStateManager();
		
		/// <summary>
		/// Creates source provider manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Source provider manager</returns>
		ISourceProviderManager CreateSourceProviderManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates an error manager.
		/// </summary>
		/// <param name="contextContainer"></param>
		/// <returns>Error Manager</returns>
		IErrorManager CreateErrorManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates a job stop manager to handle the stopping signal of the job.
		/// </summary>
		/// <param name="jobService">A service class provides functionalities to control the scheduled queue job.</param>
		/// <param name="jobHistoryService">A service class provides functionalities to control the job history.</param>
		/// <param name="jobIdentifier">Guid of the job history</param>
		/// <param name="jobId">Artifact id of the scheduled queue job</param>
		/// <param name="isStoppableJob">A boolean flag to indicate whether the job stop manager is on an unstoppable job.</param>
		/// <param name="cancellationTokenSource">Cancellation token source passed to JobStopManager</param>
		/// <returns></returns>
		IJobStopManager CreateJobStopManager(IJobService jobService, IJobHistoryService jobHistoryService, Guid jobIdentifier, long jobId, bool isStoppableJob, CancellationTokenSource cancellationTokenSource = null);

		/// <summary>
		/// Creates an audit manager.
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <param name="workspaceArtifactId">Artifact id of a workspace</param>
		/// <returns>Audit Manager</returns>
		IAuditManager CreateAuditManager(IContextContainer contextContainer, int workspaceArtifactId);

		IFederatedInstanceManager CreateFederatedInstanceManager(IContextContainer contextContainer);

		IWorkspaceManager CreateWorkspaceManager(IContextContainer contextContainer);

		IPermissionManager CreatePermissionManager(IContextContainer contextContainer);

		ITagsCreator CreateTagsCreator(IContextContainer contextContainer);

		/// <summary>
		/// Creates a Instance Settings Manager
		/// </summary>
		/// <param name="contextContainer">Container containing necessary contexts</param>
		/// <returns>Instance of Instance Settings Manager</returns>
		IInstanceSettingsManager CreateInstanceSettingsManager(IContextContainer contextContainer);

		/// <summary>
		/// Creates a Production Manager
		/// </summary>
		/// <param name="contextContainer"></param>
		/// <returns>Instance of Production Manager</returns>
		IProductionManager CreateProductionManager(IContextContainer contextContainer);
		
		ITagSavedSearchManager CreateTaggingSavedSearchManager(IContextContainer contextContainer);
	}
}
