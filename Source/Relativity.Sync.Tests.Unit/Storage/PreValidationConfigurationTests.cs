using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	internal class PreValidationConfigurationTests : ConfigurationTestBase
	{
		private PreValidationConfiguration _sut;

		[SetUp]
		public void SetUp()
		{
			_sut = new PreValidationConfiguration(_configuration);
		}

		[Test]
		public void DestinationWorkspaceArtifactId_ShouldReturnWorkspaceId()
		{
			// Arrange
			const int expectedWorkspaceId = 100000;

			_configurationRdo.DestinationWorkspaceArtifactId = expectedWorkspaceId;
			
			// Act & Assert
			_sut.DestinationWorkspaceArtifactId.Should().Be(expectedWorkspaceId);
		}
	}
}
