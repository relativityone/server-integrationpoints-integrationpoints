using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	[SynchronizedTask]
	public abstract class ServiceManagerBase : ITaskWithJobHistory
	{
		protected IAPILog Logger { get; set; }
		protected IJobService JobService { get; }
		protected ISerializer Serializer { get; }
		protected IJobHistoryService JobHistoryService { get; }
		protected IJobHistoryErrorService JobHistoryErrorService { get; }
		protected IScheduleRuleFactory ScheduleRuleFactory { get; }
		protected IManagerFactory ManagerFactory { get; }
		protected IContextContainer ContextContainer { get; }
		protected IContextContainerFactory ContextContainerFactory { get; }
		protected List<IBatchStatus> BatchStatus { get; }
		protected ICaseServiceContext CaseServiceContext { get; }
		protected IOnBehalfOfUserClaimsPrincipalFactory OnBehalfOfUserClaimsPrincipalFactory { get; }
		protected JobStatisticsService StatisticsService { get; }
		protected ISynchronizerFactory SynchronizerFactory { get; }
		protected IJobStopManager JobStopManager { get; set; }
		private IAgentValidator _agentValidator;
		protected SourceConfiguration SourceConfiguration { get; set; }
		protected ImportSettings ImportSettings { get; set; }
		protected Guid Identifier { get; set; }
		protected TaskResult Result { get; set; }

		protected ServiceManagerBase(IHelper helper,
			IJobService jobService,
			ISerializer serializer,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			IScheduleRuleFactory scheduleRuleFactory,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainerFactory,
			IEnumerable<IBatchStatus> statuses,
			ICaseServiceContext caseServiceContext,
			IOnBehalfOfUserClaimsPrincipalFactory onBehalfOfUserClaimsPrincipalFactory,
			JobStatisticsService statisticsService,
			ISynchronizerFactory synchronizerFactory,
			IAgentValidator agentValidator
			)
		{
			_agentValidator = agentValidator;
			Logger = helper.GetLoggerFactory().GetLogger().ForContext<ServiceManagerBase>();
			JobService = jobService;
			Serializer = serializer;
			JobHistoryService = jobHistoryService;
			JobHistoryErrorService = jobHistoryErrorService;
			ScheduleRuleFactory = scheduleRuleFactory;
			ManagerFactory = managerFactory;
			ContextContainer = contextContainerFactory.CreateContextContainer(helper);
			ContextContainerFactory = contextContainerFactory;
			BatchStatus = statuses.ToList();
			CaseServiceContext = caseServiceContext;
			OnBehalfOfUserClaimsPrincipalFactory = onBehalfOfUserClaimsPrincipalFactory;
			StatisticsService = statisticsService;
			SynchronizerFactory = synchronizerFactory;
			Result = new TaskResult();
		}

		public IntegrationPoint IntegrationPointDto { get; protected set; }
		public JobHistory JobHistory { get; protected set; }
		public List<FieldMap> MappedFields { get; protected set; }
		public SourceProvider SourceProvider { get; protected set; }

		public abstract void Execute(Job job);

		protected abstract void SetupSubscriptions(IDataSynchronizer synchronizer, Job job);

		protected virtual void JobHistoryErrorManagerSetup(Job job) { } //No-op for ImportServiceManager, Overridden in ExportServiceManager

		protected void InitializeService(Job job)
		{
			LogInitializeServiceStart(job);
			LoadIntegrationPointData(job);
			ConfigureBatchInstance(job);
			ConfigureJobStatistics();
			ConfigureJobHistory();
			LoadSourceProvider();
			RunValidation(job);
			SanitizeMappedFields();
			JobHistoryErrorManagerSetup(job);
			ConfigureJobStopManager(job);
			ConfigureBatchExceptions(job);
			LogInitializeServiceEnd(job);
		}

		protected void FinalizeService(Job job)
		{
			LogFinalizeServiceStart(job);

			foreach (IBatchStatus completedItem in BatchStatus)
			{
				try
				{
					completedItem.OnJobComplete(job);
				}
				catch (Exception e)
				{
					LogCompletingJobError(job, e, completedItem);
					Result.Status = TaskStatusEnum.Fail;
					JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
					if (e is IntegrationPointsException) // we want to rethrow, so it can be added to error tab if necessary
					{
						throw;
					}
				}
				finally
				{
					JobHistoryErrorService.CommitErrors();
				}
			}

			if (JobStopManager?.IsStopRequested() == true)
			{
				SetErrorStatusesToExpired(job);
			}

			UpdateIntegrationPointRuntimes(job);
			LogFinalizeServiceEnd(job);
		}

		protected IDataSynchronizer CreateDestinationProvider(string configuration)
		{
			// if you want to create add another synchronizer aka exporter, you may add it here.
			// RDO synchronizer
			var factory = SynchronizerFactory as GeneralWithCustodianRdoSynchronizerFactory;
			if (factory != null)
			{
				factory.SourceProvider = SourceProvider;
			}
			try
			{
				IDataSynchronizer synchronizer = SynchronizerFactory.CreateSynchronizer(
					Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, configuration, IntegrationPointDto.SecuredConfiguration);
				return synchronizer;
			}
			catch (Exception e)
			{
				throw LogCreateDestinationProviderError(e, configuration);
			}
		}

		protected void UpdateJobStatus(JobHistory jobHistory)
		{
			try
			{
				JobHistoryService.UpdateRdo(jobHistory);
			}
			catch (Exception e)
			{
				throw LogUpdateJobStatus(e, jobHistory);
			}
		}

		protected void UpdateJobStatus(Choice state)
		{
			JobHistory.JobStatus = state;
			UpdateJobStatus(JobHistory);
		}

		protected void ThrowNewExceptionIfAny(IEnumerable<Exception> exceptions)
		{
			Exception[] enumerable = exceptions?.ToArray();
			if (enumerable?.Any() ?? false)
			{
				int counter = 0;
				string message = string.Join(Environment.NewLine,
					enumerable.Select(exception => $"{++counter}. {exception.Message}"));

				var ex = new AggregateException(message, enumerable);
				throw ex;
			}
		}

		protected string GetUniqueJobId(Job job)
		{
			return job.JobId + "_" + Identifier;
		}

		protected void UpdateIntegrationPointRuntimes(Job job)
		{
			LogUpdateIntegrationPointRuntimesStart(job);
			try
			{
				IntegrationPointDto.LastRuntimeUTC = DateTime.UtcNow;
				if (job.SerializedScheduleRule != null)
				{
					if (Result.Status == TaskStatusEnum.None)
					{
						Result.Status = TaskStatusEnum.Success;
					}
					JobService.UpdateStopState(new List<long> { job.JobId }, StopState.None);
					IntegrationPointDto.NextScheduledRuntimeUTC = JobService.GetJobNextUtcRunDateTime(job, ScheduleRuleFactory, Result);
				}
				CaseServiceContext.RsapiService.RelativityObjectManager.Update(IntegrationPointDto);
				LogUpdateIntegrationPointRuntimesSuccesfulEnd(job);
			}
			catch (Exception e)
			{
				LogUpdatingIntegrationPointRuntimesError(job, e);
			}
		}


		protected void SetJobStateAsUnstoppable(Job job)
		{
			try
			{
				JobStopManager?.Dispose();
				LogSetJobStateAsUnstoppable(job);
				JobService.UpdateStopState(new List<long> { job.JobId }, StopState.Unstoppable);
			}
			catch (Exception e)
			{
				LogSettingJobAsUnstoppableError(job, e);
				// Do not throw exception, we will need to dispose the rest of the objects.
			}
		}

		private void SetErrorStatusesToExpired(Job job)
		{
			try
			{
				IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager(ContextContainer);
				jobHistoryManager.SetErrorStatusesToExpired(CaseServiceContext.WorkspaceID, JobHistory.ArtifactId);
			}
			catch (Exception e)
			{
				LogUpdatingStoppedJobStatusError(job, e);
				// ignore error. the status of errors will not affect the 'retry' nor the 'run' scenarios.
			}
		}

		private void LoadIntegrationPointData(Job job)
		{
			IntegrationPointDto = LoadIntegrationPointDto(job);
			SourceConfiguration = Serializer.Deserialize<SourceConfiguration>(IntegrationPointDto.SourceConfiguration);
			ImportSettings = Serializer.Deserialize<ImportSettings>(IntegrationPointDto.DestinationConfiguration);
			JobHistoryErrorService.IntegrationPoint = IntegrationPointDto;
		}

		private IntegrationPoint LoadIntegrationPointDto(Job job)
		{
			LogLoadInformationPointDtoStart(job);

			int integrationPointId = job.RelatedObjectArtifactID;
			IntegrationPoint integrationPoint = CaseServiceContext.RsapiService.RelativityObjectManager.Read<IntegrationPoint>(integrationPointId);
			if (integrationPoint == null)
			{
				LogLoadingIntegrationPointDtoError(job);
				throw new ArgumentException("Failed to retrieve corresponding Integration Point.");
			}
			LogLoadIntegrationPointDtoSuccesfulEnd(job);
			return integrationPoint;
		}

		private void ConfigureBatchInstance(Job job)
		{
			if (string.IsNullOrWhiteSpace(job.JobDetails))
			{
				var taskParameters = new TaskParameters
				{
					BatchInstance = Guid.NewGuid()
				};
				Identifier = taskParameters.BatchInstance;
				job.JobDetails = Serializer.Serialize(taskParameters);
			}
			else
			{
				TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
				Identifier = taskParameters.BatchInstance;
			}
		}
		private void ConfigureJobStatistics()
		{
			if (StatisticsService != null)
			{
				StatisticsService.IntegrationPointSourceConfiguration = SourceConfiguration;
				StatisticsService.IntegrationPointImportSettings = ImportSettings;
			}
		}

		private void ConfigureJobHistory()
		{
			JobHistory = JobHistoryService.GetOrCreateScheduledRunHistoryRdo(IntegrationPointDto, Identifier, DateTime.UtcNow);

			JobHistoryErrorService.JobHistory = JobHistory;
			JobHistory.StartTimeUTC = DateTime.UtcNow;
		}

		private void LoadSourceProvider()
		{
			LogLoadSourceProviderStart();
			SourceProvider = CaseServiceContext.RsapiService.RelativityObjectManager.Read<SourceProvider>(IntegrationPointDto.SourceProvider.Value);
			LogLoadSourceProviderEnd();
		}

		private void SanitizeMappedFields()
		{
			MappedFields = Serializer.Deserialize<List<FieldMap>>(IntegrationPointDto.FieldMappings);
			MappedFields.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);
		}

		private void ConfigureJobStopManager(Job job)
		{
			JobStopManager = ManagerFactory.CreateJobStopManager(JobService, JobHistoryService, Identifier, job.JobId, true);
			JobHistoryErrorService.JobStopManager = JobStopManager;
		}

		private void ConfigureBatchExceptions(Job job)
		{
			LogConfigureBatchExceptionsStart(job);
			var exceptions = new List<Exception>();
			BatchStatus.ForEach(batch =>
			{
				try
				{
					batch.OnJobStart(job);
				}
				catch (Exception exception)
				{
					Result.Status = TaskStatusEnum.Fail;
					JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, exception);
					exceptions.Add(exception);
				}
			});

			LogBatchExceptions(job, exceptions);
			ThrowNewExceptionIfAny(exceptions);
			LogConfigureBatchExceptionsSuccesfulEnd(job);
		}

		protected void HandleGenericException(Exception ex, Job job)
		{
			LogExecutingTaskError(job, ex);
			Result.Status = TaskStatusEnum.Fail;
			JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			if (ex is PermissionException || ex is IntegrationPointProviderValidationException)
			{
				UpdateJobStatus(JobStatusChoices.JobHistoryValidationFailed);
			}
		}

		private void RunValidation(Job job)
		{
			UpdateJobStatus(JobStatusChoices.JobHistoryValidating);
			_agentValidator.Validate(IntegrationPointDto, job.SubmittedBy);
		}

		#region Logging

		protected void LogBatchExceptions(Job job, IEnumerable<Exception> exceptions)
		{
			foreach (Exception ex in exceptions)
			{
				Logger.LogError(ex, "There was a problem while configuring batch exceptions for job: {JobId}.", job.JobId);
			}
		}

		protected virtual void LogJobStoppedException(Job job, OperationCanceledException e)
		{
			Logger.LogInformation(e, "Job {JobId} has been stopped.", job.JobId);
		}

		protected virtual void LogExecutingTaskError(Job job, Exception ex)
		{
			Logger.LogError(ex, "Failed to execute task for job {JobId}.", job.JobId);
		}

		protected virtual void LogSettingJobAsUnstoppableError(Job job, Exception e)
		{
			Logger.LogError(e, "Failed to set job state as unstoppable for job {JobId}.", job.JobId);
		}

		protected virtual void LogUpdatingStoppedJobStatusError(Job job, Exception exception)
		{
			Logger.LogError(exception, "Failed to update job ({JobId}) status after job has been stopped.", job.JobId);
		}

		protected virtual void LogCompletingJobError(Job job, Exception exception, IBatchStatus batchStatus)
		{
			Logger.LogError(exception, "Failed to complete job {JobId}. Error occured in BatchStatus {BatchStatusType}.", job.JobId, batchStatus.GetType());
		}

		protected virtual void LogDisposingObserverError(Job job, Exception e)
		{
			Logger.LogError(e, "Failed to dispose observer for job {JobId}.", job.JobId);
		}

		protected virtual void LogUpdatingIntegrationPointRuntimesError(Job job, Exception e)
		{
			Logger.LogError(e, "Failed to update Integration Point runtimes for job {JobId}.", job.JobId);
		}

		protected virtual void LogLoadingIntegrationPointDtoError(Job job)
		{
			Logger.LogError("Failed to retrieve corresponding Integration Point ({IntegrationPointId}) for job {JobId}.", job.RelatedObjectArtifactID, job.JobId);
		}

		private void LogInitializeServiceEnd(Job job)
		{
			Logger.LogInformation("Finished initializing service for job: {JobId}", job.JobId);
		}

		private void LogInitializeServiceStart(Job job)
		{
			Logger.LogInformation("Initializing service for job: {JobId}", job.JobId);
		}

		private void LogFinalizeServiceEnd(Job job)
		{
			Logger.LogInformation("Finalized service for job: {JobId}", job.JobId);
		}

		private void LogFinalizeServiceStart(Job job)
		{
			Logger.LogInformation("Started finalizing service for job: {JobId}", job.JobId);
		}

		private void LogLoadIntegrationPointDtoSuccesfulEnd(Job job)
		{
			Logger.LogInformation("Succesfully loaded integration point DTO for job : {JobId}", job.JobId);
		}

		private void LogLoadInformationPointDtoStart(Job job)
		{
			Logger.LogInformation("Loading integration point DTO for job: {JobId}", job.JobId);
		}


		private void LogUpdateIntegrationPointRuntimesSuccesfulEnd(Job job)
		{
			Logger.LogInformation("Succesfully updated integration point runtimes for job: {JobId}", job.JobId);
		}

		private void LogUpdateIntegrationPointRuntimesStart(Job job)
		{
			Logger.LogInformation("Trying to update integration point runtimes for job: {JobId}", job.JobId);
		}

		private void LogSetJobStateAsUnstoppable(Job job)
		{
			Logger.LogInformation("Updating job state to Unstoppable, job: {JobId}", job.JobId);
		}

		private void LogLoadSourceProviderStart()
		{
			Logger.LogInformation("Loading source provider in Service Manager Base.");
		}

		private void LogLoadSourceProviderEnd()
		{
			Logger.LogInformation("Finished loading source provider in Service Manager Base.");
		}

		private void LogConfigureBatchExceptionsSuccesfulEnd(Job job)
		{
			Logger.LogInformation("Succesfully configured batch exceptions for job: {JobId}", job.JobId);
		}

		private void LogConfigureBatchExceptionsStart(Job job)
		{
			Logger.LogInformation("Started configuring batch exceptions for job: {JobId}", job.JobId);
		}

		private IntegrationPointsException LogCreateDestinationProviderError(Exception e, string configuration)
		{
			var message = $"Error ocurred when creating destination provider for configuration: {configuration}";
			var template = "Error ocurred when creating destination provider for configuration: {configuration}";
			var exc = new IntegrationPointsException(message, e);
			Logger.LogError(exc, template, configuration);
			return exc;
		}

		private IntegrationPointsException LogUpdateJobStatus(Exception e, JobHistory jobHistory)
		{
			var message = "Error ocurred when updating job status";
			var template = "Error ocurred when updating job status. jobHistory: {@jobHistory}"; 
			var exc = new IntegrationPointsException(message, e);
			Logger.LogError(exc, template, jobHistory);
			return exc;
		}

		#endregion
	}
}