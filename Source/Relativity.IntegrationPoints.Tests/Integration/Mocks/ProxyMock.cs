using Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class ProxyMock
	{
		public ObjectManagerStub ObjectManager { get; }

		public WorkspaceManagerStub WorkspaceManager { get; }

		public PermissionManagerStub PermissionManager { get; }

		public InstanceSettingManagerStub InstanceSettingManager { get; set; }

		public GroupManagerStub GroupManager { get; set; }
		
		public ProxyMock(TestContext context)
		{
			ObjectManager = new ObjectManagerStub();
			WorkspaceManager = new WorkspaceManagerStub();
			PermissionManager = new PermissionManagerStub();
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
	}
}
