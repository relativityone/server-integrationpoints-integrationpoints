using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync
{
	/// <summary>
	/// Represents Sync job parameters
	/// </summary>
	public sealed class SyncJobParameters
	{
		/// <summary>
		/// Job correlation ID
		/// </summary>
		public string CorrelationId { get; }

		/// <summary>
		/// Job ID
		/// </summary>
		public int JobId { get; }

		/// <summary>
		/// ID of a workspace where job was created
		/// </summary>
		public int WorkspaceId { get; }

		/// <summary>
		/// ID of integration point job
		/// </summary>
		public int IntegrationPointArtifactId { get; }

		/// <summary>
		/// Import settings.
		/// </summary>
		public ImportSettingsDto ImportSettings { get; }

		/// <summary>
		/// Constructor for testing only
		/// </summary>
		internal SyncJobParameters(int jobId, int workspaceId, ImportSettingsDto importSettings) : 
			this(jobId, workspaceId, int.MaxValue, Guid.NewGuid().ToString(), importSettings)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public SyncJobParameters(int jobId, int workspaceId, int integrationPointArtifactId, string correlationId, ImportSettingsDto importSettings)
		{
			CorrelationId = correlationId;
			JobId = jobId;
			WorkspaceId = workspaceId;
			IntegrationPointArtifactId = integrationPointArtifactId;
			ImportSettings = importSettings;
		}
	}
}