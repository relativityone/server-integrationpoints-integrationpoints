using Relativity.Sync.Configuration;

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