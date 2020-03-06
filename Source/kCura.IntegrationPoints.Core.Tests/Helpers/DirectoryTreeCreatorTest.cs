﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemInterface.IO;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using kCura.IntegrationPoints.Core.Helpers;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
	[TestFixture, Category("Unit")]
	public class DirectoryTreeCreatorTest : TestBase
	{
		private DirectoryTreeCreator<JsTreeItemDTO> _subjectUnderTest;
		private IDirectory _directoryMock;

		private const string _ROOT_FOLDER = @"\\localhost\Root";
		private const string _SUB_FOLDER_A = @"A";
		private const string _SUB_FOLDER_B = @"B";

		private const string _SUB_FOLDER_1 = @"1";
		private const string _SUB_FOLDER_2 = @"2";

        private const string _SUB_FILE_1 = @"FILE1.txt";

        private readonly string _subFolderAPath = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_A);
		private readonly string _subFolderA1Path = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_A, _SUB_FOLDER_1);
		private readonly string _subFolderA2Path = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_A, _SUB_FOLDER_2);

		private readonly string _subFolderBPath = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_B);
		private readonly string _subFolderB1Path = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_B, _SUB_FOLDER_1);

		[SetUp]
		public override void SetUp()
		{
			_directoryMock = Substitute.For<IDirectory>();

			_subjectUnderTest = new DirectoryTreeCreator<JsTreeItemDTO>(_directoryMock, Substitute.For<IHelper>(), Substitute.For<ICryptographyHelper>());
		}

		[Test]
		public void ItShouldReturnRootSubDirectories()
		{
			//Arrange
			MockDirHierarchy();

			//Act
			List<JsTreeItemDTO> rootItem = _subjectUnderTest.GetChildren(_ROOT_FOLDER, true);

			//Assert
			Assert.That(rootItem.Count, Is.EqualTo(1));
			var subItems = rootItem[0].Children;
			Assert.That(subItems.Count, Is.EqualTo(2));
			
			var subFolderA = subItems[0];
			Assert.That(subFolderA, Is.Not.Null);
			Assert.That(subFolderA.Text, Is.EqualTo(_SUB_FOLDER_A));
			Assert.That(subFolderA.Id, Is.EqualTo(_subFolderAPath));

			var subFolderB = subItems[1];
			Assert.That(subFolderB, Is.Not.Null);
			Assert.That(subFolderB.Text, Is.EqualTo(_SUB_FOLDER_B));
			Assert.That(subFolderB.Id, Is.EqualTo(_subFolderBPath));
		}

        [Test]
        public void ItShouldReturnSubDirectoriesFiles()
        {
            //Arrange
            MockDirHierarchy();

            //Act
            List<JsTreeItemDTO> subItems = _subjectUnderTest.GetChildren(_subFolderBPath, false, true);

            //Assert
            Assert.That(subItems.Count, Is.EqualTo(3)); //should have 3 subItems total
            Assert.That(subItems.Where(i => i.IsDirectory == false).Count, Is.EqualTo(2));//2 of these subItems are not Folders
        }

        [Test]
		public void ItShouldReturnDirectoryTreeStruct()
		{
			//Arrange
			MockDirHierarchy();

			//Act
			JsTreeItemDTO rootDirJsTreeItem = _subjectUnderTest.TraverseTree(_ROOT_FOLDER);

			//Assert

			//Root
			Assert.That(rootDirJsTreeItem, Is.Not.Null);
			Assert.That(rootDirJsTreeItem.Id, Is.EqualTo(_ROOT_FOLDER));
			Assert.That(rootDirJsTreeItem.Text, Is.EqualTo(_ROOT_FOLDER));
			Assert.That(rootDirJsTreeItem.Children.Count, Is.EqualTo(2));

			//Folder Root\A
			JsTreeItemDTO subFolderA = rootDirJsTreeItem.Children.FirstOrDefault(item => item.Id == _subFolderAPath);
			Assert.That(subFolderA, Is.Not.Null);
			Assert.That(subFolderA.Text, Is.EqualTo(_SUB_FOLDER_A));
			Assert.That(subFolderA.Children.Count, Is.EqualTo(2));

			//Folder Root\A\1
			JsTreeItemDTO subFolderA1 = subFolderA.Children.FirstOrDefault(item => item.Id == _subFolderA1Path);
			Assert.That(subFolderA1, Is.Not.Null);
			Assert.That(subFolderA1.Text, Is.EqualTo(_SUB_FOLDER_1));
			Assert.That(subFolderA1.Children.Count, Is.EqualTo(0));

			//Folder Root\A\2
			JsTreeItemDTO subFolderA2 = subFolderA.Children.FirstOrDefault(item => item.Id == _subFolderA2Path);
			Assert.That(subFolderA2, Is.Not.Null);
			Assert.That(subFolderA2.Text, Is.EqualTo(_SUB_FOLDER_2));
			Assert.That(subFolderA2.Children.Count, Is.EqualTo(0));

			//Folder Root\B
			JsTreeItemDTO subFolderB = rootDirJsTreeItem.Children.FirstOrDefault(item => item.Id == _subFolderBPath);
			Assert.That(subFolderB, Is.Not.Null);
			Assert.That(subFolderB.Text, Is.EqualTo(_SUB_FOLDER_B));
			Assert.That(subFolderB.Children.Count, Is.EqualTo(1));

			//Folder Root\B\1
			JsTreeItemDTO subFolderB1 = subFolderB.Children.FirstOrDefault(item => item.Id == _subFolderB1Path);
			Assert.That(subFolderB1, Is.Not.Null);
			Assert.That(subFolderB1.Text, Is.EqualTo(_SUB_FOLDER_1));
			Assert.That(subFolderB1.Children.Count, Is.EqualTo(0));
		}

		[Test]
		public void ItShouldThrowExceptionWhenRootFolderPathIsEmpty()
		{
			//Arrange
			const string emptyPath = "";
			_directoryMock.Exists(emptyPath).Returns(false);

			//Act & Assert
			Assert.That(() => _subjectUnderTest.GetChildren(emptyPath, true),
				Throws.Exception
				.TypeOf<ArgumentException>());
		}
		
		[Test]
		public void ItShouldSkipNotAccessibleFolders()
		{
			//Arrange
			MockDirHierarchy();

			_directoryMock.GetDirectories(_ROOT_FOLDER).Throws<UnauthorizedAccessException>();

			//Act
			List<JsTreeItemDTO> rootItem = _subjectUnderTest.GetChildren(_ROOT_FOLDER, true);

			//Assert
			Assert.That(rootItem.Count,Is.EqualTo(1));
			Assert.That(rootItem[0].Children.Count, Is.EqualTo(0));
		}

        [Test]
        public void ItShouldReturnDirectoryTreeStructureFiles()
        {
            //Arrange
            MockDirHierarchy();

            //Act
            JsTreeItemDTO rootDirJsTreeItem = _subjectUnderTest.TraverseTree(_ROOT_FOLDER, true);

            //Assert

            //Root
            Assert.That(rootDirJsTreeItem, Is.Not.Null);
            Assert.That(rootDirJsTreeItem.Id, Is.EqualTo(_ROOT_FOLDER));
            Assert.That(rootDirJsTreeItem.Text, Is.EqualTo(_ROOT_FOLDER));
            Assert.That(rootDirJsTreeItem.Children.Count, Is.EqualTo(2));

            //Folder Root\B
            JsTreeItemDTO subFolderB = rootDirJsTreeItem.Children.FirstOrDefault(item => item.Id == _subFolderBPath);
            Assert.That(subFolderB, Is.Not.Null);
            Assert.That(subFolderB.Text, Is.EqualTo(_SUB_FOLDER_B));
            Assert.That(subFolderB.Children.Count, Is.EqualTo(3)); //Should have 3 children since we have 2 files under Sub Folder B
            Assert.That(subFolderB.Children.Where(i => i.IsDirectory == false).Count, Is.EqualTo(2));//2 of these children are not Folders


            //Folder Root\B\1
            JsTreeItemDTO subFolderB1 = subFolderB.Children.FirstOrDefault(item => item.Id == _subFolderB1Path);
            Assert.That(subFolderB1, Is.Not.Null);
            Assert.That(subFolderB1.Text, Is.EqualTo(_SUB_FOLDER_1));
            Assert.That(subFolderB1.Children.Count, Is.EqualTo(0));
        }

		private void MockDirHierarchy()
		{
			_directoryMock.Exists(_ROOT_FOLDER).Returns(true);

			_directoryMock.GetDirectories(_ROOT_FOLDER).Returns(new[]
			{
				_subFolderAPath,
				_subFolderBPath
			});

			_directoryMock.GetDirectories(_subFolderAPath).Returns(new[]
			{
				_subFolderA1Path,
				_subFolderA2Path
			});

			_directoryMock.GetDirectories(_subFolderBPath).Returns(new[]
			{
				_subFolderB1Path
			});

            _directoryMock.GetFiles(_subFolderBPath).Returns(new[]
            {
                _SUB_FILE_1,
                _SUB_FILE_1
            });
        }

	}
}

