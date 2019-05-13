using System.ComponentModel;
using kCura.EDDS.WebAPI.BulkImportManagerBase;

namespace Relativity.Sync.Configuration
{
	internal enum FieldOverlayBehavior
	{
		[Description("Use Field Settings")]
		UseFieldSettings = OverlayBehavior.UseRelativityDefaults,

		[Description("Merge Values")]
		MergeValues = OverlayBehavior.MergeAll,

		[Description("Replace Values")]
		ReplaceValues = OverlayBehavior.ReplaceAll
	}
}