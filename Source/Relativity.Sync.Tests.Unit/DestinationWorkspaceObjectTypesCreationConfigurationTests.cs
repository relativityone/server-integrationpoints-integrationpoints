using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class DestinationWorkspaceObjectTypesCreationConfigurationTests
	{
		private DestinationWorkspaceObjectTypesCreationConfiguration _instance;

		private Mock<IConfigurationCache> _cache;

		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfigurationCache>();

			_instance = new DestinationWorkspaceObjectTypesCreationConfiguration(_cache.Object);
		}

		[Test]
		public void ItShouldRetrieveDestinationWorkspaceArtifactId()
		{
			const int expectedValue = 123;

			_cache.Setup(x => x.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid)).Returns(expectedValue);

			_instance.DestinationWorkspaceArtifactId.Should().Be(expectedValue);
		}
	}
}