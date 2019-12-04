using System;
using System.Reflection;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync
{
	/// <summary>
	/// Represents Sync job parameters
	/// </summary>
	public sealed class SyncJobParameters
	{
		/// <summary>
		/// Job ID
		/// </summary>
		public int SyncConfigurationArtifactId { get; }

		/// <summary>
		/// ID of a workspace where job was created
		/// </summary>
		public int WorkspaceId { get; }

		/// <summary>
		/// ID of integration point job
		/// </summary>
		public int IntegrationPointArtifactId { get; }

		/// <summary>
		/// Build version of Sync
		/// </summary>
		public string SyncBuildVersion { get; }

		/// <summary>
		/// Name of the Sync application
		/// </summary>
		public string SyncApplicationName { get; set; } = "Relativity.Sync";

		/// <summary>
		/// Workflow ID.
		/// </summary>
		public Lazy<string> WorkflowId { get; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public SyncJobParameters(int syncConfigurationArtifactId, int workspaceId, int integrationPointArtifactId, int jobHistoryArtifactId)
		{
			SyncConfigurationArtifactId = syncConfigurationArtifactId;
			WorkspaceId = workspaceId;
			IntegrationPointArtifactId = integrationPointArtifactId;
			SyncBuildVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			WorkflowId = new Lazy<string>(() => $"{TelemetryConstants.PROVIDER_NAME}_{integrationPointArtifactId}_{jobHistoryArtifactId}");
		}
	}
}