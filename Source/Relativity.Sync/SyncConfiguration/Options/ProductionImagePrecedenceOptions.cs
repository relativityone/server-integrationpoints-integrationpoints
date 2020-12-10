using System.Collections.Generic;

namespace Relativity.Sync.SyncConfiguration.Options
{
	/// <summary>
	/// 
	/// </summary>
	public class ProductionImagePrecedenceOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public IEnumerable<int> ProductionImagePrecedenceIds { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public bool IncludeOriginalImagesIfNotFoundInProductions { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="productionImagePrecedenceIds"></param>
		/// <param name="includeOriginalImagesIfNotFoundInProductions"></param>
		public ProductionImagePrecedenceOptions(IEnumerable<int> productionImagePrecedenceIds, bool includeOriginalImagesIfNotFoundInProductions)
		{
			ProductionImagePrecedenceIds = productionImagePrecedenceIds;
			IncludeOriginalImagesIfNotFoundInProductions = includeOriginalImagesIfNotFoundInProductions;
		}
	}
}
