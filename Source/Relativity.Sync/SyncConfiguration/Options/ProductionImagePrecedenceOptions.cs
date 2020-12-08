using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class ProductionImagePrecedenceOptions
	{
		public IEnumerable<int> ProductionImagePrecedenceIds { get; set; }
		public bool IncludeOriginalImagesIfNotFoundInProductions { get; set; }

		public ProductionImagePrecedenceOptions(IEnumerable<int> productionImagePrecedenceIds, bool includeOriginalImagesIfNotFoundInProductions)
		{
			ProductionImagePrecedenceIds = productionImagePrecedenceIds;
			IncludeOriginalImagesIfNotFoundInProductions = includeOriginalImagesIfNotFoundInProductions;
		}
	}
}
