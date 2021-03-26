using System.Collections.Generic;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class FakeUser
	{
		public int ArtifactId { get; set; }

		public bool IsAdmin { get; set; }

		public Dictionary<int, bool> Permissions { get; set; } = new Dictionary<int, bool>();

		public List<WorkspaceTest> AssignedWorkspaces { get; set; } = new List<WorkspaceTest>();

		public FakeUser()
		{
			ArtifactId = ArtifactProvider.NextId();
		}
	}
}
