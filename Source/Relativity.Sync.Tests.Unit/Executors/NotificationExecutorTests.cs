using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs.EmailNotifications;
using Relativity.Services.EmailNotificationsManager;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public class NotificationExecutorTests
	{
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdmin;
		private Mock<IProgressRepository> _progressRepository;
		private Mock<IDestinationWorkspaceTagRepository> _destinationWorkspaceTagRepository;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;
		private Mock<IEmailNotificationsManager> _emailManager;

		private NotificationExecutor _instance;

		private const int _DESTINATION_CASE_ARTIFACT_ID = 103890;
		private const int _JOB_HISTORY_ARTIFACT_ID = 103954;
		private const int _SOURCE_CASE_ARTIFACT_ID = 102789;
		private const int _SYNC_CONFIG_ARTIFACT_ID = 107756;
		private const string _JOB_NAME = "My IP Job";
		private const string _DESTINATION_CASE_NAME = "My Review Destination Case";
		private const string _INSTANCE_NAME = "R1 Test Central";
		private const string _ERROR_MESSAGE = "The integration job failed.  Please see inner exception for details.";

		private const string _MESSAGE_COMPLETE = "A job for the following Relativity Integration Point has successfully completed.";
		private const string _MESSAGE_COMPLETE_ERRORS = "A job for the following Relativity Integration Point has successfully completed with errors.";
		private const string _MESSAGE_STOPPED = "A job for the following Relativity Integration Point has been stopped.";
		private const string _MESSAGE_FAILED = "A job for the following Relativity Integration Point has failed to complete.";

		private const string _SUBJECT_COMPLETE = "Relativity Job successfully completed for '{0}'";
		private const string _SUBJECT_COMPLETE_ERRORS = "Relativity Job completed with errors for '{0}'";
		private const string _SUBJECT_STOPPED = "Relativity Job stopped for '{0}'";

		private const string _BODY_SOURCE = "Source Workspace: {0}";
		private const string _BODY_DESTINATION = "Destination Workspace: {0}";

		private readonly string _subjectFailed = $"Relativity Job failed for '{_JOB_NAME}'";

		private readonly string _bodyName = $"Name: {_JOB_NAME}";
		private readonly string _bodyError = $"Error: {_ERROR_MESSAGE}";

		private readonly IEnumerable<string> _recipients = new[] { "user1@relativity.test.com", "user2@relativity.com" };
		private readonly string _sourceCaseTag = $"{_INSTANCE_NAME} - My IP Source Case - {_SOURCE_CASE_ARTIFACT_ID}";
		private readonly string _destinationCaseTag = $"{_INSTANCE_NAME} - {_DESTINATION_CASE_NAME} - {_DESTINATION_CASE_ARTIFACT_ID}";

		[SetUp]
		public void SetUp()
		{
			_serviceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			_progressRepository = new Mock<IProgressRepository>();
			_destinationWorkspaceTagRepository = new Mock<IDestinationWorkspaceTagRepository>();
			_jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();

			_emailManager = new Mock<IEmailNotificationsManager>();
			_serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IEmailNotificationsManager>()).ReturnsAsync(_emailManager.Object);

			_instance = new NotificationExecutor(_serviceFactoryForAdmin.Object, _progressRepository.Object, _destinationWorkspaceTagRepository.Object, _jobHistoryErrorRepository.Object, new EmptyLogger());
		}

		[Test]
		[TestCase(SyncJobStatus.Completed, _MESSAGE_COMPLETE, _SUBJECT_COMPLETE)]
		[TestCase(SyncJobStatus.CompletedWithErrors, _MESSAGE_COMPLETE_ERRORS, _SUBJECT_COMPLETE_ERRORS)]
		[TestCase(SyncJobStatus.Cancelled, _MESSAGE_STOPPED, _SUBJECT_STOPPED)]
		public async Task ExecuteAsyncSendCompletedEmailTest(SyncJobStatus expectedJobStatus, string expectedMessage, string expectedSubjectFormat)
		{
			// Arrange
			IMock<INotificationConfiguration> configuration = GetNotificationConfiguration();

			var completedProgress = new Mock<IProgress>();
			completedProgress.Setup(x => x.Status).Returns(expectedJobStatus);
			_progressRepository.Setup(x => x.QueryAllAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new []{completedProgress.Object});

			DestinationWorkspaceTag destinationWorkspaceTag = GetDestinationWorkspaceTag();
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(destinationWorkspaceTag);

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			string expectedBody = GetExpectedBody(expectedMessage, _destinationCaseTag);
			string expectedSubject = string.Format(CultureInfo.InvariantCulture, expectedSubjectFormat, _JOB_NAME);
			_emailManager.Verify(x => x.SendEmailNotificationAsync(It.Is<EmailNotificationRequest>(y => VerifyEmailRequest(y, expectedBody, expectedSubject))));
		}

		[Test]
		public async Task ExecuteAsyncSendFailedEmailTest()
		{
			//Arrange
			IMock<INotificationConfiguration> configuration = GetNotificationConfiguration();

			var completedProgress = new Mock<IProgress>();
			completedProgress.Setup(x => x.Status).Returns(SyncJobStatus.Failed);
			_progressRepository.Setup(x => x.QueryAllAsync(It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new[] {completedProgress.Object});

			DestinationWorkspaceTag destinationWorkspaceTag = GetDestinationWorkspaceTag();
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(destinationWorkspaceTag);

			var jobHistory = new Mock<IJobHistoryError>();
			jobHistory.Setup(x => x.ErrorMessage).Returns(_ERROR_MESSAGE);
			_jobHistoryErrorRepository.Setup(x => x.GetLastJobErrorAsync(It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(jobHistory.Object);
			
			//Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);
			 
			//Assert
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			string expectedBody = GetExpectedBodyWithError(_MESSAGE_FAILED, _destinationCaseTag);
			_emailManager.Verify(x => x.SendEmailNotificationAsync(It.Is<EmailNotificationRequest>(y => VerifyEmailRequest(y, expectedBody, _subjectFailed))));
		}

		[Test]
		public async Task ExecuteAsyncJobHistoryErrorRepositoryThrowsExceptionTest()
		{
			// Arrange
			IMock<INotificationConfiguration> configuration = GetNotificationConfiguration();

			var completedProgress = new Mock<IProgress>();
			completedProgress.Setup(x => x.Status).Returns(SyncJobStatus.Failed);
			_progressRepository.Setup(x => x.QueryAllAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new[] { completedProgress.Object });

			DestinationWorkspaceTag destinationWorkspaceTag = GetDestinationWorkspaceTag();
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(destinationWorkspaceTag);

			_jobHistoryErrorRepository.Setup(x => x.GetLastJobErrorAsync(It.IsAny<int>(), It.IsAny<int>())).Throws<NullReferenceException>();

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			string expectedBody = GetExpectedBody(_MESSAGE_FAILED, _destinationCaseTag);
			expectedBody = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}", expectedBody, "Error: ");
			_emailManager.Verify(x => x.SendEmailNotificationAsync(It.Is<EmailNotificationRequest>(y => VerifyEmailRequest(y, expectedBody, _subjectFailed))));
		}

		[Test]
		public async Task ExecuteAsyncSyncJobStatusExceptionTest()
		{
			// Arrange
			IMock<INotificationConfiguration> configuration = GetNotificationConfiguration();

			var completedProgress = new Mock<IProgress>();
			completedProgress.Setup(x => x.Status).Returns(SyncJobStatus.Completed);
			var completedWithErrorsProgress = new Mock<IProgress>();
			completedWithErrorsProgress.Setup(x => x.Status).Returns(SyncJobStatus.CompletedWithErrors);
			var failedProgress = new Mock<IProgress>();
			failedProgress.Setup(x => x.Status).Returns(SyncJobStatus.Failed);

			_progressRepository.Setup(x => x.QueryAllAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new[] { completedProgress.Object, failedProgress.Object,completedWithErrorsProgress.Object });

			DestinationWorkspaceTag destinationWorkspaceTag = GetDestinationWorkspaceTag();
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(destinationWorkspaceTag);

			var jobHistory = new Mock<IJobHistoryError>();
			jobHistory.Setup(x => x.ErrorMessage).Returns(_ERROR_MESSAGE);
			_jobHistoryErrorRepository.Setup(x => x.GetLastJobErrorAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(jobHistory.Object);

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			string expectedBody = GetExpectedBodyWithError(_MESSAGE_FAILED, _destinationCaseTag);
			_emailManager.Verify(x => x.SendEmailNotificationAsync(It.Is<EmailNotificationRequest>(y => VerifyEmailRequest(y, expectedBody, _subjectFailed))));
		}

		[Test]
		public async Task ExecuteAsyncDestinationWorkspaceTagRepositoryThrowsTest()
		{
			// Arrange
			IMock<INotificationConfiguration> configuration = GetNotificationConfiguration();
			var completedProgress = new Mock<IProgress>();
			completedProgress.Setup(x => x.Status).Returns(SyncJobStatus.Failed);
			_progressRepository.Setup(x => x.QueryAllAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new[] { completedProgress.Object });

			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Throws<NullReferenceException>();

			var jobHistory = new Mock<IJobHistoryError>();
			jobHistory.Setup(x => x.ErrorMessage).Returns(_ERROR_MESSAGE);
			_jobHistoryErrorRepository.Setup(x => x.GetLastJobErrorAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(jobHistory.Object);

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Status.Should().Be(ExecutionStatus.Completed);

			string expectedBody = GetExpectedBodyWithError(_MESSAGE_FAILED, String.Empty);
			_emailManager.Verify(x => x.SendEmailNotificationAsync(It.Is<EmailNotificationRequest>(y => VerifyEmailRequest(y, expectedBody, _subjectFailed))));
		}

		[Test]
		public async Task ExecuteAsyncGetEmailNotificationManagerThrowsExceptionTest()
		{
			// Arrange
			IMock<INotificationConfiguration> configuration = GetNotificationConfiguration();

			var completedProgress = new Mock<IProgress>();
			completedProgress.Setup(x => x.Status).Returns(SyncJobStatus.Failed);
			_progressRepository.Setup(x => x.QueryAllAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new[] { completedProgress.Object });

			DestinationWorkspaceTag destinationWorkspaceTag = GetDestinationWorkspaceTag();
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(destinationWorkspaceTag);

			var jobHistory = new Mock<IJobHistoryError>();
			jobHistory.Setup(x => x.ErrorMessage).Returns(_ERROR_MESSAGE);
			_jobHistoryErrorRepository.Setup(x => x.GetLastJobErrorAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(jobHistory.Object);

			_serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IEmailNotificationsManager>()).Throws<NullReferenceException>();

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			actualResult.Status.Should().Be(ExecutionStatus.Failed);
		}

		private IMock<INotificationConfiguration> GetNotificationConfiguration()
		{
			var notificationConfiguration = new Mock<INotificationConfiguration>();
			notificationConfiguration.Setup(x => x.GetJobName()).Returns(_JOB_NAME);
			notificationConfiguration.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_DESTINATION_CASE_ARTIFACT_ID);
			notificationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_CASE_ARTIFACT_ID);
			notificationConfiguration.Setup(x => x.GetSourceWorkspaceTag()).Returns(_sourceCaseTag);
			notificationConfiguration.SetupGet(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONFIG_ARTIFACT_ID);
			notificationConfiguration.SetupGet(x => x.JobHistoryArtifactId).Returns(_JOB_HISTORY_ARTIFACT_ID);
			notificationConfiguration.Setup(x => x.GetEmailRecipients()).Returns(_recipients);
			return notificationConfiguration;
		}

		private static DestinationWorkspaceTag GetDestinationWorkspaceTag()
		{
			var destinationWorkspaceTag = new DestinationWorkspaceTag
			{
				DestinationInstanceName = _INSTANCE_NAME,
				DestinationWorkspaceName = _DESTINATION_CASE_NAME,
				DestinationWorkspaceArtifactId = _DESTINATION_CASE_ARTIFACT_ID
			};
			return destinationWorkspaceTag;
		}

		private string GetExpectedBody(string expectedMessage, string expectedDestinationCaseTag)
		{
			string sourceWorkspaceFormat = string.Format(CultureInfo.InvariantCulture, _BODY_SOURCE, _sourceCaseTag);
			string destinationWorkspaceFormat = string.Format(CultureInfo.InvariantCulture, _BODY_DESTINATION, expectedDestinationCaseTag);
			string body = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}", expectedMessage, _bodyName, sourceWorkspaceFormat, destinationWorkspaceFormat);
			return body;
		}

		private string GetExpectedBodyWithError(string expectedMessage, string expectedDestinationCaseTag)
		{
			string body = GetExpectedBody(expectedMessage, expectedDestinationCaseTag);
			string errorBody = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}", body, _bodyError);
			return errorBody;
		}

		public bool VerifyEmailRequest(EmailNotificationRequest actualRequest, string body, string subjectMsg)
		{
			actualRequest.IsBodyHtml.Should().BeFalse();
			actualRequest.Recipients.Should().BeEquivalentTo(_recipients);

			actualRequest.Body.Should().Be(body);
			actualRequest.Subject.Should().Be(subjectMsg);

			return true;
		}
	}
}