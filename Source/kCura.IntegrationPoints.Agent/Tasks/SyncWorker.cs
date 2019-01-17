﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Authentication;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json.Linq;
using Relativity.API;
using Relativity.Telemetry.MetricsCollection;
using APMClient = Relativity.Telemetry.APM.Client;
using Client = Relativity.Telemetry.MetricsCollection.Client;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncWorker : IntegrationPointTaskBase, ITaskWithJobHistory
	{
		private readonly IProviderTypeService _providerTypeService;
		private IEnumerable<IBatchStatus> _batchStatus;
		private readonly bool _isStoppable;
		private readonly IAPILog _logger;
		private readonly JobStatisticsService _statisticsService;

		public SyncWorker(
			ICaseServiceContext caseServiceContext,
			IHelper helper,
			IDataProviderFactory dataProviderFactory,
			ISerializer serializer,
			ISynchronizerFactory appDomainRdoSynchronizerFactory,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			IJobManager jobManager,
			IEnumerable<IBatchStatus> statuses,
			JobStatisticsService statisticsService,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainerFactory,
			IJobService jobService, 
			IProviderTypeService providerTypeService) :
			this(caseServiceContext,
				helper,
				dataProviderFactory,
				serializer,
				appDomainRdoSynchronizerFactory,
				jobHistoryService,
				jobHistoryErrorService,
				jobManager,
				statuses,
				statisticsService,
				managerFactory,
				contextContainerFactory,
				jobService, 
				true)
		{
			_providerTypeService = providerTypeService;
		}

		protected SyncWorker(
			ICaseServiceContext caseServiceContext,
			IHelper helper,
			IDataProviderFactory dataProviderFactory,
			ISerializer serializer,
			ISynchronizerFactory appDomainRdoSynchronizerFactory,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			IJobManager jobManager,
			IEnumerable<IBatchStatus> statuses,
			JobStatisticsService statisticsService,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainerFactory,
			IJobService jobService, 
			bool isStoppable) :
			base(caseServiceContext,
				helper,
				dataProviderFactory,
				serializer,
				appDomainRdoSynchronizerFactory,
				jobHistoryService,
				jobHistoryErrorService,
				jobManager,
				managerFactory,
				contextContainerFactory,
				jobService)
		{
			BatchStatus = statuses;
			_statisticsService = statisticsService;
			_isStoppable = isStoppable;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SyncWorker>();
		}

		public IEnumerable<IBatchStatus> BatchStatus
		{
			get { return _batchStatus ?? (_batchStatus = new List<IBatchStatus>()); }
			set { _batchStatus = value; }
		}

		protected IJobStopManager JobStopManager { get; private set; }

		public void Execute(Job job)
		{
            LogExecuteStart(job);

			using (APMClient.APMClient.TimedOperation(Constants.IntegrationPoints.Telemetry.BUCKET_SYNC_WORKER_EXEC_DURATION_METRIC_COLLECTOR))
			using (Client.MetricsClient.LogDuration(
				Constants.IntegrationPoints.Telemetry.BUCKET_SYNC_WORKER_EXEC_DURATION_METRIC_COLLECTOR, Guid.Empty))
			{
				foreach (IBatchStatus batchComplete in BatchStatus)
				{
					batchComplete.OnJobStart(job);
				}
				ExecuteTask(job);
			}
            LogExecuteEnd(job);
		}

		protected virtual void ExecuteImport(IEnumerable<FieldMap> fieldMap, DataSourceProviderConfiguration configuration,
			string destinationConfiguration, List<string> entryIDs, SourceProvider sourceProviderRdo,
			DestinationProvider destinationProvider, Job job)
		{
		    LogExecuteImportStart(job);

            FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();

			JobStopManager?.ThrowIfStopRequested();

			IDataSourceProvider sourceProvider = GetSourceProvider(SourceProvider, job);

			JobStopManager?.ThrowIfStopRequested();

			IDataSynchronizer dataSynchronizer = GetDestinationProvider(destinationProvider, destinationConfiguration, job);

			//Obtain settings for destination configuration
			ImportSettings destinationSettings = Serializer.Deserialize<ImportSettings>(destinationConfiguration);

			//Make non-Relativity providers log the document RDOs' created by/modified by field as the user who submitted the job.
			//(Adjustment only needs to be made before IDataSynchronizer.SyncData)
			if (dataSynchronizer is RdoSynchronizer)
			{
				destinationSettings.OnBehalfOfUserId = job.SubmittedBy;
				destinationSettings.CorrelationId = BatchInstance;
				destinationSettings.JobID = job.RootJobId;
				destinationSettings.Provider = IntegrationPoint.GetProviderType(_providerTypeService).ToString();
				destinationConfiguration = Serializer.Serialize(destinationSettings);
			}

			//Extract source fields from field map
			List<FieldEntry> sourceFields = GetSourceFields(fieldMaps);

			using (IDataReader sourceDataReader = sourceProvider.GetData(sourceFields, entryIDs, configuration))
			{
				SetupSubscriptions(dataSynchronizer, job);
				IEnumerable<IDictionary<FieldEntry, object>> sourceData = GetSourceData(sourceFields, sourceDataReader);
				JobStopManager?.ThrowIfStopRequested();
				dataSynchronizer.SyncData(sourceData, fieldMaps, destinationConfiguration);
			}

		    LogExecuteImportSuccesfulEnd(job);
        }

	    protected virtual void ExecuteTask(Job job)
		{
			try
			{
				LogExecuteTaskStart(job);
				
				SetIntegrationPoint(job);

				DeserializeAndSetupIntegrationPointsConfigurationForStatisticsService(IntegrationPoint);

				List<string> entryIDs = GetEntryIDs(job);
				SetJobHistory();

				JobStopManager = ManagerFactory.CreateJobStopManager(JobService, JobHistoryService, BatchInstance, job.JobId,
					_isStoppable);
				JobHistoryErrorService.JobStopManager = JobStopManager;
				
				if (!IntegrationPoint.SourceProvider.HasValue)
				{
					LogUnknownSourceProvider(job);
					throw new ArgumentException("Cannot import source provider with unknown id.");
				}
				if (!IntegrationPoint.DestinationProvider.HasValue)
				{
					LogUnknownDestinationProvider(job);
					throw new ArgumentException("Cannot import destination provider with unknown id.");
				}
				IEnumerable<FieldMap> fieldMap = GetFieldMap(IntegrationPoint.FieldMappings);

				JobStopManager?.ThrowIfStopRequested();

				ExecuteImport(fieldMap, new DataSourceProviderConfiguration(IntegrationPoint.SourceConfiguration, IntegrationPoint.SecuredConfiguration), 
					IntegrationPoint.DestinationConfiguration, entryIDs, SourceProvider, DestinationProvider, job);
				
				LogExecuteTaskSuccesfulEnd(job);
			}
			catch (OperationCanceledException e)
			{
				LogJobStoppedException(job, e);
				// the job has been stopped.
			}
			catch (AuthenticationException e)
			{
				LogAuthenticationException(job, e);
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, e.Message, e.StackTrace);
			}
			catch (Exception ex)
			{
				LogExecutingTaskError(job, ex);
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
				if (ex is IntegrationPointsException)
				{
					throw;
				}
			}
			finally
			{
			    PostExecute(job);
			    LogExecuteTaskFinalize(job);
			}
		}

	    protected virtual List<string> GetEntryIDs(Job job)
		{
			TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
			BatchInstance = taskParameters.BatchInstance;
			if (taskParameters.BatchParameters != null)
			{
				if (taskParameters.BatchParameters is JArray)
				{
					return ((JArray) taskParameters.BatchParameters).ToObject<List<string>>();
				}
				if (taskParameters.BatchParameters is List<string>)
				{
					return (List<string>) taskParameters.BatchParameters;
				}
			}
			return new List<string>();
		}

		protected void PostExecute(Job job)
		{
			try
			{
			    LogPostExecuteStart(job);
                JobStopManager?.Dispose();
				JobHistoryErrorService.CommitErrors();
				
				bool isJobComplete = JobManager.CheckBatchOnJobComplete(job, BatchInstance.ToString());
				if (isJobComplete)
				{
					OnJobComplete(job);
				}
			}
			catch (Exception e)
			{
				LogPostExecuteError(job, e);
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
				if (e is IntegrationPointsException) // we want to rethrow, so it can be added to error tab if necessary
				{
					throw;
				}
			}
			finally
			{
			    JobHistoryErrorService.CommitErrors();
			    LogPostExecuteFinalize(job);
			}
		}

		private void OnJobComplete(Job job)
		{
			try
			{
				if (JobStopManager?.IsStopRequested() == true)
				{
					IList<Job> jobs = JobManager.GetJobsByBatchInstanceId(IntegrationPoint.ArtifactId, BatchInstance);
					if (jobs.Any())
					{
						List<long> ids = jobs.Select(agentJob => agentJob.JobId).ToList();

						LogUpdateStopStateToUnstoppable(ids);
						JobService.UpdateStopState(jobs.Select(agentJob => agentJob.JobId).ToList(), StopState.Unstoppable);
					}
				}
			}
			catch (Exception e)
			{
				LogStatusUpdateError(job, e);
				// IGNORE ERROR. It is possible that the user stop the job in between disposing job history manager and the updating the stop state.
			}

			SetErrorStatusesToExpiredIfStopped(job);

			foreach (IBatchStatus completedItem in BatchStatus)
			{
				try
				{
					completedItem.OnJobComplete(job);
				}
				catch (Exception e)
				{
					LogCompletingJobError(job, e, completedItem);
					JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
					if (e is IntegrationPointsException) // we want to rethrow, so it can be added to error tab if necessary
					{
						throw;
					}
				}
			}

			IList<Job> jobsToUpdate = JobManager.GetJobsByBatchInstanceId(IntegrationPoint.ArtifactId, BatchInstance);
			if (jobsToUpdate.Any())
			{
				List<long> ids = jobsToUpdate.Select(agentJob => agentJob.JobId).ToList();

				LogUpdateStopStateToNone(ids);
				JobService.UpdateStopState(ids, StopState.None);
			}
		}

		protected void SetupJobHistoryErrorSubscriptions(IDataSynchronizer synchronizer)
		{
			JobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		private void SetupStatisticsSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
				_statisticsService.Subscribe(synchronizer as IBatchReporter, job);
		}

		private void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			SetupStatisticsSubscriptions(synchronizer, job);
			SetupJobHistoryErrorSubscriptions(synchronizer);
		}

		private void SetErrorStatusesToExpiredIfStopped(Job job)
		{
			try
			{
				if (JobStopManager?.IsStopRequested() == true)
				{
					IContextContainer contextContainer = ContextContainerFactory.CreateContextContainer(Helper);
					IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager(contextContainer);
					jobHistoryManager.SetErrorStatusesToExpired(CaseServiceContext.WorkspaceID, JobHistory.ArtifactId);
				}
			}
			catch (Exception e)
			{
				LogUpdatingStoppedJobStatusError(job, e);
				// Ignore error. Job history error status only set for the consistency. This will not affect re-running the job.
			}
		}

		private void DeserializeAndSetupIntegrationPointsConfigurationForStatisticsService(IntegrationPoint ip)
		{
			SourceConfiguration sourceConfiguration = null;
			ImportSettings importSettings = null;
			try
			{
				sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(ip?.SourceConfiguration);
				importSettings = Serializer.Deserialize<ImportSettings>(ip?.DestinationConfiguration);
			}
			catch (Exception ex)
			{
				LogDeserializeIntegrationPointsConfigurationForStatisticsServiceWarning(ip, ex);
			}

			SetupIntegrationPointsConfigurationForStatisticsService(sourceConfiguration, importSettings);
		}

		private void SetupIntegrationPointsConfigurationForStatisticsService(SourceConfiguration sourceConfiguration, ImportSettings importSettings)
		{
			if (sourceConfiguration == null || importSettings == null)
			{
				LogSkippingSetupIntegrationPointsConfigurationForStatisticsServiceWarning();
			}

			try
			{
				_statisticsService.SetIntegrationPointConfiguration(importSettings, sourceConfiguration);
			}
			catch (Exception ex)
			{
				LogSetupIntegrationPointsConfigurationForStatisticsServiceError(sourceConfiguration, importSettings, ex);
				throw;
			}
		}

		#region Logging
		private void LogSkippingSetupIntegrationPointsConfigurationForStatisticsServiceWarning()
		{
			_logger.LogWarning("Skipping setup of Integration Point configuration for statistics service.");
		}
		private void LogSetupIntegrationPointsConfigurationForStatisticsServiceError(SourceConfiguration sourceConfiguration, ImportSettings importSettings, Exception ex)
		{
			string msg =
				"Failed to set up integration point configuration for statistics service. SourceConfiguration: {sourceConfiguration}. ImportSettings: {importSettings}";
			_logger.LogError(ex, msg, sourceConfiguration, importSettings);
		}

		private void LogDeserializeIntegrationPointsConfigurationForStatisticsServiceWarning(IntegrationPoint ip, Exception ex)
		{
			string msg =
				"Failed to deserialize integration point configuration for statistics service. SourceConfiguration: {sourceConfiguration}. DestinationConfiguration: {destinationConfiguration}";
			_logger.LogWarning(ex, msg, ip?.SourceConfiguration, ip?.DestinationConfiguration);
		}

		private void LogExecutingTaskError(Job job, Exception ex)
		{
			_logger.LogError(ex, "Failed to execute SyncWorker task for Job ID {JobId}.", job.JobId);
		}

		private void LogAuthenticationException(Job job, AuthenticationException e)
		{
			_logger.LogError(e, "Error occurred during authentication for Job ID {JobId}.", job.JobId);
		}

		private void LogJobStoppedException(Job job, OperationCanceledException e)
		{
			_logger.LogInformation(e, "Job {JobId} has been stopped.", job.JobId);
		}

		private void LogUnknownDestinationProvider(Job job)
		{
			_logger.LogError("Destination provider for Job ID {JobId} is unknown.", job.JobId);
		}

		private void LogUnknownSourceProvider(Job job)
		{
			_logger.LogError("Source provider for Job ID {JobId} is unknown.", job.JobId);
		}

		private void LogPostExecuteError(Job job, Exception e)
		{
			_logger.LogError(e, "Failed to execute PostExecute for job {JobId}.", job.JobId);
		}

		private void LogCompletingJobError(Job job, Exception exception, IBatchStatus batchStatus)
		{
			_logger.LogError(exception, "Failed to complete job {JobId}. Error occured in BatchStatus {BatchStatusType}.", job.JobId, batchStatus.GetType());
		}

		private void LogStatusUpdateError(Job job, Exception e)
		{
			_logger.LogError(e, "Error occurred during updating job {JobId} status in PostExecute.", job.JobId);
		}

		private void LogUpdatingStoppedJobStatusError(Job job, Exception exception)
		{
			_logger.LogError(exception, "Failed to update job ({JobId}) status after job has been stopped.", job.JobId);
		}

	    private void LogExecuteEnd(Job job)
	    {
	        _logger.LogInformation("Finished execution of job in SyncWorker for Job ID: {JobId}", job.JobId);
	    }

	    private void LogExecuteStart(Job job)
	    {
	        _logger.LogInformation("Starting execution of job in SyncWorker for Job ID: {JobId}", job.JobId);
	    }
	    private void LogExecuteImportSuccesfulEnd(Job job)
	    {
	        _logger.LogInformation("Succesfully finished execution of import in SyncWorker for Job ID: {JobId}.", job.JobId);
	    }

	    private void LogExecuteImportStart(Job job)
	    {
	        _logger.LogInformation("Starting execution of import in SyncWorker for Job ID: {JobId}.", job.JobId);
	    }
	    private void LogExecuteTaskFinalize(Job job)
	    {
	        _logger.LogInformation("Finalized execution of task in SyncWorker for Job ID: {JobId}", job.JobId);
	    }
	    private void LogExecuteTaskSuccesfulEnd(Job job)
	    {
	        _logger.LogInformation("Succesfully finished execution of task in SyncWorker for Job ID: {JobId}.", job.JobId);
	    }
        private void LogExecuteTaskStart(Job job)
	    {
	        _logger.LogInformation("Starting execution of task in SyncWorker for Job ID: {JobId}", job.JobId);
	    }
	    private void LogPostExecuteFinalize(Job job)
	    {
	        _logger.LogInformation("Finalized post execute method in SyncWorker for Job ID: {JobId}.", job.JobId);
	    }
	    private void LogUpdateStopStateToNone(List<long> ids)
	    {
	        _logger.LogInformation("Updating stop state to None in SyncWorker Job ID: {ids}.", ids);
	    }
	    private void LogUpdateStopStateToUnstoppable(List<long> ids)
	    {
	        _logger.LogInformation("Updating stop state to Unstoppable in SyncWorker Job ID: {ids}.", ids);
	    }
	    private void LogPostExecuteStart(Job job)
	    {
	        _logger.LogInformation("Starting post execute method in SyncWorker for Job ID: {JobId}.", job.JobId);
	    }
        #endregion
	}
}