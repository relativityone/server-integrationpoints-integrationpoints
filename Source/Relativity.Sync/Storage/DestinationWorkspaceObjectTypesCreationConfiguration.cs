using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class DestinationWorkspaceObjectTypesCreationConfiguration : IDestinationWorkspaceObjectTypesCreationConfiguration
	{
		private readonly IConfiguration _cache;

		public DestinationWorkspaceObjectTypesCreationConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
	}
}