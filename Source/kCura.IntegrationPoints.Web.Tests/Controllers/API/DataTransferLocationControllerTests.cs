
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class DataTransferLocationControllerTests : TestBase
    {
        private DataTransferLocationController _instance;
        private IRepositoryFactory _respositoryFactory;
        private IPermissionRepository _permissionRepository;
        private IRelativePathDirectoryTreeCreator<JsTreeItemDTO> _relativePathDirectoryTreeCreator;
        private IDataTransferLocationService _dataTransferLocationService;
        private const int _WORKSPACE_ID = 345679;
        private static readonly Guid _integrationPointTypeIdentifier = Guid.NewGuid();
        private const string _PATH = "path";
        private const bool _IS_ROOT = true;
        private const bool _INCLUDE_FILES = true;

        [SetUp]
        public override void SetUp()
        {
            _permissionRepository = Substitute.For<IPermissionRepository>();
            _respositoryFactory = Substitute.For<IRepositoryFactory>();
            _relativePathDirectoryTreeCreator = Substitute.For<IRelativePathDirectoryTreeCreator<JsTreeItemDTO>>();
            _dataTransferLocationService = Substitute.For<IDataTransferLocationService>();

            _instance = new DataTransferLocationController(_respositoryFactory, _relativePathDirectoryTreeCreator, _dataTransferLocationService)
            {
                Request = new HttpRequestMessage()
            };
            _instance.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ItShouldGetStructure(int numOfTreeElements)
        {
            // Arrange
            List<JsTreeItemDTO> expectedTree = BuildDefaultJsTreeItemDtoList(numOfTreeElements);
            _relativePathDirectoryTreeCreator.GetChildren(_PATH, _IS_ROOT, _WORKSPACE_ID, _integrationPointTypeIdentifier, _INCLUDE_FILES)
                .Returns(expectedTree);
            MockHasPermissions(true);

            // Act
            HttpResponseMessage response = _instance.GetStructure(_WORKSPACE_ID, _integrationPointTypeIdentifier, _PATH, _IS_ROOT, _INCLUDE_FILES);
            List<JsTreeItemDTO> actualResult = ExtractJsTreeItemDtoListFromResponse(response);

            // Assert
            Assert.NotNull(actualResult);
            Assert.AreEqual(expectedTree.Count, actualResult.Count);
            CollectionAssert.AreEqual(expectedTree, actualResult);
        }

        [Test]
        public void ItShouldHaveNoPermissionToGetStructure()
        {
            // Arrange
            MockHasPermissions(false);

            // Act
            HttpResponseMessage response = _instance.GetStructure(_WORKSPACE_ID, _integrationPointTypeIdentifier, _PATH, _IS_ROOT, _INCLUDE_FILES);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void ItShouldThrowWhenGetStructure()
        {
            // Arrange
            _respositoryFactory.GetPermissionRepository(_WORKSPACE_ID).Throws(new Exception());

            // Act
            var exception = Assert.Throws<Exception>(() => _instance.GetStructure(_WORKSPACE_ID, _integrationPointTypeIdentifier, _PATH, _IS_ROOT, _INCLUDE_FILES));

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(Constants.PERMISSION_CHECKING_UNEXPECTED_ERROR, exception.Message);
        }

        [Test]
        public void ItShouldGetRoot()
        {
            // Arrange
            const string expectedRelativeDataTransferLocation = "expectedRelativeDataTransferLocation";
            _dataTransferLocationService.GetDefaultRelativeLocationFor(_integrationPointTypeIdentifier)
                .Returns(expectedRelativeDataTransferLocation);
            MockHasPermissions(true);

            // Act
            HttpResponseMessage response = _instance.GetRoot(_WORKSPACE_ID, _integrationPointTypeIdentifier);
            string actualResult = ExtractRelativeDataTransferLocation(response);

            // Assert
            Assert.AreEqual(expectedRelativeDataTransferLocation, actualResult);
        }

        [Test]
        public void ItShouldHaveNoPermissionToGetRoot()
        {
            // Arrange
            MockHasPermissions(false);

            // Act
            HttpResponseMessage response = _instance.GetRoot(_WORKSPACE_ID, _integrationPointTypeIdentifier);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void ItShouldThrowWhenGetRoot()
        {
            // Arrange
            _respositoryFactory.GetPermissionRepository(_WORKSPACE_ID).Throws(new Exception());

            // Act
            var exception = Assert.Throws<Exception>(() => _instance.GetRoot(_WORKSPACE_ID, _integrationPointTypeIdentifier));

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(Constants.PERMISSION_CHECKING_UNEXPECTED_ERROR, exception.Message);
        }

        #region "Helpers"

        private void MockHasPermissions(bool hasPermissions)
        {
            // This method will manage permissions
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(hasPermissions);

            // Another methods always will return true
            _permissionRepository.UserCanExport().Returns(true);
            _permissionRepository.UserCanImport().Returns(true);
            var integrationPointGuid = new Guid(ObjectTypeGuids.IntegrationPoint);
            _permissionRepository.UserHasArtifactTypePermission(integrationPointGuid, ArtifactPermission.Edit).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(integrationPointGuid, ArtifactPermission.Create).Returns(true);

            _respositoryFactory.GetPermissionRepository(_WORKSPACE_ID).Returns(_permissionRepository);
        }

        private string ExtractRelativeDataTransferLocation(HttpResponseMessage response)
        {
            var objectContent = response.Content as ObjectContent;
            var result = (string)objectContent?.Value;
            return result;
        }

        private List<JsTreeItemDTO> ExtractJsTreeItemDtoListFromResponse(HttpResponseMessage response)
        {
            var objectContent = response.Content as ObjectContent;
            var result = (List<JsTreeItemDTO>)objectContent?.Value;
            return result;
        }

        private static List<JsTreeItemDTO> BuildDefaultJsTreeItemDtoList(int numOfElements)
        {
            var tree = new List<JsTreeItemDTO>();

            for (var i = 0; i < numOfElements; i++)
            {
                tree.Add(new JsTreeItemDTO());
            }

            return tree;
        }

        #endregion
    }
}
