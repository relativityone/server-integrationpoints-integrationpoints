using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class DataDestinationFinalizationConfiguration : IDataDestinationFinalizationConfiguration
	{
		private readonly IConfiguration _cache;

		public DataDestinationFinalizationConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}

		public int DataDestinationArtifactId => _cache.GetFieldValue(x => x.DataDestinationArtifactId);
	}
}