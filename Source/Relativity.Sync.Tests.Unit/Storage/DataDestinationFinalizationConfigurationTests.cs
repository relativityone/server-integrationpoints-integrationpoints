using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
    internal sealed class DataDestinationFinalizationConfigurationTests : ConfigurationTestBase
    {
        private DataDestinationFinalizationConfiguration _instance;

        [SetUp]
        public void Setup()
        {
            _instance = new DataDestinationFinalizationConfiguration(_configuration);
        }

        [Test]
        public void ItShouldRetrieveDataDestinationArtifactId()
        {
            const int expectedValue = 123;

            _configurationRdo.DataDestinationArtifactId = expectedValue;

            _instance.DataDestinationArtifactId.Should().Be(expectedValue);
        }
    }
}