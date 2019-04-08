﻿using System;

namespace Relativity.Sync
{
	/// <summary>
	///     Represents Sync job progress
	/// </summary>
	public sealed class SyncJobState
	{
		/// <summary>
		///     ID of the step for which this state is being reported
		/// </summary>
		public string Id { get; }

		/// <summary>
		///     Sync job state
		/// </summary>
		public string Status { get; }

		/// <summary>
		///     Error message from Sync job
		/// </summary>
		public string Message { get; }

		/// <summary>
		///     Exception that occurred during Sync job
		/// </summary>
		public Exception Exception { get; }

		internal SyncJobState(string id, string status, string message, Exception exception)
		{
			Id = id;
			Status = status;
			Message = message;
			Exception = exception;
		}

		/// <summary>
		///     Creates a <see cref="SyncJobState"/> for a failed step.
		/// </summary>
		/// <param name="id">ID of the step</param>
		/// <param name="exception">Exception causing the failure</param>
		public static SyncJobState Failure(string id, Exception exception)
		{
			return new SyncJobState(id, "An error occurred during the execution of this step.", null, exception);
		}

		/// <summary>
		///     Creates a <see cref="SyncJobState"/> for a failed step.
		/// </summary>
		/// <param name="id">ID of the step</param>
		/// <param name="message">Error message describing the failure</param>
		public static SyncJobState Failure(string id, string message)
		{
			return new SyncJobState(id, "An error occurred during the execution of this step.", message, null);
		}

		/// <summary>
		///     Creates a <see cref="SyncJobState"/> for a failed step.
		/// </summary>
		/// <param name="id">ID of the step</param>
		/// <param name="message">Error message describing the failure</param>
		/// <param name="exception">Exception causing the failure</param>
		public static SyncJobState Failure(string id, string message, Exception exception)
		{
			return new SyncJobState(id, "An error occurred during the execution of this step.", message, exception);
		}

		/// <summary>
		///     Creates a <see cref="SyncJobState"/> for a canceled step.
		/// </summary>
		/// <param name="id">ID of the step</param>
		public static SyncJobState Canceled(string id)
		{
			return new SyncJobState(id, "Execution of the Relativity Sync job was canceled.", null, null);
		}

		/// <summary>
		///     Creates a <see cref="SyncJobState"/> for a step that completed with errors.
		/// </summary>
		/// <param name="id">ID of the step</param>
		public static SyncJobState CompletedWithErrors(string id)
		{
			return new SyncJobState(id, "Step completed but errors occurred during execution.", null, null);
		}

		/// <summary>
		///     Creates a <see cref="SyncJobState"/> for a step that completed successfully.
		/// </summary>
		/// <param name="id">ID of the step</param>
		public static SyncJobState Completed(string id)
		{
			return new SyncJobState(id, "Step completed.", null, null);
		}

		/// <summary>
		///     Creates a <see cref="SyncJobState"/> for a step that started executing.
		/// </summary>
		/// <param name="id">ID of the step</param>
		public static SyncJobState Start(string id)
		{
			return new SyncJobState(id, "Executing...", null, null);
		}
	}
}