using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
    [TestFixture]
    public static class EmptyDataDestinationInitializationConfigurationTests
    {
        [Test]
        public static void ItShouldReturnDefaultValues()
        {
            EmptyDataDestinationInitializationConfiguration configuration = new EmptyDataDestinationInitializationConfiguration();

            // ASSERT
            configuration.DataDestinationName.Should().BeEmpty();
            configuration.IsDataDestinationArtifactIdSet.Should().BeTrue();
            configuration.DataDestinationArtifactId.Should().Be(0);
        }
    }
}