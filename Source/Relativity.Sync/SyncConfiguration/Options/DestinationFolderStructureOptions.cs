using Relativity.Sync.Configuration;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class DestinationFolderStructureOptions
	{
		public DestinationFolderStructureBehavior DestinationFolderStructure { get; }

		public int FolderPathSourceFieldId { get; }

		public bool MoveExistingDocuments { get; set; }

		private DestinationFolderStructureOptions(
			DestinationFolderStructureBehavior destinationFolderStructure, int folderPathSourceFieldId = default(int))
		{
			DestinationFolderStructure = destinationFolderStructure;
			FolderPathSourceFieldId = folderPathSourceFieldId;
		}

		public static DestinationFolderStructureOptions None() =>
			new DestinationFolderStructureOptions(DestinationFolderStructureBehavior.None);

		public static DestinationFolderStructureOptions RetainFolderStructureFromSourceWorkspace() =>
			new DestinationFolderStructureOptions(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure);

		public static DestinationFolderStructureOptions ReadFromField(int folderPathSourceFieldId) =>
			new DestinationFolderStructureOptions(DestinationFolderStructureBehavior.ReadFromField, folderPathSourceFieldId);
	}
}
