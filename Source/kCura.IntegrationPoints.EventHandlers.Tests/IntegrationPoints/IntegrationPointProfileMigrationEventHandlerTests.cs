using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints
{
	[TestFixture]
	public class IntegrationPointProfileMigrationEventHandlerTests
	{
		private IntegrationPointProfileMigrationEventHandler _eventHandler;
		private Mock<IErrorService> _errorService;
		private Mock<IAPILog> _logger;
		private Mock<IEHHelper> _eventHandlerHelper;
		private Mock<IRelativityObjectManagerFactory> _relativityObjectManagerFactory;
		private Mock<IRelativityObjectManager> _templateWorkspaceRelativityObjectManager;
		private Mock<IRelativityObjectManager> _createdWorkspaceRelativityObjectManager;
		private Mock<IIntegrationPointProfilesQuery> _integrationPointProfilesQuery;

		private const string _TEST_ERROR_MESSAGE = "Failed to migrate the Integration Point Profiles.";
		private const int _TEMPLATE_WORKSPACE_ARTIFACT_ID = 100111;
		private const int _CREATED_WORKSPACE_ARTIFACT_ID = 200111;
		private const int _FIRST_SYNC_PROFILE_ARTIFACT_ID = 300444;
		private const int _FIRST_NON_SYNC_PROFILE_ARTIFACT_ID = 400444;

		private static ServiceException TestException => new ServiceException(_TEST_ERROR_MESSAGE);

		private static IEnumerable<Action<IntegrationPointProfileMigrationEventHandlerTests>> ServicesFailureSetups { get; } = new Action<IntegrationPointProfileMigrationEventHandlerTests>[]
		{
			ctx => ctx._createdWorkspaceRelativityObjectManager
				.Setup(x => x.MassDeleteAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<ExecutionIdentity>()))
				.Throws(TestException),
			ctx => ctx._integrationPointProfilesQuery
				.Setup(x => x.GetAllProfilesAsync(_TEMPLATE_WORKSPACE_ARTIFACT_ID))
				.Throws(TestException)
		};

		private static IEnumerable<Action<IntegrationPointProfileMigrationEventHandlerTests>> InvalidResultsSetups { get; } = new Action<IntegrationPointProfileMigrationEventHandlerTests>[]
		{
			ctx => ctx._createdWorkspaceRelativityObjectManager
				.Setup(x => x.MassDeleteAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(false)
		};

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<IAPILog>();
			_errorService = new Mock<IErrorService>();
			_eventHandlerHelper = new Mock<IEHHelper>();
			_templateWorkspaceRelativityObjectManager = new Mock<IRelativityObjectManager>();
			_createdWorkspaceRelativityObjectManager = new Mock<IRelativityObjectManager>();
			_relativityObjectManagerFactory = new Mock<IRelativityObjectManagerFactory>();
			_integrationPointProfilesQuery = new Mock<IIntegrationPointProfilesQuery>();

			_eventHandler = new IntegrationPointProfileMigrationEventHandler(
				_errorService.Object,
				() => _relativityObjectManagerFactory.Object,
				_integrationPointProfilesQuery.Object)
			{
				Helper = _eventHandlerHelper.Object,
				TemplateWorkspaceID = _TEMPLATE_WORKSPACE_ARTIFACT_ID
			};

			// We set up logger only for the event handler to execute properly (without throwing NullReferenceException)
			var loggerFactory = new Mock<ILogFactory>();
			loggerFactory
				.Setup(x => x.GetLogger())
				.Returns(_logger.Object);
			_logger
				.Setup(x => x.ForContext<DataTransferLocationMigrationEventHandler>())
				.Returns(_logger.Object);
			_eventHandlerHelper
				.Setup(x => x.GetLoggerFactory())
				.Returns(loggerFactory.Object);

			_eventHandlerHelper
				.Setup(x => x.GetActiveCaseID())
				.Returns(_CREATED_WORKSPACE_ARTIFACT_ID);

			_relativityObjectManagerFactory
				.Setup(x => x.CreateRelativityObjectManager(It.Is<int>(y => y == _TEMPLATE_WORKSPACE_ARTIFACT_ID)))
				.Returns(_templateWorkspaceRelativityObjectManager.Object);

			_relativityObjectManagerFactory
				.Setup(x => x.CreateRelativityObjectManager(It.Is<int>(y => y == _CREATED_WORKSPACE_ARTIFACT_ID)))
				.Returns(_createdWorkspaceRelativityObjectManager.Object);

			_createdWorkspaceRelativityObjectManager
				.Setup(x => x.MassDeleteAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(true);
		}

		[Test]
		[TestCaseSource(nameof(ServicesFailureSetups))]
		public void ItShouldFailOnServicesFailures(Action<IntegrationPointProfileMigrationEventHandlerTests> serviceFailureSetup)
		{
			// Arrange
			const int syncProfilesCount = 1;
			const int nonSyncProfilesCount = 1;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);
			serviceFailureSetup(this);

			// Act
			Response response = _eventHandler.Execute();

			// Assert
			response.Success.Should().BeFalse("handler should have failed");
			response
				.Exception.Should().BeAssignableTo<Exception>()
				.Which.Message.Should().Be(_TEST_ERROR_MESSAGE); // to make sure, that this is the exact exception that we are looking for
			response.Message.Should().Be(_TEST_ERROR_MESSAGE);
		}

		[Test]
		[TestCaseSource(nameof(InvalidResultsSetups))]
		public void ItShouldFailOnInvalidRelativityObjectManagerResults(Action<IntegrationPointProfileMigrationEventHandlerTests> invalidResultSetup)
		{
			// Arrange
			const int syncProfilesCount = 5;
			const int nonSyncProfilesCount = 5;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);
			invalidResultSetup(this);

			// Act
			Response response = _eventHandler.Execute();

			// Assert
			response.Success.Should().BeFalse("handler should have failed");
		}

		[Test]
		public void ItShouldNotDeleteProfilesWhenThereAreOnlySyncProfiles()
		{
			// Arrange
			const int syncProfilesCount = 5;
			const int nonSyncProfilesCount = 0;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);

			// Act
			Response response = _eventHandler.Execute();

			// Assert
			response.Success.Should().BeTrue("handler should have completed successfully");
			response.Exception.Should().BeNull("there was no failure");

			_createdWorkspaceRelativityObjectManager
				.Verify(x => x.MassDeleteAsync(It.IsAny<List<int>>(), It.IsAny<ExecutionIdentity>()),
					Times.Never);
		}

		[Test]
		[Ignore("To be implemented in REL-351468")]
		public void ItShouldNotUpdateProfilesWhenThereAreNoSyncProfiles()
		{
			// Arrange
			const int syncProfilesCount = 0;
			const int nonSyncProfilesCount = 5;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);

			// Act

			// Assert
			Assert.Fail();
		}

		[Test]
		public void ItShouldMassDeleteNonSyncProfilesAndModifySyncProfiles()
		{
			// Arrange
			const int syncProfilesCount = 5;
			const int nonSyncProfilesCount = 5;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);

			// Act
			Response response = _eventHandler.Execute();

			//Assert
			response.Success.Should().BeTrue("handler should have completed successfully");
			response.Exception.Should().BeNull("there was no failure");

			List<int> nonSyncProfilesArtifactIds = NonSyncProfilesArtifactIds(nonSyncProfilesCount).Select(x => x.ArtifactId).ToList();

			_createdWorkspaceRelativityObjectManager
				.Verify(x => x.MassDeleteAsync(It.Is<List<int>>(l => l.SequenceEqual(nonSyncProfilesArtifactIds)), It.IsAny<ExecutionIdentity>()),
					Times.Once);

			// TODO: make assertions after implementing REL-351468
		}

		private static List<IntegrationPointProfile> SyncProfilesArtifactIds(int count)
		{
			return Enumerable
				.Range(_FIRST_SYNC_PROFILE_ARTIFACT_ID, count)
				.Select(x => new IntegrationPointProfile())
				.ToList();
		}

		private static List<IntegrationPointProfile> NonSyncProfilesArtifactIds(int count)
		{
			return Enumerable
				.Range(_FIRST_NON_SYNC_PROFILE_ARTIFACT_ID, count)
				.Select(x => new IntegrationPointProfile())
				.ToList();
		}

		private void SetUpProfilesQuery(int nonSyncProfilesCount, int syncProfilesCount)
		{
			IEnumerable<IntegrationPointProfile> syncProfiles = SyncProfilesArtifactIds(syncProfilesCount);
			IEnumerable<IntegrationPointProfile> nonSyncProfiles = NonSyncProfilesArtifactIds(nonSyncProfilesCount);

			List<IntegrationPointProfile> allProfiles = new List<IntegrationPointProfile>();
			allProfiles.AddRange(syncProfiles);
			allProfiles.AddRange(nonSyncProfiles);

			_integrationPointProfilesQuery
				.Setup(x => x.GetAllProfilesAsync(_TEMPLATE_WORKSPACE_ARTIFACT_ID))
				.ReturnsAsync(allProfiles);

			_integrationPointProfilesQuery
				.Setup(x => x.GetSyncProfilesAsync(It.IsAny<IEnumerable<IntegrationPointProfile>>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(syncProfiles.Select(x => x.ArtifactId));

			_integrationPointProfilesQuery
				.Setup(x => x.GetNonSyncProfilesAsync(It.IsAny<IEnumerable<IntegrationPointProfile>>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(nonSyncProfiles.Select(x => x.ArtifactId));
		}
	}
}
