using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	public class PreValidationConfigurationTests
	{
		private PreValidationConfiguration _sut;

		private Mock<IConfiguration> _cache;

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();

			_sut = new PreValidationConfiguration(_cache.Object);
		}

		[Test]
		public void DestinationWorkspaceArtifactId_ShouldReturnWorkspaceId()
		{
			// Arrange
			const int expectedWorkspaceId = 100000;

			_cache.Setup(x => x.GetFieldValue<int>(SyncRdoGuids.DestinationWorkspaceArtifactIdGuid))
				.Returns(expectedWorkspaceId);
			
			// Act & Assert
			_sut.DestinationWorkspaceArtifactId.Should().Be(expectedWorkspaceId);
		}
	}
}
