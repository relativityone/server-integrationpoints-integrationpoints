using Relativity.Sync.Configuration;

namespace Relativity.Sync.SyncConfiguration.Options
{
	/// <summary>
	/// 
	/// </summary>
	public class DestinationFolderStructureOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public DestinationFolderStructureBehavior DestinationFolderStructure { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int FolderPathSourceFieldId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public bool MoveExistingDocuments { get; set; }

		private DestinationFolderStructureOptions() 
		{ }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static DestinationFolderStructureOptions None()
		{
			return new DestinationFolderStructureOptions
			{
				DestinationFolderStructure = DestinationFolderStructureBehavior.None
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="moveExistingDocuments"></param>
		/// <returns></returns>
		public static DestinationFolderStructureOptions RetainFolderStructureFromSourceWorkspace(
			bool moveExistingDocuments = false)
		{
			return new DestinationFolderStructureOptions
			{
				DestinationFolderStructure = DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure,
				MoveExistingDocuments = moveExistingDocuments
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="folderPathSourceFieldId"></param>
		/// <param name="moveExistingDocuments"></param>
		/// <returns></returns>
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
