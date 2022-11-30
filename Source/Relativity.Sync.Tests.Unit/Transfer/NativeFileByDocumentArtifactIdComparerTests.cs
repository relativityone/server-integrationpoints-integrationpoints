using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    public class NativeFileByDocumentArtifactIdComparerTests
    {
        private NativeFileByDocumentArtifactIdComparer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new NativeFileByDocumentArtifactIdComparer();
        }

        [Test]
        public void GetHashCode_ShouldReturnDocumentArtifactIdHashCode()
        {
            // Arrange
            const int documentArtifactId = 1;
            INativeFile file = new NativeFile(documentArtifactId, string.Empty, string.Empty, 0);

            // Act
            int hashCode = _sut.GetHashCode(file);

            // Assert
            hashCode.Should().Be(documentArtifactId.GetHashCode());
        }

        [Test]
        public void Equals_ShouldCompareOnlyByDocumentArtifactId_WhenOnlyDocumentArtifactIdsAreDifferent()
        {
            // Arrange
            const string location = "location";
            const string filename = "filename";
            const int size = 11;
            const int firstDocumentArtifactId = 1;
            const int secondDocumentArtifactId = 2;
            INativeFile file1 = new NativeFile(firstDocumentArtifactId, location, filename, size);
            INativeFile file2 = new NativeFile(secondDocumentArtifactId, location, filename, size);

            // Act
            bool areEqual = _sut.Equals(file1, file2);

            // Assert
            areEqual.Should().BeFalse();
        }


        [Test]
        public void Equals_ShouldCompareOnlyByDocumentArtifactId_WhenDocumentArtifactIdsAreEqual()
        {
            // Arrange
#pragma warning disable RG2009 // Hardcoded Numeric Value
            const int documentArtifactId = 1;
            INativeFile file1 = new NativeFile(documentArtifactId, "location1", "filename1", 11);
            INativeFile file2 = new NativeFile(documentArtifactId, "location2", "filename2", 22);
#pragma warning restore RG2009 // Hardcoded Numeric Value

            // Act
            bool areEqual = _sut.Equals(file1, file2);

            // Assert
            areEqual.Should().BeTrue();
        }
    }
}
