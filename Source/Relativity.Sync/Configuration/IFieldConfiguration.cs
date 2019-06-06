using System.Collections.Generic;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Configuration
{
	internal interface IFieldConfiguration
	{
		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; } 
		string FolderPathSourceFieldName { get; }
		int SourceWorkspaceArtifactId { get; }
		IList<FieldMap> FieldMappings { get; }
	}
}