using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.System.Core.Runner
{
	public class FullSyncJobConfiguration
	{
		public int JobHistoryId { get; set; }
		public int SavedSearchArtifactId { get; set; }
		public int TargetWorkspaceArtifactId { get; set; }
		public bool CreateSavedSearchForTagging { get; set; }
		public int DestinationFolderArtifactId { get; set; }
		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; set; }
		public string FolderPathSourceFieldName { get; set; }
		public string EmailNotificationRecipients { get; set; }
		public IEnumerable<FieldMap> FieldsMapping { get; set; }
		public FieldOverlayBehavior FieldOverlayBehavior { get; set; }
		public ImportOverwriteMode ImportOverwriteMode { get; set; }
		public bool MoveExistingDocuments { get; set; }
		public ImportNativeFileCopyMode ImportNativeFileCopyMode { get; set; }
	}
}
