using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Utils;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class SourceWorkspaceManagerTests : TestBase
    {
        private const int _SOURCE_WORKSPACE_ID = 874817;
        private const int _DESTINATION_WORKSPACE_ID = 282573;

        private ISourceWorkspaceRepository _sourceWorkspaceRepository;
        private IWorkspaceRepository _workspaceRepository;
        private IInstanceSettingRepository _instanceSettingRepository;

        private SourceWorkspaceManager _instance;

        public override void SetUp()
        {
            _sourceWorkspaceRepository = Substitute.For<ISourceWorkspaceRepository>();
            _workspaceRepository = Substitute.For<IWorkspaceRepository>();
            _instanceSettingRepository = Substitute.For<IInstanceSettingRepository>();

            IHelper helper = Substitute.For<IHelper>();
            IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();

            repositoryFactory.GetSourceWorkspaceRepository().Returns(_workspaceRepository);
            repositoryFactory.GetSourceWorkspaceRepository(_DESTINATION_WORKSPACE_ID).Returns(_sourceWorkspaceRepository);
            repositoryFactory.GetInstanceSettingRepository().Returns(_instanceSettingRepository);

            _instance = new SourceWorkspaceManager(repositoryFactory, helper);
        }

        [Test]
        public void ItShouldCreateInstanceInThisInstance()
        {
            int expectedSourceWorkspaceArtifactId = 196923;

            int? federatedInstanceArtifactId = null;

            string workspaceName = "workspace_name_696";

            _workspaceRepository.Retrieve(_SOURCE_WORKSPACE_ID)
                .Returns(new WorkspaceDTO
                {
                    Name = workspaceName
                });

            _sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ID, FederatedInstanceManager.LocalInstance.Name, federatedInstanceArtifactId)
                .Returns((SourceWorkspaceDTO)null);

            _sourceWorkspaceRepository.Create(Arg.Any<SourceWorkspaceDTO>()).Returns(expectedSourceWorkspaceArtifactId);

            // ACT
            SourceWorkspaceDTO sourceWorkspaceDTO = _instance.CreateSourceWorkspaceDto(_DESTINATION_WORKSPACE_ID, _SOURCE_WORKSPACE_ID, federatedInstanceArtifactId);

            // ASSERT
            ValidateSourceWorkspace(sourceWorkspaceDTO, expectedSourceWorkspaceArtifactId, workspaceName, FederatedInstanceManager.LocalInstance.Name);

            _workspaceRepository.Received(1).Retrieve(_SOURCE_WORKSPACE_ID);
            _sourceWorkspaceRepository.Received(1).RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ID, FederatedInstanceManager.LocalInstance.Name, federatedInstanceArtifactId);
            _sourceWorkspaceRepository.Received(1).Create(Arg.Any<SourceWorkspaceDTO>());

            _instanceSettingRepository.DidNotReceive().GetConfigurationValue(Arg.Any<string>(), Arg.Any<string>());
            _sourceWorkspaceRepository.DidNotReceive().Update(Arg.Any<SourceWorkspaceDTO>());
        }

        [Test]
        public void ItShouldCreateInstanceInFederatedInstance()
        {
            int expectedSourceWorkspaceArtifactId = 142679;

            int? federatedInstanceArtifactId = 190817;
            string currentInstanceName = "instance_name";

            string workspaceName = "workspace_name_890";

            _workspaceRepository.Retrieve(_SOURCE_WORKSPACE_ID)
                .Returns(new WorkspaceDTO
                {
                    Name = workspaceName
                });

            _instanceSettingRepository.GetConfigurationValue("Relativity.Authentication", "FriendlyInstanceName").Returns(currentInstanceName);

            _sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ID, currentInstanceName, federatedInstanceArtifactId)
                .Returns((SourceWorkspaceDTO)null);

            _sourceWorkspaceRepository.Create(Arg.Any<SourceWorkspaceDTO>()).Returns(expectedSourceWorkspaceArtifactId);

            // ACT
            SourceWorkspaceDTO sourceWorkspaceDTO = _instance.CreateSourceWorkspaceDto(_DESTINATION_WORKSPACE_ID, _SOURCE_WORKSPACE_ID, federatedInstanceArtifactId);

            // ASSERT
            ValidateSourceWorkspace(sourceWorkspaceDTO, expectedSourceWorkspaceArtifactId, workspaceName, currentInstanceName);

            _workspaceRepository.Received(1).Retrieve(_SOURCE_WORKSPACE_ID);
            _instanceSettingRepository.Received(1).GetConfigurationValue("Relativity.Authentication", "FriendlyInstanceName");
            _sourceWorkspaceRepository.Received(1).RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ID, currentInstanceName, federatedInstanceArtifactId);
            _sourceWorkspaceRepository.Received(1).Create(Arg.Any<SourceWorkspaceDTO>());

            _sourceWorkspaceRepository.DidNotReceive().Update(Arg.Any<SourceWorkspaceDTO>());
        }

        [Test]
        public void ItShouldUpdateExistingInstance()
        {
            int expectedSourceWorkspaceArtifactId = 444307;

            int? federatedInstanceArtifactId = null;

            string workspaceName = "workspace_name_486";

            _workspaceRepository.Retrieve(_SOURCE_WORKSPACE_ID)
                .Returns(new WorkspaceDTO
                {
                    Name = workspaceName
                });

            _sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ID, FederatedInstanceManager.LocalInstance.Name, federatedInstanceArtifactId)
                .Returns(new SourceWorkspaceDTO
                {
                    ArtifactId = expectedSourceWorkspaceArtifactId,
                    SourceCaseArtifactId = _SOURCE_WORKSPACE_ID,
                    SourceCaseName = "old_case_name",
                    SourceInstanceName = "old_instance_name"
                });

            // ACT
            SourceWorkspaceDTO sourceWorkspaceDTO = _instance.CreateSourceWorkspaceDto(_DESTINATION_WORKSPACE_ID, _SOURCE_WORKSPACE_ID, federatedInstanceArtifactId);

            // ASSERT
            ValidateSourceWorkspace(sourceWorkspaceDTO, expectedSourceWorkspaceArtifactId, workspaceName, FederatedInstanceManager.LocalInstance.Name);

            _workspaceRepository.Received(1).Retrieve(_SOURCE_WORKSPACE_ID);
            _sourceWorkspaceRepository.Received(1).RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ID, FederatedInstanceManager.LocalInstance.Name, federatedInstanceArtifactId);
            _sourceWorkspaceRepository.Received(1).Update(Arg.Any<SourceWorkspaceDTO>());

            _instanceSettingRepository.DidNotReceive().GetConfigurationValue(Arg.Any<string>(), Arg.Any<string>());
            _sourceWorkspaceRepository.DidNotReceive().Create(Arg.Any<SourceWorkspaceDTO>());
        }

        [Test]
        public void ItShouldSkipCreatingForUpToDateInstance()
        {
            int expectedSourceWorkspaceArtifactId = 444307;

            int? federatedInstanceArtifactId = null;
            int sourceWorkspaceDescriptorArtifactTypeId = 153282;

            string workspaceName = "workspace_name_486";

            _workspaceRepository.Retrieve(_SOURCE_WORKSPACE_ID)
                .Returns(new WorkspaceDTO
                {
                    Name = workspaceName
                });

            _sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ID, FederatedInstanceManager.LocalInstance.Name, federatedInstanceArtifactId)
                .Returns(new SourceWorkspaceDTO
                {
                    ArtifactId = expectedSourceWorkspaceArtifactId,
                    SourceCaseArtifactId = _SOURCE_WORKSPACE_ID,
                    SourceCaseName = workspaceName,
                    SourceInstanceName = FederatedInstanceManager.LocalInstance.Name,
                    Name = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(FederatedInstanceManager.LocalInstance.Name, workspaceName, _SOURCE_WORKSPACE_ID),
                    ArtifactTypeId = sourceWorkspaceDescriptorArtifactTypeId
                });

            // ACT
            SourceWorkspaceDTO sourceWorkspaceDTO = _instance.CreateSourceWorkspaceDto(_DESTINATION_WORKSPACE_ID, _SOURCE_WORKSPACE_ID, federatedInstanceArtifactId);

            // ASSERT
            ValidateSourceWorkspace(sourceWorkspaceDTO, expectedSourceWorkspaceArtifactId, workspaceName, FederatedInstanceManager.LocalInstance.Name);

            _workspaceRepository.Received(1).Retrieve(_SOURCE_WORKSPACE_ID);
            _sourceWorkspaceRepository.Received(1).RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ID, FederatedInstanceManager.LocalInstance.Name, federatedInstanceArtifactId);

            _sourceWorkspaceRepository.DidNotReceive().Update(Arg.Any<SourceWorkspaceDTO>());
            _instanceSettingRepository.DidNotReceive().GetConfigurationValue(Arg.Any<string>(), Arg.Any<string>());
            _sourceWorkspaceRepository.DidNotReceive().Create(Arg.Any<SourceWorkspaceDTO>());
        }

        [Test]
        public void ItShouldShortenSourceWorkspaceName()
        {
            string federatedInstanceName = new string('x', 300);
            int? federatedInstanceArtifactId = 682622;
            _instanceSettingRepository.GetConfigurationValue("Relativity.Authentication", "FriendlyInstanceName").Returns(federatedInstanceName);

            string workspaceName = "workspace_name_486";
            _workspaceRepository.Retrieve(_SOURCE_WORKSPACE_ID)
                .Returns(new WorkspaceDTO
                {
                    Name = workspaceName
                });

            _sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(_SOURCE_WORKSPACE_ID, federatedInstanceName, federatedInstanceArtifactId)
                .Returns((SourceWorkspaceDTO)null);

            var suffix = $" - {workspaceName} - {_SOURCE_WORKSPACE_ID}";
            string expectedName = federatedInstanceName.Substring(0, 255 - suffix.Length) + suffix;

            // ACT
            SourceWorkspaceDTO sourceWorkspaceDTO = _instance.CreateSourceWorkspaceDto(_DESTINATION_WORKSPACE_ID, _SOURCE_WORKSPACE_ID, federatedInstanceArtifactId);

            // ASSERT
            Assert.That(sourceWorkspaceDTO.Name.Length, Is.EqualTo(Data.Constants.DEFAULT_NAME_FIELD_LENGTH));
            Assert.That(sourceWorkspaceDTO.Name, Is.EqualTo(expectedName));
        }

        private void ValidateSourceWorkspace(SourceWorkspaceDTO sourceWorkspaceDTO, int artifactId, string workspaceName, string instanceName)
        {
            Assert.IsNotNull(sourceWorkspaceDTO);
            string expectedName = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(instanceName, workspaceName, _SOURCE_WORKSPACE_ID);
            Assert.AreEqual(expectedName, sourceWorkspaceDTO.Name);
            Assert.AreEqual(artifactId, sourceWorkspaceDTO.ArtifactId);
            Assert.AreEqual(_SOURCE_WORKSPACE_ID, sourceWorkspaceDTO.SourceCaseArtifactId);
            Assert.AreEqual(workspaceName, sourceWorkspaceDTO.SourceCaseName);
            Assert.AreEqual(instanceName, sourceWorkspaceDTO.SourceInstanceName);
        }
    }
}