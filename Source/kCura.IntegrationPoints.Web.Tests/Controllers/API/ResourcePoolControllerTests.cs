using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Toggles;
using NSubstitute;
using NUnit.Framework;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class ResourcePoolControllerTests : TestBase
    {
        #region Fields

        private ResourcePoolController _subjectUnderTest;

        private IResourcePoolManager _resourcePoolManagerMock;
        private IRepositoryFactory _repositoryFactoryMock;
        private IPermissionRepository _permissionRepositoryMock;
        private IResourcePoolContext _resourcePoolContextMock;
        private IDirectoryTreeCreator<JsTreeItemDTO> _directoryTreeCreatorMock;
        private IToggleProvider _toggleProviderMock;

        private const int _WORKSPACE_ID = 1;
        private const int _PROC_SOURCE_LOC_ID = 2;

        private readonly ProcessingSourceLocationDTO _processingSourceLocation = new ProcessingSourceLocationDTO
        {
            ArtifactId = _PROC_SOURCE_LOC_ID,
            Location = @"\\localhost\Export"
        };

        #endregion //Fields

        [SetUp]
        public override void SetUp()
        {
            _resourcePoolManagerMock = Substitute.For<IResourcePoolManager>();
            _directoryTreeCreatorMock = Substitute.For<IDirectoryTreeCreator<JsTreeItemDTO>>();
            _repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
            _permissionRepositoryMock = Substitute.For<IPermissionRepository>();
            _resourcePoolContextMock = Substitute.For<IResourcePoolContext>();
            _toggleProviderMock = Substitute.For<IToggleProvider>();
            _repositoryFactoryMock.GetPermissionRepository(_WORKSPACE_ID).Returns(_permissionRepositoryMock);

            _subjectUnderTest = new ResourcePoolController(_resourcePoolManagerMock, _repositoryFactoryMock, _directoryTreeCreatorMock, _resourcePoolContextMock);


            _subjectUnderTest.Request = new HttpRequestMessage();
            _subjectUnderTest.Request.SetConfiguration(new HttpConfiguration());
        }

        [Test]
        public void ItShouldGetProcessingSourceLocations()
        {
            // Arrange
            SetUserPermissions();
            var procSourceLocations = new List<ProcessingSourceLocationDTO> { _processingSourceLocation };

            _resourcePoolManagerMock.GetProcessingSourceLocation(_WORKSPACE_ID).Returns(procSourceLocations);

            // Act
            HttpResponseMessage httpResponseMessage = _subjectUnderTest.GetProcessingSourceLocations(_WORKSPACE_ID);

            // Assert
            List<ProcessingSourceLocationDTO> retValue;
            httpResponseMessage.TryGetContentValue(out retValue);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(retValue.Count, Is.EqualTo(1));
            Assert.That(retValue[0], Is.EqualTo(_processingSourceLocation));
        }

        [Test]
        public void ItShouldGetSubItems()
        {
            // Arrange
            SetUserPermissions();
            JsTreeItemDTO directoryJsTreeItem = new JsTreeItemDTO()
            {
                Id = _processingSourceLocation.Location,
                Text = _processingSourceLocation.Location
            };

            _directoryTreeCreatorMock.GetChildren(_processingSourceLocation.Location, true).Returns(new List<JsTreeItemDTO> { directoryJsTreeItem});

            // Act
            HttpResponseMessage httpResponseMessage = _subjectUnderTest.GetSubItems(_WORKSPACE_ID, true, _processingSourceLocation.Location);

            // Assert
            List<JsTreeItemDTO> retValue;
            httpResponseMessage.TryGetContentValue(out retValue);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(retValue.Count, Is.EqualTo(1));
            Assert.That(retValue[0], Is.EqualTo(directoryJsTreeItem));
        }

        [Test]
        [TestCase(true, true, true, true, true, true)]
        [TestCase(true, false, true, true, true, true)]
        [TestCase(false, true, true, true, true, true)]
        [TestCase(false, false, true, true, true, false)]
        [TestCase(true, true, false, true, true, false)]
        [TestCase(true, true, true, false, true, true)]
        [TestCase(true, true, true, true, false, true)]
        [TestCase(true, true, true, false, false, false)]
        public void ItShouldCheckPermissions(bool hasExportPerm, bool hasImportPerm, bool hasWkspAccessPerm, bool hasEditPerm, bool hasCreatePerm, bool hasPermission)
        {
            //Arrange
            _permissionRepositoryMock.UserCanExport().Returns(hasExportPerm);
            _permissionRepositoryMock.UserCanImport().Returns(hasImportPerm);
            _permissionRepositoryMock.UserHasPermissionToAccessWorkspace().Returns(hasWkspAccessPerm);
            _permissionRepositoryMock.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create).Returns(hasCreatePerm);
            _permissionRepositoryMock.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Edit).Returns(hasPermission);

            //Act
            HttpResponseMessage httpResponseMessageGetSubItems = _subjectUnderTest.GetSubItems(_WORKSPACE_ID, true, _processingSourceLocation.Location);
            HttpResponseMessage httpResponseMessageProcSourceLoc = _subjectUnderTest.GetProcessingSourceLocations(_WORKSPACE_ID);

            //Assert
            _repositoryFactoryMock.Received(2).GetPermissionRepository(_WORKSPACE_ID);

            Assert.That(httpResponseMessageGetSubItems.StatusCode, Is.EqualTo(hasPermission ? HttpStatusCode.OK : HttpStatusCode.Unauthorized));
            Assert.That(httpResponseMessageProcSourceLoc.StatusCode, Is.EqualTo(hasPermission ? HttpStatusCode.OK : HttpStatusCode.Unauthorized));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public void ItShouldCheckIfIsProcessingSourceLocationEnabled(bool hasPermission, bool isEnabled)
        {
            //Arrange
            _permissionRepositoryMock.UserCanExport().Returns(true);
            _permissionRepositoryMock.UserCanImport().Returns(true);
            _permissionRepositoryMock.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepositoryMock.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create).Returns(hasPermission);
            _permissionRepositoryMock.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Edit).Returns(hasPermission);

            _resourcePoolContextMock.IsProcessingSourceLocationEnabled().Returns(isEnabled);

            //Act
            HttpResponseMessage response = _subjectUnderTest.IsProcessingSourceLocationEnabled(_WORKSPACE_ID);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(hasPermission ? HttpStatusCode.OK : HttpStatusCode.Unauthorized));

            bool retValue;
            response.TryGetContentValue(out retValue);

            Assert.That( retValue, Is.EqualTo(hasPermission && isEnabled) );
        }

        private void SetUserPermissions()
        {
            _permissionRepositoryMock.UserCanExport().Returns(true);
            _permissionRepositoryMock.UserCanImport().Returns(true);
            _permissionRepositoryMock.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepositoryMock.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create).Returns(true);
            _permissionRepositoryMock.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Edit).Returns(true);
    }
    }
}
