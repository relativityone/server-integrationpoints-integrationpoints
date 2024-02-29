using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class CollectionExtensionsTests
    {
        [Test]
        [TestCase(new[] { 0 }, 0)]
        [TestCase(new[] { 0 }, 1)]
        [TestCase(new[] { 0, 1 }, -1)]
        [TestCase(new[] { 0, 1 }, 0)]
        [TestCase(new[] { 0, 1 }, 2)]
        [TestCase(new[] { 0, 1 }, 50)]
        public void SplitListOneCollectionTests(int[] testValues, int testBatchSize)
        {
            // Arrange
            const int expectedNumberOfBatches = 1;

            // Act
            IList<IList<int>> actualResult = CollectionExtensions.SplitList(testValues, testBatchSize).ToList();

            // Assert
            CollectionAssert.IsNotEmpty(actualResult);
            Assert.AreEqual(expectedNumberOfBatches, actualResult.Count);
            CollectionAssert.AreEqual(testValues, actualResult[0]);
        }

        [Test]
        public void SplitListEmptyCollectionTest()
        {
            // Arrange
            int[] testValues = Array.Empty<int>();

            // Act
            IList<IList<int>> actualResult = CollectionExtensions.SplitList(testValues, 0).ToList();

            // Assert
            Assert.IsNotNull(actualResult);
            CollectionAssert.IsEmpty(actualResult);
        }

        [Test]
        [TestCase(new[] { 0, 1, 2, 3, 4, 5, 6 }, new[] { 0, 1, 2, 3 }, new[] { 4, 5, 6 })]
        [TestCase(new[] { 0, 1, 2, 3 }, new[] { 0, 1 }, new[] { 2, 3 })]
        [TestCase(new[] { 0, 1, 3 }, new[] { 0, 1 }, new[] { 3 })]
        public void SplitListTwoCollectionTests(int[] testValues, int[] expectedFirstBatch, int[] expectedSecondBatch)
        {
            // Arrange
            const int expectedNumberOfBatches = 2;

            int testBatchSize = (int)Math.Ceiling((double)testValues.Length / expectedNumberOfBatches);

            // Act
            IList<IList<int>> actualResult = CollectionExtensions.SplitList(testValues, testBatchSize).ToList();

            // Assert
            CollectionAssert.IsNotEmpty(actualResult);
            Assert.AreEqual(expectedNumberOfBatches, actualResult.Count);
            CollectionAssert.AreEqual(expectedFirstBatch, actualResult[0]);
            CollectionAssert.AreEqual(expectedSecondBatch, actualResult[1]);
        }

        [Test]
        [TestCase(new[] { 0, 1, 2, 3, 4, 5, 6 })]
        [TestCase(new[] { 0, 1, 2, 3 })]
        [TestCase(new[] { 0, 1, 3 })]
        public async Task SelectAsyncTest(int[] input)
        {
            IEnumerable<int> selected = await input
                .SelectAsync(async x => await Task.FromResult(x + 1).ConfigureAwait(false))
                .ConfigureAwait(false);

            IEnumerable<int> expected = input.Select(x => x + 1);

            selected.Should().BeEquivalentTo(expected);
        }
    }
}
