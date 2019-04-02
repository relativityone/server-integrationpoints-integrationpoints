using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class DataDestinationFinalizationConfiguration : IDataDestinationFinalizationConfiguration
	{
		private readonly IConfiguration _cache;

		private static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");

		public DataDestinationFinalizationConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}

		public int DataDestinationArtifactId => _cache.GetFieldValue<int>(DataDestinationArtifactIdGuid);
	}
}