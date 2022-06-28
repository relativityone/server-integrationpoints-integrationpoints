using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints
{
	[TestFixture, Category("Unit")]
	public class IntegrationPointProfileMigrationEventHandlerTests
	{
		private IntegrationPointProfileMigrationEventHandler _sut;
		private Mock<IErrorService> _errorServiceFake;
		private Mock<IAPILog> _loggerFake;
		private Mock<IEHHelper> _eventHandlerHelperFake;
		private Mock<IRelativityObjectManagerFactory> _relativityObjectManagerFactoryFake;
		private Mock<IRelativityObjectManager> _templateWorkspaceRelativityObjectManagerFake;
		private Mock<IRelativityObjectManager> _createdWorkspaceRelativityObjectManagerMock;
		private Mock<IIntegrationPointProfilesQuery> _integrationPointProfilesQueryFake;

		private const string _TEST_ERROR_MESSAGE = "Failed to migrate the Integration Point Profiles.";
		private const int _TEMPLATE_WORKSPACE_ARTIFACT_ID = 100111;
		private const int _CREATED_WORKSPACE_ARTIFACT_ID = 200111;
		private const int _FIRST_SYNC_PROFILE_ARTIFACT_ID = 300444;
		private const int _FIRST_NON_SYNC_PROFILE_ARTIFACT_ID = 400444;
		private const int _SAVED_SEARCH_ARTIFACT_ID = 123234;

		private static ServiceException TestException => new ServiceException(_TEST_ERROR_MESSAGE);

		private static IEnumerable<Action<IntegrationPointProfileMigrationEventHandlerTests>> ServicesFailureSetups { get; } = new Action<IntegrationPointProfileMigrationEventHandlerTests>[]
		{
			ctx => ctx._createdWorkspaceRelativityObjectManagerMock
				.Setup(x => x.MassDeleteAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<ExecutionIdentity>()))
				.Throws(TestException),
			ctx => ctx._integrationPointProfilesQueryFake
				.Setup(x => x.GetAllProfilesAsync(_TEMPLATE_WORKSPACE_ARTIFACT_ID))
				.Throws(TestException)
		};

		private static IEnumerable<Action<IntegrationPointProfileMigrationEventHandlerTests>> InvalidResultsSetups { get; } = new Action<IntegrationPointProfileMigrationEventHandlerTests>[]
		{
			ctx => ctx._createdWorkspaceRelativityObjectManagerMock
				.Setup(x => x.MassDeleteAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(false)
		};

		[SetUp]
		public void SetUp()
		{
			_loggerFake = new Mock<IAPILog>();
			_errorServiceFake = new Mock<IErrorService>();
			_eventHandlerHelperFake = new Mock<IEHHelper>();
			_templateWorkspaceRelativityObjectManagerFake = new Mock<IRelativityObjectManager>();
			_createdWorkspaceRelativityObjectManagerMock = new Mock<IRelativityObjectManager>();
			_relativityObjectManagerFactoryFake = new Mock<IRelativityObjectManagerFactory>();
			_integrationPointProfilesQueryFake = new Mock<IIntegrationPointProfilesQuery>();

			_sut = new IntegrationPointProfileMigrationEventHandler(
				_errorServiceFake.Object,
				() => _relativityObjectManagerFactoryFake.Object,
				_integrationPointProfilesQueryFake.Object,
				BuildRepositoryFactoryMock().Object)
			{
				Helper = _eventHandlerHelperFake.Object,
				TemplateWorkspaceID = _TEMPLATE_WORKSPACE_ARTIFACT_ID
			};

			// We set up logger only for the event handler to execute properly (without throwing NullReferenceException)
			var loggerFactory = new Mock<ILogFactory>();
			loggerFactory
				.Setup(x => x.GetLogger())
				.Returns(_loggerFake.Object);
			_loggerFake
				.Setup(x => x.ForContext<DataTransferLocationMigrationEventHandler>())
				.Returns(_loggerFake.Object);
			_eventHandlerHelperFake
				.Setup(x => x.GetLoggerFactory())
				.Returns(loggerFactory.Object);

			_eventHandlerHelperFake
				.Setup(x => x.GetActiveCaseID())
				.Returns(_CREATED_WORKSPACE_ARTIFACT_ID);

			_relativityObjectManagerFactoryFake
				.Setup(x => x.CreateRelativityObjectManager(It.Is<int>(y => y == _TEMPLATE_WORKSPACE_ARTIFACT_ID)))
				.Returns(_templateWorkspaceRelativityObjectManagerFake.Object);

			_relativityObjectManagerFactoryFake
				.Setup(x => x.CreateRelativityObjectManager(It.Is<int>(y => y == _CREATED_WORKSPACE_ARTIFACT_ID)))
				.Returns(_createdWorkspaceRelativityObjectManagerMock.Object);

			_createdWorkspaceRelativityObjectManagerMock
				.Setup(x => x.MassDeleteAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(true);
		}

		private Mock<IRepositoryFactory> BuildRepositoryFactoryMock()
		{
			Mock<ISavedSearchQueryRepository> searchQueryRepositoryMock = new Mock<ISavedSearchQueryRepository>();
			searchQueryRepositoryMock
				.Setup(x => x.RetrieveSavedSearch(It.Is<int>(y => y == _SAVED_SEARCH_ARTIFACT_ID)))
				.Returns(GetSavedSearch(_SAVED_SEARCH_ARTIFACT_ID));
			searchQueryRepositoryMock
				.Setup(x => x.RetrievePublicSavedSearches())
				.Returns(new [] { GetSavedSearch(_SAVED_SEARCH_ARTIFACT_ID) });

			Mock<IRepositoryFactory> repositoryFactoryMock = new Mock<IRepositoryFactory>();
			repositoryFactoryMock
				.Setup(x => x.GetSavedSearchQueryRepository(It.IsIn(_TEMPLATE_WORKSPACE_ARTIFACT_ID, _CREATED_WORKSPACE_ARTIFACT_ID)))
				.Returns(searchQueryRepositoryMock.Object);

			return repositoryFactoryMock;

			SavedSearchDTO GetSavedSearch(int artifactId)
			{
				return new SavedSearchDTO
				{
					ArtifactId = artifactId,
					Name = artifactId.ToString()
				};
			}
		}

		[Test]
		[TestCaseSource(nameof(ServicesFailureSetups))]
		public void Execute_ShouldFail_WhenServicesFailures(Action<IntegrationPointProfileMigrationEventHandlerTests> serviceFailureSetup)
		{
			// Arrange
			const int syncProfilesCount = 1;
			const int nonSyncProfilesCount = 1;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);
			serviceFailureSetup(this);

			// Act
			Response response = _sut.Execute();

			// Assert
			response.Success.Should().BeFalse("handler should have failed");
			response
				.Exception.Should().BeAssignableTo<Exception>()
				.Which.Message.Should().Be(_TEST_ERROR_MESSAGE); // to make sure, that this is the exact exception that we are looking for
			response.Message.Should().Be(_TEST_ERROR_MESSAGE);
		}

		[Test]
		[TestCaseSource(nameof(InvalidResultsSetups))]
		public void Execute_ShouldFail_WhenInvalidRelativityObjectManagerResults(Action<IntegrationPointProfileMigrationEventHandlerTests> invalidResultSetup)
		{
			// Arrange
			const int syncProfilesCount = 5;
			const int nonSyncProfilesCount = 5;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);
			invalidResultSetup(this);

			// Act
			Response response = _sut.Execute();

			// Assert
			response.Success.Should().BeFalse("handler should have failed");
		}

		[Test]
		public void Execute_ShouldNotDeleteProfiles_WhenThereAreOnlySyncProfiles()
		{
			// Arrange
			const int syncProfilesCount = 5;
			const int nonSyncProfilesCount = 0;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);
			_createdWorkspaceRelativityObjectManagerMock.Setup(x =>
					x.MassUpdateAsync(
						It.IsAny<IEnumerable<int>>(),
						It.IsAny<IEnumerable<FieldRefValuePair>>(),
						It.IsAny<FieldUpdateBehavior>(),
						It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(true);

			// Act
			Response response = _sut.Execute();

			// Assert
			response.Success.Should().BeTrue("handler should have completed successfully");
			response.Exception.Should().BeNull("there was no failure");

			_createdWorkspaceRelativityObjectManagerMock
				.Verify(x => x.MassDeleteAsync(It.IsAny<List<int>>(), It.IsAny<ExecutionIdentity>()),
					Times.Never);
		}

		[Test]
		public void Execute_ShouldNotUpdateProfiles_WhenThereAreNoSyncProfiles()
		{
			// Arrange
			const int syncProfilesCount = 0;
			const int nonSyncProfilesCount = 5;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);

			// Act
			Response response = _sut.Execute();

			// Assert
			response.Success.Should().BeTrue("handler should have completed successfully");
			response.Exception.Should().BeNull("there was no failure");
			_createdWorkspaceRelativityObjectManagerMock
				.Verify(x => x.MassUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<FieldRefValuePair>>(),
						It.IsAny<FieldUpdateBehavior>(), It.IsAny<ExecutionIdentity>()), Times.Never);
		}

		[Test]
		public void Execute_ShouldMassDeleteAndModifyProfiles()
		{
			// Arrange
			const int syncProfilesCount = 0;
			const int nonSyncProfilesCount = 5;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);

			// Act
			Response response = _sut.Execute();

			//Assert
			response.Success.Should().BeTrue("handler should have completed successfully");
			response.Exception.Should().BeNull("there was no failure");

			List<int> nonSyncProfilesArtifactIDs = ProfilesToDeleteArtifactIDs(nonSyncProfilesCount).Select(x => x.ArtifactId).ToList();

			_createdWorkspaceRelativityObjectManagerMock
				.Verify(x => x.MassDeleteAsync(It.Is<IEnumerable<int>>(l => l.SequenceEqual(nonSyncProfilesArtifactIDs)), It.IsAny<ExecutionIdentity>()),
					Times.Once);
		}

		[Test]
		public void Execute_ShouldModifyProfiles()
		{
			// Arrange
			const int syncProfilesCount = 5;
			const int nonSyncProfilesCount = 0;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);
			_createdWorkspaceRelativityObjectManagerMock.Setup(x =>
					x.MassUpdateAsync(
						It.IsAny<IEnumerable<int>>(),
						It.IsAny<IEnumerable<FieldRefValuePair>>(),
						It.IsAny<FieldUpdateBehavior>(),
						It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(true);

			// Act
			Response response = _sut.Execute();

			//Assert
			response.Success.Should().BeTrue("handler should have completed successfully");
			response.Exception.Should().BeNull("there was no failure");

			_createdWorkspaceRelativityObjectManagerMock
				.Verify(x => x.MassUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<FieldRefValuePair>>(),
					It.IsAny<FieldUpdateBehavior>(), It.IsAny<ExecutionIdentity>()), Times.Exactly(syncProfilesCount));
		}

		[Test]
		public void Execute_ShouldLogNotMigratedProfiles_WhenTheyDontExistInNewWorkspace()
		{
			// Arrange
			const int syncProfilesCount = 5;
			const int nonSyncProfilesCount = 0;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);
			_createdWorkspaceRelativityObjectManagerMock.Setup(x =>
					x.MassUpdateAsync(
						It.IsAny<IEnumerable<int>>(),
						It.IsAny<IEnumerable<FieldRefValuePair>>(),
						It.IsAny<FieldUpdateBehavior>(),
						It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(true);

			_integrationPointProfilesQueryFake
				.Setup(x => x.CheckIfProfilesExistAsync(_CREATED_WORKSPACE_ARTIFACT_ID, It.IsAny<IEnumerable<int>>()))
				.ReturnsAsync(ProfilesToModifyArtifactIds(syncProfilesCount).Skip(1).Select(x => x.ArtifactId));

			// Act
			Response response = _sut.Execute();

			//Assert
			response.Success.Should().BeTrue("handler should have completed successfully");
			response.Exception.Should().BeNull("there was no failure");

			_loggerFake.Verify(x =>
				x.LogWarning(
					IntegrationPointProfileMigrationEventHandler._profilesDoNotExistInCreatedWorkspaceMessageTemplate_Migration,
					_CREATED_WORKSPACE_ARTIFACT_ID, new[] { _FIRST_SYNC_PROFILE_ARTIFACT_ID }));

			_createdWorkspaceRelativityObjectManagerMock
				.Verify(x => x.MassUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<FieldRefValuePair>>(),
					It.IsAny<FieldUpdateBehavior>(), It.IsAny<ExecutionIdentity>()), Times.Exactly(syncProfilesCount - 1));
		}


		[Test]
		public void Execute_ShouldLogNotDeleteProfiles_WhenTheyDontExistInNewWorkspace()
		{
			// Arrange
			const int syncProfilesCount = 0;
			const int nonSyncProfilesCount = 5;
			SetUpProfilesQuery(nonSyncProfilesCount, syncProfilesCount);
			_createdWorkspaceRelativityObjectManagerMock.Setup(x =>
					x.MassUpdateAsync(
						It.IsAny<IEnumerable<int>>(),
						It.IsAny<IEnumerable<FieldRefValuePair>>(),
						It.IsAny<FieldUpdateBehavior>(),
						It.IsAny<ExecutionIdentity>()))
				.ReturnsAsync(true);

			IEnumerable<int> profilesToDelete_SkipOne = ProfilesToDeleteArtifactIDs(nonSyncProfilesCount).Skip(1).Select(x => x.ArtifactId);
			_integrationPointProfilesQueryFake
				.Setup(x => x.CheckIfProfilesExistAsync(_CREATED_WORKSPACE_ARTIFACT_ID, It.IsAny<IEnumerable<int>>()))
				.ReturnsAsync(profilesToDelete_SkipOne);

			// Act
			Response response = _sut.Execute();

			//Assert
			response.Success.Should().BeTrue("handler should have completed successfully");
			response.Exception.Should().BeNull("there was no failure");

			_loggerFake.Verify(x =>
				x.LogWarning(
					IntegrationPointProfileMigrationEventHandler._profilesDoNotExistInCreatedWorkspaceMessageTemplate_Deletion,
					_CREATED_WORKSPACE_ARTIFACT_ID, new[] { _FIRST_NON_SYNC_PROFILE_ARTIFACT_ID }));

			_createdWorkspaceRelativityObjectManagerMock
				.Verify(x => x.MassDeleteAsync(profilesToDelete_SkipOne, It.IsAny<ExecutionIdentity>()), Times.Once);
		}

		private static List<IntegrationPointProfile> ProfilesToModifyArtifactIds(int count)
		{
			return Enumerable
				.Range(_FIRST_SYNC_PROFILE_ARTIFACT_ID, count)
				.Select(CreateProfileMock)
				.ToList();
		}

		private static List<IntegrationPointProfile> ProfilesToDeleteArtifactIDs(int count)
		{
			return Enumerable
				.Range(_FIRST_NON_SYNC_PROFILE_ARTIFACT_ID, count)
				.Select(CreateProfileMock)
				.ToList();
		}

		private static IntegrationPointProfile CreateProfileMock(int artifactID)
		{
			return new IntegrationPointProfile()
			{
				ArtifactId = artifactID,
				SourceConfiguration = "{\"SavedSearchArtifactId\": 123234,\n  \"TypeOfExport\": 3,\n  \"SourceWorkspaceArtifactId\": 1017953}",
				DestinationConfiguration = "{\"DestinationFolderArtifactId\":\"1003697\",\"ProductionImport\":false,\"ImageImport\":\"true\",\"ProductionPrecedence\":1}"
			};
		}

		private void SetUpProfilesQuery(int profilesToDeleteCount, int profilesToUpdateCount)
		{
			IEnumerable<IntegrationPointProfile> profilesToUpdate = ProfilesToModifyArtifactIds(profilesToUpdateCount);
			IEnumerable<IntegrationPointProfile> profilesToDelete = ProfilesToDeleteArtifactIDs(profilesToDeleteCount);

			List<IntegrationPointProfile> allProfiles = new List<IntegrationPointProfile>();
			allProfiles.AddRange(profilesToUpdate);
			allProfiles.AddRange(profilesToDelete);

			_integrationPointProfilesQueryFake
				.Setup(x => x.GetAllProfilesAsync(_TEMPLATE_WORKSPACE_ARTIFACT_ID))
				.ReturnsAsync(allProfiles);

			_integrationPointProfilesQueryFake.Setup(x =>
					x.CheckIfProfilesExistAsync(_CREATED_WORKSPACE_ARTIFACT_ID, It.IsAny<IEnumerable<int>>()))
				.ReturnsAsync(profilesToUpdate.Concat(profilesToDelete).Select(x => x.ArtifactId));

			_integrationPointProfilesQueryFake
				.Setup(x => x.GetProfilesToUpdate(It.IsAny<IEnumerable<IntegrationPointProfile>>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns(profilesToUpdate);

			_integrationPointProfilesQueryFake
				.Setup(x => x.GetProfilesToDelete(It.IsAny<IEnumerable<IntegrationPointProfile>>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns(profilesToDelete);
		}
	}
}
