﻿using Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class ProxyMock
    {
        public ProxyMock(TestContext context)
        {
            ObjectManager = new ObjectManagerStub();
            ObjectTypeManager = new ObjectTypeManagerStub();
            WorkspaceManager = new WorkspaceManagerStub();
            PermissionManager = new PermissionManagerStub();
            InstanceSettingManager = new InstanceSettingManagerStub(context);
            GroupManager = new GroupManagerStub(context.User);
            ArtifactGuidManager = new ArtifactGuidManagerStub();
            ErrorManager = new ErrorManagerStub();
            ChoiceQueryManager = new ChoiceQueryManagerStub();
            APMManager = new APMManagerStub();
            MetricsManager = new MetricsManagerStub();
            KeywordSearchManager = new KeywordSearchManagerStub();
            ViewManager = new ViewManagerStub();
            FileShareServerManager = new FileShareServerManagerStub();
            PingService = new PingServiceStub();
        }

        public ObjectManagerStub ObjectManager { get; }

        public ObjectTypeManagerStub ObjectTypeManager { get; }

        public WorkspaceManagerStub WorkspaceManager { get; }

        public PermissionManagerStub PermissionManager { get; }

        public InstanceSettingManagerStub InstanceSettingManager { get; set; }

        public GroupManagerStub GroupManager { get; set; }

        public ArtifactGuidManagerStub ArtifactGuidManager { get; set; }

        public ErrorManagerStub ErrorManager { get; set; }

        public ChoiceQueryManagerStub ChoiceQueryManager { get; set; }

        public APMManagerStub APMManager { get; set; }

        public MetricsManagerStub MetricsManager { get; set; }

        public KeywordSearchManagerStub KeywordSearchManager { get; set; }

        public ViewManagerStub ViewManager { get; set; }

        public FileShareServerManagerStub FileShareServerManager { get; set; }

        public PingServiceStub PingService { get; set; }

        public void Setup(RelativityInstanceTest relativity)
        {
            ObjectManager.Setup(relativity);
            ObjectTypeManager.Setup(relativity);
            WorkspaceManager.Setup(relativity);
            ArtifactGuidManager.Setup(relativity);
            ErrorManager.Setup(relativity);
            ChoiceQueryManager.Setup(relativity);
            APMManager.Setup(relativity);
            MetricsManager.Setup(relativity);
            KeywordSearchManager.Setup(relativity);
            ViewManager.Setup(relativity);
            FileShareServerManager.Setup(relativity);
            PingService.Setup(relativity);

            SetupFixedMocks();
        }

        private void SetupFixedMocks()
        {
            WorkspaceManager.SetupWorkspaceMock();
            ObjectTypeManager.SetupObjectTypeManager();
            InstanceSettingManager.SetupInstanceSetting();
            GroupManager.SetupGroupManager();
            ArtifactGuidManager.SetupArtifactGuidManager();
            ErrorManager.SetupErrorManager();
            ChoiceQueryManager.SetupArtifactGuidManager();
            ObjectManager.Setup();
            APMManager.SetupAPMManagerStub();
            MetricsManager.SetupMetricsManagerStub();
            KeywordSearchManager.SetupKeywordSearchManagerStub();
            ViewManager.SetupViewManagerStub();
            FileShareServerManager.SetupFileShareServerManagerStub();
            PingService.SetupPingService();
        }
    }
}
