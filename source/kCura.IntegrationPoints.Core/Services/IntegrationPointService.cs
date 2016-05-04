using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services
{
	public class IntegrationPointService : IIntegrationPointService
	{
		private readonly ICaseServiceContext _context;
		private readonly IPermissionService _permissionService;
		private IntegrationPoint _rdo;
		private readonly ISerializer _serializer;
		private readonly ChoiceQuery _choiceQuery;
		private readonly IJobManager _jobService;
		private readonly IJobHistoryService _jobHistoryService;

		public IntegrationPointService(ICaseServiceContext context,
			IPermissionService permissionService,
			ISerializer serializer, ChoiceQuery choiceQuery, 
			IJobManager jobService,
			IJobHistoryService jobHistoryService)
		{
			_context = context;
			_permissionService = permissionService;
			_serializer = serializer;
			_choiceQuery = choiceQuery;
			_jobService = jobService;
			_jobHistoryService = jobHistoryService;
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

		public IntegrationModel ReadIntegrationPoint(int artifactId)
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
			public string TemplateId { get; set; }

			public Monthly()
			{
				TemplateId = "monthlySendOn";
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
			Guid batchInstance = Guid.NewGuid();
			var jobDetails = new TaskParameters()
			{
				BatchInstance = batchInstance
			};

			IntegrationPoint integrationPointRdo = GetRdo(integrationPointArtifactId);
			SourceProvider provider = _context.RsapiService.SourceProviderLibrary.Read(integrationPointRdo.SourceProvider.Value);
			string identifier = provider.Identifier;

			// if relativity provider is selected, we will create an export task
			if (identifier.Equals(DocumentTransferProvider.Shared.Constants.RELATIVITY_PROVIDER_GUID))
			{
				string sourceConfig = integrationPointRdo.SourceConfiguration;
				CheckForRelativityProviderAdditionalPermissions(sourceConfig, userId);
				_jobHistoryService.CreateRdo(integrationPointRdo, batchInstance, null);
				_jobService.CreateJobOnBehalfOfAUser(jobDetails, TaskType.ExportService, workspaceArtifactId, integrationPointArtifactId, userId);
			}
			else
			{
				_jobHistoryService.CreateRdo(integrationPointRdo, batchInstance, null);
				_jobService.CreateJobOnBehalfOfAUser(jobDetails, TaskType.SyncManager, workspaceArtifactId, integrationPointArtifactId, userId);
			}
		}

		private void CheckForRelativityProviderAdditionalPermissions(string config, int userId)
		{
			WorkspaceConfiguration workspaceConfiguration = JsonConvert.DeserializeObject<WorkspaceConfiguration>(config);
			if (_permissionService.UserCanImport(workspaceConfiguration.TargetWorkspaceArtifactId) == false)
			{
				throw new Exception(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT);
			}

			if (_permissionService.UserCanEditDocuments(workspaceConfiguration.SourceWorkspaceArtifactId) == false)
			{
				throw new Exception(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
			}

			if (userId == 0)
			{
				throw new Exception(Constants.IntegrationPoints.NO_USERID);
			}
		}

		internal class WorkspaceConfiguration
		{
			public int TargetWorkspaceArtifactId;
			public int SourceWorkspaceArtifactId;
		}
	}
}
