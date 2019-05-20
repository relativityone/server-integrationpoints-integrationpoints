using System;

namespace Relativity.Sync
{
	/// <summary>
	/// Represents Sync job progress
	/// </summary>
	public sealed class SyncJobState
	{
		/// <summary>
		/// ID of the step for which this state is being reported
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Sync job state
		/// </summary>
		public SyncJobStatus Status { get; }

		/// <summary>
		/// Error message from Sync job
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Exception that occurred during Sync job
		/// </summary>
		public Exception Exception { get; }

		internal SyncJobState(string id, SyncJobStatus status, string message, Exception exception)
		{
			Id = id;
			Status = status;
			Message = message;
			Exception = exception;
		}

		/// <summary>
		/// Creates a <see cref="SyncJobState"/> for a failed step.
		/// </summary>
		/// <param name="id">ID of the step</param>
		/// <param name="exception">Exception causing the failure</param>
		public static SyncJobState Failure(string id, Exception exception)
		{
			return new SyncJobState(id, SyncJobStatus.Failed, "An error occurred during the execution of this step.", exception);
		}

		/// <summary>
		/// Creates a <see cref="SyncJobState"/> for a failed step.
		/// </summary>
		/// <param name="id">ID of the step</param>
		/// <param name="message">Error message describing the failure</param>
		public static SyncJobState Failure(string id, string message)
		{
			return new SyncJobState(id, SyncJobStatus.Failed, message, null);
		}

		/// <summary>
		/// Creates a <see cref="SyncJobState"/> for a failed step.
		/// </summary>
		/// <param name="id">ID of the step</param>
		/// <param name="message">Error message describing the failure</param>
		/// <param name="exception">Exception causing the failure</param>
		public static SyncJobState Failure(string id, string message, Exception exception)
		{
			return new SyncJobState(id, SyncJobStatus.Failed, message, exception);
		}

		/// <summary>
		/// Creates a <see cref="SyncJobState"/> for a canceled step.
		/// </summary>
		/// <param name="id">ID of the step</param>
		public static SyncJobState Canceled(string id)
		{
			return new SyncJobState(id, SyncJobStatus.Cancelled, "Execution of the Relativity Sync job was canceled.", null);
		}

		/// <summary>
		/// Creates a <see cref="SyncJobState"/> for a step that completed with errors.
		/// </summary>
		/// <param name="id">ID of the step</param>
		public static SyncJobState CompletedWithErrors(string id)
		{
			return new SyncJobState(id, SyncJobStatus.CompletedWithErrors, "Step completed but errors occurred during execution.", null);
		}

		/// <summary>
		/// Creates a <see cref="SyncJobState"/> for a step that completed successfully.
		/// </summary>
		/// <param name="id">ID of the step</param>
		public static SyncJobState Completed(string id)
		{
			return new SyncJobState(id, SyncJobStatus.Completed, "Step completed.", null);
		}

		/// <summary>
		/// Creates a <see cref="SyncJobState"/> for a step that started executing.
		/// </summary>
		/// <param name="id">ID of the step</param>
		public static SyncJobState Start(string id)
		{
			return new SyncJobState(id, SyncJobStatus.Started, "Executing...", null);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			if (Exception != null)
			{
				return $"{Id} | {Status} | {Message} | {Exception.GetType().FullName}({Exception.Message})";
			}

			return $"{Id} | {Status} | {Message}";
		}
	}
}