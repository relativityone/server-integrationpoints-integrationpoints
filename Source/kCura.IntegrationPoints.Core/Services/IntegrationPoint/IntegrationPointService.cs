﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public class IntegrationPointService : IntegrationPointServiceBase<Data.IntegrationPoint>, IIntegrationPointService
	{
		private readonly IJobManager _jobService;
		private readonly IJobHistoryService _jobHistoryService;

		protected override string UnableToSaveFormat
			=> "Unable to save Integration Point:{0} cannot be changed once the Integration Point has been run";
		
		public IntegrationPointService(IHelper helper,
			ICaseServiceContext context,
			IContextContainerFactory contextContainerFactory,
			ISerializer serializer, IChoiceQuery choiceQuery,
			IJobManager jobService,
			IJobHistoryService jobHistoryService,
			IManagerFactory managerFactory)
			: base(helper, context, choiceQuery, serializer, managerFactory, contextContainerFactory,new IntegrationPointFieldGuidsConstants())
		{
			_jobService = jobService;
			_jobHistoryService = jobHistoryService;
		}

		protected override IntegrationPointModelBase GetModel(int artifactId)
		{
			return ReadIntegrationPoint(artifactId);
		}
		
		public virtual IntegrationPointModel ReadIntegrationPoint(int artifactId)
		{
			Data.IntegrationPoint integrationPoint = GetRdo(artifactId);
			var integrationModel = IntegrationPointModel.FromIntegrationPoint(integrationPoint);
			return integrationModel;
		}

		public int SaveIntegration(IntegrationPointModel model)
		{
			Data.IntegrationPoint integrationPoint;
			PeriodicScheduleRule rule;
			try
			{
				if (model.ArtifactID > 0)
				{
					IntegrationPointModel existingModel;
					try
					{
						existingModel = ReadIntegrationPoint(model.ArtifactID);
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

				rule = ConvertModelToScheduleRule(model);
				integrationPoint = model.ToRdo(choices, rule);
				
				SourceProvider sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
				DestinationProvider destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
				TaskType task = GetJobTaskType(sourceProvider, destinationProvider);

				if (sourceProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID) &&
					destinationProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID))
				{
					CheckForProviderAdditionalPermissions(integrationPoint, Constants.SourceProvider.Relativity, Context.EddsUserID);
				}
				else
				{
					CheckForProviderAdditionalPermissions(integrationPoint, Constants.SourceProvider.Other, Context.EddsUserID);
				}

				//save RDO
				if (integrationPoint.ArtifactId > 0)
				{
					Context.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Update(integrationPoint);
				}
				else
				{
					integrationPoint.ArtifactId = Context.RsapiService.GetGenericLibrary<Data.IntegrationPoint>().Create(integrationPoint);
				}

				if (integrationPoint.EnableScheduler.GetValueOrDefault(false))
				{
					_jobService.CreateJob<TaskParameters>(null, task, Context.WorkspaceID, integrationPoint.ArtifactId, rule);
				}
				else
				{
					Job job = _jobService.GetJob(Context.WorkspaceID, integrationPoint.ArtifactId, task.ToString());
					if (job != null)
					{
						_jobService.DeleteJob(job.JobId);
					}
				}
			}
			catch (PermissionException)
			{
				throw;
			}
			catch (Exception e)
			{
				CreateRelativityError(
					Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_ADMIN_MESSAGE,
					String.Join(Environment.NewLine, new[] { e.Message, e.StackTrace }));

				throw new Exception(Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_USER_MESSAGE);
			}
			return integrationPoint.ArtifactId;
		}

		public void RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId)
		{
			Data.IntegrationPoint integrationPoint = null;
			SourceProvider sourceProvider = null;
			DestinationProvider destinationProvider = null;

			try
			{
				integrationPoint = GetRdo(integrationPointArtifactId);
				sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
				destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
			}
			catch (Exception e)
			{
				CreateRelativityError(
					Core.Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE,
					String.Join(System.Environment.NewLine, new[] { e.Message, e.StackTrace }));

				throw new Exception(Core.Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_USER_MESSAGE);
			}

			CheckPermissions(workspaceArtifactId, integrationPoint, sourceProvider, destinationProvider, userId);
			CreateJob(integrationPoint, sourceProvider, destinationProvider, JobTypeChoices.JobHistoryRun, workspaceArtifactId, userId);
		}

		public void RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId)
		{
			Data.IntegrationPoint integrationPoint = null;
			SourceProvider sourceProvider = null;
			DestinationProvider destinationProvider = null;

			try
			{
				integrationPoint = GetRdo(integrationPointArtifactId);
				sourceProvider = GetSourceProvider(integrationPoint.SourceProvider);
				destinationProvider = GetDestinationProvider(integrationPoint.DestinationProvider);
			}
			catch (Exception e)
			{
				CreateRelativityError(
					Core.Constants.IntegrationPoints.UNABLE_TO_RETRY_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE,
					String.Join(System.Environment.NewLine, new[] { e.Message, e.StackTrace }));

				throw new Exception(Core.Constants.IntegrationPoints.UNABLE_TO_RETRY_INTEGRATION_POINT_USER_MESSAGE);
			}

			if (!sourceProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID))
			{
				throw new Exception(Constants.IntegrationPoints.RETRY_IS_NOT_RELATIVITY_PROVIDER);
			}

			CheckPermissions(workspaceArtifactId, integrationPoint, sourceProvider, destinationProvider, userId);

			CheckPreviousJobHistoryStatusOnRetry(workspaceArtifactId, integrationPointArtifactId);

			if (integrationPoint.HasErrors.HasValue == false || integrationPoint.HasErrors.Value == false)
			{
				throw new Exception(Constants.IntegrationPoints.RETRY_NO_EXISTING_ERRORS);
			}

			CreateJob(integrationPoint, sourceProvider, destinationProvider, JobTypeChoices.JobHistoryRetryErrors, workspaceArtifactId, userId);
		}

		private void CheckPreviousJobHistoryStatusOnRetry(int workspaceArtifactId, int integrationPointArtifactId)
		{
			const string failToRetrieveJobHistory = "Unable to retrieve the previous job history.";
			Data.JobHistory lastJobHistory = null;
			try
			{
				var jobHistoryManager = ManagerFactory.CreateJobHistoryManager(ContextContainer);
				int lastJobHistoryArtifactId = jobHistoryManager.GetLastJobHistoryArtifactId(workspaceArtifactId, integrationPointArtifactId);
				lastJobHistory = Context.RsapiService.JobHistoryLibrary.Read(lastJobHistoryArtifactId);
			}
			catch (Exception exception)
			{
				throw new Exception(failToRetrieveJobHistory, exception);
			}

			if (lastJobHistory == null)
			{
				throw new Exception(failToRetrieveJobHistory);
			}

			if (lastJobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopped))
			{
				throw new Exception(Constants.IntegrationPoints.RETRY_ON_STOPPED_JOB);
			}
		}

		private void CheckStopPermission(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IIntegrationPointManager manager = ManagerFactory.CreateIntegrationPointManager(ContextContainer);
			PermissionCheckDTO result = manager.UserHasPermissionToStopJob(workspaceArtifactId, integrationPointArtifactId);
			if (!result.Success)
			{
				CreateRelativityError(
					Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE,
					$"User is missing the following permissions:{Environment.NewLine}{String.Join(Environment.NewLine, result.ErrorMessages)}");

				throw new Exception(Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS);
			}
		}

		public void MarkIntegrationPointToStopJobs(int workspaceArtifactId, int integrationPointArtifactId)
		{
			CheckStopPermission(workspaceArtifactId, integrationPointArtifactId);

			IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager(ContextContainer);
			StoppableJobCollection stoppableJobCollection = jobHistoryManager.GetStoppableJobCollection(workspaceArtifactId, integrationPointArtifactId);
			IList<int> allStoppableJobArtifactIds = stoppableJobCollection.PendingJobArtifactIds.Concat(stoppableJobCollection.ProcessingJobArtifactIds).ToList();
			IDictionary<Guid, List<Job>> jobs = _jobService.GetScheduledAgentJobMapedByBatchInstance(integrationPointArtifactId);

			List<Exception> exceptions = new List<Exception>(); // Gotta Catch 'em All
			HashSet<int> erroredPendingJobs = new HashSet<int>();

			// Mark jobs to be stopped in queue table
			foreach (int artifactId in allStoppableJobArtifactIds)
			{
				try
				{
					StopScheduledAgentJobs(jobs, artifactId);
				}
				catch (Exception exception)
				{
					if (stoppableJobCollection.PendingJobArtifactIds.Contains(artifactId))
					{
						erroredPendingJobs.Add(artifactId);
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


		private void CheckPermissions(int workspaceArtifactId, Data.IntegrationPoint integrationPoint, SourceProvider sourceProvider, DestinationProvider destinationProvider, int userId)
		{
			if (userId == 0)
			{
				throw new Exception(Constants.IntegrationPoints.NO_USERID);
			}

			IIntegrationPointManager integrationPointManager = ManagerFactory.CreateIntegrationPointManager(ContextContainer);
			IntegrationPointDTO integrationPointDto = ConvertToIntegrationPointDto(integrationPoint);

			Constants.SourceProvider sourceProviderEnum = Constants.SourceProvider.Other;
			if (sourceProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID) &&
				destinationProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID))
			{
				sourceProviderEnum = Constants.SourceProvider.Relativity;
			}

			PermissionCheckDTO permissionCheck = integrationPointManager.UserHasPermissionToRunJob(workspaceArtifactId, integrationPointDto, sourceProviderEnum);

			if (!permissionCheck.Success)
			{
				CreateRelativityError(
					Core.Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE,
					$"User is missing the following permissions:{System.Environment.NewLine}{String.Join(System.Environment.NewLine, permissionCheck.ErrorMessages)}");

				throw new Exception(Constants.IntegrationPoints.PermissionErrors.INSUFFICIENT_PERMISSIONS);
			}
		}

		private static IntegrationPointDTO ConvertToIntegrationPointDto(Data.IntegrationPoint integrationPoint)
		{
			int[] jobHistory = null;
			try
			{
				jobHistory = integrationPoint.JobHistory;
			}
			catch
			{
				// if there are no job histories (i.e. on create) there will be no results and this will except
			}

			IntegrationPointDTO integrationPointDto = new IntegrationPointDTO
			{
				ArtifactId = integrationPoint.ArtifactId,
				Name = integrationPoint.Name,
				DestinationConfiguration = integrationPoint.DestinationConfiguration,
				DestinationProvider = integrationPoint.DestinationProvider,
				EmailNotificationRecipients = integrationPoint.EmailNotificationRecipients,
				EnableScheduler = integrationPoint.EnableScheduler,
				FieldMappings = integrationPoint.FieldMappings,
				HasErrors = integrationPoint.HasErrors,
				JobHistory = jobHistory,
				LastRuntimeUTC = integrationPoint.LastRuntimeUTC,
				LogErrors = integrationPoint.LogErrors,
				SourceProvider = integrationPoint.SourceProvider,
				SourceConfiguration = integrationPoint.SourceConfiguration,
				NextScheduledRuntimeUTC = integrationPoint.NextScheduledRuntimeUTC,
				//				OverwriteFields = integrationPoint.OverwriteFields, -- This would require further transformation
				ScheduleRule = integrationPoint.ScheduleRule
			};
			return integrationPointDto;
		}

		private void CreateJob(Data.IntegrationPoint integrationPoint, SourceProvider sourceProvider, DestinationProvider destinationProvider, Choice jobType, int workspaceArtifactId, int userId)
		{
			lock (Lock)
			{
				// If the Relativity provider is selected, we need to create an export task
				TaskType jobTaskType = GetJobTaskType(sourceProvider, destinationProvider);

				CheckForOtherJobsExecutingOrInQueue(jobTaskType, workspaceArtifactId, integrationPoint.ArtifactId);
				var jobDetails = new TaskParameters { BatchInstance = Guid.NewGuid() };

				_jobHistoryService.CreateRdo(integrationPoint, jobDetails.BatchInstance, jobType, null);
				_jobService.CreateJobOnBehalfOfAUser(jobDetails, jobTaskType, workspaceArtifactId, integrationPoint.ArtifactId, userId);
			}
		}

		private TaskType GetJobTaskType(SourceProvider sourceProvider, DestinationProvider destinationProvider)
		{
			TaskType jobTaskType =
				sourceProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID)
					? TaskType.ExportService
					: TaskType.SyncManager;

			if (destinationProvider.Identifier.Equals(Core.Services.Synchronizer.RdoSynchronizerProvider.FILES_SYNC_TYPE_GUID))
			{
				jobTaskType = TaskType.ExportManager;
			}
			return jobTaskType;
		}

		private void CheckForProviderAdditionalPermissions(Data.IntegrationPoint integrationPoint, Constants.SourceProvider providerType, int userId)
		{
			IIntegrationPointManager integrationPointManager = ManagerFactory.CreateIntegrationPointManager(ContextContainer);
			IntegrationPointDTO integrationPointDto = ConvertToIntegrationPointDto(integrationPoint);

			PermissionCheckDTO permissionCheck = integrationPointManager.UserHasPermissionToSaveIntegrationPoint(Context.WorkspaceID, integrationPointDto, providerType);

			if (userId == 0)
			{
				var errorMessages = new List<string>(permissionCheck.ErrorMessages ?? new string[0]);
				errorMessages.Add(Constants.IntegrationPoints.NO_USERID);

				permissionCheck.ErrorMessages = errorMessages.ToArray();
			}

			if (!permissionCheck.Success)
			{
				CreateRelativityError(
					Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_MESSAGE,
					$"{Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_FULLTEXT_PREFIX}{Environment.NewLine}{String.Join(Environment.NewLine, permissionCheck.ErrorMessages)}");

				throw new PermissionException(Core.Constants.IntegrationPoints.PermissionErrors.INTEGRATION_POINT_SAVE_FAILURE_USER_MESSAGE);
			}
		}

		private void CheckForOtherJobsExecutingOrInQueue(TaskType taskType, int workspaceArtifactId, int integrationPointArtifactId)
		{
			if (taskType == TaskType.ExportService || taskType == TaskType.SyncManager || taskType == TaskType.ExportManager)
			{
				IQueueManager queueManager = ManagerFactory.CreateQueueManager(ContextContainer);
				bool jobsExecutingOrInQueue = queueManager.HasJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);

				if (jobsExecutingOrInQueue)
				{
					throw new Exception(Constants.IntegrationPoints.JOBS_ALREADY_RUNNING);
				}
			}
		}
	}
}