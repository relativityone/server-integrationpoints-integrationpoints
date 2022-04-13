using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Kepler.Exceptions;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.DataContracts.DTOs.EmailNotifications;
using Relativity.Services.EmailNotificationsManager;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class NotificationExecutorTests
	{
		private CompositeCancellationToken _token;

		private Mock<IEmailNotificationsManager> _emailNotificationsManager;
		private Mock<INotificationConfiguration> _notificationConfiguration;
		private Mock<IObjectManager> _objectManager;
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdmin;
		private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUser;
		private Mock<IAPILog> _syncLog;
		private Mock<ISyncMetrics> _syncMetrics;

		private IExecutor<INotificationConfiguration> _instance;

		private const int _DEST_CASE_ARTIFACT_ID = 105448;
		private const int _JOB_HISTORY_ARTIFACT_ID = 104779;
		private const int _SOURCE_CASE_ARTIFACT_ID = 104226;
		private const int _SYNC_CONFIG_ARTIFACT_ID = 104558;
		private const string _JOB_NAME = "My Special IP Job";
		private const string _SOURCE_CASE_TAG = "My ECA Case";


		private readonly Guid _destinationWorkspaceTagObjectTypeGuid = new Guid("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");
		private readonly Guid _jobHistoryErrorObjectGuid = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
		private readonly Guid _progressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");
		private Mock<IJobHistoryErrorRepositoryConfigration> _jobHistoryErrorRepositoryConfigurationMock;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_syncLog = new Mock<IAPILog>();
			_syncMetrics = new Mock<ISyncMetrics>();
			_token = CompositeCancellationToken.None;

			_notificationConfiguration = new Mock<INotificationConfiguration>();
			_notificationConfiguration.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_DEST_CASE_ARTIFACT_ID);
			_notificationConfiguration.SetupGet(x => x.JobHistoryArtifactId).Returns(_JOB_HISTORY_ARTIFACT_ID);
			_notificationConfiguration.Setup(x => x.GetJobName()).Returns(_JOB_NAME);
			_notificationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_SOURCE_CASE_ARTIFACT_ID);
			_notificationConfiguration.Setup(x => x.GetSourceWorkspaceTag()).Returns(_SOURCE_CASE_TAG);
			_notificationConfiguration.SetupGet(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONFIG_ARTIFACT_ID);

			_jobHistoryErrorRepositoryConfigurationMock = new Mock<IJobHistoryErrorRepositoryConfigration>();
			_jobHistoryErrorRepositoryConfigurationMock.SetupGet(x => x.LogItemLevelErrors).Returns(true);
		}

		[SetUp]
		public void SetUp()
		{
			_emailNotificationsManager = new Mock<IEmailNotificationsManager>();
			_objectManager = new Mock<IObjectManager>();

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockStepsExcept<INotificationConfiguration>(containerBuilder);

			containerBuilder.RegisterInstance(_syncLog.Object).As<IAPILog>();
			containerBuilder.RegisterInstance(_syncMetrics.Object).As<ISyncMetrics>();

			_serviceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			_serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IEmailNotificationsManager>()).ReturnsAsync(_emailNotificationsManager.Object);
			_serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			containerBuilder.RegisterInstance(_serviceFactoryForAdmin.Object).As<ISourceServiceFactoryForAdmin>();

			_serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			_serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			containerBuilder.RegisterInstance(_serviceFactoryForUser.Object).As<ISourceServiceFactoryForUser>();

			containerBuilder.RegisterType<NotificationExecutor>().As<IExecutor<INotificationConfiguration>>();

			containerBuilder.RegisterInstance(_jobHistoryErrorRepositoryConfigurationMock.Object)
				.As<IJobHistoryErrorRepositoryConfigration>();

			IContainer container = containerBuilder.Build();
			_instance = container.Resolve<IExecutor<INotificationConfiguration>>();
		}

		[Test]
		public async Task ExecuteAsyncSendsCompletedWithErrorsEmailTest()
		{
			// Arrange
			SetUpProgressExpectations("Completed With Errors");
			SetUpDestinationWorkspaceTagExpectations();
			SetUpJobHistoryErrorExpectations();

			// Act
			ExecutionResult result = await _instance.ExecuteAsync(_notificationConfiguration.Object, _token).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);

			_emailNotificationsManager.Verify(x =>
				x.SendEmailNotificationAsync(It.Is<EmailNotificationRequest>(e => e.Subject.StartsWith("Relativity Job completed with errors for", StringComparison.InvariantCulture))));
		}

		[Test]
		public async Task ExecuteAsyncFailsToRetrieveProgressObjectsSendsFailedJobEmailTest()
		{
			// Arrange
			_objectManager.Setup(x => x.QueryAsync(_SOURCE_CASE_ARTIFACT_ID, It.Is<QueryRequest>(q => q.ObjectType.Guid == _progressObjectTypeGuid), 1, int.MaxValue)).Throws<ServiceNotFoundException>();

			SetUpDestinationWorkspaceTagExpectations();
			SetUpJobHistoryErrorExpectations();

			// Act
			ExecutionResult result = await _instance.ExecuteAsync(_notificationConfiguration.Object, _token).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);

			const string expectedLogMessage = "Failed to retrieve all progress information for workspace {WorkspaceArtifactID} and sync configuration object {SyncConfigArtifactID}.";
			_syncLog.Verify(x => x.LogError(It.IsAny<ServiceNotFoundException>(), expectedLogMessage, _SOURCE_CASE_ARTIFACT_ID, _SYNC_CONFIG_ARTIFACT_ID), Times.AtLeastOnce);

			_emailNotificationsManager.Verify(x => x.SendEmailNotificationAsync(It.Is<EmailNotificationRequest>(e => e.Subject.StartsWith("Relativity Job failed for", StringComparison.InvariantCulture))));
		}

		[Test]
		public async Task ExecuteAsyncFailsToRetrieveDestinationWorkspaceTagSendsCompletedEmailTest()
		{
			// Arrange
			_objectManager.Setup(x => x.QueryAsync(_SOURCE_CASE_ARTIFACT_ID, It.Is<QueryRequest>(y => y.ObjectType.Guid == _destinationWorkspaceTagObjectTypeGuid), 0, 1, It.IsAny<CancellationToken>(),
				It.IsAny<IProgress<ProgressReport>>())).Throws<ServiceNotFoundException>();

			SetUpProgressExpectations("Completed");
			SetUpJobHistoryErrorExpectations();

			// Act
			ExecutionResult result = await _instance.ExecuteAsync(_notificationConfiguration.Object, _token).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);

			string expectedLogMessage = "Failed to query {TagObject} object: {Request}.";
			_syncLog.Verify(x => x.LogError(It.IsAny<ServiceNotFoundException>(), expectedLogMessage, nameof(DestinationWorkspaceTag), It.IsAny<QueryRequest>()), Times.AtLeastOnce);

			_emailNotificationsManager.Verify(x =>
				x.SendEmailNotificationAsync(It.Is<EmailNotificationRequest>(e => e.Subject.StartsWith("Relativity Job successfully completed for", StringComparison.InvariantCulture))));
		}

		[Test]
		public async Task ExecuteAsyncFailsToRetrieveJobHistoryErrorInformationSendsFailedEmailTest()
		{
			// Arrange
			SetUpJobErrorTypeReadRequest();
			_objectManager.Setup(x => x.QueryAsync(_SOURCE_CASE_ARTIFACT_ID, It.Is<QueryRequest>(q => q.ObjectType.Guid == _jobHistoryErrorObjectGuid), 0, 1)).Throws<ServiceNotFoundException>();

			SetUpProgressExpectations("Failed");
			SetUpDestinationWorkspaceTagExpectations();

			// Act
			ExecutionResult result = await _instance.ExecuteAsync(_notificationConfiguration.Object, _token).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);

			string expectedLogMessage = "Failed to retrieve the job history error information for source workspace {SourceWorkspace} and destination workspace {DestinationWorkspace}.";
			_syncLog.Verify(x => x.LogWarning(It.IsAny<ServiceNotFoundException>(), expectedLogMessage, _SOURCE_CASE_ARTIFACT_ID, _DEST_CASE_ARTIFACT_ID), Times.AtLeastOnce);

			_emailNotificationsManager.Verify(x => x.SendEmailNotificationAsync(It.Is<EmailNotificationRequest>(e => e.Subject.StartsWith("Relativity Job failed for", StringComparison.InvariantCulture))));
		}

		private void SetUpProgressExpectations(string progressStatus)
		{
			const int progressArtifactId = 107456;
			_objectManager.Setup(x => x.QueryAsync(_SOURCE_CASE_ARTIFACT_ID, It.Is<QueryRequest>(q => q.ObjectType.Guid == _progressObjectTypeGuid), 1, int.MaxValue)).ReturnsAsync(new QueryResult
			{
				TotalCount = 1,
				Objects = new List<RelativityObject> {new RelativityObject {ArtifactID = progressArtifactId } }
			});

			var queryResult = new QueryResult
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject
					{
						ArtifactID =  progressArtifactId,
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("AE2FCA2B-0E5C-4F35-948F-6C1654D5CF95") }},
								Value = "Synchronization Step"
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("610A1E44-7AAA-47FC-8FA0-92F8C8C8A94A") }},
								Value = 1
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("698E1BBE-13B7-445C-8A28-7D40FD232E1B") }},
								Value = progressStatus
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("2F2CFC2B-C9C0-406D-BD90-FB0133BCB939") }},
								Value = string.Empty
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("2E296F79-1B81-4BF6-98AD-68DA13F8DA44") }},
								Value = string.Empty
							}
						}
					}
				}
			};
			_objectManager.Setup(x => x.QueryAsync(_SOURCE_CASE_ARTIFACT_ID, It.Is<QueryRequest>(r => r.Condition == $"'ArtifactID' == {progressArtifactId}"), 0, 1)).ReturnsAsync(queryResult);
		}

		private void SetUpDestinationWorkspaceTagExpectations()
		{
			var queryResult = new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject
					{
						ArtifactID = 1,
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("348D7394-2658-4DA4-87D0-8183824ADF98")}},
								Value = "My Review Workspace"
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("909ADC7C-2BB9-46CA-9F85-DA32901D6554")}},
								Value = "This Relativity Instance"
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("207E6836-2961-466B-A0D2-29974A4FAD36")}},
								Value = _DEST_CASE_ARTIFACT_ID
							}
						}
					}
				}
			};

			_objectManager.Setup(x => x.QueryAsync(_SOURCE_CASE_ARTIFACT_ID, It.Is<QueryRequest>(y => y.ObjectType.Guid == _destinationWorkspaceTagObjectTypeGuid), 0, 1, It.IsAny<CancellationToken>(),
				It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);
		}

		private void SetUpJobHistoryErrorExpectations()
		{
			SetUpJobErrorTypeReadRequest();
			var queryResult = new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject
					{
						ArtifactID = 1,
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("4112B894-35B0-4E53-AB99-C9036D08269D") }},
								Value = "The integration point failed to complete error message."
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678") }},
								Value = new Choice{Name = "New"}
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973") }},
								Value = new Choice{Name = "Job"}
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("84E757CC-9DA2-435D-B288-0C21EC589E66") }},
								Value = "264886A4-1853-4DFA-98F9-133506520FB4"
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("5519435E-EE82-4820-9546-F1AF46121901") }},
								Value = string.Empty
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF") }},
								Value = "Stack trace of exception"
							},
							new FieldValuePair
							{
								Field = new Field {Guids = new List<Guid> {new Guid("B9CBA772-E7C9-493E-B7F8-8D605A6BFE1F") }},
								Value = DateTime.UtcNow
							}
						}
					}
				},
				TotalCount = 1
			};
			_objectManager.Setup(x => x.QueryAsync(_SOURCE_CASE_ARTIFACT_ID, It.Is<QueryRequest>(q => q.ObjectType.Guid == _jobHistoryErrorObjectGuid), 0, 1)).ReturnsAsync(queryResult);
		}

		private void SetUpJobErrorTypeReadRequest()
		{
			var errorTypeJob = new Guid("FA8BB625-05E6-4BF7-8573-012146BAF19B");
			_objectManager.Setup(x => x.ReadAsync(_SOURCE_CASE_ARTIFACT_ID, It.Is<ReadRequest>(r => r.Object.Guid == errorTypeJob))).ReturnsAsync(new ReadResult
			{
				Object = new RelativityObject { ArtifactID = 1 }
			});
		}
	}
}
