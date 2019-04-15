using System.ComponentModel;

namespace Relativity.Sync.Executors.Validation
{
	internal enum FieldOverlayBehavior
	{
		[Description("Use Field Settings")]
		UseFieldSettings = 0,

		[Description("Replace Values")]
		ReplaceValues = 1,

		[Description("Merge Values")]
		MergeValues = 2,
	}
}