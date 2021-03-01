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
	internal sealed class DestinationWorkspaceObjectTypesCreationConfigurationTests : ConfigurationTestBase
	{
		private DestinationWorkspaceObjectTypesCreationConfiguration _instance;

		[SetUp]
		public void SetUp()
		{
			_instance = new DestinationWorkspaceObjectTypesCreationConfiguration(_configuration.Object);
		}

		[Test]
		public void ItShouldRetrieveDestinationWorkspaceArtifactId()
		{
			const int expectedValue = 123;

			_configurationRdo.DestinationWorkspaceArtifactId = expectedValue;

			_instance.DestinationWorkspaceArtifactId.Should().Be(expectedValue);
		}
	}
}