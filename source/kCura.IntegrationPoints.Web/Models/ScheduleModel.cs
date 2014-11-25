using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using kCura.Method.Web.Controls.Attributes;
using kCura.Method.Web.Controls.Attributes.Validation;
using kCura.Method.Web.Controls.Models.Controls;
using Microsoft.Ajax.Utilities;

namespace kCura.IntegrationPoints.Web.Models
{
	public class ScheduleModel 
	{
		public const string START_DATE = "ScheduleRulesStartDate";
		public const string DATE_FORMAT_TEMPLATE = "{0:MM/dd/yyyy}";
		public const int DEFAULT_REOCCUR_VALUE = 1;

		//public PeriodicScheduleRule ToScheduleRule()
		//{
		//	var periodicScheduleRule = new PeriodicScheduleRule();
		//	//periodicScheduleRule.StartDate = this.StartDate ?? DateTime.Now.Date;
		//	if (this.StartDate.HasValue) periodicScheduleRule.StartDate = this.StartDate.Value;
		//	periodicScheduleRule.EndDate = this.EndDate;
		//	//since we do not know what user local time is, time is passed in UTC
		//	TimeSpan time;
		//	if (TimeSpan.TryParse(this.ScheduledTime, out time))
		//	{
		//		periodicScheduleRule.LocalTimeOfDay = DateTime.UtcNow.Date.Add(new DateTime(time.Ticks, DateTimeKind.Utc).TimeOfDay).ToLocalTime().TimeOfDay;
		//	}
		//	periodicScheduleRule.Interval = (ScheduleInterval)int.Parse(this.Frequency.First().Value);
		//	periodicScheduleRule.Reoccur = Convert.ToInt32(this.Reoccur.Value);
		//	periodicScheduleRule.DayOfMonth = null;

		//	switch (periodicScheduleRule.Interval)
		//	{
		//		case ScheduleInterval.Weekly:
		//			periodicScheduleRule.DaysToRun = ScheduleReportsService.FromDayOfWeek(this.SendOn.SelectedWeeklyDays.Select(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x)).ToList());
		//			break;
		//		case ScheduleInterval.Monthly:
		//			if (this.SendOn.DayOfTheMonth > 0) periodicScheduleRule.DayOfMonth = this.SendOn.DayOfTheMonth;
		//			periodicScheduleRule.SetLastDayOfMonth = this.SendOn.LastDayOfMonth;
		//			var day = this.SendOn.SelectedWeeklyDays.Select(x => (DaysOfWeek)Enum.Parse(typeof(DaysOfWeek), x, true)).FirstOrDefault();
		//			periodicScheduleRule.DaysToRun = day;
		//			periodicScheduleRule.OccuranceInMonth = (OccuranceInMonth)this.SendOn.SelectedWeekOfMonth;
		//			break;
		//	}
		//	return periodicScheduleRule;
		//}

		private IEnumerable<Tuple<string, int>> GetEnumList<T>(params T[] exceptions)
		{
			return from object week in Enum.GetValues(typeof(T))
						 where !exceptions.Contains((T)week)
						 select new Tuple<string, int>(week.ToString(), (int)week);
		}

		//public ScheduleRulesDataModel(JobSchedule report)
		//{
		//	//TODO: AC - This works, but we aren't using choices anymore here, but need some sort of flag for differenct scenarios (e.g., create, edit, view)
		//	this.Frequency = new List<SelectListItem> { new SelectListItem { Text = "Select ...", Value = "0" } };

		//	this.Frequency.AddRange(EnumUtility.ScheduleIntervalEnumToListSelectListItems().Where(x =>
		//		x.Text == ScheduleInterval.Daily.ToString() ||
		//		x.Text == ScheduleInterval.Weekly.ToString() ||
		//		x.Text == ScheduleInterval.Monthly.ToString()));

		//	this.SendOn = new SendOnInputModel("Send On", "Send On");
		//	this.Reoccur = new PrefixAndSuffixInputControl("Reoccur", "Reoccur");
		//	this.Reoccur.PrefixText = "Every";
		//	this.Reoccur.Value = DEFAULT_REOCCUR_VALUE;

		//	this.SendOn.WeeksOfMonth = new List<Tuple<string, int>>();
		//	this.SendOn.WeeksOfMonth = GetEnumList(OccuranceInMonth.None).ToList();
		//	this.SendOn.MonthlyWeekDays = new List<Tuple<string, int>>();
		//	this.SendOn.MonthlyWeekDays.Add(new Tuple<string, int>(DaysOfWeek.Day.ToString().ToLower(), (int)DaysOfWeek.Day));
		//	this.SendOn.MonthlyWeekDays.AddRange(GetEnumList(DaysOfWeek.None, DaysOfWeek.All, DaysOfWeek.Day).ToList());
		//	this.SendOn.SelectedWeekOfMonth = 0;

		//	if (report != null && !string.IsNullOrEmpty(report.ScheduleRules))
		//	{
		//		var periodicScheduleRules = ScheduleRulesBase.Deserialize<PeriodicScheduleRule>(report.ScheduleRules);
		//		this.StartDate = periodicScheduleRules.StartDate;
		//		this.EndDate = periodicScheduleRules.EndDate;
		//		this.ScheduledTime = GetScheduledTime(periodicScheduleRules);

