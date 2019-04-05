namespace Relativity.Sync.Configuration
{
	/// <summary>
	/// Specifies folder structure behavior in destination.
	/// </summary>
	internal enum DestinationFolderStructureBehavior
	{
		/// <summary>
		/// Keep all documents in same destination folder
		/// </summary>
		None,

		/// <summary>
		/// Retains folder structure from source workspace
		/// </summary>
		RetainSourceWorkspaceStructure,

		/// <summary>
		/// Reads structure from specific field
		/// </summary>
		ReadFromField
	}
}