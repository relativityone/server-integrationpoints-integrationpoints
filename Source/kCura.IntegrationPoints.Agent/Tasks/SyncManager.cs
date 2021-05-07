using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.BatchProcess;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Telemetry.MetricsCollection;
using APMClient = Relativity.Telemetry.APM.Client;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	[SynchronizedTask]
	public class SyncManager : BatchManagerBase<string>, ITaskWithJobHistory
	{
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IGuidService _guidService;
		private readonly IJobHistoryErrorService _jobHistoryErrorService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobManager _jobManager;
		private readonly IJobService _jobService;
		private readonly IAPILog _logger;
		private readonly IDataProviderFactory _providerFactory;
		private readonly IScheduleRuleFactory _scheduleRuleFactory;

		protected IIntegrationPointService IntegrationPointService { get; }
		protected readonly IHelper Helper;
		protected readonly IManagerFactory ManagerFactory;
		protected readonly ISerializer Serializer;
		private IEnumerable<IBatchStatus> _batchStatus;
		private readonly IAgentValidator _agentValidator;

		public SyncManager(
			ICaseServiceContext caseServiceContext,
			IDataProviderFactory providerFactory,
			IJobManager jobManager,
			IJobService jobService,
			IHelper helper,
			IIntegrationPointService integrationPointService,
			ISerializer serializer,
			IGuidService guidService,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			IScheduleRuleFactory scheduleRuleFactory,
			IManagerFactory managerFactory,
			IEnumerable<IBatchStatus> batchStatuses,
			IAgentValidator agentValidator) : base(helper)
		{
			_caseServiceContext = caseServiceContext;
			_providerFactory = providerFactory;
			_jobManager = jobManager;
			_jobService = jobService;
			Helper = helper;
			IntegrationPointService = integrationPointService;
			Serializer = serializer;
			_guidService = guidService;
			_jobHistoryService = jobHistoryService;
			_jobHistoryErrorService = jobHistoryErrorService;
			_scheduleRuleFactory = scheduleRuleFactory;
			ManagerFactory = managerFactory;
			RaiseJobPreExecute += JobPreExecute;
			RaiseJobPostExecute += JobPostExecute;
			BatchJobCount = 0;
			BatchInstance = Guid.NewGuid();
			_batchStatus = batchStatuses;
			_agentValidator = agentValidator;
			_logger = Helper.GetLoggerFactory().GetLogger().ForContext<SyncManager>();
		}

		public IEnumerable<IBatchStatus> BatchStatus
		{
			get { return _batchStatus ?? (_batchStatus = new List<IBatchStatus>()); }
		}

		public IntegrationPoint IntegrationPoint { get; set; }
		public JobHistory JobHistory { get; set; }
		public IJobStopManager JobStopManager { get; set; }
		public Guid BatchInstance { get; set; }
		public int BatchJobCount { get; set; }

		public override int BatchSize => Config.Config.Instance.BatchSize;

		public override IEnumerable<string> GetUnbatchedIDs(Job job)
		{
			LogGetUnbatchedIds(job);
			try
			{
				if (string.IsNullOrEmpty(job.JobDetails))
				{
					//job is scheduled so give it the same look as import now
					var details = new TaskParameters
					{
						BatchInstance = BatchInstance
					};
					job.JobDetails = Serializer.Serialize(details);
				}

				OnJobStart(job);

				JobStopManager?.ThrowIfStopRequested();

				SourceProvider sourceProviderRdo = _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(IntegrationPoint.SourceProvider.Value);
				Guid applicationGuid = new Guid(sourceProviderRdo.ApplicationIdentifier);
				Guid providerGuid = new Guid(sourceProviderRdo.Identifier);
				IDataSourceProvider provider = _providerFactory.GetDataProvider(applicationGuid, providerGuid);

				JobStopManager?.ThrowIfStopRequested();

				FieldEntry idField = IntegrationPointService.GetIdentifierFieldEntry(IntegrationPoint.FieldMappings);
				IDataReader idReader = provider.GetBatchableIds(idField, new DataSourceProviderConfiguration(IntegrationPoint.SourceConfiguration, IntegrationPoint.SecuredConfiguration));

				JobStopManager?.ThrowIfStopRequested();
				return new ReaderEnumerable(idReader, JobStopManager);
			}
			catch (OperationCanceledException e)
			{
				LogJobStoppedException(job, e);
				JobStopManager?.Dispose();
				throw;
				// DO NOTHING. Someone attempted to stop the job.
			}
			catch (Exception ex)
			{
				LogRetrieveingUnbatchedIDsError(job, ex);
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
				if (ex is IntegrationPointsException) // we want to rethrow, so it can be added to error tab if necessary
				{
					throw;
				}
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
				LogGetUnbatchedIdsFinalize(job);
			}
			return new List<string>();
		}
		public override void CreateBatchJob(Job job, List<string> batchIDs)
		{
			LogCreateBatchJobStart(job, batchIDs);

			JobStopManager?.ThrowIfStopRequested();

			TaskParameters taskParameters = new TaskParameters
			{
				BatchInstance = BatchInstance,
				BatchParameters = batchIDs
			};
			_jobManager.CreateJobWithTracker(job, taskParameters, GetTaskType(), BatchInstance.ToString());
			BatchJobCount++;
			LogCreateBatchJobEnd(job, batchIDs);
		}

		protected virtual TaskType GetTaskType()
		{
			return TaskType.SyncWorker;
		}

		private void JobPreExecute(Job job, TaskResult taskResult)
		{
			try
			{
				LogJobPreExecuteStart(job);
				SetupJob(job);
				ValidateJob(job);
				LogJobPreExecuteSuccesfulEnd(job);
			}
			catch (OperationCanceledException e)
			{
				LogJobStoppedException(job, e);
				JobStopManager.Dispose();
				throw;
				// DO NOTHING. Someone attempted to stop the job.
			}
			catch (Exception ex)
			{
				AgentExceptionHelper.HandleException(_jobHistoryErrorService, _jobHistoryService, _logger, ex, job, taskResult, JobHistory);
				if (ex is IntegrationPointsException || ex is IntegrationPointValidationException || ex is PermissionException) // we want to rethrow, so it can be added to error tab if necessary
				{
					throw;
				}
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
				LogJobPreExecuteFinalize(job);
			}
		}

		protected void OnJobStart(Job job)
		{
			foreach (var batchStatus in BatchStatus)
			{
				batchStatus.OnJobStart(job);
			}
		}

		private void SetupJob(Job job)
		{
			BatchInstance = GetBatchInstance(job);
			if (job.RelatedObjectArtifactID < 1)
			{
				LogMissingJobRelatedObject(job);
				throw new ArgumentNullException("Job must have a Related Object ArtifactID");
			}

			IntegrationPoint = IntegrationPointService.ReadIntegrationPoint(job.RelatedObjectArtifactID);
			if (IntegrationPoint.SourceProvider == 0)
			{
				LogUnknownSourceProvider(job);
				throw new Exception("Cannot import source provider with unknown id.");
			}

			JobHistory = _jobHistoryService.GetOrCreateScheduledRunHistoryRdo(IntegrationPoint, BatchInstance, DateTime.UtcNow);
			_jobHistoryErrorService.JobHistory = JobHistory;
			_jobHistoryErrorService.IntegrationPoint = IntegrationPoint;
			
			JobStopManager = ManagerFactory.CreateJobStopManager(_jobService, _jobHistoryService, BatchInstance, job.JobId, true);
			JobStopManager.ThrowIfStopRequested();

			if (!JobHistory.StartTimeUTC.HasValue)
			{
				JobHistory.StartTimeUTC = DateTime.UtcNow;
				_jobHistoryService.UpdateRdo(JobHistory);
			}
		}

		private void ValidateJob(Job job)
		{
			JobHistory.JobStatus = JobStatusChoices.JobHistoryValidating;
			_jobHistoryService.UpdateRdo(JobHistory);
			_agentValidator.Validate(IntegrationPoint, job.SubmittedBy);
		}

		private void JobPostExecute(Job job, TaskResult taskResult, long items)
		{
			try
			{
				LogJobPostExecuteStart(job);
				List<Exception> exceptions = new List<Exception>();
				try
				{
					UpdateLastRuntimeAndCalculateNextRuntime(job, taskResult);
				}
				catch (Exception exception)
				{
					LogUpdateOrCalculateRuntimeError(job, exception);
					exceptions.Add(exception);
				}

				if (JobHistory != null)
				{
					if (BatchJobCount == 0)
					{
						try
						{
							FinalizeJob(job);
						}
						catch (Exception exception)
						{
							LogFinalizingJobError(job, exception);
							exceptions.Add(exception);
						}
					}

					try
					{
						UpdateJobHistoryTotalItems(job, items);
					}
					catch (Exception exception)
					{
						exceptions.Add(exception);
					}
				}

				if (exceptions.Any())
				{
					throw new AggregateException(exceptions);
				}
				LogJobPostExecuteSuccesfulEnd(job);
			}
			catch (Exception ex)
			{
				LogPostExecuteAggregatedError(job, ex);
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, new Exception("Failed to update job statistics.", ex));
				if (ex is IntegrationPointsException) // we want to rethrow, so it can be added to error tab if necessary
				{
					throw;
				}
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
				LogJobPostExecuteFinalize(job);
			}
		}

		private void UpdateJobHistoryTotalItems(Job job, long items)
		{
			// TODO we should not update JobHistory here, it is not SyncManager responsibility
			try
			{
				JobHistory jobHistory = _jobHistoryService.GetRdo(BatchInstance);
				jobHistory.TotalItems = items;
				_jobHistoryService.UpdateRdo(jobHistory);
			}
			catch (Exception exception)
			{
				LogUpdatingJobHistoryError(job, exception);
				throw;
			}
		}

		private void UpdateStopState(Job job)
		{
			if (job.SerializedScheduleRule != null)
			{
				LogUpdateStopState(job);
				_jobService.UpdateStopState(new List<long> { job.JobId }, StopState.None);
			}
		}

		private void FinalizeJob(Job job)
		{
			LogFinalizeJobStart(job);
			List<Exception> exceptions = new List<Exception>();

			try
			{
				UpdateStopState(job);
			}
			catch (Exception exception)
			{
				LogUpdatingStopStateError(job, exception);
				exceptions.Add(exception);
			}

			foreach (var batchStatus in BatchStatus)
			{
				try
				{
					batchStatus.OnJobComplete(job);
				}
				catch (Exception exception)
				{
					LogCompletingJobError(job, exception, batchStatus);
					exceptions.Add(exception);
				}
			}

			try
			{
				if ((JobHistory != null) && JobStopManager.IsStopRequested())
				{
					IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager();
					jobHistoryManager.SetErrorStatusesToExpired(_caseServiceContext.WorkspaceID, JobHistory.ArtifactId);
				}
			}
			catch (Exception exception)
			{
				LogUpdatingStoppedJobStatusError(job, exception);
				exceptions.Add(exception);
			}

			if (exceptions.Any())
			{
				throw new AggregateException("Failed to finalize the job.", exceptions);
			}
			LogFinalizeJobSuccesfulEnd(job);
		}

		private void UpdateLastRuntimeAndCalculateNextRuntime(Job job, TaskResult taskResult)
		{
			LogUpdateLastRuntimeAndCalculateNextRuntimeStart(job);
			IntegrationPoint.LastRuntimeUTC = DateTime.UtcNow;
			if (job.SerializedScheduleRule != null)
			{
				IntegrationPoint.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory, taskResult);
			}
			IntegrationPointService.UpdateIntegrationPoint(IntegrationPoint);
			LogUpdateLastRuntimeAndCalculateNextRuntimeSuccesfulEnd(job);
		}

		public Guid GetBatchInstance(Job job)
		{
			return new TaskParameterHelper(Serializer, _guidService).GetBatchInstance(job);
		}

		public override void Execute(Job job)
		{
			LogExecuteStart(job);

			base.Execute(job);

			LogExecuteEnd(job);
		}

		private class ReaderEnumerable : IEnumerable<string>, IDisposable
		{
			private readonly IJobStopManager _jobStopManager;
			private readonly IDataReader _reader;

			public ReaderEnumerable(IDataReader reader, IJobStopManager jobStopManager)
			{
				_reader = reader;
				_jobStopManager = jobStopManager;
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			public IEnumerator<string> GetEnumerator()
			{
				while (_reader.Read())
				{
					_jobStopManager?.ThrowIfStopRequested();

					var result = _reader.GetString(0);
					yield return result;
				}
				Dispose();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			private void Dispose(bool disposing)
			{
				if (disposing)
				{
					_jobStopManager?.Dispose();
					_reader?.Dispose();
				}
			}
		}

		#region Logging

		private void LogRetrieveingUnbatchedIDsError(Job job, Exception ex)
		{
			_logger.LogError(ex, "Failed to get unbatched ids for job {JobId}.", job.JobId);
		}

		private void LogJobStoppedException(Job job, OperationCanceledException e)
		{
			_logger.LogInformation(e, "Job {JobId} has been stopped.", job.JobId);
		}

		private void LogUnknownSourceProvider(Job job)
		{
			_logger.LogError("Missing source provider for Job {JobId}.", job.JobId);
		}

		private void LogMissingJobRelatedObject(Job job)
		{
			_logger.LogError("Job ({JobId}) must have a Related Object ArtifactID.", job.JobId);
		}

		private void LogPostExecuteAggregatedError(Job job, Exception ex)
		{
			_logger.LogError(ex, "JobPostExecute failed for job {JobId}.", job.JobId);
		}

		private void LogUpdatingJobHistoryError(Job job, Exception exception)
		{
			_logger.LogError(exception, "Failed to update job history ({JobId}).", job.JobId);
		}

		private void LogFinalizingJobError(Job job, Exception exception)
		{
			_logger.LogError(exception, "Failed to finalize job {JobId}.", job.JobId);
		}

		private void LogUpdateOrCalculateRuntimeError(Job job, Exception exception)
		{
			_logger.LogError(exception, "Failed to update last runtime or calculate next runtime for job {JobId}.", job.JobId);
		}

		private void LogUpdatingStoppedJobStatusError(Job job, Exception exception)
		{
			_logger.LogError(exception, "Failed to update job ({JobId}) status after job has been stopped.", job.JobId);
		}

		private void LogCompletingJobError(Job job, Exception exception, IBatchStatus batchStatus)
		{
			_logger.LogError(exception, "Failed to complete job {JobId}. Error occured in BatchStatus {BatchStatusType}.", job.JobId, batchStatus.GetType());
		}

		private void LogUpdatingStopStateError(Job job, Exception exception)
		{
			_logger.LogError(exception, "Failed to update stop state for job {JobId}.", job.JobId);
		}

		private void LogGetUnbatchedIds(Job job)
		{
			_logger.LogInformation("Started getting unbatched IDs in SyncManager for job: {JobId}.", job.JobId);
		}

		private void LogGetUnbatchedIdsFinalize(Job job)
		{
			_logger.LogInformation("Finalized getting unbatched IDs for job: {JobId}.", job.JobId);
		}
		private void LogCreateBatchJobEnd(Job job, List<string> batchIDs)
		{
			_logger.LogInformation("Finished creating batch job: {Job}, batchIDs count {count}.", job, batchIDs.Count);
		}
		private void LogCreateBatchJobStart(Job job, List<string> batchIDs)
		{
			_logger.LogInformation("Started creating batch job: {Job}, batchIDs count {count}.", job, batchIDs.Count);
		}
		private void LogJobPreExecuteStart(Job job)
		{
			_logger.LogInformation("Starting pre execute in SyncManager for job: {JobId}.", job.JobId);
		}
		private void LogJobPreExecuteSuccesfulEnd(Job job)
		{
			_logger.LogInformation("Succesfully finished pre execute in SyncManager for: {JobId}.", job.JobId);
		}
		private void LogJobPreExecuteFinalize(Job job)
		{
			_logger.LogInformation("Finished pre execute in SyncManager for: {JobId}.", job.JobId);
		}
		private void LogJobPostExecuteStart(Job job)
		{
			_logger.LogInformation("Starting post execute in SyncManager for job: {JobId}.", job.JobId);
		}

		private void LogJobPostExecuteFinalize(Job job)
		{
			_logger.LogInformation("Finalized post execute in SyncManager for job: {JobId}.", job.JobId);
		}
		private void LogJobPostExecuteSuccesfulEnd(Job job)
		{
			_logger.LogInformation("Succesfully finished post execute in SyncManager for job: {JobId}.", job.JobId);
		}
		private void LogUpdateStopState(Job job)
		{
			_logger.LogInformation("Updating stop state in SyncManager to None for job: {JobId}.", job.JobId);
		}

		private void LogFinalizeJobSuccesfulEnd(Job job)
		{
			_logger.LogInformation("Succesfully finished finalization method in SyncManager for job: {JobId}.", job.JobId);
		}
		private void LogFinalizeJobStart(Job job)
		{
			_logger.LogInformation("Starting Finalize Job method in SyncManager for job: {JobId}.", job.JobId);
		}
		private void LogUpdateLastRuntimeAndCalculateNextRuntimeSuccesfulEnd(Job job)
		{
			_logger.LogInformation("Succesfully finished updating last runtime and calculated next runtime for job: {JobId}.", job.JobId);
		}
		private void LogUpdateLastRuntimeAndCalculateNextRuntimeStart(Job job)
		{
			_logger.LogInformation("Started updating last runtime and calculating next runtime for job: {JobId}.", job.JobId);
		}
		private void LogExecuteEnd(Job job)
		{
			_logger.LogInformation("Finished execution of job in SyncManager: {JobId}.", job.JobId);
		}
		private void LogExecuteStart(Job job)
		{
			_logger.LogInformation("Starting execution of job in SyncManager: {JobId}.", job.JobId);
		}

		#endregion
	}
}