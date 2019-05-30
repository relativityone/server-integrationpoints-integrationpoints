using System;
using Relativity.Sync.Configuration;

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
		/// Import settings.
		/// </summary>
		public ImportSettingsDto ImportSettings { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public SyncJobParameters(int jobId, int workspaceId) : this(jobId, workspaceId, new ImportSettingsDto())
		{

		}

		/// <summary>
		///     Constructor
		/// </summary>
		public SyncJobParameters(int jobId, int workspaceId, ImportSettingsDto importSettings) : this(jobId, workspaceId, Guid.NewGuid().ToString(), importSettings)
		{
		}

		/// <summary>
		///     Constructor
		/// </summary>
		public SyncJobParameters(int jobId, int workspaceId, string correlationId) : this(jobId, workspaceId, correlationId, new ImportSettingsDto())
		{
		}

		/// <summary>
		///     Constructor
		/// </summary>
		public SyncJobParameters(int jobId, int workspaceId, string correlationId, ImportSettingsDto importSettings)
		{
			CorrelationId = correlationId;
			JobId = jobId;
			WorkspaceId = workspaceId;
			ImportSettings = importSettings;
		}
	}
}