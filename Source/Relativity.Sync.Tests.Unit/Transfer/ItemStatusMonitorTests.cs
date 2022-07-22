using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public sealed class ItemStatusMonitorTests
    {
        private const string _FIRST_ITEM_IDENTIFIER = "first";
        private const int _FIRST_ITEM_ARTIFACT_ID = 1;
        private const string _SECOND_ITEM_IDENTIFIER = "second";
        private const int _SECOND_ITEM_ARTIFACT_ID = 2;

        [Test]
        public void ItShouldReturnNoSuccessfulArtifactIdsAfterAdding()
        {
            // Arrange
            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);

            // Act
            IEnumerable<int> result = instance.GetSuccessfulItemArtifactIds();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void ItShouldReturnNoSuccessfulIdentifiersAfterAdding()
        {
            // Arrange
            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);

            // Act
            IEnumerable<string> result = instance.GetSuccessfulItemIdentifiers();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void ItShouldReturnNoSuccessfulArtifactIdsAfterMarkingAsRead()
        {
            // Arrange
            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);

            // Act
            IEnumerable<int> result = instance.GetSuccessfulItemArtifactIds();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void ItShouldReturnNoSuccessfulIdentifiersAfterMarkingAsRead()
        {
            // Arrange
            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);

            // Act
            IEnumerable<string> result = instance.GetSuccessfulItemIdentifiers();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void ItShouldReturnNoSuccessfulArtifactIdsAfterMarkingAllReadAsFailed()
        {
            // Arrange
            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsFailed();

            // Act
            IEnumerable<int> result = instance.GetSuccessfulItemArtifactIds();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void ItShouldReturnNoSuccessfulIdentifiersAfterMarkingAllReadAsFailed()
        {
            // Arrange
            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsFailed();

            // Act
            IEnumerable<string> result = instance.GetSuccessfulItemIdentifiers();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void ItShouldReturnOneSuccessfulArtifactIdWhenOtherNotRead()
        {
            // Arrange
            const int successfulItemArtifactId = _FIRST_ITEM_ARTIFACT_ID;
            const int notReadItemArtifactId = _SECOND_ITEM_ARTIFACT_ID;

            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsSuccessful();

            // Act
            List<int> result = instance.GetSuccessfulItemArtifactIds().ToList();

            // Assert
            result.Should().Contain(successfulItemArtifactId);
            result.Should().NotContain(notReadItemArtifactId);
        }

        [Test]
        public void ItShouldReturnOneSuccessfulIdentifiersWhenOtherNotRead()
        {
            // Arrange
            const string successfulItemIdentifier = _FIRST_ITEM_IDENTIFIER;
            const string notReadItemIdentifier = _SECOND_ITEM_IDENTIFIER;

            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsSuccessful();

            // Act
            List<string> result = instance.GetSuccessfulItemIdentifiers().ToList();

            // Assert
            result.Should().Contain(successfulItemIdentifier);
            result.Should().NotContain(notReadItemIdentifier);
        }

        [Test]
        public void ItShouldReturnSuccessfulArtifactIds()
        {
            // Arrange
            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsSuccessful();

            // Act
            List<int> result = instance.GetSuccessfulItemArtifactIds().ToList();

            // Assert
            result.Should().Contain(_FIRST_ITEM_ARTIFACT_ID);
            result.Should().Contain(_SECOND_ITEM_ARTIFACT_ID);
        }

        [Test]
        public void ItShouldReturnSuccessfulIdentifiers()
        {
            // Arrange
            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsSuccessful();

            // Act
            List<string> result = instance.GetSuccessfulItemIdentifiers().ToList();

            // Assert
            result.Should().Contain(_FIRST_ITEM_IDENTIFIER);
            result.Should().Contain(_SECOND_ITEM_IDENTIFIER);
        }

        [Test]
        public void ItShouldReturnSuccessfulArtifactIdWhenReadAfterMarkingAsFailed()
        {
            // Arrange
            const int successfulItemArtifactId = _SECOND_ITEM_ARTIFACT_ID;
            const int failedItemArtifactId = _FIRST_ITEM_ARTIFACT_ID;

            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsFailed();
            instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsSuccessful();

            // Act
            List<int> result = instance.GetSuccessfulItemArtifactIds().ToList();

            // Assert
            result.Should().NotContain(failedItemArtifactId);
            result.Should().Contain(successfulItemArtifactId);
        }

        [Test]
        public void ItShouldReturnSuccessfulIdentifierWhenReadAfterMarkingAsFailed()
        {
            // Arrange
            const string successfulItemIdentifier = _SECOND_ITEM_IDENTIFIER;
            const string failedItemIdentifier = _FIRST_ITEM_IDENTIFIER;

            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsFailed();
            instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsSuccessful();

            // Act
            List<string> result = instance.GetSuccessfulItemIdentifiers().ToList();

            // Assert
            result.Should().NotContain(failedItemIdentifier);
            result.Should().Contain(successfulItemIdentifier);
        }

        [Test]
        public void ItShouldReturnSuccessfulArtifactIdAfterMarkingOtherAsFailed()
        {
            // Arrange
            const int successfulItemArtifactId = _SECOND_ITEM_ARTIFACT_ID;
            const int failedItemArtifactId = _FIRST_ITEM_ARTIFACT_ID;

            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
            instance.MarkItemAsFailed(_FIRST_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsSuccessful();

            // Act
            List<int> result = instance.GetSuccessfulItemArtifactIds().ToList();

            // Assert
            result.Should().NotContain(failedItemArtifactId);
            result.Should().Contain(successfulItemArtifactId);
        }

        [Test]
        public void ItShouldReturnSuccessfulIdentifierAfterMarkingOtherAsFailed()
        {
            // Arrange
            const string successfulItemIdentifier = _SECOND_ITEM_IDENTIFIER;
            const string failedItemArtifactId = _FIRST_ITEM_IDENTIFIER;

            var instance = new ItemStatusMonitor();
            instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
            instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
            instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
            instance.MarkItemAsFailed(_FIRST_ITEM_IDENTIFIER);
            instance.MarkReadSoFarAsSuccessful();

            // Act
            List<string> result = instance.GetSuccessfulItemIdentifiers().ToList();

            // Assert
            result.Should().NotContain(failedItemArtifactId);
            result.Should().Contain(successfulItemIdentifier);
        }

        [Test]
        public void ItShouldGetProperArtifactIdBasedOnIdentifier()
        {
            //Arrange
            var sut = new ItemStatusMonitor();
            sut.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
            sut.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);

            //Act
            var result = sut.GetArtifactId(_SECOND_ITEM_IDENTIFIER);

            //Assert
            result.Should().Be(_SECOND_ITEM_ARTIFACT_ID);
        }

        [Test]
        public void ItShouldNotThrowOnGetArtifactIdWhenItemDoesNotExist()
        {
            //Arrange
            var sut = new ItemStatusMonitor();
            
            var notExistingIdentifier = "AnyString";
            int expectedArtifactID = -1;
            
            //Act
            var result = sut.GetArtifactId(notExistingIdentifier);

            //Assert
            result.Should().Be(expectedArtifactID);
        }
    }
}
