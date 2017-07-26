using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Validators
{
	public class PreCascadeDeleteEventHandlerValidatorTests : TestBase
	{
		#region Fields

		private const int _INTEGRATION_POINT_ID = 1;
		private const int _WORKSPACE_ID = 2;

		private PreCascadeDeleteEventHandlerValidator _instance;
		private IQueueRepository _queueRepository;
		private IRSAPIService _rsapiService;

		#endregion //Fields

		[SetUp]
		public override void SetUp()
		{
			_queueRepository = Substitute.For<IQueueRepository>();
			_rsapiService = Substitute.For<IRSAPIService>();

			IRSAPIServiceFactory rsapiServiceFactory = Substitute.For<IRSAPIServiceFactory>();
			rsapiServiceFactory.Create(_WORKSPACE_ID).Returns(_rsapiService);

			_instance = new PreCascadeDeleteEventHandlerValidator(_queueRepository, rsapiServiceFactory);
		}

		#region Tests

		[Test]
		public void ItShouldPassValidation()
		{
			// Arrange
			_queueRepository.GetNumberOfJobsExecutingOrInQueue(_WORKSPACE_ID, _INTEGRATION_POINT_ID).Returns(0);

			// Act
			_instance.Validate(_WORKSPACE_ID, _INTEGRATION_POINT_ID);

			// Assert
			_rsapiService.IntegrationPointLibrary.DidNotReceive().Read(_WORKSPACE_ID);
		}

		[Test]
		[TestCase(1)]
		[TestCase(5)]
		[TestCase(1000)]
		public void ItShouldFailValidation(int numberOfJobs)
		{
			// Arrange
			_queueRepository.GetNumberOfJobsExecutingOrInQueue(_WORKSPACE_ID, _INTEGRATION_POINT_ID).Returns(numberOfJobs);

			_rsapiService.IntegrationPointLibrary.Read(_INTEGRATION_POINT_ID).Returns(new Data.IntegrationPoint
			{
				Name = "integration_point_524"
			});

			// Act & Assert
			Assert.Throws<Exception>(() => _instance.Validate(_WORKSPACE_ID, _INTEGRATION_POINT_ID));
		}

		#endregion //Tests
	}
}