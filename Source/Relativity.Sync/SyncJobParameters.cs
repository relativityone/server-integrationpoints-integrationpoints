﻿using System;
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
		/// Constructor for testing only
		/// </summary>
		internal SyncJobParameters(int syncConfigurationArtifactId, int workspaceId) : 
			this(syncConfigurationArtifactId, workspaceId, int.MaxValue, Guid.NewGuid().ToString())
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public SyncJobParameters(int syncConfigurationArtifactId, int workspaceId, int integrationPointArtifactId, string correlationId)
		{
			CorrelationId = correlationId;
			SyncConfigurationArtifactId = syncConfigurationArtifactId;
			WorkspaceId = workspaceId;
			IntegrationPointArtifactId = integrationPointArtifactId;
		}
	}
}