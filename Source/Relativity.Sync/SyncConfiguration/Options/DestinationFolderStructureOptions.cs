using Relativity.Sync.Configuration;

namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Represents destination folder structure options.
    /// </summary>
    public class DestinationFolderStructureOptions
    {
        /// <summary>
        /// Defines destination folder structure behavior.
        /// </summary>
        public DestinationFolderStructureBehavior DestinationFolderStructure { get; }

        /// <summary>
        /// Gets folder path source field Artifact ID.
        /// </summary>
        public int FolderPathSourceFieldId { get; }

        /// <summary>
        /// Determines whether to move existing documents.
        /// </summary>
        public bool MoveExistingDocuments { get; set; }

        private DestinationFolderStructureOptions(
            DestinationFolderStructureBehavior destinationFolderStructure, int folderPathSourceFieldId = default(int))
        {
            DestinationFolderStructure = destinationFolderStructure;
            FolderPathSourceFieldId = folderPathSourceFieldId;
        }

        /// <summary>
        /// Keep all documents in the same folder.
        /// </summary>
        public static DestinationFolderStructureOptions None() =>
            new DestinationFolderStructureOptions(DestinationFolderStructureBehavior.None);

        /// <summary>
        /// Retains folder structure from source workspace.
        /// </summary>
        public static DestinationFolderStructureOptions RetainFolderStructureFromSourceWorkspace() =>
            new DestinationFolderStructureOptions(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure);

        /// <summary>
        /// Reads structure from specific field.
        /// </summary>
        /// <param name="folderPathSourceFieldId">Folder path source field Artifact ID.</param>
        public static DestinationFolderStructureOptions ReadFromField(int folderPathSourceFieldId) =>
            new DestinationFolderStructureOptions(DestinationFolderStructureBehavior.ReadFromField, folderPathSourceFieldId);
    }
}
