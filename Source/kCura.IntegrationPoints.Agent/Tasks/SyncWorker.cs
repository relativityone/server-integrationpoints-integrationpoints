using System;
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
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Injection;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json.Linq;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.MetricsCollection;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncWorker : IntegrationPointTaskBase, ITask
	{
		private readonly bool _isStoppable;
		private readonly IAPILog _logger;
		private readonly JobStatisticsService _statisticsService;
		private IEnumerable<IBatchStatus> _batchStatus;

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
			IJobService jobService) :
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
				jobService, true)
		{
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
			IJobService jobService, bool isStoppable) :
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
			using (Client.MetricsClient.LogDuration(
				Constants.IntegrationPoints.Telemetry.BUCKET_SYNC_WORKER_EXEC_DURATION_METRIC_COLLECTOR, Guid.Empty, MetricTargets.APMandSUM))
			{
				foreach (IBatchStatus batchComplete in BatchStatus)
				{
					batchComplete.OnJobStart(job);
				}
				ExecuteTask(job);
			}
		}

		protected virtual void ExecuteImport(IEnumerable<FieldMap> fieldMap,
			string sourceConfiguration, string destinationConfiguration, List<string> entryIDs,
			SourceProvider sourceProviderRdo, DestinationProvider destinationProvider, Job job)
		{
			FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();

			JobStopManager?.ThrowIfStopRequested();

			IDataSourceProvider sourceProvider = GetSourceProvider(SourceProvider, job);

			JobStopManager?.ThrowIfStopRequested();

			List<FieldEntry> sourceFields = GetSourceFields(fieldMaps);

            using (IDataReader importDataReader = new ImportDataReader(
                    fieldMaps,
                    sourceProvider,
                    sourceFields,
                    entryIDs,
                    sourceConfiguration,
                    Helper.GetLoggerFactory().GetLogger().ForContext<ImportDataReader>()))
            {
                IDataSynchronizer dataSynchronizer = GetDestinationProvider(destinationProvider, destinationConfiguration, job);
                if (dataSynchronizer is RdoSynchronizerBase)
                {
                    ImportSettings settings = Serializer.Deserialize<ImportSettings>(destinationConfiguration);
                    settings.OnBehalfOfUserId = job.SubmittedBy;
                    destinationConfiguration = Serializer.Serialize(settings);
                }

                SetupSubscriptions(dataSynchronizer, job);

                JobStopManager?.ThrowIfStopRequested();

                dataSynchronizer.SyncData(importDataReader, fieldMaps, destinationConfiguration);
            }
		}

		protected virtual void ExecuteTask(Job job)
		{
			try
			{
				InjectionManager.Instance.Evaluate("640E9695-AB99-4763-ADC5-03E1252277F7");

				SetIntegrationPoint(job);
				List<string> entryIDs = GetEntryIDs(job);
				SetJobHistory();

				JobStopManager = ManagerFactory.CreateJobStopManager(JobService, JobHistoryService, BatchInstance, job.JobId, _isStoppable);
				JobHistoryErrorService.JobStopManager = JobStopManager;

				InjectionManager.Instance.Evaluate("CB070ADB-8912-4B61-99B0-3321C0670FC6");

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

				ExecuteImport(fieldMap, IntegrationPoint.SourceConfiguration, IntegrationPoint.DestinationConfiguration, entryIDs, SourceProvider, DestinationProvider, job);

				InjectErrors();
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
			}
			finally
			{
				PostExecute(job);
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
				JobStopManager?.Dispose();
				JobHistoryErrorService.CommitErrors();

				bool isJobComplete = JobManager.CheckBatchOnJobComplete(job, BatchInstance.ToString());

				if (isJobComplete)
				{
					try
					{
						if (JobStopManager?.IsStopRequested() == true)
						{
							IList<Job> jobs = JobManager.GetJobsByBatchInstanceId(IntegrationPoint.ArtifactId, BatchInstance);
							if (jobs.Any())
							{
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
						}
					}

					IList<Job> jobsToUpdate = JobManager.GetJobsByBatchInstanceId(IntegrationPoint.ArtifactId, BatchInstance);
					if (jobsToUpdate.Any())
					{
						JobService.UpdateStopState(jobsToUpdate.Select(agentJob => agentJob.JobId).ToList(), StopState.None);
					}
				}
			}
			catch (Exception e)
			{
				LogPostExecuteError(job, e);
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
			}
			finally
			{
				JobHistoryErrorService.CommitErrors();
			}
		}

		protected void SetupJobHistoryErrorSubscriptions(IDataSynchronizer synchronizer)
		{
			JobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		private void SetupStatisticsSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			_statisticsService?.Subscribe(synchronizer as IBatchReporter, job);
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

		private void InjectErrors()
		{
			try
			{
				InjectionManager.Instance.Evaluate("DFE4D63C-3A6A-49C2-A80D-25CA60F2B31C");
			}
			catch (Exception ex)
			{
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}

			try
			{
				InjectionManager.Instance.Evaluate("40af620b-af2e-4b50-9f62-870654819df6");
			}
			catch (Exception ex)
			{
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyUniqueIdentifier", ex.Message, ex.StackTrace);
			}
		}

		#region Logging

		private void LogExecutingTaskError(Job job, Exception ex)
		{
			_logger.LogError(ex, "Failed to execute SyncWorker task for job {JobId}.", job.JobId);
		}

		private void LogAuthenticationException(Job job, AuthenticationException e)
		{
			_logger.LogError(e, "Error occurred during authentication for job {JobId}.", job.JobId);
		}

		private void LogJobStoppedException(Job job, OperationCanceledException e)
		{
			_logger.LogInformation(e, "Job {JobId} has been stopped.", job.JobId);
		}

		private void LogUnknownDestinationProvider(Job job)
		{
			_logger.LogError("Destination provider for job {JobId} is unknown.", job.JobId);
		}

		private void LogUnknownSourceProvider(Job job)
		{
			_logger.LogError("Source provider for job {JobId} is unknown.", job.JobId);
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

		#endregion
	}
}