using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class DataDestinationFinalizationConfigurationTests
	{
		private DataDestinationFinalizationConfiguration _instance;

		private Mock<IConfigurationCache> _cache;

		private static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfigurationCache>();

			_instance = new DataDestinationFinalizationConfiguration(_cache.Object);
		}

		[Test]
		public void ItShouldRetrieveDataDestinationArtifactId()
		{
			const int expectedValue = 123;

			_cache.Setup(x => x.GetFieldValue<int>(DataDestinationArtifactIdGuid)).Returns(expectedValue);

			_instance.DataDestinationArtifactId.Should().Be(expectedValue);
		}
	}
}