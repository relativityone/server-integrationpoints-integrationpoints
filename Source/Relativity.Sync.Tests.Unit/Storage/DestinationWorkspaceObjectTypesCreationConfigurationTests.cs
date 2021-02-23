using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using IConfiguration = Relativity.Sync.Storage.IConfiguration;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	public sealed class DestinationWorkspaceObjectTypesCreationConfigurationTests
	{
		private DestinationWorkspaceObjectTypesCreationConfiguration _instance;

		private Mock<IConfiguration> _cache;

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();

			_instance = new DestinationWorkspaceObjectTypesCreationConfiguration(_cache.Object);
		}

		[Test]
		public void ItShouldRetrieveDestinationWorkspaceArtifactId()
		{
			const int expectedValue = 123;

			_cache.Setup(x => x.GetFieldValue<int>(SyncRdoGuids.DestinationWorkspaceArtifactIdGuid)).Returns(expectedValue);

			_instance.DestinationWorkspaceArtifactId.Should().Be(expectedValue);
		}
	}
}