using Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class ProxyMock
	{
		public ObjectManagerStub ObjectManager { get; }

		public WorkspaceManagerStub WorkspaceManager { get; }

		public PermissionManagerStub PermissionManager { get; }

		public InstanceSettingManagerStub InstanceSettingManager { get; set; }

		public GroupManagerStub GroupManager { get; set; }

		public ArtifactGuidManagerStub ArtifactGuidManager { get; set; }

		public ErrorManagerStub ErrorManager { get; set; }
		
		public ProxyMock(TestContext context)
		{
			ObjectManager = new ObjectManagerStub();
			WorkspaceManager = new WorkspaceManagerStub();
			PermissionManager = new PermissionManagerStub();
			InstanceSettingManager = new InstanceSettingManagerStub(context);
			GroupManager = new GroupManagerStub(context.User);
			ArtifactGuidManager = new ArtifactGuidManagerStub();
			ErrorManager = new ErrorManagerStub();
		}

		public void Setup(RelativityInstanceTest relativity)
		{
			ObjectManager.Setup(relativity);
			WorkspaceManager.Setup(relativity);
			ArtifactGuidManager.Setup(relativity);
			ErrorManager.Setup(relativity);

			SetupFixedMocks();
		}

		private void SetupFixedMocks()
		{
			PermissionManager.SetupPermissionsCheck();
			WorkspaceManager.SetupWorkspaceMock();
			InstanceSettingManager.SetupInstanceSetting();
			GroupManager.SetupGroupManager();
			ArtifactGuidManager.SetupArtifactGuidManager();
			ErrorManager.SetupErrorManager();
			ObjectManager.Setup();
		}
	}
}
