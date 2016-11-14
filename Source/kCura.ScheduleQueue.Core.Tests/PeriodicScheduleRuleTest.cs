using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NUnit.Framework;

namespace kCura.ScheduleQueue.Core.Tests
{
	[TestFixture]
	internal class PeriodicScheduleRuleTest : TestBase
	{
		#region SearchMonthForForwardOccuranceOfDay

		private const string dailyRuleOldXML = @"<PeriodicScheduleRule xmlns=""http://schemas.datacontract.org/2004/07/kCura.Method.Data.ScheduleRules"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><DayOfMonth i:nil=""true""/><DaysToRun i:nil=""true""/><EndDate i:nil=""true""/><Interval>Daily</Interval><SetLastDayOfMonth i:nil=""true""/><StartDate>2010-12-29T00:00:00</StartDate><localTimeOfDayTicks>450600000000</localTimeOfDayTicks></PeriodicScheduleRule>";

		private const string weeklyRuleOldXML = @"<PeriodicScheduleRule xmlns=""http://schemas.datacontract.org/2004/07/kCura.Method.Data.ScheduleRules"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><DayOfMonth i:nil=""true""/><DaysToRun>Monday Friday</DaysToRun><EndDate i:nil=""true""/><Interval>Weekly</Interval><SetLastDayOfMonth i:nil=""true""/><StartDate>2010-12-29T00:00:00</StartDate><localTimeOfDayTicks>450600000000</localTimeOfDayTicks></PeriodicScheduleRule>";

		private const string monthlyRuleOldXML = @"<PeriodicScheduleRule xmlns=""http://schemas.datacontract.org/2004/07/kCura.Method.Data.ScheduleRules"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><DayOfMonth>15</DayOfMonth><DaysToRun i:nil=""true""/><EndDate i:nil=""true""/><Interval>Monthly</Interval><SetLastDayOfMonth i:nil=""true""/><StartDate>2010-12-29T00:00:00</StartDate><localTimeOfDayTicks>450600000000</localTimeOfDayTicks></PeriodicScheduleRule>";

		[SetUp]
		public override void SetUp()
		{
			
		}

		[Test]
		[Explicit]
		public void SerializeTest()
		{
			var rule = new PeriodicScheduleRule(ScheduleInterval.Daily, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			string xml = rule.ToString();
			rule = ScheduleRuleBase.Deserialize<PeriodicScheduleRule>(xml);

			rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, 0, null, 10);
			xml = rule.ToString();
			rule = ScheduleRuleBase.Deserialize<PeriodicScheduleRule>(xml);

			rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, 0, null, null, true);
			xml = rule.ToString();
			rule = ScheduleRuleBase.Deserialize<PeriodicScheduleRule>(xml);
		}

		[Test]
		public void LocalTimeOfDay_SetGet_ValidateCorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Immediate, DateTime.Parse("01/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("01/29/2010 10:00:00");

			DateTime expectedTime = DateTime.Parse("01/29/2010 12:31");

			var result = rule.LocalTimeOfDay;

			Assert.AreEqual(expectedTime.TimeOfDay, result);
		}

		[Test]
		public void LocalTimeOfDay_SetJanGetInJuly_ValidateCorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Immediate, DateTime.Parse("01/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("01/29/2010 10:00:00");
			DateTime expectedTime = DateTime.Parse("01/29/2010 12:31");

			var result = rule.LocalTimeOfDay;

			Assert.AreEqual(expectedTime.TimeOfDay, result);
		}

