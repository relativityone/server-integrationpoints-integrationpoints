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

		public ChoiceQueryManagerStub ChoiceQueryManager { get; set; }

		public APMManagerStub APMManager { get; set; }

		public MetricsManagerStub MetricsManager { get; set; }

		public ProxyMock(TestContext context)
		{
			ObjectManager = new ObjectManagerStub();
			WorkspaceManager = new WorkspaceManagerStub();
			PermissionManager = new PermissionManagerStub();
			InstanceSettingManager = new InstanceSettingManagerStub(context);
			GroupManager = new GroupManagerStub(context.User);
			ArtifactGuidManager = new ArtifactGuidManagerStub();
			ErrorManager = new ErrorManagerStub();
			ChoiceQueryManager = new ChoiceQueryManagerStub();
			APMManager = new APMManagerStub();
			MetricsManager = new MetricsManagerStub();
		}

		public void Setup(RelativityInstanceTest relativity)
		{
			ObjectManager.Setup(relativity);
			WorkspaceManager.Setup(relativity);
			ArtifactGuidManager.Setup(relativity);
			ErrorManager.Setup(relativity);
			ChoiceQueryManager.Setup(relativity);
			APMManager.Setup(relativity);
			MetricsManager.Setup(relativity);

			SetupFixedMocks();
		}

		private void SetupFixedMocks()
		{
			WorkspaceManager.SetupWorkspaceMock();
			InstanceSettingManager.SetupInstanceSetting();
			GroupManager.SetupGroupManager();
			ArtifactGuidManager.SetupArtifactGuidManager();
			ErrorManager.SetupErrorManager();
			ChoiceQueryManager.SetupArtifactGuidManager();
			ObjectManager.Setup();
			APMManager.SetupAPMManagerStub();
			MetricsManager.SetupMetricsManagerStub();
		}
	}
}
