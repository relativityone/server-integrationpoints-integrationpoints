namespace kCura.IntegrationPoints.Domain.Models
{
    /// <summary>
    /// Specifies the type of mapping between data fields in a source and a workspace.
    /// </summary>
    public enum FieldMapTypeEnum
    {
        /// <summary>
        /// No type specified.
        /// </summary>
        None,
        /// <summary>
        /// The unique identifier for a field.
        /// </summary>
        Identifier,
        /// <summary>
        /// The parent of a specific field.
        /// </summary>
        Parent,
        /// <summary>
        /// The path for the native file.
        /// </summary>
        NativeFilePath,
        /// <summary>
        /// The path of the folder to create.
        /// </summary>
        FolderPathInformation
    }
}
