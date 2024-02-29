using System.ComponentModel;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Configuration
{
    /// <summary>
    /// Determines import image file copy mode.
    /// </summary>
    public enum ImportImageFileCopyMode
    {
        /// <summary>
        /// Disable import of images.
        /// </summary>
        [Description("None")]
        DoNotImportImageFiles = NativeFileCopyModeEnum.DoNotImportNativeFiles,

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
