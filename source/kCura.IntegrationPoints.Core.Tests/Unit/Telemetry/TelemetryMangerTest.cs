using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Telemetry;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Telemetry
{
	public class TelemetryMangerTest
	{
		private TelemetryManager _instanceUnderTest;

		private IHelper _mockHelper;
		private IServicesMgr _mockServicesMgr;
		private IInternalMetricsCollectionManager _mockInternalMetricsCollectionManager;

		private Category _category;
		private CategoryTarget _categoryTarget;
		private List<CategoryTarget> _categoryTargets;

		[SetUp]
		public void Init()
		{
			_mockHelper = Substitute.For<IHelper>();
			_mockServicesMgr = Substitute.For<IServicesMgr>();
			_mockInternalMetricsCollectionManager = Substitute.For<IInternalMetricsCollectionManager>();

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

			_mockInternalMetricsCollectionManager.Received().UpdateCategoryTargetsAsync(_categoryTargets);

			Assert.That(_categoryTargets.Count, Is.EqualTo(1));
			Assert.That(_categoryTargets[0].IsCategoryMetricTargetEnabled[CategoryMetricTarget.APM]);
			Assert.That(_categoryTargets[0].IsCategoryMetricTargetEnabled[CategoryMetricTarget.SUM]);
		}

		[Test]
		public void ItShouldAddMetricsIdentifier()
		{
			// Arrange

			var mockTelemetryMetricProviderBase = Substitute.For<ITelemetryMetricProvider>();

			_mockInternalMetricsCollectionManager
				.CreateCategoryAsync(Arg.Any<Category>(), Arg.Any<bool>())
				.Returns(Task.FromResult(_category.ID));

			_mockInternalMetricsCollectionManager
				.GetCategoryTargetsAsync()
				.Returns(Task.FromResult(_categoryTargets));

			// Act

			_instanceUnderTest.AddMetricProviders(mockTelemetryMetricProviderBase);
			_instanceUnderTest.InstallMetrics();

			// Assert

			mockTelemetryMetricProviderBase.Received(1).Run(Arg.Any<Category>(), _mockHelper);
		}
	}
}
