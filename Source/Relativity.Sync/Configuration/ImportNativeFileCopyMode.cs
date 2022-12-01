using System.ComponentModel;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Configuration
{
    /// <summary>
    /// Determines import native file copy mode.
    /// </summary>
    public enum ImportNativeFileCopyMode
    {
        /// <summary>
        /// Disable import of natives.
        /// </summary>
        [Description("None")]
        DoNotImportNativeFiles = NativeFileCopyModeEnum.DoNotImportNativeFiles,

        /// <summary>
        /// Copy files.
        /// </summary>
        [Description("Copy")]
        CopyFiles = NativeFileCopyModeEnum.CopyFiles,

        /// <summary>
        /// Links only.
        /// </summary>
        [Description("Link")]
        SetFileLinks = NativeFileCopyModeEnum.SetFileLinks
    }
}
