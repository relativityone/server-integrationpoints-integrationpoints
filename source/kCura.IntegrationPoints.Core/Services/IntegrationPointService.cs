﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services
{
	public class IntegrationPointService : IIntegrationPointService
	{
		private const string _UNABLE_TO_SAVE_FORMAT = "Unable to save Integration Point:{0} cannot be changed once the Integration Point has been run";

		private readonly ICaseServiceContext _context;
		private readonly IContextContainer _contextContainer;
		private readonly IPermissionRepository _permissionRepository;
		private IntegrationPoint _rdo;
		private readonly ISerializer _serializer;
		private readonly IChoiceQuery _choiceQuery;
		private readonly IJobManager _jobService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IManagerFactory _managerFactory;
		private static readonly object _lock = new object();

		public IntegrationPointService(IHelper helper,
			ICaseServiceContext context,
			IPermissionRepository permissionRepository,
			IContextContainerFactory contextContainerFactory,
			ISerializer serializer, IChoiceQuery choiceQuery,
			IJobManager jobService,
			IJobHistoryService jobHistoryService,
			IManagerFactory managerFactory)
		{
			_context = context;
			_permissionRepository = permissionRepository;
			_serializer = serializer;
			_choiceQuery = choiceQuery;
			_jobService = jobService;
			_jobHistoryService = jobHistoryService;
			_managerFactory = managerFactory;

			_contextContainer = contextContainerFactory.CreateContextContainer(helper);
		}

		public IntegrationPoint GetRdo(int artifactId)
		{
			if (_rdo == null || _rdo.ArtifactId != artifactId)
			{
				_rdo = _context.RsapiService.IntegrationPointLibrary.Read(artifactId);
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
			ValidateConfigurationWhenUpdatingObject(model);

			IList<Relativity.Client.DTOs.Choice> choices = _choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields));
			IntegrationPoint ip = model.ToRdo(choices);
			PeriodicScheduleRule rule = ToScheduleRule(model);
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
			TaskParameters jobDetails = null;
			SourceProvider provider = _context.RsapiService.SourceProviderLibrary.Read(ip.SourceProvider.Value);
			if (provider.Identifier.Equals(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID))
			{
				jobDetails = new TaskParameters
				{
					BatchInstance = Guid.NewGuid()
				};

				CheckForRelativityProviderAdditionalPermissions(ip.SourceConfiguration, _context.EddsUserID);
				task = TaskType.ExportService;
			}
			else
			{
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
				_jobService.CreateJob<object>(jobDetails, task, _context.WorkspaceID, ip.ArtifactId, rule);
			}
			else
			{
				Job job = _jobService.GetJob(_context.WorkspaceID, ip.ArtifactId, task.ToString());
				if (job != null)
				{
					_jobService.DeleteJob(job.JobId);
				}
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
				}
			}

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

			if (provider.Identifier.Equals(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID))
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
			IntegrationPoint integrationPoint = GetRdo(integrationPointArtifactId);
			SourceProvider sourceProvider = GetSourceProvider(integrationPoint);

			CheckPermissions(integrationPoint, sourceProvider, userId);
			Relativity.Client.Choice jobType = GetJobType(integrationPoint.EnableScheduler);
			CreateJob(integrationPoint, sourceProvider, jobType, workspaceArtifactId, userId);
		}

		private Relativity.Client.Choice GetJobType(bool? enableScheduler)
		{
			Relativity.Client.Choice jobType;
			if (enableScheduler.HasValue && enableScheduler.Value)
			{
				jobType = JobTypeChoices.JobHistoryScheduledRun;
			}
			else
			{
				jobType = JobTypeChoices.JobHistoryRunNow;
			}
			return jobType;
		}

		public void RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, int userId)
		{
			IntegrationPoint integrationPoint = GetRdo(integrationPointArtifactId);
			SourceProvider sourceProvider = GetSourceProvider(integrationPoint);

			if (!sourceProvider.Identifier.Equals(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID))
			{
				throw new Exception(Constants.IntegrationPoints.RETRY_IS_NOT_RELATIVITY_PROVIDER);
			}

			CheckPermissions(integrationPoint, sourceProvider, userId);

			if (integrationPoint.HasErrors.HasValue == false || integrationPoint.HasErrors.Value == false)
			{
				throw new Exception(Constants.IntegrationPoints.RETRY_NO_EXISTING_ERRORS);
			}

			CreateJob(integrationPoint, sourceProvider, JobTypeChoices.JobHistoryRetryErrors, workspaceArtifactId, userId);
		}

		private void CheckPermissions(IntegrationPoint integrationPoint, SourceProvider sourceProvider, int userId)
		{
			if (sourceProvider.Identifier == DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID)
			{
				CheckForRelativityProviderAdditionalPermissions(integrationPoint.SourceConfiguration, userId);
			}
		}

		private SourceProvider GetSourceProvider(IntegrationPoint integrationPoint)
		{
			if (!integrationPoint.SourceProvider.HasValue)
			{
				throw new Exception(Constants.IntegrationPoints.NO_SOURCE_PROVIDER_SPECIFIED);
			}

			SourceProvider sourceProvider = _context.RsapiService.SourceProviderLibrary.Read(integrationPoint.SourceProvider.Value);
			return sourceProvider;
		}

		private void CreateJob(IntegrationPoint integrationPoint, SourceProvider sourceProvider, Relativity.Client.Choice jobType, int workspaceArtifactId, int userId)
		{
			lock (_lock)
			{
				CheckForOtherJobsExecutingOrInQueue(sourceProvider, workspaceArtifactId, integrationPoint.ArtifactId);
				var jobDetails = new TaskParameters { BatchInstance = Guid.NewGuid() };

				// If the Relativity provider is selected, we need to create an export task
				TaskType jobTaskType =
					sourceProvider.Identifier.Equals(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID)
						? TaskType.ExportService
						: TaskType.SyncManager;

				_jobHistoryService.CreateRdo(integrationPoint, jobDetails.BatchInstance, jobType, null);
				_jobService.CreateJobOnBehalfOfAUser(jobDetails, jobTaskType, workspaceArtifactId, integrationPoint.ArtifactId, userId);
			}
		}

		private void CheckForRelativityProviderAdditionalPermissions(string config, int userId)
		{
			WorkspaceConfiguration workspaceConfiguration = JsonConvert.DeserializeObject<WorkspaceConfiguration>(config);
			if (_permissionRepository.UserCanImport(workspaceConfiguration.TargetWorkspaceArtifactId) == false)
			{
				throw new Exception(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT);
			}

			if (_permissionRepository.UserCanEditDocuments(workspaceConfiguration.SourceWorkspaceArtifactId) == false)
			{
				throw new Exception(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
			}

			if (_permissionRepository.UserCanViewArtifact(workspaceConfiguration.SourceWorkspaceArtifactId, (int)ArtifactType.Search, workspaceConfiguration.SavedSearchArtifactId) == false)
			{
				throw new Exception(Constants.IntegrationPoints.NO_PERMISSION_TO_ACCESS_SAVEDSEARCH);	
			}

			if (userId == 0)
			{
				throw new Exception(Constants.IntegrationPoints.NO_USERID);
			}
		}

		private void CheckForOtherJobsExecutingOrInQueue(SourceProvider sourceProvider, int workspaceArtifactId, int integrationPointArtifactId)
		{
			if (sourceProvider.Identifier == DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID)
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