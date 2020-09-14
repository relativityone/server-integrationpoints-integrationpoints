using Relativity.Sync.Configuration;
using System;
using System.Collections.Generic;

namespace Relativity.Sync.Storage
{
	internal class FieldConfiguration : IFieldConfiguration
	{
		protected readonly IConfiguration _cache;
		private readonly IFieldMappings _fieldMappings;

		private static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");
		private static readonly Guid FolderPathSourceFieldNameGuid = new Guid("66A37443-EF92-47ED-BEEA-392464C853D3");

		public int SourceWorkspaceArtifactId { get; }

		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior =>
			(DestinationFolderStructureBehavior)Enum.Parse(typeof(DestinationFolderStructureBehavior), _cache.GetFieldValue<string>(DestinationFolderStructureBehaviorGuid));


		public FieldConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_fieldMappings = fieldMappings;
			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
		}

		public string GetFolderPathSourceFieldName() => _cache.GetFieldValue<string>(FolderPathSourceFieldNameGuid);

		public IList<FieldMap> GetFieldMappings() => _fieldMappings.GetFieldMappings();
	}
}
