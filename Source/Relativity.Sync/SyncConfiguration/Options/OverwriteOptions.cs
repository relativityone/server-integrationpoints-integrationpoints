using Relativity.Sync.Configuration;

namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Represents overwrite options.
    /// </summary>
    public class OverwriteOptions
    {
        /// <summary>
        /// Determines the overwrite mode.
        /// </summary>
        public ImportOverwriteMode OverwriteMode { get; }

        /// <summary>
        /// Determines the fields overlay behavior.
        /// </summary>
        public FieldOverlayBehavior FieldsOverlayBehavior { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="OverwriteOptions"/> class.
        /// </summary>
        /// <param name="overwriteMode">Import overwrite mode.</param>
        public OverwriteOptions(ImportOverwriteMode overwriteMode)
        {
            OverwriteMode = overwriteMode;
        }
    }
}
