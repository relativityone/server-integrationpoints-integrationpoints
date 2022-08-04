using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Validators
{
    [TestFixture, Category("Unit")]
    public class PreCascadeDeleteEventHandlerValidatorTests : TestBase
    {
        #region Fields

        private const int _INTEGRATION_POINT_ID = 1;
        private const int _WORKSPACE_ID = 2;

        private PreCascadeDeleteEventHandlerValidator _instance;
        private IQueueRepository _queueRepository;
        private IIntegrationPointRepository _integrationPointRepository;

        #endregion //Fields

        [SetUp]
        public override void SetUp()
        {
            _queueRepository = Substitute.For<IQueueRepository>();
            _integrationPointRepository = Substitute.For<IIntegrationPointRepository>();

            _instance = new PreCascadeDeleteEventHandlerValidator(_queueRepository, _integrationPointRepository);
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
            _integrationPointRepository.DidNotReceive().ReadWithFieldMappingAsync(_WORKSPACE_ID);
        }

        [Test]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(1000)]
        public void ItShouldFailValidation(int numberOfJobs)
        {
            // Arrange
            _queueRepository.GetNumberOfJobsExecutingOrInQueue(_WORKSPACE_ID, _INTEGRATION_POINT_ID).Returns(numberOfJobs);

            _integrationPointRepository.ReadWithFieldMappingAsync(_INTEGRATION_POINT_ID).Returns(new Data.IntegrationPoint
            {
                Name = "integration_point_524"
            });

            // Act & Assert
            Assert.Throws<Exception>(() => _instance.Validate(_WORKSPACE_ID, _INTEGRATION_POINT_ID));
        }

        #endregion //Tests
    }
}