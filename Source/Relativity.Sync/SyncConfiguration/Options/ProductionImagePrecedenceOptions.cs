using System.Collections.Generic;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class ProductionImagePrecedenceOptions
	{
		public IEnumerable<int> ProductionImagePrecedenceIds { get; }

		public bool IncludeOriginalImagesIfNotFoundInProductions { get; }

		public ProductionImagePrecedenceOptions(IEnumerable<int> productionImagePrecedenceIds, bool includeOriginalImagesIfNotFoundInProductions)
		{
			ProductionImagePrecedenceIds = productionImagePrecedenceIds;
			IncludeOriginalImagesIfNotFoundInProductions = includeOriginalImagesIfNotFoundInProductions;
		}
	}
}
