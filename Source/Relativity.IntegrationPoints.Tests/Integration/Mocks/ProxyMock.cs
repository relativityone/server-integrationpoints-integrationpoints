using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class ProxyMock
	{
		public ObjectManagerStub ObjectManager { get; }

		public WorkspaceManagerStub WorkspaceManager { get; }


		public ProxyMock()
		{
			ObjectManager = new ObjectManagerStub();
			WorkspaceManager = new WorkspaceManagerStub();
		}

		public void Clear()
		{
			ObjectManager.Mock.Reset();
			WorkspaceManager.Mock.Reset();
		}
	}
}
