using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.Tests.System.Core.Helpers;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
	public class ImageGoldFlowImagesSet : ImageGoldFlowTestsBase
	{
		public ImageGoldFlowImagesSet() : base(Dataset.Images, expectedItemsForRetry: 3, expectedDocumentsForRetry: 3)
		{
		}
	}
}
