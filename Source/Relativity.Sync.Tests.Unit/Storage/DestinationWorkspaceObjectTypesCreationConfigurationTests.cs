using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
    internal sealed class DestinationWorkspaceObjectTypesCreationConfigurationTests : ConfigurationTestBase
    {
        private DestinationWorkspaceObjectTypesCreationConfiguration _instance;

        [SetUp]
        public void SetUp()
        {
            _instance = new DestinationWorkspaceObjectTypesCreationConfiguration(_configuration);
        }

        [Test]
        public void ItShouldRetrieveDestinationWorkspaceArtifactId()
        {
            const int expectedValue = 123;

            _configurationRdo.DestinationWorkspaceArtifactId = expectedValue;

            _instance.DestinationWorkspaceArtifactId.Should().Be(expectedValue);
        }
    }
}
