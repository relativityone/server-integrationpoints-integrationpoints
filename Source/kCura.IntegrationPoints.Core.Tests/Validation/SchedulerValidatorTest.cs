using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
    [TestFixture, Category("Unit")]
    public class SchedulerValidatorTest
    {
        private IValidator _instance;

        public const string VALID_START_DATE = "11/11/2016";
        public const string VALID_SCHEDULED_TIME = "11:00";
        public const string VALID_SENDON = "{\"selectedDays\":[\"Monday\",\"Wednesday\"]}";

        [SetUp]
        public void Setup()
        {
            _instance = new SchedulerValidator(new JSONSerializer());
        }

        [Test]
        [TestCase("1/13/2016", null, "12:0", "Daily")]
        [TestCase("01/13/2016", null, "12:0", "Daily")]
        [TestCase("01/13/2016", null, "12:00", "Daily")]
        [TestCase("01/13/2016", null, "1:00", "Daily")]
        [TestCase("01/13/2016", null, "1:0", "Daily")]
        [TestCase("01/13/2016", null, "01:00", "Daily")]
        [TestCase("02/14/2016", null, "10:00", "Daily")]
        [TestCase("12/15/2016", "01/19/2017", "12:0", "Daily")]
        public void Validate_Valid_Scheduler_Dates(string startDate, string endDate, string scheduledTime, string selectedFrequency)
        {
            // Arrange
            var scheduler = new Scheduler()
            {
                StartDate = startDate,
                EndDate = endDate,
                ScheduledTime = scheduledTime,
                SelectedFrequency = ScheduleInterval.Daily.ToString()
            };

            // Act
            ValidationResult result = _instance.Validate(scheduler);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsNull(result.MessageTexts.FirstOrDefault());
        }

        [Test]
        [TestCase("1/1/2016", "12:0", "Daily")]
        [TestCase("13/01/2016", "12:0", "Daily")]
        [TestCase("13/40/2016", "12:0", "Daily")]
        [TestCase("02/31/2016", "12:0", "Daily")]
        [TestCase("//2016", "12:0", "Daily")]
        [TestCase("0/0/2016", "12:0", "Daily")]
        [TestCase("18/01/2016", "12:0", "Daily")]
        [TestCase("0/0/2016", "AM", "Daily")]
        [TestCase("0/0/2016", "25:99 AM", "Daily")]
        public void Validate_Invalid_Scheduler_Start_Date_Format(string startDate, string scheduledTime, string selectedFrequency)
        {
            // Arrange
            var scheduler = new Scheduler()
            {
                StartDate = startDate,
                EndDate = null,
                ScheduledTime = scheduledTime,
                SelectedFrequency = ScheduleInterval.Daily.ToString()
            };
            string resultMessage = IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_DATE_FORMAT + startDate;

            // Act
            ValidationResult result = _instance.Validate(scheduler);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.MessageTexts.Contains(resultMessage));
        }

        [Test]
        [TestCase("1/1/2015", "1/1/2016", "12:0", "Daily")]
        [TestCase("1/1/2015", "13/01/2016", "12:0", "Daily")]
        [TestCase("1/1/2015", "13/40/2016", "12:0", "Daily")]
        [TestCase("1/1/2015", "02/31/2016", "12:0", "Daily")]
        [TestCase("1/1/2015", "//2016", "12:0", "Daily")]
        [TestCase("1/1/2015", "0/0/2016", "12:0", "Daily")]
        [TestCase("1/1/2015", "18/01/2016", "12:0", "Daily")]
        [TestCase("1/1/2015", "0/0/2016", "AM", "Daily")]
        [TestCase("1/1/2015", "0/0/2016", "25:99 AM", "Daily")]
        public void Validate_Invalid_Scheduler_End_Date_Format(string startDate, string endDate, string scheduledTime, string selectedFrequency)
        {
            // Arrange
            var scheduler = new Scheduler()
            {
                StartDate = startDate,
                EndDate = endDate,
                ScheduledTime = scheduledTime,
                SelectedFrequency = ScheduleInterval.Daily.ToString()
            };
            string resultMessage = IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_DATE_FORMAT + endDate;

            // Act
            ValidationResult result = _instance.Validate(scheduler);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.MessageTexts.Contains(resultMessage));
        }

        [Test]
        [TestCase("10/01/2016", "09/01/2016", "12:0", "Daily")]
        [TestCase("01/20/2017", "09/01/2016", "12:0", "Daily")]
        public void Validate_Invalid_Scheduler_Dates_EndDate_Before_StartDate(string startDate, string endDate, string scheduledTime, string selectedFrequency)
        {
            // Arrange
            var scheduler = new Scheduler()
            {
                StartDate = startDate,
                EndDate = endDate,
                ScheduledTime = scheduledTime,
                SelectedFrequency = ScheduleInterval.Daily.ToString()
            };

            // Act
            ValidationResult result = _instance.Validate(scheduler);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.MessageTexts.Contains(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_END_DATE_BEFORE_START_DATE));
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void Validate_Invalid_Intervals(string interval)
        {
            // Arrange
            var scheduler = new Scheduler()
            {
                StartDate = VALID_START_DATE,
                EndDate = null,
                ScheduledTime = VALID_SCHEDULED_TIME,
                SelectedFrequency = interval
            };
            string message = interval.IsNullOrEmpty() ? IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_REQUIRED_VALUE : IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_VALUE;

            // Act
            ValidationResult result = _instance.Validate(scheduler);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.MessageTexts.Any(x => x.Contains(message)));
        }

        [Test]
        [TestCase(SchedulerValidator.REOCCUR_MIN)]
        [TestCase(SchedulerValidator.REOCCUR_MAX)]
        [TestCase((SchedulerValidator.REOCCUR_MAX - SchedulerValidator.REOCCUR_MIN) / 2)]
        public void Validate_Valid_Reoccur(int reoccur)
        {
            // Arrange
            var scheduler = new Scheduler()
            {
                StartDate = VALID_START_DATE,
                EndDate = null,
                ScheduledTime = VALID_SCHEDULED_TIME,
                SelectedFrequency = ScheduleInterval.Weekly.ToString(),
                Reoccur = reoccur,
                SendOn = VALID_SENDON
            };

            // Act
            ValidationResult result = _instance.Validate(scheduler);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsNull(result.MessageTexts.FirstOrDefault());
        }

        [Test]
        [TestCase(SchedulerValidator.REOCCUR_MIN - 1)]
        [TestCase(SchedulerValidator.REOCCUR_MAX + 1)]
        public void Validate_Inalid_Reoccur(int reoccur)
        {
            // Arrange
            var scheduler = new Scheduler()
            {
                StartDate = VALID_START_DATE,
                EndDate = null,
                ScheduledTime = VALID_SCHEDULED_TIME,
                SelectedFrequency = ScheduleInterval.Weekly.ToString(),
                Reoccur = reoccur,
                SendOn = VALID_SENDON
            };

            // Act
            ValidationResult result = _instance.Validate(scheduler);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.MessageTexts.Any(x => x.Contains(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_NOT_IN_RANGE)));
        }

        [Test]
        [TestCase("{\"selectedDays\":[\"Sunday\"]}")]
        [TestCase("{\"selectedDays\":[\"Monday\"]}")]
        [TestCase("{\"selectedDays\":[\"Tuesday\"]}")]
        [TestCase("{\"selectedDays\":[\"Wednesday\"]}")]
        [TestCase("{\"selectedDays\":[\"Thursday\"]}")]
        [TestCase("{\"selectedDays\":[\"Friday\"]}")]
        [TestCase("{\"selectedDays\":[\"Saturday\"]}")]
        [TestCase("{\"selectedDays\":[\"Monday\",\"Wednesday\"]}")]
        [TestCase("{\"selectedDays\":[\"Sunday\",\"Monday\",\"Tuesday\",\"Wednesday\",\"Thursday\",\"Friday\"]}")]
        public void Validate_Valid_SendOn_Weekle(string sendOn)
        {
            // Arrange
            var scheduler = new Scheduler()
            {
                StartDate = VALID_START_DATE,
                EndDate = null,
                ScheduledTime = VALID_SCHEDULED_TIME,
                SelectedFrequency = ScheduleInterval.Weekly.ToString(),
                Reoccur = SchedulerValidator.REOCCUR_MIN,
                SendOn = sendOn
            };

            // Act
            ValidationResult result = _instance.Validate(scheduler);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsNull(result.MessageTexts.FirstOrDefault());
        }

        [Test]
        [TestCase("")]
        [TestCase("ioasdfnoas")]
        public void Validate_Inalid_SendOn_Weekle(string day)
        {
            // Arrange
            string sendOn = "{\"selectedDays\":[\"" + day + "\"]}";
            var scheduler = new Scheduler()
            {
                StartDate = VALID_START_DATE,
                EndDate = null,
                ScheduledTime = VALID_SCHEDULED_TIME,
                SelectedFrequency = ScheduleInterval.Weekly.ToString(),
                Reoccur = SchedulerValidator.REOCCUR_MIN,
                SendOn = sendOn
            };
            string message = day.IsNullOrEmpty()
                ? IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_REQUIRED_VALUE
                : IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_VALUE;

            // Act
            ValidationResult result = _instance.Validate(scheduler);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.MessageTexts.Any(x => x.Contains(message)));
        }

        [Test]
        [TestCase("{\"monthChoice\":\"2\",\"selectedDay\":1}")]
        [TestCase("{\"monthChoice\":\"1\",\"selectedDay\":1,\"selectedDayOfTheMonth\":4,\"selectedType\":4}")]
        public void Validate_Valid_SendOn_Monthly(string sendOn)
        {
            // Arrange
            var scheduler = new Scheduler()
            {
                StartDate = VALID_START_DATE,
                EndDate = null,
                ScheduledTime = VALID_SCHEDULED_TIME,
                SelectedFrequency = ScheduleInterval.Monthly.ToString(),
                Reoccur = SchedulerValidator.REOCCUR_MIN,
                SendOn = sendOn
            };

            // Act
            ValidationResult result = _instance.Validate(scheduler);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsNull(result.MessageTexts.FirstOrDefault());
        }
    }
}