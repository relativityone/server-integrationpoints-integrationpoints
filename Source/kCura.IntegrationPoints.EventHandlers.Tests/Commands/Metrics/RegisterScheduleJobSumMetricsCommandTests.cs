using kCura.IntegrationPoints.EventHandlers.Commands.Metrics;
using NUnit.Framework;
using Relativity.API;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.InternalMetricsCollection;
using Moq;
using NSubstitute;
using kCura.IntegrationPoints.Core.Telemetry;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands.Metrics
{
    [TestFixture, Category("Unit")]
    public class RegisterScheduleJobSumMetricsCommandTests
    {
        private Mock<IEHHelper> _helperFake;
        private Mock<IServicesMgr> _servicesMgrFake;
        private Mock<IInternalMetricsCollectionManager> _metricsCollectionManagerFake;

        private IAPILog _emptyLogger;

        private RegisterScheduleJobSumMetricsCommand _sut;

        [SetUp]
        public void SetUp()
        {
            _metricsCollectionManagerFake = new Mock<IInternalMetricsCollectionManager>();
            _metricsCollectionManagerFake.Setup(x => x.CreateMetricIdentifierAsync(It.IsAny<MetricIdentifier>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(1));

            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(x => x.CreateProxy<IInternalMetricsCollectionManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_metricsCollectionManagerFake.Object);

            _emptyLogger = Substitute.For<IAPILog>();

            Mock<ILogFactory> loggerFactoryFake = new Mock<ILogFactory>();
            loggerFactoryFake.Setup(x => x.GetLogger()).Returns(_emptyLogger);

            _helperFake = new Mock<IEHHelper>();
            _helperFake.Setup(x => x.GetServicesManager()).Returns(_servicesMgrFake.Object);
            _helperFake.Setup(x => x.GetLoggerFactory()).Returns(loggerFactoryFake.Object);

            _sut = new RegisterScheduleJobSumMetricsCommand(_helperFake.Object);
        }

        [Test]
        public void Execute_ShouldRegisterMetrics_WhenCategoryAlreadyExists()
        {
            // Arrange
            List<CategoryTarget> categoryList = new List<CategoryTarget>
            {
                new CategoryTarget
                {
                    Category = new CategoryRef
                    {
                        Name = MetricsBucket.SyncSchedule.SYNC_SCHEDULE_CATEGORY
                    }
                }
            };

            _metricsCollectionManagerFake.Setup(x => x.GetCategoryTargetsAsync()).Returns(Task.FromResult(categoryList));

            // Act
            _sut.Execute();

            // Assert
            _metricsCollectionManagerFake.Verify(x => x.CreateCategoryAsync(It.IsAny<Category>(), false), Times.Never);
            _metricsCollectionManagerFake.Verify(x => x.UpdateCategoryTargetSingleAsync(It.IsAny<CategoryTarget>()), Times.Never);

            _metricsCollectionManagerFake.Verify(x => x.CreateMetricIdentifierAsync(It.IsAny<MetricIdentifier>(), false),
                Times.Exactly(MetricsBucket.SyncSchedule.METRICS.Count));
        }

        [Test]
        public void Execute_ShouldRegisterMetricsAndCreatesCategory_WhenCategoryDoesNotExist()
        {
            // Arrange
            List<CategoryTarget> categoryList = new List<CategoryTarget>();
            
            _metricsCollectionManagerFake.Setup(x => x.GetCategoryTargetsAsync()).Returns(Task.FromResult(categoryList));

            // Act
            _sut.Execute();

            // Assert
            _metricsCollectionManagerFake.Verify(x => x.CreateCategoryAsync(It.Is<Category>(category => category.Name.Equals(MetricsBucket.SyncSchedule.SYNC_SCHEDULE_CATEGORY)), false),
                Times.Once);
            _metricsCollectionManagerFake.Verify(x => x.UpdateCategoryTargetSingleAsync(It.IsAny<CategoryTarget>()), Times.Once);

            _metricsCollectionManagerFake.Verify(x => x.CreateMetricIdentifierAsync(It.IsAny<MetricIdentifier>(), false),
                Times.Exactly(MetricsBucket.SyncSchedule.METRICS.Count));
        }
    }
}
