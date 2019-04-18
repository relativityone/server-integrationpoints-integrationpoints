using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public class IntegrationPointService : IntegrationPointServiceBase<Data.IntegrationPoint>, IIntegrationPointService, IIntegrationPointForSourceService
	{
		private const string _VALIDATION_FAILED = "Failed to submit integration job. Integration Point validation failed.";

		private readonly IAPILog _logger;
		private readonly IJobHistoryErrorService _jobHistoryErrorService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobManager _jobService;
		private readonly IMessageService _messageService;
		private readonly IProviderTypeService _providerTypeService;
		private readonly IIntegrationPointRepository _integrationPointRepository;

		protected override string UnableToSaveFormat
			=> "Unable to save Integration Point:{0} cannot be changed once the Integration Point has been run";

		public IntegrationPointService(
			IHelper helper,
			ICaseServiceContext context,
			IContextContainerFactory contextContainerFactory,
			IIntegrationPointSerializer serializer, 
			IChoiceQuery choiceQuery,
			IJobManager jobService,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			IManagerFactory managerFactory,
			IValidationExecutor validationExecutor, 
			IProviderTypeService providerTypeService, 
			IMessageService messageService,
			IIntegrationPointRepository integrationPointRepository)
			: base(helper, context, choiceQuery, serializer, managerFactory, contextContainerFactory, validationExecutor)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointService>();
			_jobService = jobService;
			_jobHistoryService = jobHistoryService;
			_jobHistoryErrorService = jobHistoryErrorService;
			_providerTypeService = providerTypeService;
			_messageService = messageService;
			_validationExecutor = validationExecutor;
			_integrationPointRepository = integrationPointRepository;
		}

		protected override IntegrationPointModelBase GetModel(int artifactID)
		{
			return ReadIntegrationPointModel(artifactID);
		}

		public virtual IntegrationPointModel ReadIntegrationPointModel(int artifactID)
		{
			Data.IntegrationPoint integrationPoint = _integrationPointRepository.ReadAsync(artifactID).GetAwaiter().GetResult();
			IntegrationPointModel integrationModel = IntegrationPointModel.FromIntegrationPoint(integrationPoint);
			return integrationModel;
		}

		public virtual Data.IntegrationPoint ReadIntegrationPoint(int artifactID)
		{
			return _integrationPointRepository.ReadAsync(artifactID).GetAwaiter().GetResult();
		}

		public int SaveIntegration(IntegrationPointModel model)
		{
			try
			{
				if (model.ArtifactID > 0)
				{
					IntegrationPointModel existingModel;
					try
					{
						existingModel = ReadIntegrationPointModel(model.ArtifactID);
					}
					catch (Exception e)
					{
						throw new Exception("Unable to save Integration Point: Unable to retrieve Integration Point", e);
					}

					if (existingModel.LastRun.HasValue)
					{
						ValidateConfigurationWhenUpdatingObject(model, existingModel);
						model.HasErrors = existingModel.HasErrors;
						model.LastRun = existingModel.LastRun;
					}
				}

				IList<Choice> choices =
					ChoiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields));

				PeriodicScheduleRule rule = ConvertModelToScheduleRule(model);
				Data.IntegrationPoint integrationPoint = model.ToRdo(choices, rule);

				IntegrationPointModel integrationPointModel = IntegrationPointModel.FromIntegrationPoint(integrationPoint);

				SourceProvider sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
				DestinationProvider destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
				IntegrationPointType integrationPointType = GetIntegrationPointType(integrationPoint.Type);

				RunValidation(integrationPointModel, sourceProvider, destinationProvider, integrationPointType, ObjectTypeGuids.IntegrationPoint);

				//save RDO
				if (integrationPoint.ArtifactId > 0)
				{
					Context.RsapiService.RelativityObjectManager.Update(integrationPoint);
				}
				else
				{
					integrationPoint.ArtifactId = Context.RsapiService.RelativityObjectManager.Create(integrationPoint);
				}

				TaskType task = GetJobTaskType(sourceProvider, destinationProvider);

				if (integrationPoint.EnableScheduler.GetValueOrDefault(false))
				{
					var taskParameters = new TaskParameters()
					{
						BatchInstance = Guid.NewGuid()
					};
					_jobService.CreateJob(taskParameters, task, Context.WorkspaceID, integrationPoint.ArtifactId, rule);
				}
				else
				{
					Job job = _jobService.GetJob(Context.WorkspaceID, integrationPoint.ArtifactId, task.ToString());
					if (job != null)
					{
						_jobService.DeleteJob(job.JobId);
					}
				}

				return integrationPoint.ArtifactId;
			}
			catch (PermissionException ex)
			{
				CreateRelativityError(
					Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_MESSAGE,
					$"{Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_FULLTEXT_PREFIX}{Environment.NewLine}{ex.Message}");
				throw;
			}
			catch (IntegrationPointValidationException validationException)
			{
			    CreateRelativityError(
			        Constants.IntegrationPoints.UNABLE_TO_SAVE_INTEGRATION_POINT_VALIDATION_FAILED,
			        string.Join(Environment.NewLine, validationException.ValidationResult.MessageTexts)
			    );
				throw;
			}
			catch (Exception exception)
			{
				CreateRelativityError(
					Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_ADMIN_MESSAGE,
					String.Join(Environment.NewLine, new[] { exception.Message, exception.StackTrace })
				);

				throw new Exception(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_USER_MESSAGE, exception);
			}
		}

		public void RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId)
		{
			Data.IntegrationPoint integrationPoint;
			SourceProvider sourceProvider;
			DestinationProvider destinationProvider;

			try
			{
				integrationPoint = _integrationPointRepository.ReadAsync(integrationPointArtifactId).GetAwaiter().GetResult();
				sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
				destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
			}
			catch (Exception e)
			{
				CreateRelativityError(
					Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE,
					string.Join(Environment.NewLine, e.Message, e.StackTrace));

				throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_USER_MESSAGE);
			}

			var jobDetails = new TaskParameters { BatchInstance = Guid.NewGuid() };
			Data.JobHistory jobHistory = CreateJobHistory(integrationPoint, jobDetails, JobTypeChoices.JobHistoryRun);

			ValidateIntegrationPointBeforeRun(integrationPointArtifactId, userId, integrationPoint, sourceProvider, destinationProvider, jobHistory);
			CreateJob(integrationPoint, sourceProvider, destinationProvider, jobDetails, workspaceArtifactId, userId);
		}

		public void RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId)
		{
			Data.IntegrationPoint integrationPoint;
			SourceProvider sourceProvider;
			DestinationProvider destinationProvider;

			try
			{
				integrationPoint = _integrationPointRepository.ReadAsync(integrationPointArtifactId).GetAwaiter().GetResult();
				sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
				destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
			}
			catch (Exception e)
			{
				CreateRelativityError(
					Constants.IntegrationPoints.UNABLE_TO_RETRY_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE,
					string.Join(Environment.NewLine, e.Message, e.StackTrace));

				throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRY_INTEGRATION_POINT_USER_MESSAGE);
			}

			var jobDetails = new TaskParameters { BatchInstance = Guid.NewGuid() };
			Data.JobHistory jobHistory = CreateJobHistory(integrationPoint, jobDetails, JobTypeChoices.JobHistoryRetryErrors);

			ValidateIntegrationPointBeforeRetryErrors(workspaceArtifactId, integrationPointArtifactId, userId, integrationPoint, sourceProvider, destinationProvider, jobHistory);
			CreateJob(integrationPoint, sourceProvider, destinationProvider, jobDetails, workspaceArtifactId, userId);
		}

		public IList<Data.IntegrationPoint> GetAllForSourceProvider(string sourceProviderGuid)
		{
			ISourceProviderManager sourceProviderManager = ManagerFactory.CreateSourceProviderManager(SourceContextContainer);
			int relativityProviderArtifactId = sourceProviderManager.GetArtifactIdFromSourceProviderTypeGuidIdentifier(Context.WorkspaceID, sourceProviderGuid);
			return this.GetAllRDOsForSourceProvider(new List<int> { relativityProviderArtifactId });
		}

		private void CheckPreviousJobHistoryStatusOnRetry(int workspaceArtifactId, int integrationPointArtifactId)
		{
			Data.JobHistory lastJobHistory = null;
			try
			{
				IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager(SourceContextContainer);
				int lastJobHistoryArtifactId = jobHistoryManager.GetLastJobHistoryArtifactId(workspaceArtifactId, integrationPointArtifactId);
				lastJobHistory = Context.RsapiService.RelativityObjectManager.Read<Data.JobHistory>(lastJobHistoryArtifactId);
			}
			catch (Exception exception)
			{
				throw new Exception(Constants.IntegrationPoints.FAILED_TO_RETRIEVE_JOB_HISTORY, exception);
			}

			if (lastJobHistory == null)
			{
				throw new Exception(Constants.IntegrationPoints.FAILED_TO_RETRIEVE_JOB_HISTORY);
			}

			if (lastJobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopped))
			{
				throw new Exception(Constants.IntegrationPoints.RETRY_ON_STOPPED_JOB);
			}
		}

		private void CheckStopPermission(int integrationPointArtifactId)
		{
			Data.IntegrationPoint integrationPoint =
				_integrationPointRepository.ReadAsync(integrationPointArtifactId).GetAwaiter().GetResult();
			SourceProvider sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
			DestinationProvider destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
			IntegrationPointType integrationPointType = GetIntegrationPointType(integrationPoint.Type);

			var context = new ValidationContext
			{
				DestinationProvider = destinationProvider,
				IntegrationPointType = integrationPointType,
				Model = IntegrationPointModel.FromIntegrationPoint(integrationPoint),
				ObjectTypeGuid = ObjectTypeGuids.IntegrationPoint,
				SourceProvider = sourceProvider,
				UserId = -1
			};

			try
			{
				_validationExecutor.ValidateOnStop(context);
			}
			catch (PermissionException ex)
			{
				CreateRelativityError(
					Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE,
					$"User is missing the following permissions:{Environment.NewLine}{String.Join(Environment.NewLine, ex.Message)}");
				throw;
			}
		}

		public void MarkIntegrationPointToStopJobs(int workspaceArtifactId, int integrationPointArtifactId)
		{
			CheckStopPermission(integrationPointArtifactId);

			IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager(SourceContextContainer);
			StoppableJobCollection stoppableJobCollection = jobHistoryManager.GetStoppableJobCollection(workspaceArtifactId, integrationPointArtifactId);
			IList<int> allStoppableJobArtifactIds = stoppableJobCollection.PendingJobArtifactIds.Concat(stoppableJobCollection.ProcessingJobArtifactIds).ToList();
			IDictionary<Guid, List<Job>> jobs = _jobService.GetScheduledAgentJobMapedByBatchInstance(integrationPointArtifactId);

			List<Exception> exceptions = new List<Exception>(); // Gotta Catch 'em All
			HashSet<int> erroredPendingJobs = new HashSet<int>();

			// Mark jobs to be stopped in queue table
			foreach (int artifactID in allStoppableJobArtifactIds)
			{
				try
				{
					StopScheduledAgentJobs(jobs, artifactID);
				}
				catch (Exception exception)
				{
					if (stoppableJobCollection.PendingJobArtifactIds.Contains(artifactID))
					{
						erroredPendingJobs.Add(artifactID);
					}
					exceptions.Add(exception);
				}
			}

			IEnumerable<int> pendingJobIdsMarkedToStop = stoppableJobCollection.PendingJobArtifactIds
													.Where(x => !erroredPendingJobs.Contains(x));

			// Update the status of the Pending jobs
			foreach (int artifactId in pendingJobIdsMarkedToStop)
			{
				try
				{
					var jobHistoryRdo = new Data.JobHistory()
					{
						ArtifactId = artifactId,
						JobStatus = JobStatusChoices.JobHistoryStopping
					};
					_jobHistoryService.UpdateRdo(jobHistoryRdo);
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
		}

		private void StopScheduledAgentJobs(IDictionary<Guid, List<Job>> agentJobsReference, int jobHistoryArtifactId)
		{
			Data.JobHistory jobHistory = _jobHistoryService.GetJobHistory(new List<int>() { jobHistoryArtifactId }).FirstOrDefault();
			if (jobHistory != null)
			{
				Guid batchInstance = new Guid(jobHistory.BatchInstance);
				if (agentJobsReference.ContainsKey(batchInstance))
				{
					List<long> jobIds = agentJobsReference[batchInstance].Select(job => job.JobId).ToList();
					_jobService.StopJobs(jobIds);
				}
				else
				{
					throw new InvalidOperationException("Unable to retrieve job(s) in the queue. Please contact your system administrator.");
				}
			}
			else
			{
				// I don't think this is currently possible. SAMO - 7/27/2016
				throw new Exception("Failed to retrieve job history RDO. Please retry the operation.");
			}
		}

		private void CreateJob(Data.IntegrationPoint integrationPoint, SourceProvider sourceProvider, DestinationProvider destinationProvider, TaskParameters jobDetails, int workspaceArtifactId, int userId)
		{
			lock (Lock)
			{
				// If the Relativity provider is selected, we need to create an export task
				TaskType jobTaskType = GetJobTaskType(sourceProvider, destinationProvider, integrationPoint.SourceConfiguration);

				CheckForOtherJobsExecutingOrInQueue(jobTaskType, workspaceArtifactId, integrationPoint.ArtifactId);
				_jobService.CreateJobOnBehalfOfAUser(jobDetails, jobTaskType, workspaceArtifactId, integrationPoint.ArtifactId, userId);
			}
		}

		private Data.JobHistory CreateJobHistory(Data.IntegrationPoint integrationPoint, TaskParameters taskParameters, Choice jobType)
		{
			Data.JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, taskParameters.BatchInstance, jobType, null);
			return jobHistory;
		}

		private void SetJobHistoryStatus(Data.JobHistory jobHistory, Choice status)
		{
			if (jobHistory != null)
			{
				jobHistory.JobStatus = status;
				_jobHistoryService.UpdateRdo(jobHistory);
			}
			else
			{
				_logger.LogWarning("Unable to set JobHistory status - jobHistory object is null.");
			}
		}

		private TaskType GetJobTaskType(SourceProvider sourceProvider, DestinationProvider destinationProvider, string sourceConfiguration = null)
		{
			//The check on the destinationProvider should come first in the if block.
			//If destProvider is load file, it should be ExportManager type no matter what the sourceProvider is.
			if (destinationProvider.Identifier.Equals(Core.Services.Synchronizer.RdoSynchronizerProvider.FILES_SYNC_TYPE_GUID))
			{
				return TaskType.ExportManager;
			}
			else if (sourceProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID))
			{
				return TaskType.ExportService;
			}
			else if (sourceProvider.Identifier.Equals(Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE))
			{
				return TaskType.ImportService;
			}

			return TaskType.SyncManager;
		}

		private void CheckForOtherJobsExecutingOrInQueue(TaskType taskType, int workspaceArtifactId, int integrationPointArtifactId)
		{
			if (taskType == TaskType.ExportService || taskType == TaskType.SyncManager || taskType == TaskType.ExportManager)
			{
				IQueueManager queueManager = ManagerFactory.CreateQueueManager(SourceContextContainer);
				bool jobsExecutingOrInQueue = queueManager.HasJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);

				if (jobsExecutingOrInQueue)
				{
					throw new Exception(Constants.IntegrationPoints.JOBS_ALREADY_RUNNING);
				}
			}
		}

		private void HandleValidationError(Data.JobHistory jobHistory, int integrationPointArtifactId, Data.IntegrationPoint integrationPoint, Exception ex)
		{
			AddValidationErrorToJobHistory(jobHistory, ex);
			AddValidationErrorToErrorTab(ex);
			SetJobHistoryStatus(jobHistory, JobStatusChoices.JobHistoryValidationFailed);
			SetHasErrorOnIntegrationPoint(integrationPointArtifactId);
			SendValidationFailedMessage(integrationPoint, jobHistory.BatchInstance);
		}

		private void SendValidationFailedMessage(Data.IntegrationPoint integrationPoint, string batchInstance)
		{
			_messageService.Send(new JobValidationFailedMessage
			{
				Provider = integrationPoint.GetProviderType(_providerTypeService).ToString(),
				CorrelationID = batchInstance
			});
		}

		private void AddValidationErrorToJobHistory(Data.JobHistory jobHistory, Exception ex)
		{
			string errorMessage = GetValidationErrorMessage(ex);

			_jobHistoryErrorService.JobHistory = jobHistory;
			_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, errorMessage, string.Empty);
			_jobHistoryErrorService.CommitErrors();
		}

		private void AddValidationErrorToErrorTab(Exception ex)
		{
			CreateRelativityError(_VALIDATION_FAILED, ex.Message);
		}

		private void SetHasErrorOnIntegrationPoint(int integrationPointArtifactId)
		{
			Data.IntegrationPoint integrationPoint =
				_integrationPointRepository.ReadAsync(integrationPointArtifactId).GetAwaiter().GetResult();
			integrationPoint.HasErrors = true;
			Context.RsapiService.RelativityObjectManager.Update(integrationPoint);
		}

		public IEnumerable<FieldMap> GetFieldMap(int artifactID)
		{
			return _integrationPointRepository.GetFieldMappingAsync(artifactID).GetAwaiter().GetResult();
		}

		private static string GetValidationErrorMessage(Exception ex)
		{
			string errorMessage;
			var aggregatedException = ex as AggregateException;
			if (aggregatedException != null)
			{
				IEnumerable<string> innerMessages = aggregatedException.InnerExceptions.Select(x => x.Message);
				errorMessage = $"{aggregatedException.Message} : {string.Join(",", innerMessages)}";
			}
			else
			{
				errorMessage = ex.Message;
			}

			return errorMessage;
		}

		private void ValidateIntegrationPointBeforeRun(int integrationPointArtifactId, int userId, Data.IntegrationPoint integrationPoint, 
			SourceProvider sourceProvider, DestinationProvider destinationProvider, Data.JobHistory jobHistory)
		{
			try
			{
				IntegrationPointType integrationPointType = GetIntegrationPointType(integrationPoint.Type);
				IntegrationPointModel model = IntegrationPointModel.FromIntegrationPoint(integrationPoint);

				var context = new ValidationContext
				{
					DestinationProvider = destinationProvider,
					IntegrationPointType = integrationPointType,
					Model = model,
					ObjectTypeGuid = ObjectTypeGuids.IntegrationPoint,
					SourceProvider = sourceProvider,
					UserId = userId
				};

				_validationExecutor.ValidateOnRun(context);
			}
			catch (Exception ex)
			{
				HandleValidationError(jobHistory, integrationPointArtifactId, integrationPoint, ex);
				throw;
			}
		}

		private void ValidateIntegrationPointBeforeRetryErrors(int workspaceArtifactId, int integrationPointArtifactId, int userId, Data.IntegrationPoint integrationPoint, SourceProvider sourceProvider, DestinationProvider destinationProvider, Data.JobHistory jobHistory)
		{
			if (!sourceProvider.Identifier.Equals(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID))
			{
				throw new Exception(Constants.IntegrationPoints.RETRY_IS_NOT_RELATIVITY_PROVIDER);
			}

			ValidateIntegrationPointBeforeRun(integrationPointArtifactId, userId, integrationPoint,
				sourceProvider, destinationProvider, jobHistory);

			CheckPreviousJobHistoryStatusOnRetry(workspaceArtifactId, integrationPointArtifactId);

			
			if (integrationPoint.HasErrors.HasValue == false || integrationPoint.HasErrors.Value == false)
			{
				throw new Exception(Constants.IntegrationPoints.RETRY_NO_EXISTING_ERRORS);
			}
		}
	}
}