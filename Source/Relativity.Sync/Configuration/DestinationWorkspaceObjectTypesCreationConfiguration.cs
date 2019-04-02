using System;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Configuration
{
	internal sealed class DestinationWorkspaceObjectTypesCreationConfiguration : IDestinationWorkspaceObjectTypesCreationConfiguration
	{
		private readonly Storage.IConfiguration _cache;

		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");

		public DestinationWorkspaceObjectTypesCreationConfiguration(Storage.IConfiguration cache)
		{
			_cache = cache;
		}

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid);
	}
}