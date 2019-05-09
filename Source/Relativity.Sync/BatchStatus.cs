using System.ComponentModel;

namespace Relativity.Sync
{
	internal enum BatchStatus
	{
		[Description("New")]
		New = 0,

		[Description("Started")]
		Started,

		[Description("In Progress")]
		InProgress,

		[Description("Completed")]
		Completed,

		[Description("Completed With Errors")]
		CompletedWithErrors,

		[Description("Failed")]
		Failed,

		[Description("Cancelled")]
		Cancelled
	}
}