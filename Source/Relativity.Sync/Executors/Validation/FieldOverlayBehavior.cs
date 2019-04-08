using System.ComponentModel;

namespace Relativity.Sync.Executors.Validation
{
	internal enum FieldOverlayBehavior
	{
		[Description("Use Field Settings")]
		Default = 0,

		[Description("Replace Values")]
		Replace = 1,

		[Description("Merge Values")]
		Merge = 2,
	}
}