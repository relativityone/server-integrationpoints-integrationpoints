using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.Core.Services
{
	public class IntegrationPointService
	{
		private readonly ICaseServiceContext _context;
		private Data.IntegrationPoint _rdo;
		private readonly kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		private readonly ChoiceQuery _choiceQuery;
		private readonly IJobManager _jobService;

		public IntegrationPointService(ICaseServiceContext context, kCura.Apps.Common.Utils.Serializers.ISerializer serializer, ChoiceQuery choiceQuery, IJobManager jobService)
		{
			_context = context;
			_serializer = serializer;
			_choiceQuery = choiceQuery;
			_jobService = jobService;
		}

		public Data.IntegrationPoint GetRdo(int rdoID)
		{
			if (_rdo == null || _rdo.ArtifactId != rdoID)
			{
				_rdo = _context.RsapiService.IntegrationPointLibrary.Read(rdoID);
			}
			return _rdo;
		}

		public virtual string GetSourceOptions(int artifactID)
		{
			return GetRdo(artifactID).SourceConfiguration;
		}

		public virtual FieldEntry GetIdentifierFieldEntry(int artifactID)
		{
			var rdo = GetRdo(artifactID);
			var fields = _serializer.Deserialize<List<FieldMap>>(rdo.FieldMappings);
			return fields.First(x => x.FieldMapType == FieldMapTypeEnum.Identifier).SourceField;
		}

		public IntegrationModel ReadIntegrationPoint(int artifactID)
		{
			var point = GetRdo(artifactID);
			return new IntegrationModel(point);
		}

		public IEnumerable<FieldMap> GetFieldMap(int artifactID)
		{
			IEnumerable<FieldMap> mapping = new List<FieldMap>();
			if (artifactID > 0)
			{
				string fieldmap;
				if (_rdo != null)
					fieldmap = _rdo.FieldMappings;
				else
					fieldmap = _context.RsapiService.IntegrationPointLibrary.Read(artifactID, new Guid(Data.IntegrationPointFieldGuids.FieldMappings)).FieldMappings;

				if (!string.IsNullOrEmpty(fieldmap))
				{
					mapping = _serializer.Deserialize<IEnumerable<FieldMap>>(fieldmap);
				}
			}
			return mapping;
		}

		public int SaveIntegration(IntegrationModel model)
		{
			var choices = _choiceQuery.GetChoicesOnField(Guid.Parse(Data.IntegrationPointFieldGuids.OverwriteFields));

			var ip = model.ToRdo(choices);
			var rule = this.ToScheduleRule(model);
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
				_jobService.CreateJob<object>(null, TaskType.SyncManager, _context.WorkspaceID, ip.ArtifactId, rule);
			}
			else
			{
				Job job = _jobService.GetJob(_context.WorkspaceID, ip.ArtifactId, TaskType.SyncManager.ToString());
				if (job != null)
				{
					_jobService.DeleteJob(job.JobId);
				}
			}
			return ip.ArtifactId;
		}

		public IEnumerable<string> GetRecipientEmails(int integrationPoint)
		{
			return (this.GetRdo(integrationPoint).EmailNotificationRecipients ?? string.Empty).Split(';').Select(x => x.Trim());
		}
		#region Please refactor
		public class Weekly
		{
			public List<string> SelectedDays { get; set; }
			public string TemplateID { get; set; }

			public Weekly()
			{
				this.TemplateID = "weeklySendOn";
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
				this.TemplateID = "monthlySendOn";
			}
		}

		private static DaysOfWeek FromDayOfWeek(List<DayOfWeek> days)
		{
			var map = kCura.ScheduleQueue.Core.ScheduleRules.ScheduleRuleBase.DaysOfWeekMap.ToDictionary(x => x.Value, x => x.Key);
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
			DateTime startDate = DateTime.Now;
			if (DateTime.TryParse(model.Scheduler.StartDate, out startDate))
			{
				periodicScheduleRule.StartDate = startDate;
			}
			DateTime endDate = DateTime.Now;
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


	}
}