		//		if (periodicScheduleRules.Interval == ScheduleInterval.Weekly && periodicScheduleRules.DaysToRun.HasValue)
		//		{
		//			this.SendOn.SelectedWeeklyDays = ScheduleReportsService.FromDaysOfWeek(periodicScheduleRules.DaysToRun.Value).Select(x => x.ToString()).ToList();
		//			var value = periodicScheduleRules.Reoccur.GetValueOrDefault(1);
		//			this.Reoccur.Value = value;
		//			this.Reoccur.SuffixText = "week(s)";
		//		}
		//		else if (periodicScheduleRules.Interval == ScheduleInterval.Monthly)
		//		{
		//			this.SendOn.LastDayOfMonth = periodicScheduleRules.SetLastDayOfMonth.GetValueOrDefault(false);
		//			this.SendOn.DayOfTheMonth = periodicScheduleRules.DayOfMonth.GetValueOrDefault(0);
		//			var dayToRun = periodicScheduleRules.DaysToRun.GetValueOrDefault(DaysOfWeek.Monday);
		//			if (dayToRun == DaysOfWeek.Day)
		//			{
		//				this.SendOn.SelectedWeeklyDays = new List<string> { DaysOfWeek.Day.ToString().ToLower() };
		//			}
		//			else
		//			{
		//				this.SendOn.SelectedWeeklyDays = ScheduleReportsService.FromDaysOfWeek(dayToRun).Select(x => x.ToString()).ToList();
		//			}

		//			var value = periodicScheduleRules.Reoccur.GetValueOrDefault(1);

		//			this.Reoccur.Value = value;
		//			this.Reoccur.SuffixText = "month(s)";
		//			this.SendOn.SelectedWeekOfMonth = (int)periodicScheduleRules.OccuranceInMonth.GetValueOrDefault(OccuranceInMonth.First);
		//		}
		//		else if (periodicScheduleRules.Interval == ScheduleInterval.Daily)
		//		{
		//			this.SendOn.Visible = false;
		//			this.Reoccur.Visible = false;

		//		}
		//		this.Frequency.ForEach(x => x.Selected = x.Value.Equals(periodicScheduleRules.Interval.GetHashCode().ToString()));
		//	}
		//	else
		//	{
		//		ScheduledTime = string.Empty;
		//	}
		//}

		//private string GetScheduledTime(PeriodicScheduleRule periodicScheduleRules)
		//{
		//	string scheduledTime = string.Empty;
		//	if (periodicScheduleRules.LocalTimeOfDay.HasValue)
		//	{
		//		scheduledTime =
		//		 DateTime.Now.Date.Add(new DateTime(periodicScheduleRules.LocalTimeOfDay.Value.Ticks, DateTimeKind.Local).TimeOfDay)
		//			 .ToUniversalTime()
		//			 .ToString("HH:mm");
		//	}
		//	return scheduledTime;
		//}

		public ScheduleModel()
		{
			this.SendOn = new SendOnInputModel(string.Empty,string.Empty);
			this.SendOn.WeeksOfMonth = new List<Tuple<string, int>>();
			this.SendOn.MonthlyWeekDays = new List<Tuple<string, int>>();
			this.SendOn.SelectedWeeklyDays = new List<string>();
			this.Frequency = new List<SelectListItem>();
			this.Reoccur = new PrefixAndSuffixInputControl(string.Empty, string.Empty);
		}


		[DisplayName("Enable Scheduler")]
		[ControlDisplay(10, "ScheduleRulesEnabled", ControlTypeEnum.CheckBox)]
		public bool Enable { get; set; }
		
		[HideIcon]
		[DisplayName("Frequency")]
		[ControlDisplay(10, "ScheduleRulesFrequency", ControlTypeEnum.Select)]
		[SelectDropDownRequired(ErrorMessage = "Frequency is required.", DefaultValue = 0)]
		public List<SelectListItem> Frequency { get; set; }

		[HideIcon]
		[PrefixRangeAttribute(1, 999, "Value")]
		[DisplayName("Reoccur")]
		[ControlDisplay(20, "ScheduleRulesReoccur", ControlTypeEnum.Prefix)]
		[Required(ErrorMessage = "Reoccur is required.")]
		public PrefixAndSuffixInputControl Reoccur { get; set; }

		[DisplayName("Send On")]
		[ControlDisplay(30, "ScheduleRulesSendOn", ControlTypeEnum.SendOn)]
		public SendOnInputModel SendOn { get; set; }

		[DataType(DataType.Date)]
		[Placeholder("mm/dd/yyyy")]
		[DisplayName("Start Date")]
		[ControlDisplay(40, START_DATE, ControlTypeEnum.Date)]
		[Required(ErrorMessage = "Start date is required.")]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = DATE_FORMAT_TEMPLATE)]
		public DateTime? StartDate { get; set; }

		[DataType(DataType.Date)]
		[DisplayName("End Date")]
		[Placeholder("mm/dd/yyyy")]
		[ControlDisplay(50, "ScheduleRulesEndDate", ControlTypeEnum.Date)]
		[CompareDate(Comparitor = Comparitor.Greater, CompareField = START_DATE, ErrorMessage = "The start date must come before the end date.")]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = DATE_FORMAT_TEMPLATE)]
		public DateTime? EndDate { get; set; }

		[DisplayName("Scheduled Time")]
		[ControlDisplay(60, "ScheduleRulesScheduledTime", ControlTypeEnum.Time)]
		[TimeValidate()] // ErrorMessage is defaulted in TimeValidateAttribute.cs
		[Required(ErrorMessage = "Scheduled time is required.")]
		[FormParse(ParseType.UTCTime)]
		[Placeholder("hh:mm 24hr")]
		public string ScheduledTime { get; set; }


	}
}