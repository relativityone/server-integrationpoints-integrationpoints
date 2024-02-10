namespace Relativity.IntegrationPoints.Services.Interfaces.Private.Models.IntegrationPoint
{
    /// <summary>
    /// Enum representing the modes for importing files in an integration point.
    /// </summary>
    public enum ImportFileCopyModeEnum
    {
        /// <summary>
        /// Do not import native files.
        /// </summary>
        DoNotImportNativeFiles,

        /// <summary>
        /// Set file links.
        /// </summary>
        SetFileLinks,

        /// <summary>
        /// Copy files.
        /// </summary>
        CopyFiles,
    }
}
