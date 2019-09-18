using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints
{
	[TestFixture]
	public class IntegrationPointProfileMigrationEventHandlerTests
	{
		private IntegrationPointProfileMigrationEventHandler _eventHandler;
		private Mock<IErrorService> _errorService;
		private Mock<IAPILog> _logger;
		private Mock<IEHHelper> _eventHandlerHelper;
		private Mock<IObjectManager> _objectManager;
		private Mock<IServicesMgr> _servicesManager;
		private Mock<IRelativityObjectManagerFactory> _relativityObjectManagerFactory;
		private Mock<IRelativityObjectManager> _relativityObjectManager;
		private Mock<RetryHandler> _retryHandler;
		private List<int> _nonSyncProfilesArtifactIds;

		private const string _TEST_EXCEPTION_GUID = "EED05107-CB7F-4916-BC1C-C8DB3C8597C8";
		private const int _TEMPLATE_WORKSPACE_ARTIFACT_ID = 100111;
		private const int _CREATED_WORKSPACE_ARTIFACT_ID = 200111;
		private const int _DESTINATION_PROVIDER_IN_TEMPLATE_WORKSPACE_ARTIFACT_ID = 100222;
		private const int _SOURCE_PROVIDER_IN_TEMPLATE_WORKSPACE_ARTIFACT_ID = 100333;
		private const int _STARTING_ARTIFACT_ID_OF_CREATED_PROFILES = 300444;
		private const int _RETRY_TIMES = 3;
		private const int _TOTAL_PROFILES_COUNT = 5;

		private static readonly Guid _integrationPointProfileObjectTypeGuid = Guid.Parse("6DC915A9-25D7-4500-97F7-07CB98A06F64");
		private static readonly Guid _destinationProviderObjectTypeGuid = Guid.Parse("d014f00d-f2c0-4e7a-b335-84fcb6eae980");
		private static readonly Guid _sourceProviderObjectTypeGuid = Guid.Parse("5BE4A1F7-87A8-4CBE-A53F-5027D4F70B80");

		private static readonly Func<ServiceException> CreateTestException = () => new ServiceException(_TEST_EXCEPTION_GUID);

		public static IEnumerable<Action<IntegrationPointProfileMigrationEventHandlerTests>> ServicesFailureSetups { get; } = new Action<IntegrationPointProfileMigrationEventHandlerTests>[]
		{
			ctx => ctx._objectManager
				.Setup(x => x.QueryAsync(_TEMPLATE_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), 0, 1))
				.ThrowsAsync(CreateTestException()),
			ctx => ctx._objectManager
				.Setup(x => x.QueryAsync(_TEMPLATE_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), 0, int.MaxValue))
				.ThrowsAsync(CreateTestException()),
			ctx => ctx._objectManager
				.Setup(x => x.DeleteAsync(_CREATED_WORKSPACE_ARTIFACT_ID, It.IsAny<MassDeleteByCriteriaRequest>()))
				.ThrowsAsync(CreateTestException()),
			ctx => ctx._servicesManager
				.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>()))
				.Throws(CreateTestException()),
			ctx => ctx._eventHandlerHelper
				.Setup(x => x.GetServicesManager())
				.Throws(CreateTestException())
		};

		public static IEnumerable<Action<IntegrationPointProfileMigrationEventHandlerTests>> InvalidResultsSetups { get; } = new Action<IntegrationPointProfileMigrationEventHandlerTests>[]
		{
			ctx => ctx.SetUpObjectManagerQueryForSingleObjectCall(_TEMPLATE_WORKSPACE_ARTIFACT_ID, _destinationProviderObjectTypeGuid, _DESTINATION_PROVIDER_IN_TEMPLATE_WORKSPACE_ARTIFACT_ID, false),
			ctx => ctx.SetUpObjectManagerQueryForSingleObjectCall(_TEMPLATE_WORKSPACE_ARTIFACT_ID, _sourceProviderObjectTypeGuid, _SOURCE_PROVIDER_IN_TEMPLATE_WORKSPACE_ARTIFACT_ID, false),
			ctx => ctx.SetUpObjectManagerMassDeleteCall(false)
		};

		[SetUp]
		public void SetUp()
		{
			_eventHandlerHelper = new Mock<IEHHelper>();
			_objectManager = new Mock<IObjectManager>();
			_servicesManager = new Mock<IServicesMgr>();
			_logger = new Mock<IAPILog>();
			_errorService = new Mock<IErrorService>();
			_relativityObjectManagerFactory = new Mock<IRelativityObjectManagerFactory>();
			_relativityObjectManager = new Mock<IRelativityObjectManager>();
			_retryHandler = new Mock<RetryHandler>();

			_eventHandler = new IntegrationPointProfileMigrationEventHandler(_errorService.Object, () => _relativityObjectManagerFactory.Object)
			{
				Helper = _eventHandlerHelper.Object,
				TemplateWorkspaceID = _TEMPLATE_WORKSPACE_ARTIFACT_ID
			};

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

			_eventHandlerHelper
				.Setup(x => x.GetServicesManager())
				.Returns(_servicesManager.Object);

			_servicesManager
				.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_objectManager.Object);

			SetUpObjectManagerQueryForSingleObjectCall(_TEMPLATE_WORKSPACE_ARTIFACT_ID, _destinationProviderObjectTypeGuid, _DESTINATION_PROVIDER_IN_TEMPLATE_WORKSPACE_ARTIFACT_ID);
			SetUpObjectManagerQueryForSingleObjectCall(_TEMPLATE_WORKSPACE_ARTIFACT_ID, _sourceProviderObjectTypeGuid, _SOURCE_PROVIDER_IN_TEMPLATE_WORKSPACE_ARTIFACT_ID);
			SetUpObjectManagerQueryForNonSyncProfilesCall(_TEMPLATE_WORKSPACE_ARTIFACT_ID, _TOTAL_PROFILES_COUNT);

			SetUpObjectManagerMassDeleteCall(true);
		}

		private void SetUpObjectManagerMassDeleteCall(bool isSuccessful)
		{
			var massDeleteResult = new MassDeleteResult
			{
				Success = isSuccessful
			};

			_objectManager
				.Setup(x => x.DeleteAsync(_CREATED_WORKSPACE_ARTIFACT_ID, It.IsAny<MassDeleteByCriteriaRequest>()))
				.ReturnsAsync(massDeleteResult);
		}

		private void SetUpObjectManagerQueryForSingleObjectCall(int workspaceId, Guid objectTypeGuid, int relativityObjectArtifactId, bool isSuccessful = true)
		{
			var destinationProviderObject = new RelativityObject { ArtifactID = relativityObjectArtifactId };
			var relativityObjects = new List<RelativityObject>();

			if (isSuccessful)
			{
				relativityObjects.Add(destinationProviderObject);
			}

			var syncDestinationProviderQueryResult = new QueryResult
			{
				Objects = relativityObjects,
				ResultCount = relativityObjects.Count,
				TotalCount = relativityObjects.Count
			};

			_objectManager
				.Setup(x => x.QueryAsync(workspaceId, It.Is<QueryRequest>(request => request.ObjectType.Guid.Equals(objectTypeGuid)), 0, 1))
				.ReturnsAsync(syncDestinationProviderQueryResult);
		}

		private void SetUpObjectManagerQueryForNonSyncProfilesCall(int workspaceId, int totalCount)
		{
			_nonSyncProfilesArtifactIds = Enumerable
				.Range(_STARTING_ARTIFACT_ID_OF_CREATED_PROFILES, totalCount)
				.ToList();
			List<RelativityObject> relativityObjects = _nonSyncProfilesArtifactIds
				.Select(i => new RelativityObject { ArtifactID = i })
				.ToList();

			var syncDestinationProviderQueryResult = new QueryResult
			{
				Objects = relativityObjects,
				ResultCount = totalCount,
				TotalCount = totalCount
			};

			_objectManager
				.Setup(x => x.QueryAsync(workspaceId, It.Is<QueryRequest>(request => request.ObjectType.Guid.Equals(_integrationPointProfileObjectTypeGuid)), 0, int.MaxValue))
				.ReturnsAsync(syncDestinationProviderQueryResult);
		}

		[Test]
		[TestCaseSource(nameof(ServicesFailureSetups))]
		public void ItShouldRetryThreeTimesAndFailOnServiceFailures(Action<IntegrationPointProfileMigrationEventHandlerTests> serviceFailureSetup)
		{
			// Arrange
			serviceFailureSetup(this);

			// Act
			Response response = _eventHandler.Execute();

			// Assert
			_logger.Verify(x => x.LogWarning(
				It.Is<ServiceException>(exception => exception.Message.Equals(_TEST_EXCEPTION_GUID)),
				It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(_RETRY_TIMES));

			response.Success.Should().BeFalse("handler should have failed for 3 times");
			response.Exception.Should().BeAssignableTo<Exception>();
			response.Exception.Message.Should().Be(_TEST_EXCEPTION_GUID); // to make sure, that this is the exact exception that we are looking for
		}

		[Test]
		[TestCaseSource(nameof(InvalidResultsSetups))]
		public void ItShouldRetryThreeTimesAndFailOnInvalidResults(Action<IntegrationPointProfileMigrationEventHandlerTests> invalidResultSetup)
		{
			// Arrange
			invalidResultSetup(this);

			// Act
			Response response = _eventHandler.Execute();

			// Assert
			_logger.Verify(x => x.LogWarning(
				It.IsAny<IntegrationPointsException>(),
				It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(_RETRY_TIMES));

			response.Success.Should().BeFalse("handler should have failed for 3 times");
			response.Exception.Should().BeAssignableTo<Exception>();
		}

		[Test]
		public void ItShouldRetryUpToThreeTimesAndThenSucceed([Range(0, _RETRY_TIMES + 1)] int numberOfFailures)
		{
			// Arrange
			var successfulMassDeleteResult = new MassDeleteResult { Success = true };
			var failedMassDeleteResult = new MassDeleteResult { Success = false };

			List<MassDeleteResult> massDeleteResults = Enumerable
				.Repeat(failedMassDeleteResult, numberOfFailures)
				.ToList();
			massDeleteResults.Add(successfulMassDeleteResult);

			MassDeleteResult CreateResult()
			{
				MassDeleteResult massDeleteResult = massDeleteResults.Head();
				massDeleteResults.RemoveAt(0);
				return massDeleteResult;
			}

			_objectManager
				.Setup(x => x.DeleteAsync(_CREATED_WORKSPACE_ARTIFACT_ID, It.IsAny<MassDeleteByCriteriaRequest>()))
				.Returns(() => Task.FromResult(CreateResult()));

			// Act
			Response response = _eventHandler.Execute();

			// Assert
			_logger.Verify(x => x.LogWarning(
				It.IsAny<IntegrationPointsException>(),
				It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(Math.Min(_RETRY_TIMES, numberOfFailures)));

			if (numberOfFailures > _RETRY_TIMES)
			{
				response.Success.Should().BeFalse("handler should have failed for more than 3 times");
				response.Exception.Should().BeAssignableTo<Exception>();
			}
			else
			{
				response.Success.Should().BeTrue("handler should have failed for 3 times, and for the 4th time - it should have succeeded");
				response.Exception.Should().BeNull("there was no failure");
			}
		}

		[Test]
		public void ItShouldDoNothingWhenThereAreNoNonSyncProfiles()
		{
			// Arrange
			SetUpObjectManagerQueryForNonSyncProfilesCall(_TEMPLATE_WORKSPACE_ARTIFACT_ID, 0);

			// Act
			Response response = _eventHandler.Execute();

			// Assert
			response.Success.Should().BeTrue("handler should have completed successfully");
			response.Exception.Should().BeNull("there was no failure");

			_objectManager
				.Verify(x => x.DeleteAsync(_CREATED_WORKSPACE_ARTIFACT_ID,
					It.Is<MassDeleteByCriteriaRequest>(request =>
						request.ObjectIdentificationCriteria.ObjectType.Guid.Equals(_integrationPointProfileObjectTypeGuid))),
					Times.Never);
		}

		[Test]
		public void ItShouldMassDeleteNonSyncProfiles()
		{
			// Act
			Response response = _eventHandler.Execute();

			//Assert
			response.Success.Should().BeTrue("handler should have completed successfully");
			response.Exception.Should().BeNull("there was no failure");

			Condition expectedCondition = new WholeNumberCondition("ArtifactID", NumericConditionEnum.In, _nonSyncProfilesArtifactIds);
			_objectManager
				.Verify(x => x.DeleteAsync(_CREATED_WORKSPACE_ARTIFACT_ID,
						It.Is<MassDeleteByCriteriaRequest>(request =>
							request.ObjectIdentificationCriteria.ObjectType.Guid.Equals(_integrationPointProfileObjectTypeGuid) &&
							request.ObjectIdentificationCriteria.Condition.Equals(expectedCondition.ToQueryString(), StringComparison.OrdinalIgnoreCase))),
					Times.Once);
		}
	}
}
