namespace Relativity.Sync
{
	/// <summary>
	/// Indicates execution status of the command.
	/// </summary>
	internal enum CommandExecutionStatus
	{
		None,
		Completed,
		Canceled,
		Failed
	}
}