using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Toggles;
using kCura.IntegrationPoints.Common.Toggles;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[TestType.UI, TestType.MainFlow]
	public class SyncTests : TestsBase
	{
		private readonly SyncTestsImplementation _testsImplementation;

		public SyncTests() : base(nameof(SyncTests))
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

		[Ignore("REL-695806")]
		[Test, TestType.Critical]
		public void SavedSearch_NativesAndMetadata_GoldFlow()
		{
			_testsImplementation.SavedSearchNativesAndMetadataGoldFlow();
		}

		[TestCase(YesNo.No)]
		[TestCase(YesNo.Yes)]
		public void Production_Images_GoldFlow(YesNo copyFilesToRepository)
		{
			_testsImplementation.ProductionImagesGoldFlow(copyFilesToRepository);
		}

		[Test]
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
