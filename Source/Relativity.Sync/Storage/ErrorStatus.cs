using System.ComponentModel;

namespace Relativity.Sync.Storage
{
	internal enum ErrorStatus
	{
		[Description("New")]
		New = 0,

		[Description("Expired")]
		Expired = 1,

		[Description("In Progress")]
		InProgress = 2,

		[Description("Retried")]
		Retried = 3
	}
}