using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class PreValidationConfiguration : IPreValidationConfiguration
	{
		private readonly IConfiguration _cache;

		public int DestinationWorkspaceArtifactId =>
			_cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);

		public PreValidationConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}
	}
}
