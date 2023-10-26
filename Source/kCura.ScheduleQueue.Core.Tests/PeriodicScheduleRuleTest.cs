using System;
using System.Globalization;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;

namespace kCura.ScheduleQueue.Core.Tests
{
    [TestFixture]
    [Category("Unit")]
    internal class PeriodicScheduleRuleTest : TestBase
    {
        private const string dailyRuleOldXML = @"<PeriodicScheduleRule xmlns=""http://schemas.datacontract.org/2004/07/kCura.Method.Data.ScheduleRules"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><DayOfMonth i:nil=""true""/><DaysToRun i:nil=""true""/><EndDate i:nil=""true""/><Interval>Daily</Interval><SetLastDayOfMonth i:nil=""true""/><StartDate>2010-12-29T00:00:00</StartDate><TimeZoneId>UTC</TimeZoneId><localTimeOfDayTicks>450600000000</localTimeOfDayTicks></PeriodicScheduleRule>";
        private const string weeklyRuleOldXML = @"<PeriodicScheduleRule xmlns=""http://schemas.datacontract.org/2004/07/kCura.Method.Data.ScheduleRules"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><DayOfMonth i:nil=""true""/><DaysToRun>Monday Friday</DaysToRun><EndDate i:nil=""true""/><Interval>Weekly</Interval><SetLastDayOfMonth i:nil=""true""/><StartDate>2010-12-29T00:00:00</StartDate><TimeZoneId>UTC</TimeZoneId><localTimeOfDayTicks>450600000000</localTimeOfDayTicks></PeriodicScheduleRule>";
        private const string monthlyRuleOldXML = @"<PeriodicScheduleRule xmlns=""http://schemas.datacontract.org/2004/07/kCura.Method.Data.ScheduleRules"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><DayOfMonth>15</DayOfMonth><DaysToRun i:nil=""true""/><EndDate i:nil=""true""/><Interval>Monthly</Interval><SetLastDayOfMonth i:nil=""true""/><StartDate>2010-12-29T00:00:00</StartDate><TimeZoneId>UTC</TimeZoneId><localTimeOfDayTicks>450600000000</localTimeOfDayTicks></PeriodicScheduleRule>";

        [TestCase("12/29/2023", "UTC", "12/29/2023 12:31")]
        [TestCase("12/29/2023", "Central European Standard Time", "12/29/2023 11:31")]
        [TestCase("12/29/2023", "Central America Standard Time", "12/29/2023 18:31")]
        [TestCase("05/29/2023", "AUS Central Standard Time", "05/29/2023 03:01")]
        public void GetFirstUtcRunDateTime_Daily(string startDate, string timeZoneId, string expectedDateTime)
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Daily,
                DateTime.Parse(startDate),
                TimeSpan.Parse("12:31"),
                timeZoneId: timeZoneId);
            DateTime expectedTime = DateTime.Parse(expectedDateTime);

