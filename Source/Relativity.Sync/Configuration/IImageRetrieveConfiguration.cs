using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	internal interface IImageRetrieveConfiguration
	{
		List<int> ProductionIds { get; }
		bool IncludeOriginalImageIfNotFoundInProductions { get; }
	}
}
