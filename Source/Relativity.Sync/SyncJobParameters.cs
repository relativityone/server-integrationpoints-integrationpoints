using System;
using System.Reflection;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

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
		public string WorkflowId { get; }
		
		/// <summary>
		/// Default constructor
		/// </summary>
		public SyncJobParameters(int syncConfigurationArtifactId, int workspaceId, Guid workflowId)
		{
			SyncConfigurationArtifactId = syncConfigurationArtifactId;
			WorkspaceId = workspaceId;
			SyncBuildVersion = GetVersion();
			WorkflowId = workflowId.ToString();
		}

		private static string GetVersion()
		{
			string ver;

			try
			{
				ver = typeof(SyncJobParameters).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
			}
			catch (Exception)
			{
				ver = "[file version not specified]";
			}

			return ver;
		}

	}
}