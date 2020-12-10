using Relativity.Sync.Configuration;

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class DestinationFolderStructureOptions
	{
		public DestinationFolderStructureBehavior DestinationFolderStructure { get; set; }
		public int FolderPathSourceFieldId { get; set; }
		public bool MoveExistingDocuments { get; set; }

		private DestinationFolderStructureOptions() 
		{ }

		public static DestinationFolderStructureOptions None()
		{
			return new DestinationFolderStructureOptions
			{
				DestinationFolderStructure = DestinationFolderStructureBehavior.None
			};
		}

		public static DestinationFolderStructureOptions RetainFolderStructureFromSourceWorkspace(
			bool moveExistingDocuments = false)
		{
			return new DestinationFolderStructureOptions
			{
				DestinationFolderStructure = DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure,
				MoveExistingDocuments = moveExistingDocuments
			};
		}

		public static DestinationFolderStructureOptions ReadFromField(
			int folderPathSourceFieldId, bool moveExistingDocuments = false)
		{
			return new DestinationFolderStructureOptions
			{
				DestinationFolderStructure = DestinationFolderStructureBehavior.ReadFromField,
				FolderPathSourceFieldId = folderPathSourceFieldId,
				MoveExistingDocuments = moveExistingDocuments
			};
		}
	}
}
