using System;
using System.Collections.Generic;
using System.IO;
using SystemInterface.IO;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using kCura.IntegrationPoints.Core.Helpers;
using NSubstitute.ExceptionExtensions;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;


namespace kCura.IntegrationPoints.Core.Tests.Services
{
    [TestFixture, Category("Unit")]
    public class DataTransferLocationServiceTests : TestBase
    {
        private DataTransferLocationService _sut;

        private IHelper _helperMock;
        private IServicesMgr _servicesMgr;
        private IWorkspaceManager _workspaceManager;
        private IIntegrationPointTypeService _integrationPointTypeServiceMock;
        private IDirectory _directoryMock;
        private ICryptographyHelper _cryptographyHelperMock;
        private IAPILog _loggerMock;

        private const int _WKSP_ID = 1234;
        private const string _RESOURCE_POOL_FILESHARE = @"\\localhost\Fileshare";
        private const string _EXPORT_PROV_TYPE_NAME = "Exp";
        private const string _IMPORT_PROV_TYPE_NAME = "Imp";
        private const string _PARENT_FOLDER = "DataTransfer";

        private const string _WKSP_FOLDER = "EDDS1234";

        public override void SetUp()
        {
            _helperMock = Substitute.For<IHelper>();
            _servicesMgr = Substitute.For<IServicesMgr>();
            _helperMock.GetServicesManager().Returns(_servicesMgr);
            _workspaceManager = Substitute.For<IWorkspaceManager>();
            _workspaceManager.GetDefaultWorkspaceFileShareResourceServerAsync(Arg.Is<WorkspaceRef>(r => r.ArtifactID == _WKSP_ID))
                .Returns(new FileShareResourceServer
            {
                UNCPath = _RESOURCE_POOL_FILESHARE
            });
            _servicesMgr.CreateProxy<IWorkspaceManager>(Arg.Any<ExecutionIdentity>()).Returns(_workspaceManager);
            _integrationPointTypeServiceMock = Substitute.For<IIntegrationPointTypeService>();
            _directoryMock = Substitute.For<IDirectory>();
            _cryptographyHelperMock = Substitute.For<ICryptographyHelper>();
            _loggerMock = Substitute.For<IAPILog>();

            ILogFactory logFactoryMock = Substitute.For<ILogFactory>();

            logFactoryMock.GetLogger().Returns(_loggerMock);

            _integrationPointTypeServiceMock.GetAllIntegrationPointTypes().Returns(
                new List<IntegrationPointType>
                {
                    new IntegrationPointType()
                    {
                        Name = _EXPORT_PROV_TYPE_NAME
                    },
                    new IntegrationPointType()
                    {
                        Name = _IMPORT_PROV_TYPE_NAME
                    }
                });
            _sut = new DataTransferLocationService(_helperMock, _integrationPointTypeServiceMock, _directoryMock, _cryptographyHelperMock);
        }

        [Test]
        public void ItShouldCreateForAllTypes()
        {
            _sut.CreateForAllTypes(_WKSP_ID);

            // Assert
            _directoryMock.Received(1).CreateDirectory(Path.Combine(_RESOURCE_POOL_FILESHARE, _WKSP_FOLDER, _PARENT_FOLDER, _IMPORT_PROV_TYPE_NAME));
            _directoryMock.Received(1).CreateDirectory(Path.Combine(_RESOURCE_POOL_FILESHARE, _WKSP_FOLDER, _PARENT_FOLDER, _EXPORT_PROV_TYPE_NAME));
        }

        [Test]
        public void ItShouldGetDefaultRelativeLocationFor()
        {
            Guid type = Guid.NewGuid();

            _integrationPointTypeServiceMock.GetIntegrationPointType(type).Returns(
                new IntegrationPointType
                {
                    Name = _EXPORT_PROV_TYPE_NAME
                });

            var defaultRelativePath = _sut.GetDefaultRelativeLocationFor(type);
            // Assert
            Assert.That(defaultRelativePath, Is.EqualTo(Path.Combine(_PARENT_FOLDER, _EXPORT_PROV_TYPE_NAME)));
        }

        [Test]
        public void ItShouldVerifyCorrectPathAndPrepareFolder()
        {
            Guid type = Guid.NewGuid();

            _integrationPointTypeServiceMock.GetIntegrationPointType(type).Returns(
                new IntegrationPointType
                {
                    Name = _EXPORT_PROV_TYPE_NAME
                });

            string folderName = "Folder";
            string path = Path.Combine(_PARENT_FOLDER, _EXPORT_PROV_TYPE_NAME, folderName);
            string physicalPath = Path.Combine(_RESOURCE_POOL_FILESHARE, _WKSP_FOLDER, path);

            
            _directoryMock.Exists(physicalPath).Returns(false);

            var returnedPath = _sut.VerifyAndPrepare(_WKSP_ID, path, type);
            // Assert
            _directoryMock.Received(1).CreateDirectory(physicalPath);
            Assert.That(returnedPath, Is.EqualTo(physicalPath));
        }

        [Test]
        public void ItShouldVerifyIncorrectPath()
        {
            Guid type = Guid.NewGuid();

            _integrationPointTypeServiceMock.GetIntegrationPointType(type).Returns(
                new IntegrationPointType
                {
                    Name = _EXPORT_PROV_TYPE_NAME
                });

            string folderName = "Folder";

            // We try to pass DataTransfer\Folder path which is not correct (it should be DataTransfer\Exp\Folder)
            string path = Path.Combine(_PARENT_FOLDER, folderName);

            string physicalPath = Path.Combine(_RESOURCE_POOL_FILESHARE, _WKSP_FOLDER, path);

            _directoryMock.Exists(physicalPath).Returns(false);

            Assert.Throws<ArgumentException>(() => _sut.VerifyAndPrepare(_WKSP_ID, path, type));

            // Assert
            _directoryMock.Received(0).CreateDirectory(physicalPath);
        }

        [Test]
        public void ItShouldThrowIfPathIsNotChildOfDataTransferLocation()
        {
            Guid type = Guid.NewGuid();

            _integrationPointTypeServiceMock.GetIntegrationPointType(type).Returns(
                new IntegrationPointType
                {
                    Name = _EXPORT_PROV_TYPE_NAME
                });


            // Pass path that is not child of DataTransfer Location 
            string path = $"{_PARENT_FOLDER}\\{_EXPORT_PROV_TYPE_NAME}\\..\\..";

            string physicalPath = Path.Combine(_RESOURCE_POOL_FILESHARE, _WKSP_FOLDER, path);

            _directoryMock.Exists(physicalPath).Returns(false);

            Assert.Throws<ArgumentException>(() => _sut.VerifyAndPrepare(_WKSP_ID, path, type));

            // Assert
            _directoryMock.Received(0).CreateDirectory(physicalPath);
        }

        [Test]
        public void ItShouldThrowNotFoundExceptionIfWorkspaceDoesntExist()
        {
            // Arrange
            _workspaceManager.GetDefaultWorkspaceFileShareResourceServerAsync(Arg.Is<WorkspaceRef>(r => r.ArtifactID == _WKSP_ID)).Throws(new NotFoundException());

            // Act & Assert
            Assert.Throws<NotFoundException>(() => _sut.GetWorkspaceFileLocationRootPath(_WKSP_ID));
        }
    }
}
