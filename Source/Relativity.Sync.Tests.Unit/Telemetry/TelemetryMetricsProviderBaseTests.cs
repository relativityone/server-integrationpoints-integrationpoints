using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit.Telemetry
{
    public class TelemetryMetricsProviderBaseTests
    {
        private Mock<IAPILog> _logger;
        private Mock<TelemetryMetricsProviderBase> _telemetryMetricsProvider;
        private Mock<IInternalMetricsCollectionManager> _metricsCollectionManager;
        private List<MetricIdentifier> _metricIdentifierList;
        private CategoryRef _category;

        private const string _PROVIDER_NAME = "TheProvider";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _category = new CategoryRef
            {
                ID = 0
            };
        }

        [SetUp]
        public void SetUp()
        {
            _metricsCollectionManager = new Mock<IInternalMetricsCollectionManager>();
            _logger = new Mock<IAPILog>();

            _metricsCollectionManager.Setup(x => x.CreateMetricIdentifierAsync(It.IsAny<MetricIdentifier>(), It.Is<bool>(y => y == false)))
                .Returns(Task.FromResult(1));

            _telemetryMetricsProvider = new Mock<TelemetryMetricsProviderBase>(_logger.Object);

            _telemetryMetricsProvider.Protected()
                .SetupGet<string>("ProviderName")
                .Returns(_PROVIDER_NAME);
        }

        private void SetupMetricIdentifierList(List<MetricIdentifier> metricIdentifierList)
        {
            _metricIdentifierList = metricIdentifierList;

            _telemetryMetricsProvider.Protected()
                .Setup<IEnumerable<MetricIdentifier>>("GetMetricIdentifiers")
                .Returns(_metricIdentifierList);
        }

        [Test]
        public void ItShouldNotThrowExceptionOnAddMetricsForCategoryFailure()
        {
            // ARRANGE
            var metricIdentifiers = new List<MetricIdentifier>
            {
                new MetricIdentifier
                {
                    Name = "abc",
                    Description = "def"
                }
            };
            SetupMetricIdentifierList(metricIdentifiers);

            _metricsCollectionManager.Setup(x => x.CreateMetricIdentifierAsync(It.IsAny<MetricIdentifier>(), It.Is<bool>(y => y == false)))
                .Throws<Exception>();

            // ACT
            Assert.DoesNotThrowAsync(async () => await _telemetryMetricsProvider.Object.AddMetricsForCategory(_metricsCollectionManager.Object, _category).ConfigureAwait(false));

            // ASSERT
            _logger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Contains(_category, _metricIdentifierList.First().Categories);
        }

        [Test]
        public async Task ItShouldCreateListWithAddedCategoryIfListDoesNotExist()
        {
            // ARRANGE
            var metricIdentifiers = new List<MetricIdentifier>
            {
                new MetricIdentifier
                {
                    Name = "abc",
                    Description = "def"
                }
            };
            SetupMetricIdentifierList(metricIdentifiers);

            // ACT
            await _telemetryMetricsProvider.Object.AddMetricsForCategory(_metricsCollectionManager.Object, _category).ConfigureAwait(false);

            // ASSERT
            _metricsCollectionManager.Verify(x => x.CreateMetricIdentifierAsync(It.IsAny<MetricIdentifier>(), It.Is<bool>(y => y == false)), Times.Once);
            _logger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);

            Assert.Contains(_category, _metricIdentifierList.First().Categories);
        }

        [Test]
        public async Task ItShouldAddCategoryToListIfListExists()
        {
            // ARRANGE
            var secondCategory = new CategoryRef()
            {
                ID = 1
            };
            var metricIdentifiers = new List<MetricIdentifier>
            {
                new MetricIdentifier
                {
                    Name = "abc",
                    Description = "def",
                    Categories = new List<CategoryRef>
                    {
                        secondCategory
                    }
                }
            };

            SetupMetricIdentifierList(metricIdentifiers);

            // ACT
            await _telemetryMetricsProvider.Object.AddMetricsForCategory(_metricsCollectionManager.Object, _category).ConfigureAwait(false);

            // ASSERT
            _metricsCollectionManager.Verify(x => x.CreateMetricIdentifierAsync(It.IsAny<MetricIdentifier>(), It.Is<bool>(y => y == false)), Times.Once);
            _logger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);

            Assert.Contains(_category, _metricIdentifierList.First().Categories);
            Assert.Contains(secondCategory, _metricIdentifierList.First().Categories);
        }
    }
}