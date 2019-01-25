using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Productions.Services;
using Relativity.Services.Search;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class ProductionDataSource
	{
		private static ITestHelper Helper => new TestHelper();

		public static async Task<int> CreateDataSourceWithPlaceholderAsync(
			int workspaceId,
			int productionId,
			int savedSearchId,
			UseImagePlaceholderOption useImagePlaceholder,
			int placeholderId)
		{
			const int markupSetArtifactId = 1034197; // TODO why it is harcoded???

			using (var productionManager = Helper.CreateAdminProxy<IProductionDataSourceManager>())
			{
				var productionDataSource = new global::Relativity.Productions.Services.ProductionDataSource
				{
					ProductionType = ProductionType.ImagesAndNatives,
					SavedSearch = new SavedSearchRef(savedSearchId),
					UseImagePlaceholder = useImagePlaceholder,
					Placeholder = new ProductionPlaceholderRef(placeholderId) { Name = "CustomPlaceholder" },
					MarkupSet = new MarkupSetRef(markupSetArtifactId),
					BurnRedactions = true,
					Name = "TestDataSource"
				};

				return await productionManager.CreateSingleAsync(workspaceId, productionId, productionDataSource).ConfigureAwait(false);
			}
		}
	}
}