		[Test]
		public void GetNextUTCRunDateTime_ImmediateStartDateTimeAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Immediate, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("12/28/2010 10:00:00");
			DateTime expectedTime = DateTime.Parse("12/29/2010 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime().AddMinutes(3), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_ImmediateStartDateBeforeTimeAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Immediate, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("12/30/2010 10:00:00");
			DateTime expectedTime = DateTime.Parse("12/30/2010 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime().AddMinutes(3), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_ImmediateStartDateTimeBeforeNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Immediate, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			var utcNow = DateTime.Parse("12/30/2010 21:00:00");
			rule.ArrangeTimeServiceBaseOnUtcNow("12/30/2010 21:00:00");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(utcNow.AddMinutes(3), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_HourlyStartDateTimeAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Hourly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("12/28/2010 10:00:00");

			DateTime expectedTime = DateTime.Parse("12/29/2010 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_HourlyStartDateBeforeTimeAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Hourly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("12/30/2010 10:00:00");

			DateTime expectedTime = DateTime.Parse("12/30/2010 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_HourlyStartDateTimeBeforeNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Hourly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("12/30/2010 21:00:00");

			DateTime expectedTime = DateTime.Parse("12/30/2010 21:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void GetNextUTCRunDateTime_DailyStartDateTimeAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Daily, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("12/28/2010 10:00:00");
			DateTime expectedTime = DateTime.Parse("12/29/2010 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_DailyStartDateSameTimeAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Daily, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("12/29/2010 10:00:00");
			DateTime expectedTime = DateTime.Parse("12/29/2010 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_DailyStartDateBeforeTimeAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Daily, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("12/30/2010 10:00:00");
			DateTime expectedTime = DateTime.Parse("12/30/2010 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_DailyStartDateTimeBeforeNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Daily, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"));
			rule.ArrangeTimeServiceBaseOnUtcNow("12/30/2010 21:00:00");
			DateTime expectedTime = DateTime.Parse("12/31/2010 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_WeeklyMondaysOnlyStartDateTimeThursdayBeforeNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday);
			rule.ArrangeTimeServiceBaseOnUtcNow("12/30/2010 21:00:00"); //--Thursday

			DateTime expectedTime = DateTime.Parse("01/03/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
			Assert.AreEqual(DayOfWeek.Monday, result.Value.DayOfWeek);
		}

		[Test]
		public void GetNextUTCRunDateTime_WeeklyMondaysOnlyStartDateTimeMondayBeforeNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday);
			rule.ArrangeTimeServiceBaseOnUtcNow("01/03/2011 10:00:00"); //--Monday

			DateTime expectedTime = DateTime.Parse("01/03/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
			Assert.AreEqual(DayOfWeek.Monday, result.Value.DayOfWeek);
		}

		[Test]
		public void GetNextUTCRunDateTime_WeeklyMondaysOnlyStartDateTimeMondayAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday);
			rule.ArrangeTimeServiceBaseOnUtcNow("01/03/2011 21:00:00"); //--Monday

			DateTime expectedTime = DateTime.Parse("01/10/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
			Assert.AreEqual(DayOfWeek.Monday, result.Value.DayOfWeek);
		}

		[Test]
		public void GetNextUTCRunDateTime_WeeklyMondaysFridaysOnlyNowDateTimeTuesday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday | DaysOfWeek.Friday);
			rule.ArrangeTimeServiceBaseOnUtcNow("01/04/2011 21:00:00"); //--Tuesday

			DateTime expectedTime = DateTime.Parse("01/07/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
			Assert.AreEqual(DayOfWeek.Friday, result.Value.DayOfWeek);
		}

		[Test]
		public void GetNextUTCRunDateTime_WeeklyMondaysFridaysOnlyNowDateTimeSaturday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday | DaysOfWeek.Friday);
			rule.ArrangeTimeServiceBaseOnUtcNow("01/08/2011 21:00:00"); //--Saturday
			DateTime expectedTime = DateTime.Parse("01/10/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
			Assert.AreEqual(DayOfWeek.Monday, result.Value.DayOfWeek);
		}

		[Test]
		public void GetNextUTCRunDateTime_Every15DayMonthlyStartDateTimeBeforeNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, null, 15);
			rule.ArrangeTimeServiceBaseOnUtcNow("12/30/2010 10:00:00");

			DateTime expectedTime = DateTime.Parse("01/15/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
			Assert.AreEqual(15, result.Value.Day);
		}

		[Test]
		public void GetNextUTCRunDateTime_Every31DayMonthlyIn28DayMonth_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, null, 31);
			rule.ArrangeTimeServiceBaseOnUtcNow("02/01/2011 10:00:00");

			DateTime expectedTime = DateTime.Parse("02/28/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_Every31DayMonthlyIn29DayMonth_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, null, 31);
			rule.ArrangeTimeServiceBaseOnUtcNow("02/01/2012 10:00:00");

			DateTime expectedTime = DateTime.Parse("02/29/2012 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_Every31DayMonthlyIn30DayMonth_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, null, 31);
			rule.ArrangeTimeServiceBaseOnUtcNow("04/01/2011 10:00:00");

			DateTime expectedTime = DateTime.Parse("04/30/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_Every31DayMonthlyIn31DayMonth_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, null, 31);
			rule.ArrangeTimeServiceBaseOnUtcNow("05/01/2011 10:00:00");
			DateTime expectedTime = DateTime.Parse("05/31/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_Every15DayMonthlyStartDateLaterThenNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, null, 15);
			rule.ArrangeTimeServiceBaseOnUtcNow("05/01/2010 10:00:00");
			DateTime expectedTime = DateTime.Parse("01/15/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_NextRunTimeDateBeforeEndDate_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), DateTime.Parse("2/1/2011"), null, null, 15);
			rule.ArrangeTimeServiceBaseOnUtcNow("05/01/2010 10:00:00");

			var result = rule.GetNextUTCRunDateTime();

			Assert.IsNotNull(result);
		}

		[Test]
		public void GetNextUTCRunDateTime_NextRunTimeDateOnEndDate_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), DateTime.Parse("1/15/2011"), 0, null, 15);
			rule.ArrangeTimeServiceBaseOnUtcNow("05/01/2010 10:00:00");

			var result = rule.GetNextUTCRunDateTime();

			Assert.IsNotNull(result);
		}

		[Test]
		public void GetNextUTCRunDateTime_NextRunTimeDateAfterEndDate_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), DateTime.Parse("1/1/2011"), null, null, 15);
			rule.ArrangeTimeServiceBaseOnUtcNow("05/01/2010 10:00:00");
			var result = rule.GetNextUTCRunDateTime();

			Assert.IsNull(result);
		}

