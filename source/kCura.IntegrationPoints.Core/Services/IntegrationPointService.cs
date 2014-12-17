﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.ScheduleQueueAgent.ScheduleRules;
using Newtonsoft.Json;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.Core.Services
{
	public class IntegrationPointService
	{
		private readonly IServiceContext _context;
		private Data.IntegrationPoint _rdo;
		private readonly kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		private readonly ChoiceQuery _choiceQuery;

		private Data.IntegrationPoint GetRDO(int rdoID)
		{
			if (_rdo == null)
			{
				_rdo = _context.RsapiService.IntegrationPointLibrary.Read(rdoID);
			}
			return _rdo;
		}
		public IntegrationPointService(IServiceContext context, kCura.Apps.Common.Utils.Serializers.ISerializer serializer, ChoiceQuery choiceQuery)
		{
			_context = context;
			_serializer = serializer;
			_choiceQuery = choiceQuery;
		}

		public virtual string GetSourceOptions(int artifactID)
		{
			return GetRDO(artifactID).SourceConfiguration;
		}

		public virtual FieldEntry GetIdentifierFieldEntry(int artifactID)
		{
			var rdo = GetRDO(artifactID);
			var fields = _serializer.Deserialize<List<FieldMap>>(rdo.FieldMappings);
			return fields.First(x => x.FieldMapType == FieldMapTypeEnum.Identifier).SourceField;
		}

		public IntegrationModel ReadIntegrationPoint(int objectId)
		{
			var point = _context.RsapiService.IntegrationPointLibrary.Read(objectId);
			return new IntegrationModel(point);
		}

		public IEnumerable<FieldMap> GetFieldMap(int objectId)
		{
			var fieldmap = _context.RsapiService.IntegrationPointLibrary.Read(objectId, new Guid(Data.IntegrationPointFieldGuids.FieldMappings)).FieldMappings;
			IEnumerable<FieldMap> mapping = new List<FieldMap>();
			if (!string.IsNullOrEmpty(fieldmap))
			{
				mapping = _serializer.Deserialize<IEnumerable<FieldMap>>(fieldmap);
			}
			return mapping;
		}

		public void SaveIntegration(IntegrationModel model)
		{
			var ip = model.ToRdo();
			var choices = _choiceQuery.GetChoicesOnField(Guid.Parse(Data.IntegrationPointFieldGuids.Frequency));
			var choiceDto = choices.First(x => x.Name.Equals(model.Scheduler.SelectedFrequency));
			ip.Frequency = new Choice(choiceDto.ArtifactID, choiceDto.Name);
			var rule = this.ToScheduleRule(model);
			//save RDO
			if (ip.ArtifactId > 0)
			{
				_context.RsapiService.IntegrationPointLibrary.Update(ip);
			}
			else
			{
				_context.RsapiService.IntegrationPointLibrary.Create(ip);
			}
			//create job
		}

		public class Weekly
		{
			public List<string> SelectedDays { get; set; }
		}

		public enum MonthlyType
		{
			None = 0,
			Month = 1,
			Days = 2
		}


		public class Monthly
		{
			public MonthlyType MonthChoice { get; set; }
			public int SelectedDay { get; set; }
			public OccuranceInMonth? SelectedType { get; set; }
			public DaysOfWeek SelectedDayOfTheMonth { get; set; }
		}

		private PeriodicScheduleRule ToScheduleRule(IntegrationModel model)
		{
			var periodicScheduleRule = new PeriodicScheduleRule();
			var startDate = DateTime.Parse(model.Scheduler.StartDate);
			periodicScheduleRule.StartDate = startDate;
			if (!string.IsNullOrEmpty(model.Scheduler.EndDate))
			{
				var endDate = DateTime.Parse(model.Scheduler.EndDate);
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
					}else if (monthlySendOn.MonthChoice == MonthlyType.Month)
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

		private static DaysOfWeek FromDayOfWeek(List<DayOfWeek> days)
		{
			var map = kCura.ScheduleQueueAgent.ScheduleRules.ScheduleRuleBase.DaysOfWeekMap.ToDictionary(x => x.Value, x => x.Key);
			return days.Aggregate(DaysOfWeek.None, (current, dayOfWeek) => current | map[dayOfWeek]);
		}

	}
}
