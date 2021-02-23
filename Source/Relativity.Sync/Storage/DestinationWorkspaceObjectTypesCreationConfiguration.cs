using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class DestinationWorkspaceObjectTypesCreationConfiguration : IDestinationWorkspaceObjectTypesCreationConfiguration
	{
		private readonly Storage.IConfiguration _cache;

		public DestinationWorkspaceObjectTypesCreationConfiguration(Storage.IConfiguration cache)
		{
			_cache = cache;
		}

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
	}
}