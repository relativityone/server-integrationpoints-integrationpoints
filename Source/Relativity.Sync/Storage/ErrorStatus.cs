using System.ComponentModel;

namespace Relativity.Sync.Storage
{
	/// <summary>
	/// In RIP this has multiple value. For now, Sync only uses New
	/// </summary>
	internal enum ErrorStatus
	{
		[Description("New")]
		New = 0,
	}
}