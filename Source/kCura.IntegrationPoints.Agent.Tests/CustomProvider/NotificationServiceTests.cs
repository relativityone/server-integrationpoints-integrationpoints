using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.EmailNotifications;
using Relativity.Services.EmailNotificationsManager;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class NotificationServiceTests
    {
        private Mock<IKeplerServiceFactory> _keplerServiceFactoryMock;
        private Mock<IJobHistoryService> _jobHistoryServiceMock;
        private Mock<IJobHistoryErrorService> _jobHistoryErrorServiceMock;
        private Mock<IEmailNotificationsManager> _emailNotificationManagerMock;
        private Mock<IAPILog> _loggerMock;

        private NotificationService _sut;
        private IntegrationPointDto _integrationPointFake;
        private ImportJobContext _importJobContextFake;
        private JobHistory _jobHistoryFake;

        [SetUp]
        public void SetUp()
        {
            _keplerServiceFactoryMock = new Mock<IKeplerServiceFactory>();
            _jobHistoryErrorServiceMock = new Mock<IJobHistoryErrorService>();
            _jobHistoryServiceMock = new Mock<IJobHistoryService>();
            _emailNotificationManagerMock = new Mock<IEmailNotificationsManager>();
            _loggerMock = new Mock<IAPILog>();

            _keplerServiceFactoryMock.Setup(x => x.CreateProxyAsync<IEmailNotificationsManager>())
                .ReturnsAsync(_emailNotificationManagerMock.Object);

            PrepareObjectParameters();

            _sut = new NotificationService(_jobHistoryServiceMock.Object, _keplerServiceFactoryMock.Object, _jobHistoryErrorServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task PrepareAndSendEmailNotificationAsync_ShouldSendNotification_GoldFlow()
        {
            // Arrange
            _jobHistoryServiceMock.Setup(x => x.ReadJobHistoryByGuidAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(_jobHistoryFake);

            // Act
            await _sut.PrepareAndSendEmailNotificationAsync(_importJobContextFake, _integrationPointFake).ConfigureAwait(false);

            // Assert
            _emailNotificationManagerMock.Verify(x => x.SendEmailNotificationAsync(It.IsAny<EmailNotificationRequest>()), Times.Once);
            _jobHistoryErrorServiceMock.Verify(x => x.GetLastJobLevelError(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("      ")]
        public async Task PrepareAndSendEmailNotificationAsync_ShouldNotSendNotification_WhenRecipientsListIsNullOrEmpty(string emails)
        {
            // Arrange
            _integrationPointFake.EmailNotificationRecipients = emails;
            _jobHistoryServiceMock.Setup(x => x.ReadJobHistoryByGuidAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(_jobHistoryFake);

            // Act
            await _sut.PrepareAndSendEmailNotificationAsync(_importJobContextFake, _integrationPointFake).ConfigureAwait(false);

            // Assert
            _emailNotificationManagerMock.Verify(x => x.SendEmailNotificationAsync(It.IsAny<EmailNotificationRequest>()), Times.Never);
        }

        [Test]
        public void PrepareAndSendEmailNotificationAsync_ShouldNotThrow_WhenImplementationContentThrows()
        {
            // Arrange
            _jobHistoryServiceMock.Setup(x => x.ReadJobHistoryByGuidAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                      .ThrowsAsync(new Exception());

            // Act
            Func<Task> sendEmail = async () => await _sut.PrepareAndSendEmailNotificationAsync(_importJobContextFake, _integrationPointFake).ConfigureAwait(false);

            // Assert
            sendEmail.ShouldNotThrow<Exception>();
            _emailNotificationManagerMock.Verify(x => x.SendEmailNotificationAsync(It.IsAny<EmailNotificationRequest>()), Times.Never);
            _loggerMock.Verify(x => x.LogWarning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));
        }

        private void PrepareObjectParameters()
        {
            _integrationPointFake = new IntegrationPointDto()
            {
                EmailNotificationRecipients = "a@a.com; b@b.com"
            };
            _importJobContextFake = new ImportJobContext(12345, 111222, Guid.NewGuid(), 333444);

            _jobHistoryFake = new JobHistory()
            {
                JobStatus = JobStatusChoices.JobHistoryCompleted,
                TotalItems = 10,
                ItemsTransferred = 10,
                ItemsWithErrors = 0,
                DestinationWorkspace = "Destination",
                Name = "Test Job"
            };
        }
    }
}
