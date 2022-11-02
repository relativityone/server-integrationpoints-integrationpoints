using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using Moq;
using NUnit.Framework;

namespace kCura.ScheduleQueue.Core.Tests.Validation
{
    internal class ScheduledJobConsecutiveFailsValidatorTests
    {
        private Mock<IConfig> _configFake;
        private Mock<IScheduleRuleFactory> _scheduleRuleFactoryFake;

        ScheduledJobConsecutiveFailsValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _configFake = new Mock<IConfig>();
            _scheduleRuleFactoryFake = new Mock<IScheduleRuleFactory>();

            _sut = new ScheduledJobConsecutiveFailsValidator(_configFake.Object, _scheduleRuleFactoryFake.Object);
        }

        [Test]
        public async Task ValidateAsync_ShouldReturnSuccess_WhenTheJobIsNotScheduled()
        {
            // Arrange
            _scheduleRuleFactoryFake.Setup(x => x.Deserialize(It.IsAny<Job>()))
                .Returns((IScheduleRule)null);

            // Act
            var result = await _sut.ValidateAsync(It.IsAny<Job>()).ConfigureAwait(false);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public async Task ValidateAsync_ShouldReturnSuccess_WhenTheConsecutiveFailsLimitIsNotReached()
        {
            // Arrange
            const int consecutiveFailsCount = 10;
            const int consecutiveFailsLimit = 20;

            Mock<IScheduleRule> rule = new Mock<IScheduleRule>();
            rule.Setup(x => x.GetNumberOfContinuouslyFailedScheduledJobs())
                .Returns(consecutiveFailsCount);

            _scheduleRuleFactoryFake.Setup(x => x.Deserialize(It.IsAny<Job>()))
                .Returns(rule.Object);

            _configFake.Setup(x => x.MaxFailedScheduledJobsCount).Returns(consecutiveFailsLimit);

            // Act
            var result = await _sut.ValidateAsync(It.IsAny<Job>()).ConfigureAwait(false);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public async Task ValidateAsync_ShouldReturnInvalid_WhenTheConsecutiveFailsLimitIsReached()
        {
            // Arrange
            const int consecutiveFailsCount = 30;
            const int consecutiveFailsLimit = 20;

            Mock<IScheduleRule> rule = new Mock<IScheduleRule>();
            rule.Setup(x => x.GetNumberOfContinuouslyFailedScheduledJobs())
                .Returns(consecutiveFailsCount);

            _scheduleRuleFactoryFake.Setup(x => x.Deserialize(It.IsAny<Job>()))
                .Returns(rule.Object);

            _configFake.Setup(x => x.MaxFailedScheduledJobsCount).Returns(consecutiveFailsLimit);

            // Act
            var result = await _sut.ValidateAsync(It.IsAny<Job>()).ConfigureAwait(false);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldBreakSchedule.Should().BeTrue();
        }
    }
}
