using Relativity.Services.Interfaces.Workspace;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public class WorkspaceManagerStub : KeplerStubBase<IWorkspaceManager>
	{
		public WorkspaceManagerStub(InMemoryDatabase database) : base(database)
		{
		}
	}
}
