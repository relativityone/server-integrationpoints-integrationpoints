using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
    using RdoExpressionInt = Expression<Func<SyncConfigurationRdo, int>>;


    internal sealed class DestinationWorkspaceSavedSearchCreationConfigurationTests : ConfigurationTestBase
    {
        private DestinationWorkspaceSavedSearchCreationConfiguration _instance;

        [SetUp]
        public void SetUp()
        {
            _instance = new DestinationWorkspaceSavedSearchCreationConfiguration(_configuration);
        }

        [Test]
        public void ItShouldRetrieveDestinationWorkspaceArtifactId()
        {
            const int expectedValue = 852147;

            _configurationRdo.DestinationWorkspaceArtifactId = expectedValue;

            _instance.DestinationWorkspaceArtifactId.Should().Be(expectedValue);
        }

        [Test]
        public void ItShouldRetrieveSourceJobTagName()
        {
            const string expectedValue = "tag name";

            _configurationRdo.SourceJobTagName = expectedValue;

            _instance.GetSourceJobTagName().Should().Be(expectedValue);
        }

        [Test]
        public void ItShouldRetrieveSourceJobTagArtifactId()
        {
            const int expectedValue = 789456;

            _configurationRdo.SourceJobTagArtifactId = expectedValue;

            _instance.SourceJobTagArtifactId.Should().Be(expectedValue);
        }

        [Test]
        public void ItShouldRetrieveSourceWorkspaceTagArtifactId()
        {
            const int expectedValue = 258963;

            _configurationRdo.SourceWorkspaceTagArtifactId = expectedValue;

            _instance.SourceWorkspaceTagArtifactId.Should().Be(expectedValue);
        }

        [Test]
        public void ItShouldRetrieveCreateSavedSearchInDestination()
        {
            const bool expectedValue = true;

            _configurationRdo.CreateSavedSearchInDestination = expectedValue;

            _instance.CreateSavedSearchForTags.Should().Be(expectedValue);
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(789123, true)]
        public void ItShouldRetrieveIsSavedSearchArtifactId(int artifactId, bool expectedValue)
        {
            _configurationRdo.SavedSearchInDestinationArtifactId = artifactId;

            _instance.IsSavedSearchArtifactIdSet.Should().Be(expectedValue);
        }

        [Test]
        public async Task ItShouldUpdateSavedSearchArtifactId()
        {
            // Arrange
            const int artifactId = 589632;

            // Act
            await _instance.SetSavedSearchInDestinationArtifactIdAsync(artifactId).ConfigureAwait(false);

            // Assert
            _configurationRdo.SavedSearchInDestinationArtifactId.Should().Be(artifactId);
        }
    }
}
