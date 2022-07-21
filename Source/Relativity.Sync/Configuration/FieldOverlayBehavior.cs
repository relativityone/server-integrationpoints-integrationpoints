using System.ComponentModel;
using kCura.EDDS.WebAPI.BulkImportManagerBase;

namespace Relativity.Sync.Configuration
{
    /// <summary>
    /// Defines behavior for field overlay.
    /// </summary>
    public enum FieldOverlayBehavior
    {
        /// <summary>
        /// Reads settings from the field.
        /// </summary>
        [Description("Use Field Settings")]
        UseFieldSettings = OverlayBehavior.UseRelativityDefaults,

        /// <summary>
        /// Merge values.
        /// </summary>
        [Description("Merge Values")]
        MergeValues = OverlayBehavior.MergeAll,

        /// <summary>
        /// Replace values.
        /// </summary>
        [Description("Replace Values")]
        ReplaceValues = OverlayBehavior.ReplaceAll
    }
}