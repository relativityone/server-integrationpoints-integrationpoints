using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Toggles;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Toggles;


namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[TestType.UI, TestType.MainFlow]
	public class SyncTests : TestsBase
	{
		private readonly IToggleProvider _toggleProvider;

		private readonly SyncTestsImplementation _testsImplementation;

		public SyncTests()
			: base(nameof(SyncTests))
		{
			_testsImplementation = new SyncTestsImplementation(this);
			_toggleProvider = ToggleProvider.Current;
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

		[TestType.Critical]
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

		[IdentifiedTest("0AB920A7-7F1A-4C72-82A7-F1A1CEB42863")]
		public async Task Production_Images_WithKeplerizedImportAPI()
		{
			await _toggleProvider.SetAsync<EnableKeplerizedImportAPIToggle>(true).ConfigureAwait(false);

			_testsImplementation.ProductionImagesGoldFlow();

			await _toggleProvider.SetAsync<EnableKeplerizedImportAPIToggle>(false).ConfigureAwait(false);
		}
	}
}
