﻿using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public abstract class RdoTestBase
	{
		public ArtifactTest Artifact { get; }

		public int ArtifactId => Artifact.ArtifactId;

		public int ParenObjectArtifactId { get; set; }

		public int WorkspaceId { get; set; }

		protected RdoTestBase(string artifactTypeName)
		{
			Artifact = new ArtifactTest
			{
				ArtifactId = ArtifactProvider.NextId(),
				ArtifactType = artifactTypeName
			};
		}

		public abstract RelativityObject ToRelativityObject();
	}
}