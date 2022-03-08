using System;
using System.Globalization;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NUnit.Framework;

namespace kCura.ScheduleQueue.Core.Tests
{
    [TestFixture, Category("Unit")]
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

        [TestCase("02/01/2016 21:00:00", ScheduleInterval.Monthly, "02/01/2016", "12:31", null, 0, DaysOfWeek.Monday, null, null, 1, OccuranceInMonth.Last, "02/29/2016")]
        [TestCase("02/01/2016 21:00:00", ScheduleInterval.Monthly, "02/01/2016", "12:31", null, 0, null, 30, null, 1, null, "02/29/2016")]
        [TestCase("02/23/2016 21:00:00", ScheduleInterval.Weekly, "02/23/2016", "12:31", null, 0, DaysOfWeek.Monday, null, null, 1, null, "02/29/2016")]
        public void GetNextUTCRunDateTime_CorrectValue(string utcNowTime, ScheduleInterval interval, string startDate, string scheduledLocalTime, DateTime? endDate, int timeZoneOffSet, DaysOfWeek? daysToRun, int? dayOfMonth, bool? setLastDay, int? reoccur, OccuranceInMonth? occuranceInMonth, string expectedDate)
        {
            const string timeZoneId = "UTC";
            PeriodicScheduleRule rule = new PeriodicScheduleRule(interval, DateTime.Parse(startDate), TimeSpan.Parse(scheduledLocalTime), endDate, timeZoneOffSet, daysToRun, dayOfMonth, setLastDay, reoccur, occuranceInMonth, timeZoneId);
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

        [TestCase("Tokyo Standard Time", "9/13/2016", "12:30 PM", "Central Standard Time", "9/13/2016 3:30 AM")]
        [TestCase("Central Standard Time", "9/13/2016", "12:30 PM", "Tokyo Standard Time", "9/13/2016 5:30 PM")]
        [TestCase("Tokyo Standard Time", "9/13/2016", "7:30 AM", "Central Standard Time", "9/12/2016 10:30 PM")]
        [TestCase("Central Standard Time", "9/12/2016", "11:30 PM", "Tokyo Standard Time", "9/14/2016 4:30 AM")]
        [TestCase("Nepal Standard Time", "9/13/2016","10:30 PM", "Tokyo Standard Time", "9/13/2016 4:45 PM")]           //Nepal Standard Time (UTC+05:45)
        [TestCase("AUS Central Standard Time", "9/13/2016","8:00 AM", "Tokyo Standard Time", "9/12/2016 10:30 PM")]     //AUS Central Standard Time (UTC+09:30)
        public void CalculateLastDayOfScheduledDailyJob(string clientTimeZone, string clientDate, string clientLocalTime, string serverTimeZone, string expectedRunUtcTime)
        {
            DateTime date = DateTime.Parse("9/13/2016");

            // arrange
            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;
            DateTime clientTime = DateTime.Parse(clientDate).Add(clientlocalTime);
            //For tesat purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

            // server time
            TimeZoneInfo serverTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(serverTimeZone);
            TimeSpan serverClientOffSet = clientUtcOffSet.Add(serverTimeZoneInfo.BaseUtcOffset);
            const int serverTimeShift = -2;     //To set server time before expectedRunUtcTime
            DateTime serverLocalTime = clientTime.Add(serverClientOffSet).AddHours(serverTimeShift);

            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Daily, date, clientlocalTime, date, (int)clientUtcOffSet.TotalMinutes, null, null, null, null, null, clientTimeZone)
            {
                TimeService = Substitute.For<ITimeService>()
            };
            rule.TimeService.UtcNow.Returns(serverLocalTime.Add(-serverTimeZoneInfo.BaseUtcOffset));            
            rule.TimeZoneId = clientTimeZone;

            // act
            DateTime? nextRunTime = rule.GetNextUTCRunDateTime();

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


        [TestCase("Eastern Standard Time", "11:00 PM", "10/13/2016", "Eastern Standard Time", "10/20/2016 3:00 AM")]
        [TestCase("Central Standard Time", "11:00 PM", "10/13/2016", "Central Standard Time", "10/20/2016 4:00 AM")]
        [TestCase("Central Standard Time", "11:00 PM", "10/13/2016", "Central European Standard Time", "10/20/2016 4:00 AM")]
        [TestCase("Central Standard Time", "3:00 AM", "10/13/2016", "Central Standard Time", "10/19/2016 8:00 AM")]
        [TestCase("Central European Standard Time", "6:00 AM", "10/13/2016", "Central Standard Time", "10/19/2016 4:00 AM")]
        [TestCase("Central European Standard Time", "6:00 AM", "10/13/2016", "Central European Standard Time", "10/19/2016 4:00 AM")]
        [TestCase("Central European Standard Time", "1:00 AM", "10/13/2016", "Central European Standard Time", "10/18/2016 11:00 PM")]
        [TestCase("Central European Standard Time", "2:00 PM", "10/13/2016", "Central European Standard Time", "10/19/2016 12:00 PM")]
        [TestCase("Central European Standard Time", "2:00 PM", "10/13/2016", "Tokyo Standard Time", "10/19/2016 12:00 PM")]
        [TestCase("Tokyo Standard Time", "11:00 AM", "10/13/2016", "Central European Standard Time", "10/19/2016 2:00 AM")]
        [TestCase("Tokyo Standard Time", "11:00 AM", "10/13/2016", "Central Standard Time", "10/19/2016 2:00 AM")]
        [TestCase("Tokyo Standard Time", "4:00 AM", "10/13/2016", "Central European Standard Time", "10/18/2016 7:00 PM")]
        [TestCase("Tokyo Standard Time", "4:00 AM", "10/13/2016", "Central Standard Time", "10/18/2016 7:00 PM")]
        [TestCase("GMT Standard Time", "5:00 AM", "10/13/2016", "GMT Standard Time", "10/19/2016 4:00 AM")]
        [TestCase("UTC", "7:00 AM", "10/13/2016", "UTC", "10/19/2016 7:00 AM")]
        [TestCase("Central European Standard Time", "5:00 AM", "11/3/2016", "Central Standard Time", "11/9/2016 4:00 AM")]
        [TestCase("Central Standard Time", "11:00 PM", "10/27/2016", "Central Standard Time", "11/03/2016 4:00 AM")]
        [TestCase("Central Standard Time", "11:00 PM", "11/3/2016", "Central European Standard Time", "11/10/2016 5:00 AM")]
        [TestCase("Central European Standard Time", "6:30 AM", "10/25/2016", "Central Standard Time", "10/26/2016 4:30 AM")]
        [TestCase("Central European Standard Time", "12:45 AM", "10/25/2016", "Central Standard Time", "10/25/2016 10:45 PM")]
        // Daylight Saving Time (DST) changes
        [TestCase("Central Standard Time", "11:15 PM", "03/10/2016", "Central Standard Time", "03/17/2016 4:15 AM")]    //stat day: 11:15PM 03/10/2016 CST = 5:15AM 03/11/2016 UTC;		CST → CDT change 03/12/2016 
        [TestCase("Central European Standard Time", "1:20 AM", "03/25/2016", "Central Standard Time", "03/29/2016 11:20 PM")]    //stat day: 1:20AM 03/25/2016 CET = 12:20AM 03/25/2016 UTC;		CET → CEST change 03/26/2016 
        [TestCase("Central European Standard Time", "2:25 AM", "10/28/2016", "Central Standard Time", "11/2/2016 1:25 AM")]    //stat day: 2:25AM 10/28/2016 CEST = 12:25AM 10/28/2016 UTC;		CEST → CET change 10/30/2016
        public void CalculateLastDayOfScheduledWeeklyJob(string clientTimeZone, string clientLocalTime, string clientStartDate, string serverTimeZone, string expectedRunUtcTime)
        {
            // arrange
            DateTime startDate = DateTime.Parse(clientStartDate);
            DateTime endDate = startDate.AddDays(8);
            const DaysOfWeek dayToRun = DaysOfWeek.Wednesday;

            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;
            DateTime clientTime = startDate.Add(clientlocalTime);
            //For tesat purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

            // server time
            TimeZoneInfo serverTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(serverTimeZone);
            TimeSpan serverClientOffSet = clientUtcOffSet.Add(serverTimeZoneInfo.BaseUtcOffset);
            const int serverTimeShift = -2;     //To set server time before expectedRunUtcTime
            DateTime serverLocalTime = clientTime.Add(serverClientOffSet).AddHours(serverTimeShift);

            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, startDate, clientlocalTime, endDate, (int)clientUtcOffSet.TotalMinutes, dayToRun, null, null, null, null, clientTimeZone)
            {
                TimeService = Substitute.For<ITimeService>()
            };
            rule.TimeService.UtcNow.Returns(serverLocalTime.Add(-serverTimeZoneInfo.BaseUtcOffset));

            // act
            DateTime? nextRunTime = rule.GetNextUTCRunDateTime();

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

        [Test]
        [TestCase("10/27/2016", "2:25 AM", "10/27/2016 12:25 AM")]
        [TestCase("10/27/2016", "2:00 AM", "10/27/2016 12:00 AM")]
        [TestCase("10/27/2016", "3:25 AM", "10/27/2016 1:25 AM")]
        [TestCase("10/27/2016", "3:00 AM", "10/27/2016 1:00 AM")]
        public void NextRunJobScheduledToMissingHourDueToDst(string clientStartDate, string clientLocalTime, string expectedRunUtcTime)
        {
            // arrange
            const string clientTimeZone = "Central European Standard Time"; //Daylight Saving Time (DST) change: 2016 Sun, 27 Mar, 02:00 CET → CEST
            DateTime startDate = DateTime.Parse(clientStartDate);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;


            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Daily, startDate, clientlocalTime, startDate, 0, null, null, null, null, null, clientTimeZone)
            {
                TimeService = Substitute.For<ITimeService>()
            };
            rule.TimeService.UtcNow.Returns(startDate);

            // act
            DateTime? nextRunTime = rule.GetNextUTCRunDateTime();

            //assert
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

        [TestCase("Central Standard Time", "08:15 PM", "05/05/2016", 1, "Central Standard Time", "06/02/2016 1:15 AM")]
        [TestCase("Central European Standard Time", "01:45 AM", "05/01/2016", 1, "Central Standard Time", "04/30/2016 11:45 PM")]
        [TestCase("Central Standard Time", "08:15 PM", "05/05/2016", 31, "Central Standard Time", "06/01/2016 1:15 AM")]
        [TestCase("Central Standard Time", "08:15 PM", "02/05/2016", 31, "Central Standard Time", "03/01/2016 2:15 AM")]
        [TestCase("Central Standard Time", "11:15 PM", "03/10/2016", 17, "Central Standard Time", "03/18/2016 4:15 AM")]    //stat day: 11:15PM 03/10/2016 CST = 5:15AM 03/11/2016 UTC;		CST → CDT change 03/12/2016 
        [TestCase("Central European Standard Time", "1:20 AM", "03/25/2016", 29, "Central Standard Time", "03/28/2016 11:20 PM")]    //stat day: 1:20AM 03/25/2016 CET = 12:20AM 03/25/2016 UTC;		CET → CEST change 03/26/2016 
        [TestCase("Central European Standard Time", "2:25 AM", "10/28/2016", 2, "Central Standard Time", "11/2/2016 1:25 AM")]    //stat day: 2:25AM 10/28/2016 CEST = 12:25AM 10/28/2016 UTC;		CEST → CET change 10/30/2016
        public void CalculateLastDayOfScheduledMonthlyJob(string clientTimeZone, string clientLocalTime, string clientStartDate, int? dayOfMonth,
            string serverTimeZone, string expectedRunUtcTime)
        {
            // arrange
            DateTime startDate = DateTime.Parse(clientStartDate);
            DateTime endDate = startDate.AddYears(1);

            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;
            DateTime clientTime = startDate.Add(clientlocalTime);
            //For tesat purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

            // server time
            TimeZoneInfo serverTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(serverTimeZone);
            TimeSpan serverClientOffSet = clientUtcOffSet.Add(serverTimeZoneInfo.BaseUtcOffset);
            const int serverTimeShift = -2;     //To set server time before expectedRunUtcTime
            DateTime serverLocalTime = clientTime.Add(serverClientOffSet).AddHours(serverTimeShift);

            var rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, startDate, clientlocalTime, endDate, 0, null, dayOfMonth, null, null, null, clientTimeZone)
            {
                TimeService = Substitute.For<ITimeService>()
            };
            rule.TimeService.UtcNow.Returns(serverLocalTime.Add(-serverTimeZoneInfo.BaseUtcOffset));

            // act
            DateTime? nextRunTime = rule.GetNextUTCRunDateTime();

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

        [Test]
        [TestCase("01/01/2017", "08:50 PM", "1/1/2017 7:50 PM")]
        public void NextRunJobScheduledToFirstDayOfMonth(string clientStartDate, string clientLocalTime, string expectedRunUtcTime)
        {
            // arrange
            const string clientTimeZone = "Central European Standard Time"; //Daylight Saving Time (DST) change: 2016 Sun, 27 Mar, 02:00 CET → CEST
            DateTime startDate = DateTime.Parse(clientStartDate);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;


            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, startDate, clientlocalTime, startDate, 0, DaysOfWeek.Day, null, null, null, OccuranceInMonth.First, clientTimeZone)
            {
                TimeService = Substitute.For<ITimeService>()
            };
            rule.TimeService.UtcNow.Returns(startDate);

            // act
            DateTime? nextRunTime = rule.GetNextUTCRunDateTime();

            //assert
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

        //10/0*1*/2022 8:15 PM(UTC+10:00) Canberra, Melbourne.

        [TestCase("Central European Standard Time", "1:20 AM", "02/25/2017", "09/03/2017", "09/05/2017 11:20 PM")]
        [TestCase("Central European Standard Time", "1:20 AM", "02/25/2017", "10/28/2017", "11/01/2017 12:20 AM")]  //DST: CEST → CET change 10/30/2016
        public void NextRunJobServerAhead(string clientTimeZone, string clientLocalTime, string clientStartDate, string serverDateTime, string expectedRunUtcTime)
        {
            // arrange
            DateTime startDate = DateTime.Parse(clientStartDate, CultureInfo.GetCultureInfo("en-us"));
            DateTime endDate = startDate.AddYears(1);
            const DaysOfWeek dayToRun = DaysOfWeek.Wednesday;

            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime, CultureInfo.GetCultureInfo("en-us")).TimeOfDay;
            //For test purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

            // server time
            DateTime serverTime = DateTime.Parse(serverDateTime, CultureInfo.GetCultureInfo("en-us"));

            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, startDate, clientlocalTime, endDate, (int)clientUtcOffSet.TotalMinutes, dayToRun, null, null, null, null, clientTimeZone)
            {
                TimeService = Substitute.For<ITimeService>()
            };
            rule.TimeService.UtcNow.Returns(serverTime);

