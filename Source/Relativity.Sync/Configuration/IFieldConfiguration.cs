﻿using System.Collections.Generic;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Configuration
{
	internal interface IFieldConfiguration
	{
		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; } 
		int FolderPathSourceFieldArtifactId { get; }
		int SourceWorkspaceArtifactId { get; }
	}
}