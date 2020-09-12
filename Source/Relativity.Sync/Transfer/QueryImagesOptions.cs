using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Transfer
{
	internal class QueryImagesOptions
	{
		public List<int> ProductionIds { get; set; }
		public bool IncludeOriginalImageIfNotFoundInProductions { get; set; }

		public bool ProductionImagePrecedence => ProductionIds?.Any() ?? false;
	}
}
