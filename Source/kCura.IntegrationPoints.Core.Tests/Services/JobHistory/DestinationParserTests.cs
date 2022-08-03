using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.JobHistory
{
    [TestFixture, Category("Unit")]
    public class DestinationParserTests : TestBase
    {
        private DestinationParser _destinationParser;

        public override void SetUp()
        {
            _destinationParser = new DestinationParser();
        }

        [Test]
        [TestCase("workspace - 1", 1)]
        [TestCase("!@#$%^&*()workpace - 2", 2)]
        [TestCase("workspace - 5 - 3", 3)]
        [TestCase("142 - 4", 4)]
        [TestCase("1 - 2 - 5", 5)]
        [TestCase("- 6", 6)]
        [TestCase("workspace -7", 7)]
        [TestCase("workspace-8", 8)]
        [TestCase("workspace- 9", 9)]
        [TestCase("10", 10)]
        [TestCase("workspace- 18156165", 18156165)]
        public void ItShouldParseValidDestination(string destination, int expectedArtifactId)
        {
            var actualArtifactId = _destinationParser.GetArtifactId(destination);

            Assert.That(actualArtifactId, Is.EqualTo(expectedArtifactId));
        }

        [Test]
        [TestCase("workspace - ")]
        [TestCase("workspace - 1 -")]
        [TestCase("2 -")]
        [TestCase("workspace - workspace")]
        [TestCase("workspace - workspace - workspace")]
        public void ItShouldThrowExceptionForInvalidDestination(string destination)
        {
            Assert.That(() => _destinationParser.GetArtifactId(destination),
                Throws.TypeOf<Exception>().With.Message.EqualTo($"Destination workspace object: {destination} could not be parsed."));
        }
    }
}