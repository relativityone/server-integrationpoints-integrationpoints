using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Configuration
{
    /// <summary>
    /// Import overwrite mode.
    /// </summary>
    public enum ImportOverwriteMode
    {
        /// <summary>
        /// Append only.
        /// </summary>
        AppendOnly = OverwriteModeEnum.Append,

        /// <summary>
        /// Overlay only,
        /// </summary>
        OverlayOnly = OverwriteModeEnum.Overlay,

        /// <summary>
        /// Append/overlay.
        /// </summary>
        AppendOverlay = OverwriteModeEnum.AppendOverlay,
    }
}