		[Test]
		public void GetNextUTCRunDateTime_NextRunTimeDateNextDayAfterEndDate_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), DateTime.Parse("1/14/2011"), null, null, 15);
			rule.ArrangeTimeServiceBaseOnUtcNow("12/29/2010 22:00:00");

			var result = rule.GetNextUTCRunDateTime();

			Assert.IsNull(result);
		}

		[Test]
		public void GetNextUTCRunDateTime_DailyRuleMigration_CorrectValue()
		{
			//TODO: when we start using this scheduler in Method, we need to convert namespaces in serialized xml:
			string xml = dailyRuleOldXML.Replace("kCura.Method.Data.ScheduleRules", "kCura.ScheduleQueue.Core.ScheduleRules");
			PeriodicScheduleRule rule = (PeriodicScheduleRule)SerializerHelper.DeserializeUsingTypeName(System.AppDomain.CurrentDomain, typeof(PeriodicScheduleRule).FullName, xml);
			//PeriodicScheduleRule rule = ScheduleRuleBase.Deserialize<PeriodicScheduleRule>(xml);
			rule.ArrangeTimeServiceBaseOnUtcNow("12/30/2010 21:00:00");
			DateTime expectedTime = DateTime.Parse("12/31/2010 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_WeeklyRuleMigration_CorrectValue()
		{
			//TODO: when we start using this scheduler in Method, we need to convert namespaces in serialized xml:
			string xml = weeklyRuleOldXML.Replace("kCura.Method.Data.ScheduleRules", "kCura.ScheduleQueue.Core.ScheduleRules");
			PeriodicScheduleRule rule = (PeriodicScheduleRule)SerializerHelper.DeserializeUsingTypeName(System.AppDomain.CurrentDomain, typeof(PeriodicScheduleRule).FullName, xml);
			//PeriodicScheduleRule rule = ScheduleRuleBase.Deserialize<PeriodicScheduleRule>(xml);
			rule.ArrangeTimeServiceBaseOnUtcNow("01/08/2011 21:00:00"); //--Saturday
			DateTime expectedTime = DateTime.Parse("01/10/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_MonthlyRuleMigration_CorrectValue()
		{
			//TODO: when we start using this scheduler in Method, we need to convert namespaces in serialized xml:
			string xml = monthlyRuleOldXML.Replace("kCura.Method.Data.ScheduleRules", "kCura.ScheduleQueue.Core.ScheduleRules");
			PeriodicScheduleRule rule = (PeriodicScheduleRule)SerializerHelper.DeserializeUsingTypeName(System.AppDomain.CurrentDomain, typeof(PeriodicScheduleRule).FullName, xml);
			//PeriodicScheduleRule rule = ScheduleRuleBase.Deserialize<PeriodicScheduleRule>(xml);
			rule.ArrangeTimeServiceBaseOnUtcNow("05/01/2010 10:00:00");

			DateTime expectedTime = DateTime.Parse("01/15/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_WeeklyMondaysSaturdaysStartDateTimeWednesdayAfterNowReoccur2_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday | DaysOfWeek.Saturday, null, null, 2);
			rule.ArrangeTimeServiceBaseOnUtcNow("01/05/2011 21:00:00"); //--Wednesday
			DateTime expectedTime = DateTime.Parse("01/08/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
			Assert.AreEqual(DayOfWeek.Saturday, result.Value.DayOfWeek);
		}

		[Test]
		public void GetNextUTCRunDateTime_WeeklyMondaysSaturdaysStartDateTimeSundayAfterNowReoccur2_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, DateTime.Parse("12/29/2010"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday | DaysOfWeek.Saturday, null, null, 2);
			rule.ArrangeTimeServiceBaseOnUtcNow("01/09/2011 21:00:00"); //--Sunday
			DateTime expectedTime = DateTime.Parse("01/17/2011 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
			Assert.AreEqual(DayOfWeek.Monday, result.Value.DayOfWeek);
		}

		[Test]
		public void GetNextUTCRunDateTime_MonthlyFirstMondaysReoccurEveryMonthStartDateBeforeNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("9/15/2014"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday, null, null, 1, OccuranceInMonth.First);
			rule.ArrangeTimeServiceBaseOnUtcNow("10/01/2014 21:00:00");
			DateTime expectedTime = DateTime.Parse("10/06/2014 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_MonthlyFirstMondaysReoccurEveryMonthStartDateAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/15/2014"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday, null, null, 1, OccuranceInMonth.First);
			rule.ArrangeTimeServiceBaseOnUtcNow("10/01/2014 21:00:00");
			DateTime expectedTime = DateTime.Parse("11/03/2014 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_MonthlySecondTuesdayReoccurEveryMonthStartDateAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/15/2014"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Tuesday, null, null, 1, OccuranceInMonth.Second);
			rule.ArrangeTimeServiceBaseOnUtcNow("10/01/2014 21:00:00");
			DateTime expectedTime = DateTime.Parse("11/11/2014 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_MonthlyFourthWendesdayReoccurEvery3MonthStartDateAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/25/2014"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Wednesday, null, null, 3, OccuranceInMonth.Fourth);
			rule.ArrangeTimeServiceBaseOnUtcNow("10/24/2014 21:00:00");
			DateTime expectedTime = DateTime.Parse("1/28/2015 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_MonthlyThirdSaturdayReoccurEvery3MonthStartDateAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/15/2014"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Saturday, null, null, 3, OccuranceInMonth.Third);
			rule.ArrangeTimeServiceBaseOnUtcNow("10/01/2014 21:00:00");
			DateTime expectedTime = DateTime.Parse("10/18/2014 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[Test]
		public void GetNextUTCRunDateTime_MonthlyLastFridayReoccurEvery3MonthStartDateAfterNow_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/15/2014"), TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Friday, null, null, 3, OccuranceInMonth.Last);
			rule.ArrangeTimeServiceBaseOnUtcNow("10/01/2014 21:00:00");
			DateTime expectedTime = DateTime.Parse("10/31/2014 12:31");

			var result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime.ToUniversalTime(), result);
		}

		[TestCase("10/01/2014 21:00:00", ScheduleInterval.Monthly, "02/01/2016", "12:31", null, 0, DaysOfWeek.Monday, null, null, 1, OccuranceInMonth.Last, "02/29/2016")]
		[TestCase("10/01/2014 21:00:00", ScheduleInterval.Monthly, "02/01/2016", "12:31", null, 0, null, 30, null, 1, null, "02/29/2016")]
		[TestCase("10/01/2014 21:00:00", ScheduleInterval.Weekly, "02/23/2016", "12:31", null, 0, DaysOfWeek.Monday, null, null, 1, null, "02/29/2016")]
		public void GetNextUTCRunDateTime_CorrectValue(string utcNowTime, ScheduleInterval interval, string startDate, string scheduledLocalTime, DateTime? endDate, int timeZoneOffSet, DaysOfWeek? daysToRun, int? dayOfMonth, bool? setLastDay, int? reoccur, OccuranceInMonth? occuranceInMonth, string expectedDate)
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule(interval, DateTime.Parse(startDate), TimeSpan.Parse(scheduledLocalTime), endDate, timeZoneOffSet, daysToRun, dayOfMonth, setLastDay, reoccur, occuranceInMonth);
			var utcNow = DateTime.Parse(utcNowTime);
			rule.TimeService = NSubstitute.Substitute.For<ITimeService>();
			rule.TimeService.UtcNow.ReturnsForAnyArgs(utcNow);
			DateTime expectedTime = DateTime.Parse(expectedDate + " " + scheduledLocalTime);

			DateTime? result = rule.GetNextUTCRunDateTime();

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void SearchMonthForForwardOccuranceOfDay_FirstMonday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/3/2014");

			var result = rule.SearchMonthForForwardOccuranceOfDay(2014, 2, ForwardValidOccurance.First, DayOfWeek.Monday);

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void SearchMonthForForwardOccuranceOfDay_ForthMonday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/24/2014");

			var result = rule.SearchMonthForForwardOccuranceOfDay(2014, 2, ForwardValidOccurance.Fourth, DayOfWeek.Monday);

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void SearchMonthForForwardOccuranceOfDay_FirstSaturday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/1/2014");

			var result = rule.SearchMonthForForwardOccuranceOfDay(2014, 2, ForwardValidOccurance.First, DayOfWeek.Saturday);

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void SearchMonthForForwardOccuranceOfDay_FourthFriday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/28/2014");

			var result = rule.SearchMonthForForwardOccuranceOfDay(2014, 2, ForwardValidOccurance.Fourth, DayOfWeek.Friday);

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void SearchMonthForForwardOccuranceOfDay_FirstWednesday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/1/2012");

			var result = rule.SearchMonthForForwardOccuranceOfDay(2012, 2, ForwardValidOccurance.First, DayOfWeek.Wednesday);

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void SearchMonthForForwardOccuranceOfDay_FourthWednesday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/22/2012");

			var result = rule.SearchMonthForForwardOccuranceOfDay(2012, 2, ForwardValidOccurance.Fourth, DayOfWeek.Wednesday);

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void SearchMonthForForwardOccuranceOfDay_FourthTuesday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/28/2012");

			var result = rule.SearchMonthForForwardOccuranceOfDay(2012, 2, ForwardValidOccurance.Fourth, DayOfWeek.Tuesday);

			Assert.AreEqual(expectedTime, result);
		}

		#region SearchMonthForLastOccuranceOfDay

		#endregion SearchMonthForForwardOccuranceOfDay

		[Test]
		public void SearchMonthForLastOccuranceOfDay_LastMonday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/24/2014");

			var result = rule.SearchMonthForLastOccuranceOfDay(2014, 2, DayOfWeek.Monday);

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void SearchMonthForLastOccuranceOfDay_LastFriday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/28/2014");

			var result = rule.SearchMonthForLastOccuranceOfDay(2014, 2, DayOfWeek.Friday);

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void SearchMonthForLastOccuranceOfDay_LastWednesday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/29/2012");

			var result = rule.SearchMonthForLastOccuranceOfDay(2012, 2, DayOfWeek.Wednesday);

			Assert.AreEqual(expectedTime, result);
		}

		[Test]
		public void SearchMonthForLastOccuranceOfDay_LastTuesday_CorrectValue()
		{
			PeriodicScheduleRule rule = new PeriodicScheduleRule();
			DateTime expectedTime = DateTime.Parse("2/28/2012");

			var result = rule.SearchMonthForLastOccuranceOfDay(2012, 2, DayOfWeek.Tuesday);

			Assert.AreEqual(expectedTime, result);
		}

		#region ForwardValidOccurance

		#endregion SearchMonthForLastOccuranceOfDay

		[Test]
		public void ForwardValidOccurance_CorrectValue()
		{
			Assert.AreEqual((int)OccuranceInMonth.First, (int)ForwardValidOccurance.First);
			Assert.AreEqual((int)OccuranceInMonth.Second, (int)ForwardValidOccurance.Second);
			Assert.AreEqual((int)OccuranceInMonth.Third, (int)ForwardValidOccurance.Third);
			Assert.AreEqual((int)OccuranceInMonth.Fourth, (int)ForwardValidOccurance.Fourth);
		}

		#region ForwardValidOccurance

		#endregion ForwardValidOccurance

		[Test]
		public void DaysOfWeekMap_CorrectValue()
		{
			Assert.AreEqual(DayOfWeek.Monday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Monday]);
			Assert.AreEqual(DayOfWeek.Tuesday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Tuesday]);
			Assert.AreEqual(DayOfWeek.Wednesday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Wednesday]);
			Assert.AreEqual(DayOfWeek.Thursday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Thursday]);
			Assert.AreEqual(DayOfWeek.Friday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Friday]);
			Assert.AreEqual(DayOfWeek.Saturday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Saturday]);
			Assert.AreEqual(DayOfWeek.Sunday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Sunday]);
		}

		#endregion ForwardValidOccurance

		[TestCase("Tokyo Standard Time", "12:30 PM", "Central Standard Time", "9/13/2016 3:30 AM")]
		[TestCase("Central Standard Time", "12:30 PM", "Tokyo Standard Time", "9/13/2016 6:30 PM")]
		[TestCase("Tokyo Standard Time", "7:30 AM", "Central Standard Time", "9/12/2016 10:30 PM")]
		[TestCase("Central Standard Time", "11:30 PM", "Tokyo Standard Time", "9/14/2016 5:30 AM")]
		[TestCase("Nepal Standard Time", "10:30 PM", "Tokyo Standard Time", "9/13/2016 4:45 PM")]           //Nepal Standard Time (UTC+05:45)
		[TestCase("AUS Central Standard Time", "8:00 AM", "Tokyo Standard Time", "9/12/2016 10:30 PM")]     //AUS Central Standard Time (UTC+09:30)
		public void CalculateLastDayOfScheduledJob(string clientTimeZone, string clientLocalTime, string serverTimeZone, string expectedRunUtcTime)
		{
			DateTime date = DateTime.Parse("9/13/2016");

			// arrange
			// client time
			TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
			TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;
			DateTime clientTime = date.Add(clientlocalTime);
			//For tesat purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
			TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

			// server time
			TimeZoneInfo serverTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(serverTimeZone);
			TimeSpan serverClientOffSet = clientUtcOffSet.Add(serverTimeZoneInfo.BaseUtcOffset);
			DateTime serverLocalTime = clientTime.Add(serverClientOffSet);

			PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Daily, date, clientlocalTime, date, (int)clientUtcOffSet.TotalMinutes)
			{
				TimeService = Substitute.For<ITimeService>()
			};
			rule.TimeService.UtcNow.Returns(serverLocalTime.Add(-serverTimeZoneInfo.BaseUtcOffset));
			rule.TimeService.LocalTime.Returns(serverLocalTime);

			// act
			DateTime? nextRunTime = rule.GetNextUTCRunDateTime(null, TaskStatusEnum.None);

			// assert
			if (expectedRunUtcTime == null)
			{
				Assert.IsNull(nextRunTime);
			}
			else
			{
				Assert.IsNotNull(nextRunTime);
				Assert.AreEqual(DateTime.Parse(expectedRunUtcTime), nextRunTime);
			}
		}

		//server ahead

		[TestCase(ScheduleInterval.Daily, "9/15/2016", "10/15/2016", "9/15/2016", "9/17/2016", null, null, null, null, null)]
		[TestCase(ScheduleInterval.Daily, "9/15/2016", "10/15/2016", "9/15/2016", null, "10/15/2016", null, null, null, null)]
		[TestCase(ScheduleInterval.Daily, "9/15/2016", "10/15/2016", "11/15/2016", null, "11/15/2016", null, null, null, null)]
		[TestCase(ScheduleInterval.Weekly, "9/15/2016", "10/15/2016", "9/15/2016", "9/17/2016", null, 1, DaysOfWeek.Monday | DaysOfWeek.Sunday, null, null)]
		[TestCase(ScheduleInterval.Weekly, "9/15/2016", "10/15/2016", "9/15/2016", null, "10/16/2016", 1, DaysOfWeek.Monday | DaysOfWeek.Sunday, null, null)]
		[TestCase(ScheduleInterval.Weekly, "9/15/2016", "10/15/2016", "11/15/2016", null, "11/20/2016", 1, DaysOfWeek.Monday | DaysOfWeek.Sunday, null, null)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "10/15/2016", "9/15/2016", "9/17/2016", null, 1, null, 8, null)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "10/15/2016", "9/15/2016", null, "11/8/2016", 1, null, 8, null)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "10/15/2016", "11/15/2016", null, "12/8/2016", 1, null, 8, null)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "10/15/2016", "9/15/2016", "9/17/2016", null, 1, DaysOfWeek.Thursday, null, OccuranceInMonth.Fourth)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "10/15/2016", "9/15/2016", null, "10/27/2016", 1, DaysOfWeek.Thursday, null, OccuranceInMonth.Fourth)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "10/15/2016", "11/15/2016", null, "11/24/2016", 1, DaysOfWeek.Thursday, null, OccuranceInMonth.Fourth)]
		[TestCase(ScheduleInterval.Monthly, "1/15/2016", "2/15/2016", "1/15/2016", "3/1/2016", "2/29/2016", 1, DaysOfWeek.Monday, null, OccuranceInMonth.Last)]
		[TestCase(ScheduleInterval.Monthly, "1/15/2016", "2/15/2016", "1/15/2016", "3/1/2016", "2/29/2016", 1, DaysOfWeek.Day, null, OccuranceInMonth.Last)]
		//client ahead
		[TestCase(ScheduleInterval.Daily, "9/15/2016", "8/15/2016", "9/15/2016", "9/17/2016", "9/15/2016", null, null, null, null)]
		[TestCase(ScheduleInterval.Daily, "9/15/2016", "8/15/2016", "7/15/2016", null, "8/15/2016", null, null, null, null)]
		[TestCase(ScheduleInterval.Daily, "9/15/2016", "8/15/2016", "7/15/2016", "7/17/2016", null, null, null, null, null)]
		[TestCase(ScheduleInterval.Weekly, "9/15/2016", "8/15/2016", "9/15/2016", "9/17/2016", null, 1, DaysOfWeek.Monday | DaysOfWeek.Sunday, null, null)]
		[TestCase(ScheduleInterval.Weekly, "9/15/2016", "8/15/2016", "9/15/2016", "9/20/2016", "9/18/2016", 1, DaysOfWeek.Monday | DaysOfWeek.Sunday, null, null)]
		[TestCase(ScheduleInterval.Weekly, "9/15/2016", "8/15/2016", "7/15/2016", null, "8/15/2016", 1, DaysOfWeek.Monday | DaysOfWeek.Sunday, null, null)]
		[TestCase(ScheduleInterval.Weekly, "9/15/2016", "8/15/2016", "7/15/2016", "7/17/2016", null, 1, DaysOfWeek.Monday | DaysOfWeek.Sunday, null, null)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "8/15/2016", "9/15/2016", "9/17/2016", null, 1, null, 8, null)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "8/15/2016", "9/8/2016", "9/8/2016", "9/8/2016", 1, null, 8, null)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "8/15/2016", "7/15/2016", null, "9/8/2016", 1, null, 8, null)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "8/15/2016", "7/15/2016", "7/17/2016", null, 1, null, 8, null)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "8/15/2016", "9/15/2016", "9/17/2016", null, 1, DaysOfWeek.Thursday, null, OccuranceInMonth.Fourth)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "8/15/2016", "9/15/2016", null, "9/22/2016", 1, DaysOfWeek.Thursday, null, OccuranceInMonth.Fourth)]
		[TestCase(ScheduleInterval.Monthly, "9/15/2016", "8/15/2016", "7/15/2016", null, "8/25/2016", 1, DaysOfWeek.Thursday, null, OccuranceInMonth.Fourth)]
		[TestCase(ScheduleInterval.Monthly, "3/15/2016", "2/15/2016", "2/15/2016", "3/1/2016", "2/29/2016", 1, DaysOfWeek.Monday, null, OccuranceInMonth.Last)]
		[TestCase(ScheduleInterval.Monthly, "3/15/2016", "2/15/2016", "2/15/2016", "3/1/2016", "2/29/2016", 1, DaysOfWeek.Day, null, OccuranceInMonth.Last)]
		public void GetNextRunDate_ClientAndServerOnDifferentDates(
			ScheduleInterval frequency,
			string clientDate,
			string serverDate,
			string startDate,
			string endDate,
			string expectedNextRunTime,
			int? reoccur,
			DaysOfWeek? daysToRun,
			int? monthlySendOnDayOfMonth,
			OccuranceInMonth? monthlySendOnOccurenceInMonth)
		{
			// arrange
			TimeSpan clientTimeOfDay = DateTime.Parse("12:00 PM").TimeOfDay;

			DateTime serverDateTime = DateTime.Parse(serverDate);
			TimeSpan serverDateTimeTime = DateTime.Parse("12:00 PM").TimeOfDay;
			serverDateTime = serverDateTime.Date + serverDateTimeTime;

			DateTime startDateTime = DateTime.Parse(startDate);
			DateTime? endDateTime = String.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate);

			PeriodicScheduleRule rule = new PeriodicScheduleRule(
				interval: frequency,
				startDate: startDateTime,
				localTimeOfDay: clientTimeOfDay,
				endDate: endDateTime,
				timeZoneOffset: null,
				daysToRun: daysToRun,
				dayOfMonth: monthlySendOnDayOfMonth,
				setLastDayOfMonth: daysToRun == DaysOfWeek.Day && monthlySendOnOccurenceInMonth == OccuranceInMonth.Last ? true : (bool?)null,
				reoccur: reoccur,
				occuranceInMonth: monthlySendOnOccurenceInMonth)
			{
				TimeService = Substitute.For<ITimeService>()
			};
			rule.TimeService.LocalTime.Returns(serverDateTime);

			// act
			DateTime? nextRunTime = rule.GetNextUTCRunDateTime(null, TaskStatusEnum.None);

			// assert
			if (expectedNextRunTime == null)
			{
				Assert.IsNull(nextRunTime);
			}
			else
			{
				Assert.IsNotNull(nextRunTime);
				Assert.AreEqual(DateTime.Parse(expectedNextRunTime).Date, nextRunTime.Value.Date);
			}
		}
	}

	public static class ExtendedPeriodicScheduleRule
	{
		public static void ArrangeTimeServiceBaseOnUtcNow(this PeriodicScheduleRule rule, string utc)
		{
			var utcNow = DateTime.Parse(utc);
			rule.TimeService = Substitute.For<ITimeService>();
			rule.TimeService.UtcNow.Returns(utcNow);
			rule.TimeService.LocalTime.Returns(utcNow.ToLocalTime());
		}
	}
}