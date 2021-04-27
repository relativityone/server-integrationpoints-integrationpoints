using System.ComponentModel;

namespace Relativity.Sync
{
	/// <summary>
	/// Describes the current status of the batch.
	/// </summary>
	public enum BatchStatus
	{
		/// <summary>
		/// The batch is in a newly created state with no work executed.
		/// </summary>
		[Description("New")]
		New = 0,

		/// <summary>
		/// The batch is started and preparing to be executed.
		/// </summary>
		[Description("Started")]
		Started,

		/// <summary>
		/// The batch is in progress and executing.
		/// </summary>
		[Description("In Progress")]
		InProgress,

		/// <summary>
		/// The batch completed successfully without errors.
		/// </summary>
		[Description("Completed")]
		Completed,

		/// <summary>
		/// The batch completed successfully with errors.
		/// </summary>
		[Description("Completed With Errors")]
		CompletedWithErrors,

		/// <summary>
		/// The batch failed to execute.
		/// </summary>
		[Description("Failed")]
		Failed,

		/// <summary>
		/// The batch was cancelled by an external entity.
		/// </summary>
		[Description("Cancelled")]
		Cancelled,
		
		/// <summary>
		/// Batch was paused due to job drain-stop
		/// </summary>
		[Description("Paused")]
		Paused
	}
}