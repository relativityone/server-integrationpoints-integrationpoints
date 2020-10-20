using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.Tests.System.Core.Helpers;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
	public class ImageGoldFlowMultipleImagesSet : ImageGoldFlowTestsBase
	{
		public ImageGoldFlowMultipleImagesSet() : base(Dataset.MultipleImagesPerDocument, expectedItemsForRetry: 2, expectedDocumentsForRetry: 1)
		{
	
		}
	}
}
