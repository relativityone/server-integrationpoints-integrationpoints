using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class ProxyMock
	{
		public ObjectManagerStub ObjectManager { get; }

		public WorkspaceManagerStub WorkspaceManager { get; }

		public ProductionManagerStub ProductionManager { get; }

		public PermissionManagerStub PermissionManager { get; }

		public InstanceSettingManagerStub InstanceSettingManager { get; set; }

		public GroupManagerStub GroupManager { get; set; }
		
		public ProxyMock(TestContext context)
		{
			ObjectManager = new ObjectManagerStub();
			WorkspaceManager = new WorkspaceManagerStub();
			ProductionManager = new ProductionManagerStub();
			PermissionManager = new PermissionManagerStub(context.User);
			InstanceSettingManager = new InstanceSettingManagerStub(context);
			GroupManager = new GroupManagerStub(context.User);

			SetupFixedMocks();
		}

		private void SetupFixedMocks()
		{
			PermissionManager.SetupPermissionsCheck();
			InstanceSettingManager.SetupInstanceSetting();
			GroupManager.SetupGroupManager();
		}

		public void Clear()
		{
			ObjectManager.Mock.Reset();
			WorkspaceManager.Mock.Reset();
			ProductionManager.Mock.Reset();
		}
	}
}
