using System;

namespace Relativity.Sync
{
	/// <summary>
	///     Represents Sync job parameters
	/// </summary>
	public sealed class SyncJobParameters
	{
		/// <summary>
		///     Job correlation ID
		/// </summary>
		public string CorrelationId { get; }

		/// <summary>
		///     Job ID
		/// </summary>
		public int JobId { get; }

		/// <summary>
		///     ID of a workspace where job was created
		/// </summary>
		public int WorkspaceId { get; }

		/// <summary>
		///     Constructor
		/// </summary>
		public SyncJobParameters(int jobId, int workspaceId) : this(jobId, workspaceId, Guid.NewGuid().ToString())
		{
		}

		/// <summary>
		///     Constructor
		/// </summary>
		public SyncJobParameters(int jobId, int workspaceId, string correlationId)
		{
			CorrelationId = correlationId;
			JobId = jobId;
			WorkspaceId = workspaceId;
		}
	}
}