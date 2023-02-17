using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Web.DataStructures;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.DataStructures
{
    [TestFixture, Category("Unit")]
    public class FolderTreeTests : TestBase
    {
        private FolderTree _folderTree = new FolderTree();

        [SetUp]
        public override void SetUp()
        {
            _folderTree = new FolderTree();
        }

        [TestCase(new[] { @"a" }, 1)]
        [TestCase(new[] { @"\a" }, 1)]
        [TestCase(new[] { @"a\" }, 1)]
        [TestCase(new[] { @"\a\" }, 1)]
        [TestCase(new[] { @"\\a" }, 1)]
        [TestCase(new[] { @"a\\" }, 1)]
        [TestCase(new[] { @"\a", @"\A" }, 1)]
        [TestCase(new[] { @"\\a\\" }, 1)]
        [TestCase(new[] { @"a\b" }, 2)]
        [TestCase(new[] { @"\a\b" }, 2)]
        [TestCase(new[] { @"a\b" }, 2)]
        [TestCase(new[] { @"\a\b\" }, 2)]
        [TestCase(new[] { @"\\a\b\" }, 2)]
        [TestCase(new[] { @"\\a\\b\" }, 2)]
        [TestCase(new[] { @"\\a\\b\\" }, 2)]
        [TestCase(new[] { @"\\aa\\bb\\" }, 2)]
        [TestCase(new[] { @"a\b", @"a\b" }, 2)]
        [TestCase(new[] { @"a\b", @"a\c" }, 3)]
        [TestCase(new[] { @"a\b", @"b\c" }, 4)]
        [TestCase(new[] { @"a\b", @"b\a" }, 4)]
        [TestCase(new[] { @"a/b" }, 1)]
        [TestCase(new[] { @"\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a" }, 15)]
        [TestCase(new[] { @"\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a", @"\a\a\a\a\a\a\a\a\a\a\a\a\a\b\a" }, 17)]
        [TestCase(new[] { @"\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a", @"\a\a\a\a\a\a\a\a\a\a\a\a\a\a" }, 15)]
        public void FolderTree_CountFoldersCorrectly(IEnumerable<string> folderPaths, int expectedFolderCount)
        {
            // ACT
            foreach (string folderPath in folderPaths)
            {
                _folderTree.AddEntry(folderPath);
            }

            // ASSERT
            Assert.AreEqual(expectedFolderCount, _folderTree.FolderCount);
        }

        /// <summary>
        /// Creates a random path containing maxFolderDepth folders. This is just used to quickly
        /// see the high level performance of the FolderTree implementation.
        /// </summary>
        /// <param name="maxFolderDepth"></param>
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(25)]
        [TestCase(50)]
        [TestCase(100)]
        [Explicit]
        public void FolderTree_StressTest(int maxFolderDepth)
        {
            // ARRANGE
            int numDocuments = 1000;
            Random random = new Random();

            string[] uniqueFolders = new string[numDocuments];
            for (int i = 0; i < numDocuments; i++)
            {
                StringBuilder uniquePath = new StringBuilder();
                for (int j = 0; j < maxFolderDepth; j++)
                {
                    char letter = (char)('a' + random.Next(0, 26));
                    uniquePath.Append(@"\" + letter);
                }
                uniqueFolders[i] = uniquePath.ToString();
            }

            // ACT
            FolderTree folderTree = new FolderTree();
            Stopwatch watch1 = Stopwatch.StartNew();
            foreach (string folderPath in uniqueFolders)
            {
                folderTree.AddEntry(folderPath);
            }
            watch1.Stop();
            long elapsedMilliseconds1 = watch1.ElapsedMilliseconds;

            Console.WriteLine(String.Format("Milliseconds elapsed {0}", elapsedMilliseconds1));
        }
    }
}
