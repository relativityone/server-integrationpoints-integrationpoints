using FluentAssertions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests
{
    [TestFixture]
    internal sealed class PageInteractionHelperTests
    {
        [Test]
        public void GetApplicationRelativeUri_ShouldReturnAppRelativeUri()
        {
            // arrange
            const string expectedUri = "/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C";

            // act
            string actualUri = PageInteractionHelper.GetApplicationRelativeUri();

            // assert
            actualUri.Should().Be(expectedUri);
        }
    }
}