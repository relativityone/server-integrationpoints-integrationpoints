using System.Collections.Generic;

namespace Relativity.Sync.SyncConfiguration.Options
{
	/// <summary>
	/// Represents production image precedence options.
	/// </summary>
	public class ProductionImagePrecedenceOptions
	{
		/// <summary>
		/// Get the collection of production image precedence Artifact IDs.
		/// </summary>
		public IEnumerable<int> ProductionImagePrecedenceIds { get; }

		/// <summary>
		/// Specifies whether to include original images if not found in production.
		/// </summary>
		public bool IncludeOriginalImagesIfNotFoundInProductions { get; }

		/// <summary>
		/// Creates new instance of <see cref="ProductionImagePrecedenceOptions"/> class.
		/// </summary>
		/// <param name="productionImagePrecedenceIds">Collection of production image precedence Artifact IDs.</param>
		/// <param name="includeOriginalImagesIfNotFoundInProductions">Value indicating whether to include original images if not found in production.</param>
		public ProductionImagePrecedenceOptions(IEnumerable<int> productionImagePrecedenceIds, bool includeOriginalImagesIfNotFoundInProductions)
		{
			ProductionImagePrecedenceIds = productionImagePrecedenceIds;
			IncludeOriginalImagesIfNotFoundInProductions = includeOriginalImagesIfNotFoundInProductions;
		}
	}
}
