using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	public sealed class DataDestinationFinalizationConfigurationTests
	{
		private DataDestinationFinalizationConfiguration _instance;

		private Mock<IConfiguration> _cache;

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();

			_instance = new DataDestinationFinalizationConfiguration(_cache.Object);
		}

		[Test]
		public void ItShouldRetrieveDataDestinationArtifactId()
		{
			const int expectedValue = 123;

			_cache.Setup(x => x.GetFieldValue<int>(SyncRdoGuids.DataDestinationArtifactIdGuid)).Returns(expectedValue);

			_instance.DataDestinationArtifactId.Should().Be(expectedValue);
		}
	}
}