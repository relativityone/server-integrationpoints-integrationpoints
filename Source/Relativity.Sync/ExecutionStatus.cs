namespace Relativity.Sync
{
	/// <summary>
	///     Indicates execution status of the command.
	/// </summary>
	internal enum ExecutionStatus
	{
		None,
		Completed,
		CompletedWithErrors,
		Canceled,
		Failed
	}
}