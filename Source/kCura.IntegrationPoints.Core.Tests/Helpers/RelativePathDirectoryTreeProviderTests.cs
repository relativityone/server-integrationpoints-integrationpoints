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
	class RelativePathDirectoryTreeProviderTests : TestBase
	{
		private const int _WKSP_ID = 1234;
		private readonly Guid _ipType = Guid.NewGuid();

		private const string _ROOT_PATH = @"\\localhost\FileShare";
		private const string _IP_TYPE_PATH = @"DataTransfer\Type";
		private const string _FOLDER_NAME = "Folder";

		private IDirectoryTreeCreator<JsTreeItemDTO> _directoryTreeCreatorMock;
		private IDataTransferLocationService _dataTransferLocationServiceMock;

		private RelativePathDirectoryTreeProvider<JsTreeItemDTO> _subjectUnderTest;

		public override void SetUp()
		{
			_directoryTreeCreatorMock = Substitute.For<IDirectoryTreeCreator<JsTreeItemDTO>>();
			_dataTransferLocationServiceMock = Substitute.For<IDataTransferLocationService>();

			_dataTransferLocationServiceMock.GetRootLocationFor(_WKSP_ID).Returns(_ROOT_PATH);
			_dataTransferLocationServiceMock.GetDefaultRelativeLocationFor(_ipType).Returns(_IP_TYPE_PATH);


			_subjectUnderTest = new RelativePathDirectoryTreeProvider<JsTreeItemDTO>(_directoryTreeCreatorMock, _dataTransferLocationServiceMock);
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

			_directoryTreeCreatorMock.GetChildren(path, true).Returns(new List<JsTreeItemDTO>
			{
				root,
				folder
			});

			string expectedFolderPath = Path.Combine(_IP_TYPE_PATH, _FOLDER_NAME);

			List<JsTreeItemDTO> treeItems = _subjectUnderTest.GetChildren(string.Empty, true, _WKSP_ID, _ipType);

			Assert.That(treeItems.Count, Is.EqualTo(2));
			Assert.That(!treeItems.Any(item => item.Id.Contains(path) || item.Text.Contains(path) ));

			Assert.That(treeItems.Any(item => item.Id.Contains(_IP_TYPE_PATH)));
			Assert.That(treeItems.Any(item => item.Id.Contains(expectedFolderPath)));
		}
	}
}
