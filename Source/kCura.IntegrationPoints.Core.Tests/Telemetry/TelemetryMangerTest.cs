using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Telemetry;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace kCura.IntegrationPoints.Core.Tests.Telemetry
{
    [TestFixture, Category("Unit")]
    public class TelemetryMangerTest : TestBase
    {
        #region Fields

        private TelemetryManager _instanceUnderTest;

        private IHelper _mockHelper;
        private IServicesMgr _mockServicesMgr;
        private IInternalMetricsCollectionManager _mockInternalMetricsCollectionManager;
        private ITelemetryMetricProvider _mockTelemetryMetricProviderBase;

        private Category _category;
        private CategoryTarget _categoryTarget;
        private List<CategoryTarget> _categoryTargets;

        #endregion //Fields

        [SetUp]
        public override void SetUp()
        {
            _mockHelper = Substitute.For<IHelper>();
            _mockServicesMgr = Substitute.For<IServicesMgr>();
            _mockInternalMetricsCollectionManager = Substitute.For<IInternalMetricsCollectionManager>();
            _mockTelemetryMetricProviderBase = Substitute.For<ITelemetryMetricProvider>();

            _mockHelper.GetServicesManager().Returns(_mockServicesMgr);

            _mockServicesMgr.CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System).Returns(_mockInternalMetricsCollectionManager);

            _instanceUnderTest = new TelemetryManager(_mockHelper);

            _category = new Category
            {
                ID = 1,
                Name = Constants.IntegrationPoints.Telemetry.TELEMETRY_CATEGORY
            };

            _categoryTarget = new CategoryTarget
            {
                Category = _category
            };

            _categoryTargets = new List<CategoryTarget>(new[] { _categoryTarget });
        }

        #region Tests

        [Test]
        public void ItShouldAddIntegrationPointCategory()
        {
            // Arrange
            _mockInternalMetricsCollectionManager
                .CreateCategoryAsync(Arg.Any<Category>(), false)
                .Returns(Task.FromResult(_category.ID));

            _mockInternalMetricsCollectionManager
                .GetCategoryTargetsAsync()
                .Returns(Task.FromResult(_categoryTargets));

            // Act
            _instanceUnderTest.InstallMetrics();

            // Assert
            _mockInternalMetricsCollectionManager
                .Received(1)
                .CreateCategoryAsync(Arg.Is<Category>(item => item.Name == Constants.IntegrationPoints.Telemetry.TELEMETRY_CATEGORY), false);

            _mockInternalMetricsCollectionManager
                .Received()
                .UpdateCategoryTargetsAsync(_categoryTargets);

            _mockTelemetryMetricProviderBase
                .DidNotReceive()
                .Run(Arg.Any<Category>(), Arg.Any<IHelper>());
        }

        [Test]
        public void ItShouldThrowExceptionOnCategoryCreation()
        {
            // Arrange
            _mockInternalMetricsCollectionManager
                .CreateCategoryAsync(Arg.Any<Category>(), false)
                .Throws<AggregateException>();

            // Act/Assert
            Assert.That(() => _instanceUnderTest.InstallMetrics(), Throws.TypeOf<Exception>());
        }

        [Test]
        public void ItShouldEnabledCategoryMetrics()
        {
            // Arrange

            _mockInternalMetricsCollectionManager
                .CreateCategoryAsync(Arg.Any<Category>(), false)
                .Returns(Task.FromResult(_category.ID));

            _mockInternalMetricsCollectionManager
                .GetCategoryTargetsAsync()
                .Returns(Task.FromResult(_categoryTargets));

            // Act
            _instanceUnderTest.InstallMetrics();

            // Assert
            Assert.That(_categoryTargets.Count, Is.EqualTo(1));
            Assert.That(_categoryTargets[0].IsCategoryMetricTargetEnabled[CategoryMetricTarget.APM]);
            Assert.That(_categoryTargets[0].IsCategoryMetricTargetEnabled[CategoryMetricTarget.SUM]);
        }


        [Test]
        public void ItShouldThrowExceptionOnMetricEnabled()
        {
            // Arrange
            _mockInternalMetricsCollectionManager
                .CreateCategoryAsync(Arg.Any<Category>(), Arg.Any<bool>())
                .Returns(Task.FromResult(_category.ID));

            _mockInternalMetricsCollectionManager
                .GetCategoryTargetsAsync()
                .Throws<AggregateException>();

            // Act/Assert
            Assert.That(() => _instanceUnderTest.InstallMetrics(), Throws.TypeOf<Exception>());
        }

        [Test]
        public void ItShouldAddMetricsIdentifier()
        {
            // Arrange
            _mockInternalMetricsCollectionManager
                .CreateCategoryAsync(Arg.Any<Category>(), Arg.Any<bool>())
                .Returns(Task.FromResult(_category.ID));

            _mockInternalMetricsCollectionManager
                .GetCategoryTargetsAsync()
                .Returns(Task.FromResult(_categoryTargets));

            // Act
            _instanceUnderTest.AddMetricProviders(_mockTelemetryMetricProviderBase);
            _instanceUnderTest.InstallMetrics();

            // Assert
            _mockTelemetryMetricProviderBase
                .Received(1)
                .Run(Arg.Is<Category>(item => item.Name == Constants.IntegrationPoints.Telemetry.TELEMETRY_CATEGORY), _mockHelper);
        }

        [Test]
        public void ItShouldThrowExceptionOnAddNullMetricProvider()
        {
            // Act/Assert
            Assert.That(() => _instanceUnderTest.AddMetricProviders(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ItShouldThrowExceptionOnNotFoundCategory()
        {
            // Arrange
            _mockInternalMetricsCollectionManager
                .CreateCategoryAsync(Arg.Any<Category>(), false)
                .Returns(Task.FromResult(_category.ID));

            _mockInternalMetricsCollectionManager
                .GetCategoryTargetsAsync()
                .Returns(Task.FromResult(new List<CategoryTarget>()));

            // Act/Assert
            Assert.That(() => _instanceUnderTest.InstallMetrics(), Throws.TypeOf<Exception>());
        }

        #endregion //Tests
    }
}
