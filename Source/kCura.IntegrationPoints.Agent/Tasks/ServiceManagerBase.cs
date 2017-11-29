using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	[SynchronizedTask]
	public abstract class ServiceManagerBase : ITask
	{
		protected IAPILog Logger { get; set; }
		protected IJobService JobService { get; private set; }
		protected ISerializer Serializer { get; private set; }
		protected IJobHistoryService JobHistoryService { get; private set; }
		protected IJobHistoryErrorService JobHistoryErrorService { get; private set; }
		protected IScheduleRuleFactory ScheduleRuleFactory { get; private set; }
		protected IManagerFactory ManagerFactory { get; private set; }
		protected IContextContainer ContextContainer { get; private set; }
		protected IContextContainerFactory ContextContainerFactory { get; private set; }
		protected List<IBatchStatus> BatchStatus { get; private set; }
		protected ICaseServiceContext CaseServiceContext { get; private set; }
		protected IOnBehalfOfUserClaimsPrincipalFactory OnBehalfOfUserClaimsPrincipalFactory { get; private set; }
		protected JobStatisticsService StatisticsService { get; private set; }
		protected ISynchronizerFactory SynchronizerFactory { get; private set; }
		protected IJobStopManager JobStopManager { get; set; }
		protected SourceConfiguration SourceConfiguration { get; set; }
		protected ImportSettings ImportSettings { get; set; }
		protected Guid Identifier { get; set; }
		protected TaskResult Result { get; set; }

		public ServiceManagerBase(IHelper helper,
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
			ISynchronizerFactory synchronizerFactory
			)
		{
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
		public JobHistory JobHistoryDto { get; protected set; }
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
			UpdateJobStatus();
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
				try
				{
					IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager(ContextContainer);
					jobHistoryManager.SetErrorStatusesToExpired(CaseServiceContext.WorkspaceID, JobHistoryDto.ArtifactId);
				}
				catch (Exception e)
				{
					LogUpdatingStoppedJobStatusError(job, e);
					// ignore error. the status of errors will not affect the 'retry' nor the 'run' scenarios.
				}
			}
			UpdateIntegrationPointRuntimes(job);
		    LogFinalizeServiceEnd(job);
        }



	    protected IDataSynchronizer CreateDestinationProvider(string configuration)
		{
			// if you want to create add another synchronizer aka exporter, you may add it here.
			// RDO synchronizer
			GeneralWithCustodianRdoSynchronizerFactory factory = SynchronizerFactory as GeneralWithCustodianRdoSynchronizerFactory;
			if (factory != null)
			{
				factory.SourceProvider = SourceProvider;
			}
			IDataSynchronizer synchronizer = SynchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, configuration, IntegrationPointDto.SecuredConfiguration);
			return synchronizer;
		}

		protected void UpdateJobStatus()
		{
			JobHistoryService.UpdateRdo(JobHistoryDto);
		}

		protected void ThrowNewExceptionIfAny(IEnumerable<Exception> exceptions)
		{
			if (exceptions != null)
			{
				Exception ex = null;
				Exception[] enumerable = exceptions as Exception[] ?? exceptions.ToArray();
				if (!enumerable.IsNullOrEmpty())
				{
					int counter = 0;
					string message = string.Join(Environment.NewLine,
						enumerable.Select(exception => $"{++counter}. {exception.Message}"));
					ex = new AggregateException(message, enumerable);
				}

				if (ex != null)
				{
					throw ex;
				}
			}
		}
		protected IntegrationPoint LoadIntegrationPointDto(Job job)
		{
		    LogLoadInformationPointDtoStart(job);

            int integrationPointId = job.RelatedObjectArtifactID;
			IntegrationPoint integrationPoint = CaseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointId);
			if (integrationPoint == null)
			{
				LogLoadingIntegrationPointDtoError(job);
				throw new ArgumentException("Failed to retrieve corresponding Integration Point.");
			}
		    LogLoadIntegrationPointDtoSuccesfulEnd(job);
            return integrationPoint;
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
					JobService.UpdateStopState(new List<long> {job.JobId}, StopState.None);
					IntegrationPointDto.NextScheduledRuntimeUTC = JobService.GetJobNextUtcRunDateTime(job, ScheduleRuleFactory, Result);
				}
				CaseServiceContext.RsapiService.IntegrationPointLibrary.Update(IntegrationPointDto);
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
                JobService.UpdateStopState(new List<long> {job.JobId}, StopState.Unstoppable);
			}
			catch (Exception e)
			{
				LogSettingJobAsUnstoppableError(job, e);
				// Do not throw exception, we will need to dispose the rest of the objects.
			}
		}

	    private void LoadIntegrationPointData(Job job)
		{
			IntegrationPointDto = LoadIntegrationPointDto(job);
			SourceConfiguration = Serializer.Deserialize<SourceConfiguration>(IntegrationPointDto.SourceConfiguration);
			ImportSettings = Serializer.Deserialize<ImportSettings>(IntegrationPointDto.DestinationConfiguration);
			JobHistoryErrorService.IntegrationPoint = IntegrationPointDto;
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
				var taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
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
			JobHistoryDto = JobHistoryService.GetOrCreateScheduledRunHistoryRdo(IntegrationPointDto, Identifier, DateTime.UtcNow);

			JobHistoryErrorService.JobHistory = JobHistoryDto;
			JobHistoryDto.StartTimeUTC = DateTime.UtcNow;
		}

		private void LoadSourceProvider()
		{
		    LogLoadSourceProviderStart();
		    SourceProvider = CaseServiceContext.RsapiService.SourceProviderLibrary.Read(IntegrationPointDto.SourceProvider.Value);
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
					if (exception is IntegrationPointsException) // we want to rethrow, so it can be added to error tab if necessary
					{
						throw;
					}
				}
			});
            
            LogBatchExceptions(job, exceptions);
			ThrowNewExceptionIfAny(exceptions);
		    LogConfigureBatchExceptionsSuccesfulEnd(job);
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

        #endregion
    }
}