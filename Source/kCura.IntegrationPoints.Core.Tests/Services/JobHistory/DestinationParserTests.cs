using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.JobHistory
{
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
				Throws.TypeOf<Exception>().With.Message.EqualTo("The formatting of the destination workspace information has changed and cannot be parsed."));
		}

		[TestCase("instance - workspace - 1", "instance")]
		[TestCase(" instance - workspace - 1", "instance")]
		[TestCase("instance1 - workspace1 - 1", "instance1")]
		[TestCase("instance", "instance")]
		[TestCase("instance - workspace", "instance")]
		public void ItShouldReturnNames(string destination, string expectedName)
		{
			var actualName = _destinationParser.GetName(destination);

			Assert.That(actualName, Is.EqualTo(expectedName));
		}
	}
}