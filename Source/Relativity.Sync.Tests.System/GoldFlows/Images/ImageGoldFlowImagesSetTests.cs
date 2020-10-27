﻿using Relativity.Sync.Tests.System.Core.Helpers;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
	public class ImageGoldFlowImagesSetTests : ImageGoldFlowTestsBase
	{
		public ImageGoldFlowImagesSetTests() : base(Dataset.Images, expectedItemsForRetry: 3, expectedDocumentsForRetry: 3)
		{
		}
	}
}
