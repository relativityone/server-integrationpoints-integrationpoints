using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    internal class WorkspaceFinderControllerTests : TestBase
    {
        #region Fields

        private WorkspaceFinderController _subjectUnderTest;

        private IManagerFactory _managerFactoryMock;
        private ITextSanitizer _htmlSanitizerManagerMock;
        private IWorkspaceManager _workspaceManagerMock;
        private IServicesMgr _servicesMgrMock;

        private const int _CURRENT_WORKSPACE_ARTIFACT_ID = 10;
        private const int _DESTINATION_WORKSPACE_1_ARTIFACT_ID = 1;
        private const int _DESTINATION_WORKSPACE_2_ARTIFACT_ID = 2;
        private const string _DESTINATION_WORKSPACE_1_NAME = "A vs B";
        private const string _DESTINATION_WORKSPACE_2_NAME = "C vs D";
        private const string _DESTINATION_WORKSPACE_1_DISPLAY_NAME = "A vs B - 1";
        private const string _DESTINATION_WORKSPACE_2_DISPLAY_NAME = "C vs D - 2";

        private const int _FEDERATED_INSTANCE_ID = 100;
        private const int _REMOTE_WORKSPACE_1_ARTIFACT_ID = 11;
        private const int _REMOTE_WORKSPACE_2_ARTIFACT_ID = 12;
        private const string _REMOTE_WORKSPACE_1_NAME = "E vs F";
        private const string _REMOTE_WORKSPACE_2_NAME = "G vs H";
        private const string _REMOTE_WORKSPACE_1_DISPLAY_NAME = "E vs F - 11";
        private const string _REMOTE_WORKSPACE_2_DISPLAY_NAME = "G vs H - 12";

        private const string _CREDENTIALS = "password";
        private readonly WorkspaceModel[] _localWorkspaces = new WorkspaceModel[]
        {
            new WorkspaceModel() {Value = _DESTINATION_WORKSPACE_1_ARTIFACT_ID, DisplayName = _DESTINATION_WORKSPACE_1_DISPLAY_NAME },
            new WorkspaceModel() {Value = _DESTINATION_WORKSPACE_2_ARTIFACT_ID, DisplayName = _DESTINATION_WORKSPACE_2_DISPLAY_NAME }
        };

        private readonly IList<WorkspaceDTO> _localWorkspacesDTOs = new List<WorkspaceDTO>
        {
            new WorkspaceDTO() { ArtifactId = _DESTINATION_WORKSPACE_1_ARTIFACT_ID, Name = _DESTINATION_WORKSPACE_1_NAME },
            new WorkspaceDTO() { ArtifactId = _DESTINATION_WORKSPACE_2_ARTIFACT_ID, Name = _DESTINATION_WORKSPACE_2_NAME }
        };

        private readonly WorkspaceModel[] _remoteWorkspaces = new WorkspaceModel[]
        {
            new WorkspaceModel() {Value = _REMOTE_WORKSPACE_1_ARTIFACT_ID, DisplayName = _REMOTE_WORKSPACE_1_DISPLAY_NAME },
            new WorkspaceModel() {Value = _REMOTE_WORKSPACE_2_ARTIFACT_ID, DisplayName = _REMOTE_WORKSPACE_2_DISPLAY_NAME }
        };

        private readonly IList<WorkspaceDTO> _remoteWorkspacesDTOs = new List<WorkspaceDTO>
        {
            new WorkspaceDTO() { ArtifactId = _REMOTE_WORKSPACE_1_ARTIFACT_ID, Name = _REMOTE_WORKSPACE_1_NAME },
            new WorkspaceDTO() { ArtifactId = _REMOTE_WORKSPACE_2_ARTIFACT_ID, Name = _REMOTE_WORKSPACE_2_NAME }
        };

        #endregion // Fields

        [SetUp]
        public override void SetUp()
        {
            _managerFactoryMock = Substitute.For<IManagerFactory>();
            _htmlSanitizerManagerMock = Substitute.For<ITextSanitizer>();
            _workspaceManagerMock = Substitute.For<IWorkspaceManager>();
            _servicesMgrMock = Substitute.For<IServicesMgr>();
            IWorkspaceContext workspaceIdProviderMock = CreateWorkspaceIdProviderMock();

            _subjectUnderTest = new WorkspaceFinderController(
                _managerFactoryMock,
                _htmlSanitizerManagerMock,
                workspaceIdProviderMock)
            {
                Request = new HttpRequestMessage()
            };

            _subjectUnderTest.Request.SetConfiguration(new HttpConfiguration());

            _managerFactoryMock.CreateWorkspaceManager().Returns(_workspaceManagerMock);

            _htmlSanitizerManagerMock
                .Sanitize(Arg.Any<string>())
                .Returns(x => new SanitizationResult(sanitizedText: x.Arg<string>(), hasErrors: false));
        }

        [Test]
        public void ItShouldGetCurrentInstanceWorkspaces()
        {
            // Arrange
            _workspaceManagerMock.GetUserAvailableDestinationWorkspaces(_CURRENT_WORKSPACE_ARTIFACT_ID).Returns(_localWorkspacesDTOs);

            // Act
            HttpResponseMessage httpResponseMessage = _subjectUnderTest.GetCurrentInstanceWorkspaces();

            //// Assert
            WorkspaceModel[] retValue;
            httpResponseMessage.TryGetContentValue(out retValue);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.AreEqual(_localWorkspaces.Length, retValue.Length);
            Assert.AreEqual(_localWorkspaces[0].DisplayName, retValue[0].DisplayName);
            Assert.AreEqual(_localWorkspaces[0].Value, retValue[0].Value);
            Assert.AreEqual(_localWorkspaces[1].DisplayName, retValue[1].DisplayName);
            Assert.AreEqual(_localWorkspaces[1].Value, retValue[1].Value);
        }

        [Test]
        public void ItShouldGetFederatedInstanceWorkspaces()
        {
            // Arrange
            _workspaceManagerMock.GetUserActiveWorkspaces().Returns(_remoteWorkspacesDTOs);

            // Act
            HttpResponseMessage httpResponseMessage =
                _subjectUnderTest.GetFederatedInstanceWorkspaces(_FEDERATED_INSTANCE_ID, _CREDENTIALS);

            //// Assert
            WorkspaceModel[] retValue;
            httpResponseMessage.TryGetContentValue(out retValue);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.AreEqual(_remoteWorkspaces.Length, retValue.Length);
            Assert.AreEqual(_remoteWorkspaces[0].DisplayName, retValue[0].DisplayName);
            Assert.AreEqual(_remoteWorkspaces[0].Value, retValue[0].Value);
            Assert.AreEqual(_remoteWorkspaces[1].DisplayName, retValue[1].DisplayName);
            Assert.AreEqual(_remoteWorkspaces[1].Value, retValue[1].Value);
        }

        private static IWorkspaceContext CreateWorkspaceIdProviderMock()
        {
            IWorkspaceContext workspaceIdProviderMock = Substitute.For<IWorkspaceContext>();
            workspaceIdProviderMock.GetWorkspaceID().Returns(_CURRENT_WORKSPACE_ARTIFACT_ID);
            return workspaceIdProviderMock;
        }
    }
}
