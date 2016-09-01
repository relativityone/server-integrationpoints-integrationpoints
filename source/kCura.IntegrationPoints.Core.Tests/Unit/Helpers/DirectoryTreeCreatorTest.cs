
using System;
using System.IO;
using System.Linq;
using SystemInterface.IO;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Helpers
{
	public class DirectoryTreeCreatorTest
	{
		private DirectoryTreeCreator _subjectUnderTest;
		private IDirectory _directoryMock;

		private const string _ROOT_FOLDER = @"\\localhost\Root";
		private const string _SUB_FOLDER_A = @"A";
		private const string _SUB_FOLDER_B = @"B";

		private const string _SUB_FOLDER_1 = @"1";
		private const string _SUB_FOLDER_2 = @"2";

		DirectoryTreeItem dirTreeItem = new DirectoryTreeItem();

		
		[SetUp]
		public void SetUp()
		{
			_directoryMock = Substitute.For<IDirectory>();

			_subjectUnderTest = new DirectoryTreeCreator(_directoryMock);
		}

		[Test]
		public void ItShouldReturnDirectoryTreeStruct()
		{
			//Arrange
			string subFolderAPath = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_A);
			string subFolderA1Path = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_A, _SUB_FOLDER_1);
			string subFolderA2Path = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_A, _SUB_FOLDER_2);

			string subFolderBPath = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_B);
			string subFolderB1Path = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_B, _SUB_FOLDER_1);


			_directoryMock.Exists(_ROOT_FOLDER).Returns(true);

			_directoryMock.GetDirectories(_ROOT_FOLDER).Returns(new []
			{
				subFolderAPath,
				subFolderBPath
			});

			_directoryMock.GetDirectories(subFolderAPath).Returns(new[]
			{
				subFolderA1Path,
				subFolderA2Path
			});

			_directoryMock.GetDirectories(subFolderBPath).Returns(new[]
			{
				subFolderB1Path
			});

			//Act
			DirectoryTreeItem rootDirTreeItem = _subjectUnderTest.TraverseTree(_ROOT_FOLDER);

			//Assert

			//Root
			Assert.That(rootDirTreeItem, Is.Not.Null);
			Assert.That(rootDirTreeItem.Id, Is.EqualTo(_ROOT_FOLDER));
			Assert.That(rootDirTreeItem.Text, Is.EqualTo(_ROOT_FOLDER));
			Assert.That(rootDirTreeItem.Children.Count, Is.EqualTo(2));

			//Folder Root\A
			DirectoryTreeItem subFolderA = rootDirTreeItem.Children.FirstOrDefault(item => item.Id == subFolderAPath);
			Assert.That(subFolderA, Is.Not.Null);
			Assert.That(subFolderA.Text, Is.EqualTo(_SUB_FOLDER_A));
			Assert.That(subFolderA.Children.Count, Is.EqualTo(2));

			//Folder Root\A\1
			DirectoryTreeItem subFolderA1 = subFolderA.Children.FirstOrDefault(item => item.Id == subFolderA1Path);
			Assert.That(subFolderA1, Is.Not.Null);
			Assert.That(subFolderA1.Text, Is.EqualTo(_SUB_FOLDER_1));
			Assert.That(subFolderA1.Children.Count, Is.EqualTo(0));

			//Folder Root\A\2
			DirectoryTreeItem subFolderA2 = subFolderA.Children.FirstOrDefault(item => item.Id == subFolderA2Path);
			Assert.That(subFolderA2, Is.Not.Null);
			Assert.That(subFolderA2.Text, Is.EqualTo(_SUB_FOLDER_2));
			Assert.That(subFolderA2.Children.Count, Is.EqualTo(0));

			//Folder Root\B
			DirectoryTreeItem subFolderB = rootDirTreeItem.Children.FirstOrDefault(item => item.Id == subFolderBPath);
			Assert.That(subFolderB, Is.Not.Null);
			Assert.That(subFolderB.Text, Is.EqualTo(_SUB_FOLDER_B));
			Assert.That(subFolderB.Children.Count, Is.EqualTo(1));

			//Folder Root\B\1
			DirectoryTreeItem subFolderB1 = subFolderB.Children.FirstOrDefault(item => item.Id == subFolderB1Path);
			Assert.That(subFolderB1, Is.Not.Null);
			Assert.That(subFolderB1.Text, Is.EqualTo(_SUB_FOLDER_1));
			Assert.That(subFolderB1.Children.Count, Is.EqualTo(0));
		}

		[Test]
		public void ItShouldThrowExceptionWhenRootNotExists()
		{
			//Arrange
			_directoryMock.Exists(_ROOT_FOLDER).Returns(false);

			//Act & Assert
			Assert.That(() => _subjectUnderTest.TraverseTree(_ROOT_FOLDER),
				Throws.Exception
				.TypeOf<ArgumentException>());
		}

		[Test]
		public void ItShouldSkippedNotAvailableFolder()
		{
			//Arrange
			string subFolderBPath = Path.Combine(_ROOT_FOLDER, _SUB_FOLDER_B);

			_directoryMock.Exists(_ROOT_FOLDER).Returns(true);

			_directoryMock.GetDirectories(_ROOT_FOLDER).Returns(new[]
			{
				subFolderBPath
			});

			_directoryMock.GetDirectories(subFolderBPath).Throws<UnauthorizedAccessException>();

			//Act
			DirectoryTreeItem rootDirTreeItem = _subjectUnderTest.TraverseTree(_ROOT_FOLDER);

			//Root
			Assert.That(rootDirTreeItem, Is.Not.Null);
			Assert.That(rootDirTreeItem.Id, Is.EqualTo(_ROOT_FOLDER));
			Assert.That(rootDirTreeItem.Text, Is.EqualTo(_ROOT_FOLDER));
			Assert.That(rootDirTreeItem.Children.Count, Is.EqualTo(1));

			//Folder Root\A
			DirectoryTreeItem subFolderB = rootDirTreeItem.Children.FirstOrDefault(item => item.Id == subFolderBPath);
			Assert.That(subFolderB, Is.Not.Null);
			Assert.That(subFolderB.Text, Is.EqualTo(_SUB_FOLDER_B));
			Assert.That(subFolderB.Children.Count, Is.EqualTo(0));
		}
	}
}
