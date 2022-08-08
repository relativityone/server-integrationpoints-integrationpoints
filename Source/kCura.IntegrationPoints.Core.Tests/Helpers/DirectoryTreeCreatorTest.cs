using System;
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

