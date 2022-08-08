using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
    [TestFixture, Category("Unit")]
    class RelativePathDirectoryTreeProviderTests : TestBase
    {
        private const int _WKSP_ID = 1234;
        private readonly Guid _ipType = Guid.NewGuid();

        private const string _ROOT_PATH = @"\\localhost\FileShare";
        private const string _IP_TYPE_PATH = @"DataTransfer\Type";
        private const string _FOLDER_NAME = "Folder";

        private IDirectoryTreeCreator<JsTreeItemDTO> _directoryTreeCreatorMock;
        private IDataTransferLocationService _dataTransferLocationServiceMock;

        private RelativePathDirectoryTreeCreator<JsTreeItemDTO> _subjectUnderTest;

        public override void SetUp()
        {
            _directoryTreeCreatorMock = Substitute.For<IDirectoryTreeCreator<JsTreeItemDTO>>();
            _dataTransferLocationServiceMock = Substitute.For<IDataTransferLocationService>();

            _dataTransferLocationServiceMock.GetWorkspaceFileLocationRootPath(_WKSP_ID).Returns(_ROOT_PATH);
            _dataTransferLocationServiceMock.GetDefaultRelativeLocationFor(_ipType).Returns(_IP_TYPE_PATH);


            _subjectUnderTest = new RelativePathDirectoryTreeCreator<JsTreeItemDTO>(_directoryTreeCreatorMock, _dataTransferLocationServiceMock);
        }

        [Test]
        public void ItShouldReturnTreeStructure()
        {
            string path = Path.Combine(_ROOT_PATH, _IP_TYPE_PATH);

            JsTreeItemDTO root = new JsTreeItemDTO
            {
                Id = path,
                Text = path
            };

            JsTreeItemDTO folder = new JsTreeItemDTO
            {
                Text = _FOLDER_NAME,
                Id = Path.Combine(path, _FOLDER_NAME)
            };

            root.Children = new List<JsTreeItemDTO>
            {
                folder
            };

            _directoryTreeCreatorMock.GetChildren(path, true).Returns(new List<JsTreeItemDTO>
            {
                root
            });

            string expectedFolderPath = Path.Combine(_IP_TYPE_PATH, _FOLDER_NAME);

            List<JsTreeItemDTO> treeItems = _subjectUnderTest.GetChildren(string.Empty, true, _WKSP_ID, _ipType);

            Assert.That(treeItems.Count, Is.EqualTo(1));
            JsTreeItemDTO rootTreeItem = treeItems.First();

            Assert.That(!rootTreeItem.Id.Contains(path) && !rootTreeItem.Text.Contains(path) );
            // We expect Id always point to valid Url Path -> only Text property should be allow to append special characters/variables in the prefix, eg: "%Fileshare Path%", ".\"
            Assert.That(!rootTreeItem.Id.StartsWith(RelativePathDirectoryTreeCreator<JsTreeItemDTO>.FILESHARE_PLACEHOLDER_PREFIX));
            Assert.That(rootTreeItem.Text.StartsWith(RelativePathDirectoryTreeCreator<JsTreeItemDTO>.FILESHARE_PLACEHOLDER_PREFIX));

            Assert.That(rootTreeItem.Children.Count, Is.EqualTo(1));

            JsTreeItemDTO folderTreeItem = rootTreeItem.Children.First();
            Assert.That(folderTreeItem.Id, Is.EqualTo(expectedFolderPath));
            Assert.That(folderTreeItem.Text, Is.EqualTo(_FOLDER_NAME));
        }
    }
}
