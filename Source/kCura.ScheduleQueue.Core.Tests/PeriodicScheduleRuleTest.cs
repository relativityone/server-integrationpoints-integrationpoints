using System;
using System.Globalization;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.ScheduleQueue.Core.Helpers;
using kCura.ScheduleQueue.Core.ScheduleRules;
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

        [Test]
        public void GetNextUTCRunDateTime_DailyStartDateTimeForUTC_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Daily,
                DateTime.Parse("12/29/2020"),
                TimeSpan.Parse("12:31"),
                timeZoneId: "UTC");
            DateTime expectedTime = DateTime.Parse("12/29/2020 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_DailyStartDateTimeAfterNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Daily,
                DateTime.Parse("12/29/2020"),
                TimeSpan.Parse("12:31"),
                timeZoneId: "Central European Standard Time");
            DateTime expectedTime = DateTime.Parse("12/29/2020 11:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_DailyStartDateBeforeTimeAfterNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Daily,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                timeZoneId: "Central America Standard Time");
            DateTime expectedTime = DateTime.Parse("12/29/2010 18:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_WeeklyMondaysOnlyStartDateTimeThursdayBeforeNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Weekly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                daysToRun: DaysOfWeek.Monday,
                timeZoneId: "Central European Standard Time");

            DateTime expectedTime = DateTime.Parse("01/03/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(DayOfWeek.Monday, result.Value.DayOfWeek);
        }

        [Test]
        public void GetNextUTCRunDateTime_WeeklyMondaysOnlyStartDateTimeMondayBeforeNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Weekly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                daysToRun: DaysOfWeek.Monday,
                timeZoneId: "Central European Standard Time");

            DateTime expectedTime = DateTime.Parse("01/03/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(DayOfWeek.Monday, result.Value.DayOfWeek);
        }

        [Test]
        public void GetNextUTCRunDateTime_WeeklyMondaysOnlyStartDateTimeMondayAfterNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Weekly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                daysToRun: DaysOfWeek.Monday,
                timeZoneId: "Central European Standard Time");

            DateTime expectedTime = DateTime.Parse("01/10/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(DayOfWeek.Monday, result.Value.DayOfWeek);
        }

        [Test]
        public void GetNextUTCRunDateTime_WeeklyMondaysFridaysOnlyNowDateTimeTuesday_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Weekly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                daysToRun: DaysOfWeek.Monday | DaysOfWeek.Friday,
                timeZoneId: "Central European Standard Time");
            DateTime expectedTime = DateTime.Parse("01/07/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(DayOfWeek.Friday, result.Value.DayOfWeek);
        }

        [Test]
        public void GetNextUTCRunDateTime_WeeklyMondaysFridaysOnlyNowDateTimeSaturday_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Weekly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                daysToRun: DaysOfWeek.Monday | DaysOfWeek.Friday,
                timeZoneId: "Central European Standard Time");
            DateTime expectedTime = DateTime.Parse("01/10/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(DayOfWeek.Monday, result.Value.DayOfWeek);
        }

        [Test]
        public void GetNextUTCRunDateTime_Every15DayMonthlyStartDateTimeBeforeNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                dayOfMonth: 15,
                timeZoneId: "Central European Standard Time");

            DateTime expectedTime = DateTime.Parse("01/15/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(15, result.Value.Day);
        }

        [Test]
        public void GetNextUTCRunDateTime_Every31DayMonthlyIn28DayMonth_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                dayOfMonth: 31,
                timeZoneId: "Central European Standard Time");

            DateTime expectedTime = DateTime.Parse("02/28/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_Every31DayMonthlyIn29DayMonth_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                dayOfMonth: 31,
                timeZoneId: "Central European Standard Time");

            DateTime expectedTime = DateTime.Parse("02/29/2012 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_Every31DayMonthlyIn30DayMonth_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                dayOfMonth: 31,
                timeZoneId: "Central European Standard Time");

            DateTime expectedTime = DateTime.Parse("04/30/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_Every31DayMonthlyIn31DayMonth_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                dayOfMonth: 31,
                timeZoneId: "Central European Standard Time");
            DateTime expectedTime = DateTime.Parse("05/31/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_Every15DayMonthlyStartDateLaterThenNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                dayOfMonth: 15,
                timeZoneId: "Central European Standard Time");
            DateTime expectedTime = DateTime.Parse("01/15/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_NextRunTimeDateBeforeEndDate_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"),
                DateTime.Parse("2/1/2011"),
                dayOfMonth: 15,
                timeZoneId: "Central European Standard Time");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetNextUTCRunDateTime_NextRunTimeDateOnEndDate_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"), DateTime.Parse("1/15/2011"), 0, null, 15);

            var result = rule.GetFirstUtcRunDateTime();

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetNextUTCRunDateTime_NextRunTimeDateAfterEndDate_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"), DateTime.Parse("1/1/2011"), null, null, 15);
            var result = rule.GetFirstUtcRunDateTime();

            Assert.IsNull(result);
        }

        [Test]
        public void GetNextUTCRunDateTime_NextRunTimeDateNextDayAfterEndDate_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"), DateTime.Parse("1/14/2011"), null, null, 15);

            var result = rule.GetFirstUtcRunDateTime();

            Assert.IsNull(result);
        }

        [Test]
        public void GetNextUTCRunDateTime_DailyRuleMigration_CorrectValue()
        {
            // TODO: when we start using this scheduler in Method, we need to convert namespaces in serialized xml:
            string xml = dailyRuleOldXML.Replace("kCura.Method.Data.ScheduleRules",
                "kCura.ScheduleQueue.Core.ScheduleRules");
            PeriodicScheduleRule rule =
                (PeriodicScheduleRule)SerializerHelper.DeserializeUsingTypeName(System.AppDomain.CurrentDomain,
                    typeof(PeriodicScheduleRule).FullName, xml);
            DateTime expectedTime = DateTime.Parse("12/31/2010 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_WeeklyRuleMigration_CorrectValue()
        {
            // TODO: when we start using this scheduler in Method, we need to convert namespaces in serialized xml:
            string xml = weeklyRuleOldXML.Replace("kCura.Method.Data.ScheduleRules",
                "kCura.ScheduleQueue.Core.ScheduleRules");
            PeriodicScheduleRule rule =
                (PeriodicScheduleRule)SerializerHelper.DeserializeUsingTypeName(System.AppDomain.CurrentDomain,
                    typeof(PeriodicScheduleRule).FullName, xml);
            DateTime expectedTime = DateTime.Parse("01/10/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_MonthlyRuleMigration_CorrectValue()
        {
            // TODO: when we start using this scheduler in Method, we need to convert namespaces in serialized xml:
            string xml = monthlyRuleOldXML.Replace("kCura.Method.Data.ScheduleRules",
                "kCura.ScheduleQueue.Core.ScheduleRules");
            PeriodicScheduleRule rule =
                (PeriodicScheduleRule)SerializerHelper.DeserializeUsingTypeName(System.AppDomain.CurrentDomain,
                    typeof(PeriodicScheduleRule).FullName, xml);

            DateTime expectedTime = DateTime.Parse("01/15/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_WeeklyMondaysSaturdaysStartDateTimeWednesdayAfterNowReoccur2_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday | DaysOfWeek.Saturday, null, null, 2);
            DateTime expectedTime = DateTime.Parse("01/08/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(DayOfWeek.Saturday, result.Value.DayOfWeek);
        }

        [Test]
        public void GetNextUTCRunDateTime_WeeklyMondaysSaturdaysStartDateTimeSundayAfterNowReoccur2_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Weekly, DateTime.Parse("12/29/2010"),
                TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday | DaysOfWeek.Saturday, null, null, 2);
            DateTime expectedTime = DateTime.Parse("01/17/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(DayOfWeek.Monday, result.Value.DayOfWeek);
        }

        [Test]
        public void GetNextUTCRunDateTime_MonthlyFirstMondaysReoccurEveryMonthStartDateBeforeNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("9/15/2014"),
                TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday, null, null, 1, 0, OccuranceInMonth.First);

            DateTime expectedTime = DateTime.Parse("10/06/2014 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_MonthlyFirstMondaysReoccurEveryMonthStartDateAfterNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/15/2014"),
                TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Monday, null, null, 1, 0, OccuranceInMonth.First);
            DateTime expectedTime = DateTime.Parse("11/03/2014 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_MonthlySecondTuesdayReoccurEveryMonthStartDateAfterNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/15/2014"),
                TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Tuesday, null, null, 1, 0, OccuranceInMonth.Second);
            DateTime expectedTime = DateTime.Parse("11/11/2014 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_MonthlyFourthWendesdayReoccurEvery3MonthStartDateAfterNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/25/2014"),
                TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Wednesday, null, null, 3, 0, OccuranceInMonth.Fourth);
            DateTime expectedTime = DateTime.Parse("1/28/2015 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_MonthlyThirdSaturdayReoccurEvery3MonthStartDateAfterNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/15/2014"),
                TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Saturday, null, null, 3, 0, OccuranceInMonth.Third);
            DateTime expectedTime = DateTime.Parse("10/18/2014 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetNextUTCRunDateTime_MonthlyLastFridayReoccurEvery3MonthStartDateAfterNow_CorrectValue()
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("10/15/2014"),
                TimeSpan.Parse("12:31"),
                null,
                null,
                DaysOfWeek.Friday,
                null,
                null,
                3,
                0,
                null);
            DateTime expectedTime = DateTime.Parse("10/31/2014 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }
        
        [TestCase("02/01/2016 21:00:00", ScheduleInterval.Monthly, "02/01/2016", "12:31", null, 0, null, 30, null, 1, null, "02/29/2016")]
        [TestCase("02/23/2016 21:00:00", ScheduleInterval.Weekly, "02/23/2016", "12:31", null, 0, DaysOfWeek.Monday, null, null, 1, null, "02/29/2016")]
        public void GetNextUTCRunDateTime_CorrectValue(
            string utcNowTime,
            ScheduleInterval interval,
            string startDate,
            string scheduledLocalTime,
            DateTime? endDate,
            int timeZoneOffSet,
            DaysOfWeek? daysToRun,
            int? dayOfMonth,
            bool? setLastDay,
            int? reoccur,
            OccuranceInMonth? occurrenceInMonth,
            string expectedDate)
        {
            const string timeZoneId = "UTC";
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                interval,
                DateTime.Parse(startDate),
                TimeSpan.Parse(scheduledLocalTime),
                endDate,
                timeZoneOffSet,
                daysToRun,
                dayOfMonth,
                setLastDay,
                reoccur,
                0,
                occurrenceInMonth,
                timeZoneId);
            var utcNow = DateTime.Parse(utcNowTime);
            DateTime expectedTime = DateTime.Parse(expectedDate + " " + scheduledLocalTime);

            DateTime? result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void IncrementConsecutiveFailedScheduledJobsCount_True_UpdatesCounter()
        {
            int numberOfContinuouslyFailedScheduledJobs = 11;
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/15/2014"),
                TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Friday, null, null, null,
                numberOfContinuouslyFailedScheduledJobs, OccuranceInMonth.Last);

            rule.IncrementConsecutiveFailedScheduledJobsCount();

            Assert.AreEqual(numberOfContinuouslyFailedScheduledJobs + 1, rule.FailedScheduledJobsCount);
        }

        [Test]
        public void ResetConsecutiveFailedScheduledJobsCount_False_ResetsCounter()
        {
            int numberOfContinuouslyFailedScheduledJobs = 11;
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/15/2014"),
                TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Friday, null, null, null,
                numberOfContinuouslyFailedScheduledJobs, OccuranceInMonth.Last);

            rule.ResetConsecutiveFailedScheduledJobsCount();

            Assert.AreEqual(0, rule.FailedScheduledJobsCount);
        }

        [Test]
        public void GetNumberOfContinuouslyFailedScheduledJobs_ReturnsCorrectValue()
        {
            int numberOfContinuouslyFailedScheduledJobs = 11;
            PeriodicScheduleRule rule = new PeriodicScheduleRule(ScheduleInterval.Monthly, DateTime.Parse("10/15/2014"),
                TimeSpan.Parse("12:31"), null, null, DaysOfWeek.Friday, null, null, null,
                numberOfContinuouslyFailedScheduledJobs, OccuranceInMonth.Last);

            int result = rule.GetNumberOfContinuouslyFailedScheduledJobs();

            Assert.AreEqual(numberOfContinuouslyFailedScheduledJobs, result);
        }

        //[Test]
        //public void SearchMonthForForwardOccuranceOfDay_FirstMonday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/3/2014");

        //    var result = rule.SearchMonthForForwardOccuranceOfDay(2014, 2, ForwardValidOccurance.First, DayOfWeek.Monday);

        //    Assert.AreEqual(expectedTime, result);
        //}

        //[Test]
        //public void SearchMonthForForwardOccuranceOfDay_ForthMonday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/24/2014");

        //    var result =
        //        rule.SearchMonthForForwardOccuranceOfDay(2014, 2, ForwardValidOccurance.Fourth, DayOfWeek.Monday);

        //    Assert.AreEqual(expectedTime, result);
        //}

        //[Test]
        //public void SearchMonthForForwardOccuranceOfDay_FirstSaturday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/1/2014");

        //    var result =
        //        rule.SearchMonthForForwardOccuranceOfDay(2014, 2, ForwardValidOccurance.First, DayOfWeek.Saturday);

        //    Assert.AreEqual(expectedTime, result);
        //}

        //[Test]
        //public void SearchMonthForForwardOccuranceOfDay_FourthFriday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/28/2014");

        //    var result =
        //        rule.SearchMonthForForwardOccuranceOfDay(2014, 2, ForwardValidOccurance.Fourth, DayOfWeek.Friday);

        //    Assert.AreEqual(expectedTime, result);
        //}

        //[Test]
        //public void SearchMonthForForwardOccuranceOfDay_FirstWednesday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/1/2012");

        //    var result =
        //        rule.SearchMonthForForwardOccuranceOfDay(2012, 2, ForwardValidOccurance.First, DayOfWeek.Wednesday);

        //    Assert.AreEqual(expectedTime, result);
        //}

        //[Test]
        //public void SearchMonthForForwardOccuranceOfDay_FourthWednesday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/22/2012");

        //    var result =
        //        rule.SearchMonthForForwardOccuranceOfDay(2012, 2, ForwardValidOccurance.Fourth, DayOfWeek.Wednesday);

        //    Assert.AreEqual(expectedTime, result);
        //}

        //[Test]
        //public void SearchMonthForForwardOccuranceOfDay_FourthTuesday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/28/2012");

        //    var result =
        //        rule.SearchMonthForForwardOccuranceOfDay(2012, 2, ForwardValidOccurance.Fourth, DayOfWeek.Tuesday);

        //    Assert.AreEqual(expectedTime, result);
        //}

        //#region SearchMonthForLastOccuranceOfDay

        //#endregion SearchMonthForForwardOccuranceOfDay

        //[Test]
        //public void SearchMonthForLastOccuranceOfDay_LastMonday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/24/2014");

        //    var result = rule.SearchMonthForLastOccuranceOfDay(2014, 2, DayOfWeek.Monday);

        //    Assert.AreEqual(expectedTime, result);
        //}

        //[Test]
        //public void SearchMonthForLastOccuranceOfDay_LastFriday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/28/2014");

        //    var result = rule.SearchMonthForLastOccuranceOfDay(2014, 2, DayOfWeek.Friday);

        //    Assert.AreEqual(expectedTime, result);
        //}

        //[Test]
        //public void SearchMonthForLastOccuranceOfDay_LastWednesday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/29/2012");

        //    var result = rule.SearchMonthForLastOccuranceOfDay(2012, 2, DayOfWeek.Wednesday);

        //    Assert.AreEqual(expectedTime, result);
        //}

        //[Test]
        //public void SearchMonthForLastOccuranceOfDay_LastTuesday_CorrectValue()
        //{
        //    PeriodicScheduleRule rule = new PeriodicScheduleRule();
        //    DateTime expectedTime = DateTime.Parse("2/28/2012");

        //    var result = rule.SearchMonthForLastOccuranceOfDay(2012, 2, DayOfWeek.Tuesday);

        //    Assert.AreEqual(expectedTime, result);
        //}

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
        [TestCase("Nepal Standard Time", "9/13/2016", "10:30 PM", "Tokyo Standard Time",
            "9/13/2016 4:45 PM")] // Nepal Standard Time (UTC+05:45)
        [TestCase("AUS Central Standard Time", "9/13/2016", "8:00 AM", "Tokyo Standard Time",
            "9/12/2016 10:30 PM")] // AUS Central Standard Time (UTC+09:30)
        public void CalculateLastDayOfScheduledDailyJob(
            string clientTimeZone,
            string clientDate,
            string clientLocalTime,
            string serverTimeZone,
            string expectedRunUtcTime)
        {
            DateTime date = DateTime.Parse("9/13/2016");

            // arrange
            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;
            DateTime clientTime = DateTime.Parse(clientDate).Add(clientlocalTime);
            // For tesat purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

            // server time
            TimeZoneInfo serverTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(serverTimeZone);
            TimeSpan serverClientOffSet = clientUtcOffSet.Add(serverTimeZoneInfo.BaseUtcOffset);
            const int serverTimeShift = -2; // To set server time before expectedRunUtcTime
            DateTime serverLocalTime = clientTime.Add(serverClientOffSet).AddHours(serverTimeShift);

            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Daily,
                date,
                clientlocalTime,
                date,
                (int)clientUtcOffSet.TotalMinutes,
                null,
                null,
                null,
                null,
                0,
                null,
                clientTimeZone);
            rule.TimeZoneId = clientTimeZone;

            // act
            DateTime? nextRunTime = rule.GetFirstUtcRunDateTime();

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
        [TestCase("Central Standard Time", "11:00 PM", "10/13/2016", "Central European Standard Time",
            "10/20/2016 4:00 AM")]
        [TestCase("Central Standard Time", "3:00 AM", "10/13/2016", "Central Standard Time", "10/19/2016 8:00 AM")]
        [TestCase("Central European Standard Time", "6:00 AM", "10/13/2016", "Central Standard Time",
            "10/19/2016 4:00 AM")]
        [TestCase("Central European Standard Time", "6:00 AM", "10/13/2016", "Central European Standard Time",
            "10/19/2016 4:00 AM")]
        [TestCase("Central European Standard Time", "1:00 AM", "10/13/2016", "Central European Standard Time",
            "10/18/2016 11:00 PM")]
        [TestCase("Central European Standard Time", "2:00 PM", "10/13/2016", "Central European Standard Time",
            "10/19/2016 12:00 PM")]
        [TestCase("Central European Standard Time", "2:00 PM", "10/13/2016", "Tokyo Standard Time",
            "10/19/2016 12:00 PM")]
        [TestCase("Tokyo Standard Time", "11:00 AM", "10/13/2016", "Central European Standard Time",
            "10/19/2016 2:00 AM")]
        [TestCase("Tokyo Standard Time", "11:00 AM", "10/13/2016", "Central Standard Time", "10/19/2016 2:00 AM")]
        [TestCase("Tokyo Standard Time", "4:00 AM", "10/13/2016", "Central European Standard Time",
            "10/18/2016 7:00 PM")]
        [TestCase("Tokyo Standard Time", "4:00 AM", "10/13/2016", "Central Standard Time", "10/18/2016 7:00 PM")]
        [TestCase("GMT Standard Time", "5:00 AM", "10/13/2016", "GMT Standard Time", "10/19/2016 4:00 AM")]
        [TestCase("UTC", "7:00 AM", "10/13/2016", "UTC", "10/19/2016 7:00 AM")]
        [TestCase("Central European Standard Time", "5:00 AM", "11/3/2016", "Central Standard Time",
            "11/9/2016 4:00 AM")]
        [TestCase("Central Standard Time", "11:00 PM", "10/27/2016", "Central Standard Time", "11/03/2016 4:00 AM")]
        [TestCase("Central Standard Time", "11:00 PM", "11/3/2016", "Central European Standard Time",
            "11/10/2016 5:00 AM")]
        [TestCase("Central European Standard Time", "6:30 AM", "10/25/2016", "Central Standard Time",
            "10/26/2016 4:30 AM")]
        [TestCase("Central European Standard Time", "12:45 AM", "10/25/2016", "Central Standard Time",
            "10/25/2016 10:45 PM")]
        // Daylight Saving Time (DST) changes
        [TestCase("Central Standard Time", "11:15 PM", "03/10/2016", "Central Standard Time",
            "03/17/2016 4:15 AM")] // stat day: 11:15PM 03/10/2016 CST = 5:15AM 03/11/2016 UTC;        CST → CDT change 03/12/2016
        [TestCase("Central European Standard Time", "1:20 AM", "03/25/2016", "Central Standard Time",
            "03/29/2016 11:20 PM")] // stat day: 1:20AM 03/25/2016 CET = 12:20AM 03/25/2016 UTC;        CET → CEST change 03/26/2016
        [TestCase("Central European Standard Time", "2:25 AM", "10/28/2016", "Central Standard Time",
            "11/2/2016 1:25 AM")] // stat day: 2:25AM 10/28/2016 CEST = 12:25AM 10/28/2016 UTC;        CEST → CET change 10/30/2016
        public void CalculateLastDayOfScheduledWeeklyJob(string clientTimeZone, string clientLocalTime,
            string clientStartDate, string serverTimeZone, string expectedRunUtcTime)
        {
            // arrange
            DateTime startDate = DateTime.Parse(clientStartDate);
            DateTime endDate = startDate.AddDays(8);
            const DaysOfWeek dayToRun = DaysOfWeek.Wednesday;

            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;
            DateTime clientTime = startDate.Add(clientlocalTime);
            // For tesat purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

            // server time
            TimeZoneInfo serverTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(serverTimeZone);
            TimeSpan serverClientOffSet = clientUtcOffSet.Add(serverTimeZoneInfo.BaseUtcOffset);
            const int serverTimeShift = -2; // To set server time before expectedRunUtcTime
            DateTime serverLocalTime = clientTime.Add(serverClientOffSet).AddHours(serverTimeShift);

            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Weekly,
                startDate,
                clientlocalTime,
                endDate,
                (int)clientUtcOffSet.TotalMinutes,
                dayToRun,
                null,
                null,
                1,
                0,
                null,
                clientTimeZone);

            // act
            DateTime? nextRunTime = rule.GetFirstUtcRunDateTime();

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
        public void NextRunJobScheduledToMissingHourDueToDst(string clientStartDate, string clientLocalTime,
            string expectedRunUtcTime)
        {
            // arrange
            const string
                clientTimeZone =
                    "Central European Standard Time"; // Daylight Saving Time (DST) change: 2016 Sun, 27 Mar, 02:00 CET → CEST
            DateTime startDate = DateTime.Parse(clientStartDate);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;

            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Daily,
                startDate,
                clientlocalTime,
                startDate,
                0,
                null,
                null,
                null,
                null,
                0,
                null,
                clientTimeZone);

            // act
            DateTime? nextRunTime = rule.GetFirstUtcRunDateTime();

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

        [TestCase("Central Standard Time", "08:15 PM", "05/05/2016", 1, "Central Standard Time", "06/02/2016 1:15 AM")]
        [TestCase("Central European Standard Time", "01:45 AM", "05/01/2016", 1, "Central Standard Time", "04/30/2016 11:45 PM")]
        [TestCase("Central Standard Time", "08:15 PM", "05/05/2016", 31, "Central Standard Time", "06/01/2016 1:15 AM")]
        [TestCase("Central Standard Time", "08:15 PM", "02/05/2016", 31, "Central Standard Time", "03/01/2016 2:15 AM")]
        [TestCase("Central Standard Time", "11:15 PM", "03/10/2016", 17, "Central Standard Time", "03/18/2016 4:15 AM")] // stat day: 11:15PM 03/10/2016 CST = 5:15AM 03/11/2016 UTC;        CST → CDT change 03/12/2016
        [TestCase("Central European Standard Time", "1:20 AM", "03/25/2016", 29, "Central Standard Time", "03/28/2016 11:20 PM")] // stat day: 1:20AM 03/25/2016 CET = 12:20AM 03/25/2016 UTC;        CET → CEST change 03/26/2016
        [TestCase("Central European Standard Time", "2:25 AM", "10/28/2016", 2, "Central Standard Time", "11/2/2016 1:25 AM")] // stat day: 2:25AM 10/28/2016 CEST = 12:25AM 10/28/2016 UTC;        CEST → CET change 10/30/2016
        public void CalculateLastDayOfScheduledMonthlyJob(
            string clientTimeZone,
            string clientLocalTime,
            string clientStartDate,
            int? dayOfMonth,
            string serverTimeZone,
            string expectedRunUtcTime)
        {
            // arrange
            DateTime startDate = DateTime.Parse(clientStartDate);
            DateTime endDate = startDate.AddYears(1);

            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;
            DateTime clientTime = startDate.Add(clientlocalTime);
            // For tesat purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

            // server time
            TimeZoneInfo serverTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(serverTimeZone);
            TimeSpan serverClientOffSet = clientUtcOffSet.Add(serverTimeZoneInfo.BaseUtcOffset);
            const int serverTimeShift = -2; // To set server time before expectedRunUtcTime
            DateTime serverLocalTime = clientTime.Add(serverClientOffSet).AddHours(serverTimeShift);

            var rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                startDate,
                clientlocalTime,
                endDate,
                0,
                null,
                dayOfMonth,
                null,
                1,
                0,
                null,
                clientTimeZone);

            // act
            DateTime? nextRunTime = rule.GetFirstUtcRunDateTime();

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

        [TestCase("AUS Eastern Standard Time", "8:30 PM", "10/01/2022", "10/02/2022 9:30 AM")] // job scheduled on 10/1/2022 (GMT+10), expected offset on 10/03/2022 => GMT+11
        [TestCase("AUS Eastern Standard Time", "8:30 PM", "10/03/2022", "10/04/2022 9:30 AM")] // expected UTC for GMT+11
        [TestCase("AUS Eastern Standard Time", "8:30 PM", "04/02/2022", "04/03/2022 10:30 AM")] // job scheduled on 04/02/2022 (GMT+11), expected offset on "04/03/2022" => GMT+10
        [TestCase("AUS Eastern Standard Time", "8:30 PM", "04/04/2022", "04/05/2022 10:30 AM")] // expected UTC for GMT+10
        public void DaylightSaveTimeTest_CorrectValues(
            string clientTimeZone,
            string clientLocalTime,
            string clientStartDate,
            string expectedRunUtcTime)
        {
            // arrange
            DateTime startDate = DateTime.Parse(clientStartDate, CultureInfo.GetCultureInfo("en-us"));
            DateTime endDate = startDate.AddYears(1);

            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime, CultureInfo.GetCultureInfo("en-us")).TimeOfDay;
            // For test purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Daily,
                startDate,
                clientlocalTime,
                endDate,
                (int)clientUtcOffSet.TotalMinutes,
                null,
                null,
                null,
                null,
                0,
                null,
                clientTimeZone);

            // act
            DateTime? nextRunTime = rule.GetNextUtcRunDateTime(startDate.Subtract(clientTimeZoneInfo.BaseUtcOffset).AddMinutes(clientlocalTime.TotalMinutes));

            // assert
            Assert.IsNotNull(nextRunTime);
            Assert.AreEqual(DateTime.Parse(expectedRunUtcTime, CultureInfo.GetCultureInfo("en-us")), nextRunTime);
        }

        [Test]
        public void SampleTest()
        {
            int diff = DaysOfWeekConverter.DayOfWeekToIndex(DayOfWeek.Sunday) - DaysOfWeekConverter.DayOfWeekToIndex(DayOfWeek.Monday);
        }
    }
}