            // act
            DateTime? nextRunTime = rule.GetNextUTCRunDateTime();

            //assert
            Assert.IsNotNull(nextRunTime);
            Assert.AreEqual(DateTime.Parse(expectedRunUtcTime, CultureInfo.GetCultureInfo("en-us")), nextRunTime);
        }
        
        [TestCase("AUS Eastern Standard Time", "8:30 PM", "10/01/2022", "10/02/2022", "9:32 AM", "10/03/2022 9:30 AM")]//job scheduled on 10/1/2022 (GMT+10), expected offset on 10/03/2022 => GMT+11
        [TestCase("AUS Eastern Standard Time", "8:30 PM", "10/01/2022", "10/15/2022", "9:31 AM", "10/16/2022 9:30 AM")] //expected UTC for GMT+11
        public void DaylightSaveTimeTest_CorrectValues(string clientTimeZone, string clientLocalTime, string clientStartDate,
            string serverDate, string serverTime, string expectedRunUtcTime)
        {
            // arrange
            DateTime startDate = DateTime.Parse(clientStartDate, CultureInfo.GetCultureInfo("en-us"));
            DateTime endDate = startDate.AddYears(1);
            TimeSpan time = DateTime.Parse(serverTime).TimeOfDay;

            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime, CultureInfo.GetCultureInfo("en-us")).TimeOfDay;
            //For test purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

            // server time
            DateTime serverDateTime = DateTime.Parse(serverDate, CultureInfo.GetCultureInfo("en-us")) + time;

            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Daily, startDate, clientlocalTime, endDate, (int)clientUtcOffSet.TotalMinutes, null, null, null, null, null, clientTimeZone)
            {
                TimeService = Substitute.For<ITimeService>()
            };
            rule.ArrangeTimeServiceBaseOnUtcNow(serverDateTime.ToString());
            //rule.TimeService.UtcNow.Returns(serverTime);

            // act
            DateTime? nextRunTime = rule.GetNextUTCRunDateTime();

            //assert
            Assert.IsNotNull(nextRunTime);
            Assert.AreEqual(DateTime.Parse(expectedRunUtcTime, CultureInfo.GetCultureInfo("en-us")), nextRunTime);
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
            DateTime? nextRunTime = rule.GetNextUTCRunDateTime();

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