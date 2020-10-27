using Relativity.Sync.Tests.System.Core.Helpers;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
	public class ImageGoldFlowMultipleImagesSetTests : ImageGoldFlowTestsBase
	{
		public ImageGoldFlowMultipleImagesSetTests() : base(Dataset.MultipleImagesPerDocument, expectedItemsForRetry: 2)
		{
	
		}
	}
}
