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
		/// Build version of Sync
		/// </summary>
		public string SyncBuildVersion { get; }
		
		/// <summary>
		/// Executing application
		/// </summary>
		public string ExecutingApplication { get; }
		
		/// <summary>
		/// Executing application version
		/// </summary>
		public string ExecutingApplicationVersion { get; }

		/// <summary>
		/// Name of the Sync application
		/// </summary>
		public string SyncApplicationName { get; set; } = "Relativity.Sync";

		/// <summary>
		/// Id for trigger input
		/// </summary>
		public string TriggerId { get; set; } = "type";

		/// <summary>
		/// Value for RAW trigger input
		/// </summary>
		public string TriggerValue { get; set; } = "sync";

		/// <summary>
		/// Name for RAW trigger
		/// </summary>
		public string TriggerName { get; set; } = "relativity@on-new-documents-added";

		/// <summary>
		/// Workflow ID.
		/// </summary>
		public Lazy<string> WorkflowId { get; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public SyncJobParameters(int syncConfigurationArtifactId, int workspaceId, int jobHistoryArtifactId, string executingApplication, Version executingApplicationVersion)
		{
			SyncConfigurationArtifactId = syncConfigurationArtifactId;
			WorkspaceId = workspaceId;
			ExecutingApplication = executingApplication;
			ExecutingApplicationVersion = executingApplicationVersion.ToString();
			SyncBuildVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			WorkflowId = new Lazy<string>(() => $"{TelemetryConstants.PROVIDER_NAME}_{jobHistoryArtifactId}");
		}

		/// <summary>
		/// Internal constructor to simplify testing
		/// </summary>
		internal SyncJobParameters(int syncConfigurationArtifactId, int workspaceId, int jobHistoryArtifactId) :
			this(syncConfigurationArtifactId, workspaceId, jobHistoryArtifactId, "SyncTests", new Version(1, 2, 3))
		{
		}
	}
}