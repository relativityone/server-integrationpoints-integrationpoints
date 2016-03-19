using System.Collections.Generic;
using kCura.IntegrationPoints.Web.DataStructures;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.DataStructures
{
	public class FolderTreeTests
	{
		private FolderTree _folderTree = new FolderTree();

		[SetUp]
		public void Setup()
		{
			_folderTree = new FolderTree();
		}

		[TestCase(new [] { @"a" }, 1)]
		[TestCase(new [] { @"\a" }, 1)]
		[TestCase(new [] { @"a\" }, 1)]
		[TestCase(new [] { @"\a\" }, 1)]
		[TestCase(new [] { @"\\a" }, 1)]
		[TestCase(new [] { @"a\\" }, 1)]
		[TestCase(new [] { @"\\a\\" }, 1)]
		[TestCase(new [] { @"a\b" }, 2)]
		[TestCase(new [] { @"\a\b" }, 2)]
		[TestCase(new [] { @"a\b" }, 2)]
		[TestCase(new [] { @"\a\b\" }, 2)]
		[TestCase(new [] { @"\\a\b\" }, 2)]
		[TestCase(new [] { @"\\a\\b\" }, 2)]
		[TestCase(new [] { @"\\a\\b\\" }, 2)]
		[TestCase(new [] { @"\\aa\\bb\\" }, 2)]
		[TestCase(new [] { @"a\b", @"a\b" }, 2)]
		[TestCase(new [] { @"a\b", @"a\c" }, 3)]
		[TestCase(new [] { @"a\b", @"b\c" }, 4)]
		[TestCase(new [] { @"a\b", @"b\a" }, 4)]
		[TestCase(new [] { @"a/b" }, 1)]
		[TestCase(new [] { @"\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a" }, 15)]
		[TestCase(new [] { @"\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a", @"\a\a\a\a\a\a\a\a\a\a\a\a\a\b\a" }, 17)]
		[TestCase(new [] { @"\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a", @"\a\a\a\a\a\a\a\a\a\a\a\a\a\a" }, 15)]
		public void Test(IEnumerable<string> folderPaths, int expectedFolderCount)
		{
			// ACT
			foreach (string folderPath in folderPaths)
			{
				_folderTree.AddNode(folderPath);
			}

			// ASSERT
			Assert.AreEqual(expectedFolderCount, _folderTree.FolderCount);
		}
	}
}
