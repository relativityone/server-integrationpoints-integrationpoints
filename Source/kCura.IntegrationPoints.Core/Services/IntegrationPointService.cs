﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services
{
	public class IntegrationPointService : IIntegrationPointService
	{
		private const string _UNABLE_TO_SAVE_FORMAT = "Unable to save Integration Point:{0} cannot be changed once the Integration Point has been run";

		private readonly ICaseServiceContext _context;
		private readonly IContextContainer _contextContainer;
		private IntegrationPoint _rdo;
		private readonly ISerializer _serializer;
		private readonly IChoiceQuery _choiceQuery;
		private readonly IJobManager _jobService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IManagerFactory _managerFactory;
		private static readonly object _lock = new object();

		public IntegrationPointService(IHelper helper,
			ICaseServiceContext context,
			IContextContainerFactory contextContainerFactory,
			ISerializer serializer, IChoiceQuery choiceQuery,
			IJobManager jobService,
			IJobHistoryService jobHistoryService,
			IManagerFactory managerFactory)
		{
			_context = context;
			_serializer = serializer;
			_choiceQuery = choiceQuery;
			_jobService = jobService;
			_jobHistoryService = jobHistoryService;
			_managerFactory = managerFactory;

			_contextContainer = contextContainerFactory.CreateContextContainer(helper);
		}

		public IntegrationPoint GetRdo(int artifactId)
		{
			try
			{
				_rdo = _context.RsapiService.IntegrationPointLibrary.Read(artifactId);
			}
			catch (Exception ex)
			{
				throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_INTEGRATION_POINT, ex);
			}	
			return _rdo;
		}

		public IList<IntegrationPoint> GetAllIntegrationPoints()
		{
			IntegrationPointQuery integrationPointQuery = new IntegrationPointQuery(_context.RsapiService);
			IList<IntegrationPoint> integrationPoints = integrationPointQuery.GetAllIntegrationPoints();
			return integrationPoints;
		}

		public virtual string GetSourceOptions(int artifactId)
		{
			return GetRdo(artifactId).SourceConfiguration;
		}

		public virtual FieldEntry GetIdentifierFieldEntry(int artifactId)
		{
			var rdo = GetRdo(artifactId);
			var fields = _serializer.Deserialize<List<FieldMap>>(rdo.FieldMappings);
			return fields.First(x => x.FieldMapType == FieldMapTypeEnum.Identifier).SourceField;
		}

		public virtual IntegrationModel ReadIntegrationPoint(int artifactId)
		{
			IntegrationPoint integrationPoint = GetRdo(artifactId);
			var integrationModel = new IntegrationModel(integrationPoint);
			return integrationModel;
		}

		public IEnumerable<FieldMap> GetFieldMap(int artifactId)
		{
			IEnumerable<FieldMap> mapping = new List<FieldMap>();
			if (artifactId > 0)
			{
				string fieldmap;
				if (_rdo != null)
				{
					fieldmap = _rdo.FieldMappings;
				}
				else
				{
					fieldmap =
						_context.RsapiService.IntegrationPointLibrary.Read(artifactId, new Guid(IntegrationPointFieldGuids.FieldMappings))
							.FieldMappings;
				}

				if (!String.IsNullOrEmpty(fieldmap))
				{
					mapping = _serializer.Deserialize<IEnumerable<FieldMap>>(fieldmap);
				}
			}
			return mapping;
		}

		public int SaveIntegration(IntegrationModel model)
		{
			IntegrationPoint ip = null;
			PeriodicScheduleRule rule = null;
			try
			{
				ValidateConfigurationWhenUpdatingObject(model);

				IList<Relativity.Client.DTOs.Choice> choices =
					_choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields));
				ip = model.ToRdo(choices);
				rule = ToScheduleRule(model);

				if (ip.EnableScheduler.GetValueOrDefault(false))
				{
					ip.ScheduleRule = rule.ToSerializedString();
					ip.NextScheduledRuntimeUTC = rule.GetNextUTCRunDateTime();
				}
				else
				{
					ip.ScheduleRule = string.Empty;
					ip.NextScheduledRuntimeUTC = null;
					rule = null;
				}

				TaskType task;
				SourceProvider provider = _context.RsapiService.SourceProviderLibrary.Read(ip.SourceProvider.Value);


				if (provider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID))
				{
					CheckForProviderAdditionalPermissions(ip, Constants.SourceProvider.Relativity, _context.EddsUserID);
					task = TaskType.ExportService;
				}
				else
				{
					CheckForProviderAdditionalPermissions(ip, Constants.SourceProvider.Other, _context.EddsUserID);
					task = TaskType.SyncManager;
				}

				//save RDO
				if (ip.ArtifactId > 0)
				{
					_context.RsapiService.IntegrationPointLibrary.Update(ip);
				}
				else
				{
					ip.ArtifactId = _context.RsapiService.IntegrationPointLibrary.Create(ip);
				}

				if (rule != null)
				{
					_jobService.CreateJob<TaskParameters>(null, task, _context.WorkspaceID, ip.ArtifactId, rule);
				}
				else
				{
					Job job = _jobService.GetJob(_context.WorkspaceID, ip.ArtifactId, task.ToString());
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
					Core.Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_ADMIN_MESSAGE,
					String.Join(System.Environment.NewLine, new[] { e.Message, e.StackTrace }));

				throw new Exception(Core.Constants.IntegrationPoints.PermissionErrors.UNABLE_TO_SAVE_INTEGRATION_POINT_USER_MESSAGE);	
			}
			return ip.ArtifactId;
		}

		private void ValidateConfigurationWhenUpdatingObject(IntegrationModel model)
		{
			// check that only fields that are allowed to be changed are changed
			List<string> invalidProperties = new List<string>();
			IntegrationModel existingModel = null;
			if (model.ArtifactID > 0)
			{
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
					if (existingModel.Name != model.Name)
					{
						invalidProperties.Add("Name");
					}
					if (existingModel.DestinationProvider != model.DestinationProvider)
					{
						invalidProperties.Add("Destination Provider");
					}
					if (existingModel.Destination != model.Destination)
					{
						dynamic existingDestination = JsonConvert.DeserializeObject(existingModel.Destination);
						dynamic newDestination = JsonConvert.DeserializeObject(model.Destination);

						if (existingDestination.artifactTypeID != newDestination.artifactTypeID)
						{
							invalidProperties.Add("Destination RDO");
						}
						if (existingDestination.CaseArtifactId != newDestination.CaseArtifactId)
						{
							invalidProperties.Add("Case");
						}
					}
					if (existingModel.SourceProvider != model.SourceProvider)
					{
						// If the source provider has been changed, the code below this exception is invalid
						invalidProperties.Add("Source Provider");
						throw new Exception(String.Format(_UNABLE_TO_SAVE_FORMAT, String.Join(",", invalidProperties.Select(x => $" {x}"))));
					}

					model.HasErrors = existingModel.HasErrors;
					model.LastRun = existingModel.LastRun;

					// check permission if we want to push
					// needs to be here because custom page is the only place that has user context
					SourceProvider provider = null;
					try
					{
						provider = _context.RsapiService.SourceProviderLibrary.Read(model.SourceProvider);
					}
					catch (Exception e)
					{
						throw new Exception("Unable to save Integration Point: Unable to retrieve source provider", e);
					}

					if (provider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID))
					{
						if (existingModel != null && (existingModel.SourceConfiguration != model.SourceConfiguration))
						{
							invalidProperties.Add("Source Configuration");
						}
					}

					if (invalidProperties.Any())
					{
						throw new Exception(String.Format(_UNABLE_TO_SAVE_FORMAT, String.Join(",", invalidProperties.Select(x => $" {x}"))));
					}
				}
			}

			if (invalidProperties.Any())
			{
				throw new Exception(String.Format(_UNABLE_TO_SAVE_FORMAT, String.Join(",", invalidProperties.Select(x => $" {x}"))));
			}
		}

		public IEnumerable<string> GetRecipientEmails(int artifactId)
		{
			IntegrationPoint integrationPoint = GetRdo(artifactId);
			string emailRecipients = integrationPoint.EmailNotificationRecipients ?? string.Empty;
			IEnumerable<string> emailRecipientList = emailRecipients.Split(';').Select(x => x.Trim());
			return emailRecipientList;
		}
		#region Please refactor
		public class Weekly
		{
			public List<string> SelectedDays { get; set; }
			public string TemplateID { get; set; }

			public Weekly()
			{
				TemplateID = "weeklySendOn";
			}
		}

		public enum MonthlyType
		{
			Month = 1,
			Days = 2
		}

		public class Monthly
		{
			public MonthlyType MonthChoice { get; set; }
			public int SelectedDay { get; set; }
			public OccuranceInMonth? SelectedType { get; set; }
			public DaysOfWeek SelectedDayOfTheMonth { get; set; }
			public string TemplateID { get; set; }

			public Monthly()
			{
				TemplateID = "monthlySendOn";
			}
		}

		private static DaysOfWeek FromDayOfWeek(List<DayOfWeek> days)
		{
			var map = ScheduleRuleBase.DaysOfWeekMap.ToDictionary(x => x.Value, x => x.Key);
			return days.Aggregate(DaysOfWeek.None, (current, dayOfWeek) => current | map[dayOfWeek]);
		}

		public static List<DayOfWeek> FromDaysOfWeek(DaysOfWeek days)
		{
			var map = ScheduleRuleBase.DaysOfWeekMap;
			if (days == DaysOfWeek.None)
			{
				return new List<DayOfWeek>();
			}
			var values = (DaysOfWeek[])Enum.GetValues(typeof(DaysOfWeek));
			return (from value in values where (days & value) == value && map.ContainsKey(value) select map[value]).ToList();
		}

		#endregion

		private PeriodicScheduleRule ToScheduleRule(IntegrationModel model)
		{
			var periodicScheduleRule = new PeriodicScheduleRule();
			DateTime startDate;
			if (DateTime.TryParse(model.Scheduler.StartDate, out startDate))
			{
				periodicScheduleRule.StartDate = startDate;
			}
			DateTime endDate;
			if (DateTime.TryParse(model.Scheduler.EndDate, out endDate))
			{
				periodicScheduleRule.EndDate = endDate;
			}

			//since we do not know what user local time is, time is passed in UTC
			TimeSpan time;
			if (TimeSpan.TryParse(model.Scheduler.ScheduledTime, out time))
			{
				periodicScheduleRule.LocalTimeOfDay = DateTime.UtcNow.Date.Add(new DateTime(time.Ticks, DateTimeKind.Utc).TimeOfDay).ToLocalTime().TimeOfDay;
			}
			ScheduleInterval interval;
			if (Enum.TryParse(model.Scheduler.SelectedFrequency, true, out interval))
			{
				periodicScheduleRule.Interval = interval;
			}
			periodicScheduleRule.Reoccur = Convert.ToInt32(model.Scheduler.Reoccur);
			periodicScheduleRule.DayOfMonth = null;

			switch (periodicScheduleRule.Interval)
			{
				case ScheduleInterval.Weekly:
					var sendOn = _serializer.Deserialize<Weekly>(model.Scheduler.SendOn);
					periodicScheduleRule.DaysToRun = FromDayOfWeek(sendOn.SelectedDays.Select(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x)).ToList());
					break;
				case ScheduleInterval.Monthly:
					var monthlySendOn = _serializer.Deserialize<Monthly>(model.Scheduler.SendOn);
					if (monthlySendOn.MonthChoice == MonthlyType.Days)
					{
						periodicScheduleRule.DayOfMonth = monthlySendOn.SelectedDay;
					}
					else if (monthlySendOn.MonthChoice == MonthlyType.Month)
					{
						periodicScheduleRule.DaysToRun = monthlySendOn.SelectedDayOfTheMonth;
						periodicScheduleRule.SetLastDayOfMonth = monthlySendOn.SelectedType == OccuranceInMonth.Last;
						periodicScheduleRule.OccuranceInMonth = monthlySendOn.SelectedType;
					}
					//if (this.SendOn.DayOfTheMonth > 0) periodicScheduleRule.DayOfMonth = this.SendOn.DayOfTheMonth;
					//periodicScheduleRule.SetLastDayOfMonth = this.SendOn.LastDayOfMonth;
					//var day = this.SendOn.SelectedWeeklyDays.Select(x => (DaysOfWeek)Enum.Parse(typeof(DaysOfWeek), x, true)).FirstOrDefault();
					//periodicScheduleRule.DaysToRun = day;
					//periodicScheduleRule.OccuranceInMonth = (OccuranceInMonth)this.SendOn.SelectedWeekOfMonth;
					break;
			}
			return periodicScheduleRule;
		}

		public void RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId)
		{
			IntegrationPoint integrationPoint = null;
			SourceProvider sourceProvider = null;
			try
			{
				integrationPoint = GetRdo(integrationPointArtifactId);
				sourceProvider = GetSourceProvider(integrationPoint);
			}
			catch (Exception e)
			{
				CreateRelativityError(
						Core.Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE,
						String.Join(System.Environment.NewLine, new[] { e.Message, e.StackTrace }));

				throw new Exception(Core.Constants.IntegrationPoints.UNABLE_TO_RUN_INTEGRATION_POINT_USER_MESSAGE);
			}

			CheckPermissions(workspaceArtifactId, integrationPoint, sourceProvider, userId);
			CreateJob(integrationPoint, sourceProvider, JobTypeChoices.JobHistoryRun, workspaceArtifactId, userId);
		}

		public void RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId)
		{
			IntegrationPoint integrationPoint = null;
			SourceProvider sourceProvider = null;
			try
			{
				integrationPoint = GetRdo(integrationPointArtifactId);
				sourceProvider = GetSourceProvider(integrationPoint);
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

			CheckPermissions(workspaceArtifactId, integrationPoint, sourceProvider, userId);

			if (integrationPoint.HasErrors.HasValue == false || integrationPoint.HasErrors.Value == false)
			{
				throw new Exception(Constants.IntegrationPoints.RETRY_NO_EXISTING_ERRORS);
			}

			CreateJob(integrationPoint, sourceProvider, JobTypeChoices.JobHistoryRetryErrors, workspaceArtifactId, userId);
		}

		public void MarkIntegrationPointToStopJobs(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IJobHistoryManager jobHistoryManager = _managerFactory.CreateJobHistoryManager(_contextContainer);
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


		private void CheckPermissions(int workspaceArtifactId, IntegrationPoint integrationPoint, SourceProvider sourceProvider, int userId)
		{
			IIntegrationPointManager integrationPointManager = _managerFactory.CreateIntegrationPointManager(_contextContainer);
			IntegrationPointDTO integrationPointDto = ConvertToIntegrationPointDto(integrationPoint);

			Constants.SourceProvider sourceProviderEnum = Constants.SourceProvider.Other;
			if (sourceProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID))
			{
				if (userId == 0)
				{
					throw new Exception(Constants.IntegrationPoints.NO_USERID);
				}

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

		private static IntegrationPointDTO ConvertToIntegrationPointDto(IntegrationPoint integrationPoint)
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

		private SourceProvider GetSourceProvider(IntegrationPoint integrationPoint)
		{
			if (!integrationPoint.SourceProvider.HasValue)
			{
				throw new Exception(Constants.IntegrationPoints.NO_SOURCE_PROVIDER_SPECIFIED);
			}

			SourceProvider sourceProvider = null;
			try
			{
				sourceProvider = _context.RsapiService.SourceProviderLibrary.Read(integrationPoint.SourceProvider.Value);
			}
			catch (Exception e)
			{
				throw new Exception(Core.Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_SOURCE_PROVIDER, e);				
			}

			return sourceProvider;
		}

		private void CreateJob(IntegrationPoint integrationPoint, SourceProvider sourceProvider, Choice jobType, int workspaceArtifactId, int userId)
		{
			lock (_lock)
			{
				// If the Relativity provider is selected, we need to create an export task
				TaskType jobTaskType = GetJobTaskType(integrationPoint, sourceProvider);

				CheckForOtherJobsExecutingOrInQueue(jobTaskType, workspaceArtifactId, integrationPoint.ArtifactId);
				var jobDetails = new TaskParameters { BatchInstance = Guid.NewGuid() };

				_jobHistoryService.CreateRdo(integrationPoint, jobDetails.BatchInstance, jobType, null);
				_jobService.CreateJobOnBehalfOfAUser(jobDetails, jobTaskType, workspaceArtifactId, integrationPoint.ArtifactId, userId);
			}
		}

		private TaskType GetJobTaskType(IntegrationPoint integrationPoint, SourceProvider sourceProvider)
		{
			TaskType jobTaskType =
				sourceProvider.Identifier.Equals(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID)
					? TaskType.ExportService
					: TaskType.SyncManager;

			var importSettings = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration);
			if (
				importSettings.DestinationProviderType.Equals(Core.Services.Synchronizer.RdoSynchronizerProvider.FILES_SYNC_TYPE_GUID))
			{
				jobTaskType = TaskType.ExportManager;
			}
			return jobTaskType;
		}

		private void CheckForProviderAdditionalPermissions(IntegrationPoint integrationPoint, Constants.SourceProvider providerType, int userId)
		{
			IIntegrationPointManager integrationPointManager = _managerFactory.CreateIntegrationPointManager(_contextContainer);
			IntegrationPointDTO integrationPointDto = ConvertToIntegrationPointDto(integrationPoint);

			PermissionCheckDTO permissionCheck = integrationPointManager.UserHasPermissionToSaveIntegrationPoint(_context.WorkspaceID, integrationPointDto, providerType);

			if (userId == 0)
			{
				permissionCheck.Success = false;

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

		private void CreateRelativityError(string message, string fullText)
		{
			IErrorManager errorManager = _managerFactory.CreateErrorManager(_contextContainer);
			var error = new ErrorDTO()
			{
				Message = message,
				FullText = fullText,
				Source = Core.Constants.IntegrationPoints.APPLICATION_NAME,
				WorkspaceId = _context.WorkspaceID
			};

			errorManager.Create(new[] {error});
		}

		private void CheckForOtherJobsExecutingOrInQueue(TaskType taskType, int workspaceArtifactId, int integrationPointArtifactId)
		{
			if (taskType == TaskType.ExportService || taskType == TaskType.SyncManager)
			{
				IQueueManager queueManager = _managerFactory.CreateQueueManager(_contextContainer);
				bool jobsExecutingOrInQueue = queueManager.HasJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);

				if (jobsExecutingOrInQueue)
				{
					throw new Exception(Constants.IntegrationPoints.JOBS_ALREADY_RUNNING);
				}
			}
		}

		internal class WorkspaceConfiguration
		{
			public int TargetWorkspaceArtifactId;
			public int SourceWorkspaceArtifactId;
			public int SavedSearchArtifactId;
		}
	}
}