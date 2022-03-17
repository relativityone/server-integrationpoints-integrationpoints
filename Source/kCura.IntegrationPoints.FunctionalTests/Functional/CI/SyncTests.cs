using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Toggles;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Toggles;
using kCura.IntegrationPoints.Common.Toggles;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
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
			IToggleProvider toggleProvider = SqlToggleProvider.Create();
			try
            {
                SetIAPICommunicationMode(IAPICommunicationMode.ForceKepler);
				await toggleProvider.SetAsync<EnableKeplerizedImportAPIToggle>(true).ConfigureAwait(false);

				_testsImplementation.ProductionImagesGoldFlow();
			}
			finally
			{
				await toggleProvider.SetAsync<EnableKeplerizedImportAPIToggle>(false).ConfigureAwait(false);
                SetIAPICommunicationMode(IAPICommunicationMode.WebAPI);
			}
		}

		[IdentifiedTest("6E4C0033-D728-4C20-95ED-023527B598DE")]
		public async Task Entities_GoldFlow()
		{
			IToggleProvider toggleProvider = SqlToggleProvider.Create();
			try
			{
				await toggleProvider.SetAsync<EnableSyncNonDocumentFlowToggle>(true).ConfigureAwait(false);
				_testsImplementation.EntitiesPushGoldFlow();
			}
			finally
			{
				await toggleProvider.SetAsync<EnableSyncNonDocumentFlowToggle>(false).ConfigureAwait(false);
			}
		}

		private void SetIAPICommunicationMode(IAPICommunicationMode iapiCommunicationModeValue)
		{
			RelativityFacade
				.Instance
				.Resolve<IInstanceSettingsService>()
				.Require(new Testing.Framework.Models.InstanceSetting
					{
						Name = "IAPICommunicationMode",
						Section = "DataTransfer.Legacy",
						Value = iapiCommunicationModeValue.ToString(),
						ValueType = InstanceSettingValueType.Text
					}
				);
		}
    }
}