            DateTime? result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [TestCase("12/29/2023", DaysOfWeek.Monday, "UTC", 1, "01/01/2024 12:31", DayOfWeek.Monday)]
        [TestCase("12/29/2023", DaysOfWeek.Monday, "Central European Standard Time", 1, "01/01/2024 11:31", DayOfWeek.Monday)]
        [TestCase("12/29/2023", DaysOfWeek.Monday, "Central America Standard Time", 1, "01/01/2024 18:31", DayOfWeek.Monday)]
        [TestCase("05/29/2023", DaysOfWeek.Monday, "AUS Central Standard Time", 1, "06/05/2023 03:01", DayOfWeek.Monday)]
        [TestCase("05/29/2023", DaysOfWeek.Wednesday, "AUS Central Standard Time", 1, "05/31/2023 03:01", DayOfWeek.Wednesday)]
        [TestCase("05/29/2023", DaysOfWeek.Monday | DaysOfWeek.Wednesday, "AUS Central Standard Time", 1, "05/31/2023 03:01", DayOfWeek.Wednesday)]
        [TestCase("12/29/2023", DaysOfWeek.Monday, "Central European Standard Time", 2, "01/08/2024 11:31", DayOfWeek.Monday)]
        [TestCase("05/29/2023", DaysOfWeek.Monday, "AUS Central Standard Time", 2, "06/12/2023 03:01", DayOfWeek.Monday)]
        public void GetFirstUtcRunDateTime_Weekly(string startDate, DaysOfWeek daysOfWeek, string timeZoneId, int reOccur, string expectedDateTime, DayOfWeek expectedDayOfWeek)
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Weekly,
                DateTime.Parse(startDate),
                TimeSpan.Parse("12:31"),
                daysToRun: daysOfWeek,
                timeZoneId: timeZoneId,
                reoccur: reOccur);

            DateTime expectedTime = DateTime.Parse(expectedDateTime);

            DateTime? result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(expectedDayOfWeek, result.Value.DayOfWeek);
        }

        [TestCase("12/29/2023", 15, "UTC", 1, "01/15/2024 12:31")]
        [TestCase("12/29/2023", 15, "Central European Standard Time", 1, "01/15/2024 11:31")]
        [TestCase("12/29/2023", 15, "Central America Standard Time", 1, "01/15/2024 18:31")]
        [TestCase("05/29/2023", 15, "AUS Central Standard Time", 1, "06/15/2023 03:01")]
        [TestCase("05/10/2023", 15, "AUS Central Standard Time", 1, "05/15/2023 03:01")]
        [TestCase("05/10/2023", 15, "AUS Central Standard Time", 2, "05/15/2023 03:01")]
        [TestCase("12/29/2023", 15, "Central America Standard Time", 2, "02/15/2024 18:31")]
        public void GetFirstUtcRunDateTime_Monthly(string startDate, int dayOfMonth, string timeZoneId, int reOccur, string expectedDateTime)
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse(startDate),
                TimeSpan.Parse("12:31"),
                dayOfMonth: dayOfMonth,
                timeZoneId: timeZoneId,
                reoccur: reOccur);

            DateTime expectedTime = DateTime.Parse(expectedDateTime);

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(dayOfMonth, result.Value.Day);
        }

        [Test]
        public void GetFirstUtcRunDateTime_DailyRuleMigration()
        {
            // TODO: when we start using this scheduler in Method, we need to convert namespaces in serialized xml:
            string xml = dailyRuleOldXML.Replace("kCura.Method.Data.ScheduleRules", "kCura.ScheduleQueue.Core.ScheduleRules");
            PeriodicScheduleRule rule = (PeriodicScheduleRule)SerializerHelper.DeserializeUsingTypeName(
                AppDomain.CurrentDomain,
                typeof(PeriodicScheduleRule).FullName,
                xml);
            DateTime expectedTime = DateTime.Parse("12/29/2010 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetFirstUtcRunDateTime_WeeklyRuleMigration()
        {
            // TODO: when we start using this scheduler in Method, we need to convert namespaces in serialized xml:
            string xml = weeklyRuleOldXML.Replace("kCura.Method.Data.ScheduleRules", "kCura.ScheduleQueue.Core.ScheduleRules");
            PeriodicScheduleRule rule = (PeriodicScheduleRule)SerializerHelper.DeserializeUsingTypeName(
                    AppDomain.CurrentDomain,
                    typeof(PeriodicScheduleRule).FullName,
                    xml);
            DateTime expectedTime = DateTime.Parse("12/31/2010 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetFirstUtcRunDateTime_MonthlyRuleMigration()
        {
            // TODO: when we start using this scheduler in Method, we need to convert namespaces in serialized xml:
            string xml = monthlyRuleOldXML.Replace("kCura.Method.Data.ScheduleRules", "kCura.ScheduleQueue.Core.ScheduleRules");
            PeriodicScheduleRule rule = (PeriodicScheduleRule)SerializerHelper.DeserializeUsingTypeName(
                    AppDomain.CurrentDomain,
                    typeof(PeriodicScheduleRule).FullName,
                    xml);

            DateTime expectedTime = DateTime.Parse("01/15/2011 12:31");

            var result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [TestCase(ScheduleInterval.Monthly, "02/01/2016", "12:31", null, 0, null, 30, null, 1, null, "02/29/2016")]
        [TestCase(ScheduleInterval.Weekly, "02/23/2016", "12:31", null, 0, DaysOfWeek.Monday, null, null, 1, null, "02/29/2016")]
        public void GetFirstUtcRunDateTime(
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

            DateTime expectedTime = DateTime.Parse(expectedDate + " " + scheduledLocalTime);

            DateTime? result = rule.GetFirstUtcRunDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [TestCase("Tokyo Standard Time","12:30 PM", "9/13/2016 3:30 AM")]
        [TestCase("Central Standard Time",  "12:30 PM", "9/13/2016 5:30 PM")]
        [TestCase("Tokyo Standard Time",  "7:30 AM", "9/12/2016 10:30 PM")]
        [TestCase("Central Standard Time", "11:30 PM", "9/14/2016 4:30 AM")]
        [TestCase("Nepal Standard Time",  "10:30 PM", "9/13/2016 4:45 PM")] // Nepal Standard Time (UTC+05:45)
        [TestCase("AUS Central Standard Time", "8:00 AM", "9/12/2016 10:30 PM")] // AUS Central Standard Time (UTC+09:30)
        public void GetFirstUtcRunDateTime_CalculateLastDayOfScheduledDailyJob(
            string clientTimeZone,
            string clientLocalTime,
            string expectedRunUtcTime)
        {
            DateTime date = DateTime.Parse("9/13/2016");

            // arrange
            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;

            // For tests purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

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

        [TestCase("Eastern Standard Time", "11:00 PM", "10/13/2016", "10/20/2016 3:00 AM")]
        [TestCase("Central Standard Time", "11:00 PM", "10/13/2016", "10/20/2016 4:00 AM")]
        [TestCase("Central Standard Time", "11:00 PM", "10/13/2016", "10/20/2016 4:00 AM")]
        [TestCase("Central Standard Time", "3:00 AM", "10/13/2016",  "10/19/2016 8:00 AM")]
        [TestCase("Central European Standard Time", "6:00 AM", "10/13/2016", "10/19/2016 4:00 AM")]
        [TestCase("Central European Standard Time", "6:00 AM", "10/13/2016", "10/19/2016 4:00 AM")]
        [TestCase("Central European Standard Time", "1:00 AM", "10/13/2016", "10/18/2016 11:00 PM")]
        [TestCase("Central European Standard Time", "2:00 PM", "10/13/2016", "10/19/2016 12:00 PM")]
        [TestCase("Central European Standard Time", "2:00 PM", "10/13/2016", "10/19/2016 12:00 PM")]
        [TestCase("Tokyo Standard Time", "11:00 AM", "10/13/2016", "10/19/2016 2:00 AM")]
        [TestCase("Tokyo Standard Time", "11:00 AM", "10/13/2016", "10/19/2016 2:00 AM")]
        [TestCase("Tokyo Standard Time", "4:00 AM", "10/13/2016", "10/18/2016 7:00 PM")]
        [TestCase("Tokyo Standard Time", "4:00 AM", "10/13/2016","10/18/2016 7:00 PM")]
        [TestCase("GMT Standard Time", "5:00 AM", "10/13/2016","10/19/2016 4:00 AM")]
        [TestCase("UTC", "7:00 AM", "10/13/2016", "10/19/2016 7:00 AM")]
        [TestCase("Central European Standard Time", "5:00 AM", "11/3/2016", "11/9/2016 4:00 AM")]
        [TestCase("Central Standard Time", "11:00 PM", "10/27/2016","11/03/2016 4:00 AM")]
        [TestCase("Central Standard Time", "11:00 PM", "11/3/2016", "11/10/2016 5:00 AM")]
        [TestCase("Central European Standard Time", "6:30 AM", "10/25/2016", "10/26/2016 4:30 AM")]
        [TestCase("Central European Standard Time", "12:45 AM", "10/25/2016", "10/25/2016 10:45 PM")] // Daylight Saving Time (DST) changes
        [TestCase("Central Standard Time", "11:15 PM", "03/10/2016", "03/17/2016 4:15 AM")] // stat day: 11:15PM 03/10/2016 CST = 5:15AM 03/11/2016 UTC;        CST → CDT change 03/12/2016
        [TestCase("Central European Standard Time", "1:20 AM", "03/25/2016", "03/29/2016 11:20 PM")] // stat day: 1:20AM 03/25/2016 CET = 12:20AM 03/25/2016 UTC;        CET → CEST change 03/26/2016
        [TestCase("Central European Standard Time", "2:25 AM", "10/28/2016", "11/2/2016 1:25 AM")] // stat day: 2:25AM 10/28/2016 CEST = 12:25AM 10/28/2016 UTC;        CEST → CET change 10/30/2016
        public void GetFirstUtcRunDateTime_CalculateLastDayOfScheduledWeeklyJob(
            string clientTimeZone,
            string clientLocalTime,
            string clientStartDate,
            string expectedRunUtcTime)
        {
            // arrange
            DateTime startDate = DateTime.Parse(clientStartDate);
            DateTime endDate = startDate.AddDays(8);
            const DaysOfWeek dayToRun = DaysOfWeek.Wednesday;

            // client time
            TimeZoneInfo clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone);
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;

            // For tests purpose, flip the offset because the browsers have this offset value negated and RIP takes that into account.
            TimeSpan clientUtcOffSet = -clientTimeZoneInfo.BaseUtcOffset;

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

        [TestCase("Central Standard Time", "08:15 PM", "05/05/2016", 1, "06/02/2016 1:15 AM")]
        [TestCase("Central European Standard Time", "01:45 AM", "05/01/2016", 1, "04/30/2016 11:45 PM")]
        [TestCase("Central Standard Time", "08:15 PM", "05/05/2016", 31, "06/01/2016 1:15 AM")]
        [TestCase("Central Standard Time", "08:15 PM", "02/05/2016", 31, "03/01/2016 2:15 AM")]
        [TestCase("Central Standard Time", "11:15 PM", "03/10/2016", 17, "03/18/2016 4:15 AM")] // stat day: 11:15PM 03/10/2016 CST = 5:15AM 03/11/2016 UTC;        CST → CDT change 03/12/2016
        [TestCase("Central European Standard Time", "1:20 AM", "03/25/2016", 29, "03/28/2016 11:20 PM")] // stat day: 1:20AM 03/25/2016 CET = 12:20AM 03/25/2016 UTC;        CET → CEST change 03/26/2016
        [TestCase("Central European Standard Time", "2:25 AM", "10/28/2016", 2, "11/2/2016 1:25 AM")] // stat day: 2:25AM 10/28/2016 CEST = 12:25AM 10/28/2016 UTC;        CEST → CET change 10/30/2016
        public void GetFirstUtcRunDateTime_CalculateLastDayOfScheduledMonthlyJob(
            string clientTimeZone,
            string clientLocalTime,
            string clientStartDate,
            int? dayOfMonth,
            string expectedRunUtcTime)
        {
            // arrange
            DateTime startDate = DateTime.Parse(clientStartDate);
            DateTime endDate = startDate.AddYears(1);

            // client time
            TimeSpan clientlocalTime = DateTime.Parse(clientLocalTime).TimeOfDay;

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

        [TestCase("10/27/2016", "2:25 AM", "10/27/2016 12:25 AM")]
        [TestCase("10/27/2016", "2:00 AM", "10/27/2016 12:00 AM")]
        [TestCase("10/27/2016", "3:25 AM", "10/27/2016 1:25 AM")]
        [TestCase("10/27/2016", "3:00 AM", "10/27/2016 1:00 AM")]
        public void GetFirstUtcRunDateTime_JobScheduledToMissingHourDueToDst(string clientStartDate, string clientLocalTime, string expectedRunUtcTime)
        {
            // arrange
            const string clientTimeZone =
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

        [TestCase("AUS Eastern Standard Time", "8:30 PM", "10/01/2022", "10/02/2022 9:30 AM")] // job scheduled on 10/1/2022 (GMT+10), expected offset on 10/03/2022 => GMT+11
        [TestCase("AUS Eastern Standard Time", "8:30 PM", "10/03/2022", "10/04/2022 9:30 AM")] // expected UTC for GMT+11
        [TestCase("AUS Eastern Standard Time", "8:30 PM", "04/02/2022", "04/03/2022 10:30 AM")] // job scheduled on 04/02/2022 (GMT+11), expected offset on "04/03/2022" => GMT+10
        [TestCase("AUS Eastern Standard Time", "8:30 PM", "04/04/2022", "04/05/2022 10:30 AM")] // expected UTC for GMT+10
        public void GetNextUtcRunDateTime_DaylightSaveTime(
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

        [TestCase("12/29/2023", "Central European Standard Time", "12/31/2023", "12/30/2023 11:31")]
        [TestCase("12/29/2023", "Central European Standard Time", "12/30/2023", "12/30/2023 11:31")]
        [TestCase("12/29/2023", "Central European Standard Time", "12/29/2023", null)]
        public void GetNextUtcRunDateTime_EndDate(string startDate, string timeZoneId, string endDate, string expectedDateTime)
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Daily,
                DateTime.Parse(startDate),
                TimeSpan.Parse("11:31"),
                DateTime.Parse(endDate),
                timeZoneId: timeZoneId);

            DateTime? result = rule.GetNextUtcRunDateTime(DateTime.Parse($"{startDate} 11:31"));
            if (string.IsNullOrEmpty(expectedDateTime))
            {
                Assert.IsNull(result);
            }
            else
            {
                Assert.AreEqual(DateTime.Parse(expectedDateTime, CultureInfo.GetCultureInfo("en-us")), result);
            }
        }

        [TestCase("12/29/2023", "UTC", "12/29/2023 12:31")]
        [TestCase("12/29/2023", "Central European Standard Time", "12/29/2023 11:31")]
        [TestCase("12/29/2023", "Central America Standard Time", "12/29/2023 18:31")]
        [TestCase("05/29/2023", "AUS Central Standard Time", "05/29/2023 03:01")]
        public void GetNextUtcRunDateTime_Daily(string startDateTime, string timeZoneId, string expectedDateTime)
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Daily,
                DateTime.Parse(startDateTime),
                TimeSpan.Parse("12:31"),
                timeZoneId: timeZoneId);
            DateTime expectedTime = DateTime.Parse(expectedDateTime);
            DateTime lastNextUtcRunDateTime = expectedTime.AddDays(-1);

            DateTime? result = rule.GetNextUtcRunDateTime(lastNextUtcRunDateTime);

            Assert.AreEqual(expectedTime, result);
        }

        [TestCase("12/29/2023", "12/29/2023 12:31", DaysOfWeek.Monday, "UTC", 1, "01/01/2024 12:31", DayOfWeek.Monday)]
        [TestCase("12/29/2023", "12/29/2023 11:31", DaysOfWeek.Monday, "Central European Standard Time", 1, "01/01/2024 11:31", DayOfWeek.Monday)]
        [TestCase("12/29/2023", "12/29/2023 18:31", DaysOfWeek.Monday, "Central America Standard Time", 1, "01/01/2024 18:31", DayOfWeek.Monday)]
        [TestCase("05/29/2023", "05/29/2023 03:01", DaysOfWeek.Monday, "AUS Central Standard Time", 1, "06/05/2023 03:01", DayOfWeek.Monday)]
        [TestCase("05/29/2023", "05/29/2023 03:01", DaysOfWeek.Wednesday, "AUS Central Standard Time", 1, "05/31/2023 03:01", DayOfWeek.Wednesday)]
        [TestCase("05/29/2023", "05/29/2023 03:01", DaysOfWeek.Monday | DaysOfWeek.Wednesday, "AUS Central Standard Time", 1, "05/31/2023 03:01", DayOfWeek.Wednesday)]
        [TestCase("12/29/2023", "12/29/2023 11:31", DaysOfWeek.Monday, "Central European Standard Time", 2, "01/08/2024 11:31", DayOfWeek.Monday)]
        [TestCase("05/29/2023", "05/29/2023 03:01", DaysOfWeek.Monday, "AUS Central Standard Time", 2, "06/12/2023 03:01", DayOfWeek.Monday)]
        public void GetNextUtcRunDateTime_Weekly(string startDateTime, string lastNextUtcRun, DaysOfWeek daysOfWeek, string timeZoneId, int reOccur, string expectedDateTime, DayOfWeek expectedDayOfWeek)
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Weekly,
                DateTime.Parse(startDateTime),
                TimeSpan.Parse("12:31"),
                daysToRun: daysOfWeek,
                timeZoneId: timeZoneId,
                reoccur: reOccur);

            DateTime expectedTime = DateTime.Parse(expectedDateTime);
            DateTime? result = rule.GetNextUtcRunDateTime(DateTime.Parse(lastNextUtcRun));

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(expectedDayOfWeek, result.Value.DayOfWeek);
        }

        [TestCase("12/29/2023", 15, "UTC", 1, "01/15/2024 12:31")]
        [TestCase("12/29/2023", 15, "Central European Standard Time", 1, "01/15/2024 11:31")]
        [TestCase("12/29/2023", 15, "Central America Standard Time", 1, "01/15/2024 18:31")]
        [TestCase("05/29/2023", 15, "AUS Central Standard Time", 1, "06/15/2023 03:01")]
        [TestCase("05/10/2023", 15, "AUS Central Standard Time", 1, "05/15/2023 03:01")]
        [TestCase("05/10/2023", 15, "AUS Central Standard Time", 2, "05/15/2023 03:01")]
        [TestCase("12/29/2023", 15, "Central America Standard Time", 2, "02/15/2024 18:31")]
        public void GetNextUtcRunDateTime_Monthly(string startDateTime, int dayOfMonth, string timeZoneId, int reOccur, string expectedDateTime)
        {
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse(startDateTime),
                TimeSpan.Parse("12:31"),
                dayOfMonth: dayOfMonth,
                timeZoneId: timeZoneId,
                reoccur: reOccur);

            DateTime expectedTime = DateTime.Parse(expectedDateTime);
            DateTime lastNextUtcRunDateTime = expectedTime.AddMonths(-reOccur);

            DateTime? result = rule.GetNextUtcRunDateTime(lastNextUtcRunDateTime);

            Assert.AreEqual(expectedTime, result);
            Assert.AreEqual(dayOfMonth, result.Value.Day);
        }

        [Test]
        public void IncrementConsecutiveFailedScheduledJobsCount_True_UpdatesCounter()
        {
            int numberOfContinuouslyFailedScheduledJobs = 11;
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("10/15/2014"),
                TimeSpan.Parse("12:31"),
                null,
                null,
                DaysOfWeek.Friday,
                null,
                null,
                null,
                numberOfContinuouslyFailedScheduledJobs,
                OccuranceInMonth.Last);

            rule.IncrementConsecutiveFailedScheduledJobsCount();

            Assert.AreEqual(numberOfContinuouslyFailedScheduledJobs + 1, rule.FailedScheduledJobsCount);
        }

        [Test]
        public void ResetConsecutiveFailedScheduledJobsCount_False_ResetsCounter()
        {
            int numberOfContinuouslyFailedScheduledJobs = 11;
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("10/15/2014"),
                TimeSpan.Parse("12:31"),
                null,
                null,
                DaysOfWeek.Friday,
                null,
                null,
                null,
                numberOfContinuouslyFailedScheduledJobs,
                OccuranceInMonth.Last);

            rule.ResetConsecutiveFailedScheduledJobsCount();

            Assert.AreEqual(0, rule.FailedScheduledJobsCount);
        }

        [Test]
        public void GetNumberOfContinuouslyFailedScheduledJobs_ReturnsCorrectValue()
        {
            int numberOfContinuouslyFailedScheduledJobs = 11;
            PeriodicScheduleRule rule = new PeriodicScheduleRule(
                ScheduleInterval.Monthly,
                DateTime.Parse("10/15/2014"),
                TimeSpan.Parse("12:31"),
                null,
                null,
                DaysOfWeek.Friday,
                null,
                null,
                null,
                numberOfContinuouslyFailedScheduledJobs, OccuranceInMonth.Last);

            int result = rule.GetNumberOfContinuouslyFailedScheduledJobs();

            Assert.AreEqual(numberOfContinuouslyFailedScheduledJobs, result);
        }

        [Test]
        public void ForwardValidOccuranceMap()
        {
            Assert.AreEqual((int)OccuranceInMonth.First, (int)ForwardValidOccurance.First);
            Assert.AreEqual((int)OccuranceInMonth.Second, (int)ForwardValidOccurance.Second);
            Assert.AreEqual((int)OccuranceInMonth.Third, (int)ForwardValidOccurance.Third);
            Assert.AreEqual((int)OccuranceInMonth.Fourth, (int)ForwardValidOccurance.Fourth);
        }

        [Test]
        public void DaysOfWeekMap()
        {
            Assert.AreEqual(DayOfWeek.Monday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Monday]);
            Assert.AreEqual(DayOfWeek.Tuesday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Tuesday]);
            Assert.AreEqual(DayOfWeek.Wednesday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Wednesday]);
            Assert.AreEqual(DayOfWeek.Thursday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Thursday]);
            Assert.AreEqual(DayOfWeek.Friday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Friday]);
            Assert.AreEqual(DayOfWeek.Saturday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Saturday]);
            Assert.AreEqual(DayOfWeek.Sunday, PeriodicScheduleRule.DaysOfWeekMap[DaysOfWeek.Sunday]);
        }
    }
}
