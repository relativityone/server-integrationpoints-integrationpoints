using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public abstract class RdoTestBase
	{
		public int ArtifactId { get; set; }

		public int WorkspaceId { get; set; }

		protected RdoTestBase()
		{
			ArtifactId = Artifact.NextId();
		}

		public abstract RelativityObject ToRelativityObject();
	}
}