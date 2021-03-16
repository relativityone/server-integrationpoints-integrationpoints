using System.Threading.Tasks;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
	internal sealed class ImageGoldFlowMultipleImagesSetTests : ImageGoldFlowTestsBase
	{
		public ImageGoldFlowMultipleImagesSetTests() : base(Dataset.MultipleImagesPerDocument, expectedItemsForRetry: 2)
		{
	
		}

		[IdentifiedTest("7C83DFD4-28F6-4BD2-9113-E894554C5866")]
		[TestType.MainFlow]
		public override Task SyncJob_Should_SyncImages()
		{
			return base.SyncJob_Should_SyncImages();
		}

		[IdentifiedTest("6982AC16-8444-426C-A648-654DB3B08C40")]
		[TestType.MainFlow]
		public override Task SyncJob_Should_RetryImages()
		{
			return base.SyncJob_Should_RetryImages();
		}
	}
}
