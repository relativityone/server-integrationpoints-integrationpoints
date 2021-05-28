using System.Threading.Tasks;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;


namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[IdentifiedTestFixture("74f17f40-697f-42b7-bad5-f83b8eeaa86a", Description = "RIP SYNC CI GOLD FLOWS")]
	[TestType.UI, TestType.MainFlow]
	public class SyncTests : TestsBase
	{
		private readonly SyncTestsImplementation _testsImplementation;

		public SyncTests()
			: base(nameof(SyncTests))
		{
			_testsImplementation = new SyncTestsImplementation(this);
		}

		protected override void OnSetUpFixture()
		{
			base.OnSetUpFixture();

			_testsImplementation.OnSetUpFixture();
		}

		protected override void OnTearDownFixture()
		{
			base.OnTearDownFixture();

			_testsImplementation.OnTearDownFixture();
		}

		[IdentifiedTest("b0afe8eb-e898-4763-9f95-e998f220b421")]
		public void SavedSearch_NativesAndMetadata_GoldFlow()
		{
			_testsImplementation.SavedSearchNativesAndMetadataGoldFlow();
		}

		[IdentifiedTest("26b72aab-a7ef-44ed-8338-81f91523388c")]
		public void Production_Images_GoldFlow()
		{
			_testsImplementation.ProductionImagesGoldFlow();
		}
	}
}
