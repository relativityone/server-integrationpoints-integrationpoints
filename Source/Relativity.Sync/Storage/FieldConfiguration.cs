using Relativity.Sync.Configuration;
using System.Collections.Generic;

namespace Relativity.Sync.Storage
{
	internal class FieldConfiguration : IFieldConfiguration
	{
		protected readonly IConfiguration _cache;
		private readonly IFieldMappings _fieldMappings;

		public int SourceWorkspaceArtifactId { get; }

		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior =>_cache.GetFieldValue(x => x.DestinationFolderStructureBehavior);
		
		public string GetFolderPathSourceFieldName() => _cache.GetFieldValue(x => x.FolderPathSourceFieldName);

		public IList<FieldMap> GetFieldMappings() => _fieldMappings.GetFieldMappings();

		public FieldConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_fieldMappings = fieldMappings;
			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
		}
		public ImportNativeFileCopyMode ImportNativeFileCopyMode => _cache.GetFieldValue(x => x.NativesBehavior);
	}
}
