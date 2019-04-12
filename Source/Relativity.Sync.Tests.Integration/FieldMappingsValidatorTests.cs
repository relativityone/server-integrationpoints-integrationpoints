using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class FieldMappingsValidatorTests
	{
		private ConfigurationStub _configuration;
		private FieldMappingsValidator _sut;
		private Mock<IObjectManager> _objectManager;

		private const int _TEST_DEST_WORKSPACE_ARTIFACT_ID = 202567;
		private const int _TEST_SOURCE_WORKSPACE_ARTIFACT_ID = 101234;
		private const int _TEST_DEST_FIELD_ARTIFACT_ID = 1003668;
		private const int _TEST_SOURCE_FIELD_ARTIFACT_ID = 1003667;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			Mock<IDestinationServiceFactoryForUser> destinationServiceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			Mock<ISourceServiceFactoryForUser> sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();

			destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			containerBuilder.RegisterInstance(destinationServiceFactoryForUser.Object).As<IDestinationServiceFactoryForUser>();
			containerBuilder.RegisterInstance(sourceServiceFactoryForUser.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterType<FieldMappingsValidator>();
			IContainer container = containerBuilder.Build();

			_configuration = new ConfigurationStub()
			{
				DestinationWorkspaceArtifactId = _TEST_DEST_WORKSPACE_ARTIFACT_ID,
				SourceWorkspaceArtifactId = _TEST_SOURCE_WORKSPACE_ARTIFACT_ID
			};
			SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, _TEST_SOURCE_FIELD_ARTIFACT_ID);
			SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, _TEST_DEST_FIELD_ARTIFACT_ID);

			_sut = container.Resolve<FieldMappingsValidator>();
		}

		[Test]
		public async Task ItShouldProperlyDeserializeFieldMappings()
		{
			const string fieldsMap = @"[{
		        ""sourceField"": {
		            ""displayName"": ""Control Number [Object Identifier]"",
		            ""isIdentifier"": true,
		            ""fieldIdentifier"": ""1003667"",
		            ""isRequired"": true
		        },
		        ""destinationField"": {
		            ""displayName"": ""Control Number [Object Identifier]"",
		            ""isIdentifier"": true,
		            ""fieldIdentifier"": ""1003668"",
		            ""isRequired"": true
		        },
		        ""fieldMapType"": ""Identifier""
		    }]";
			_configuration.FieldMappings = fieldsMap;

			// act
			ValidationResult result = await _sut.ValidateAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.IsValid.Should().Be(true);
		}

		private void SetUpObjectManagerQuery(int testWorkspaceArtifactId, int testFieldArtifactId)
		{
			var queryResult = new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject
					{
						ArtifactID = testFieldArtifactId,
					}
				}
			};
			_objectManager.Setup(x => x.QueryAsync(It.Is<int>(y => y == testWorkspaceArtifactId), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(),
				It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);
		}

	}
}