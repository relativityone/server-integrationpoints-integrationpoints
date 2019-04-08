﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public class FieldMappingsValidatorTests
	{
		private CancellationToken _cancellationToken;

		private Mock<IDestinationServiceFactoryForUser> _destinationServiceFactoryForUser;
		private Mock<ISourceServiceFactoryForUser> _sourceServiceFactoryForUser;
		private Mock<ISerializer> _serializer;
		private Mock<ISyncLog> _syncLog;
		private Mock<IObjectManager> _objectManager;

		private Mock<IValidationConfiguration> _validationConfiguration;

		private JSONSerializer _jsonSerializer;

		private FieldMappingsValidator _instance;

		private const int _TEST_DEST_WORKSPACE_ARTIFACT_ID = 202567;
		private const int _TEST_SOURCE_WORKSPACE_ARTIFACT_ID = 101234;
		private const int _TEST_DEST_FIELD_ARTIFACT_ID = 1003668;
		private const int _TEST_SOURCE_FIELD_ARTIFACT_ID = 1003667;

		private const string _TEST_FIELDS_MAP = @"[{
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

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_jsonSerializer = new JSONSerializer();
		}

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;

			_destinationServiceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			_sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			_serializer = new Mock<ISerializer>();
			_syncLog = new Mock<ISyncLog>();
			_objectManager = new Mock<IObjectManager>();

			_destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			_sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_serializer.Setup(x => x.Deserialize<List<FieldMap>>(It.IsAny<string>())).Returns((string x) =>
			{
				List<FieldMap> fieldMaps = _jsonSerializer.Deserialize<List<FieldMap>>(x);
				return fieldMaps;
			});

			_validationConfiguration = new Mock<IValidationConfiguration>();
			_validationConfiguration.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_TEST_DEST_WORKSPACE_ARTIFACT_ID);
			_validationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID);
			_validationConfiguration.SetupGet(x => x.FieldsMap).Returns(_TEST_FIELDS_MAP).Verifiable();
			_validationConfiguration.SetupGet(x => x.ImportOverwriteMode).Returns(ImportOverwriteMode.AppendOverlay);
			_validationConfiguration.SetupGet(x => x.FieldOverlayBehavior).Returns(FieldOverlayBehavior.Default);

			_instance = new FieldMappingsValidator(_sourceServiceFactoryForUser.Object, _destinationServiceFactoryForUser.Object, _serializer.Object, _syncLog.Object);
		}

		[Test]
		public async Task ValidateAsyncGoldFlowTest()
		{
			// Arrange
			SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, _TEST_SOURCE_FIELD_ARTIFACT_ID);
			SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, _TEST_DEST_FIELD_ARTIFACT_ID);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsTrue(actualResult.IsValid);

			VerifyObjectManagerQueryRequest();
		}

		[Test]
		public async Task ValidateAsyncDeserializeThrowsExceptionTest()
		{
			// Arrange
			_serializer.Setup(x => x.Deserialize<List<FieldMap>>(It.IsAny<string>())).Throws(new Exception()).Verifiable();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);

			Mock.Verify(_validationConfiguration, _serializer);
		}

		[Test]
		public async Task ValidateAsyncDestinationFieldMissingTest()
		{
			// Arrange
			SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, _TEST_SOURCE_FIELD_ARTIFACT_ID);
			SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, 0);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			ValidationMessage actualMessage = actualResult.Messages.First();
			actualMessage.ErrorCode.Should().Be("20.005");
			actualMessage.ShortMessage.Should().StartWith("Destination field(s) mapped");

			VerifyObjectManagerQueryRequest();
		}

		[Test]
		public async Task ValidateAsyncSourceFieldMissingTest()
		{
			// Arrange
			SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, 0);
			SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, _TEST_DEST_FIELD_ARTIFACT_ID);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage.Should().StartWith("Source field(s) mapped");

			VerifyObjectManagerQueryRequest();
		}

		[Test]
		public async Task ValidateAsyncSourceObjectQueryThrowsExceptionTest()
		{
			// Arrange
			SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, _TEST_DEST_FIELD_ARTIFACT_ID);

			_objectManager.Setup(x => x.QueryAsync(It.Is<int>(y => y == _TEST_SOURCE_WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(),
				It.IsAny<IProgress<ProgressReport>>())).ThrowsAsync(new Exception());

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage.Should().Be("Exception occurred during field mappings validation.");

			VerifyObjectManagerQueryRequest();
		}

		[Test]
		public async Task ValidateAsyncDestinationObjectQueryThrowsExceptionTest()
		{
			// Arrange
			SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, _TEST_SOURCE_FIELD_ARTIFACT_ID);

			_objectManager.Setup(x => x.QueryAsync(It.Is<int>(y => y == _TEST_DEST_WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(),
				It.IsAny<IProgress<ProgressReport>>())).ThrowsAsync(new Exception());

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage.Should().Be("Exception occurred during field mappings validation.");

			VerifyObjectManagerQueryRequest();
		}

		[Test]
		[TestCaseSource(nameof(_invalidUniqueIdentifiersFieldMap))]
		public async Task ValidateAsyncUniqueIdentifierInvalidTest(string testInvalidFieldMap, string expectedErrorMessage)
		{
			// Arrange
			SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, _TEST_SOURCE_FIELD_ARTIFACT_ID);
			SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, _TEST_DEST_FIELD_ARTIFACT_ID);

			_validationConfiguration.SetupGet(x => x.FieldsMap).Returns(testInvalidFieldMap).Verifiable();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage.Should().Be(expectedErrorMessage);

			Mock.Verify(_validationConfiguration, _serializer);
		}

		private static IEnumerable<TestCaseData> _invalidUniqueIdentifiersFieldMap => new[]
		{
			new TestCaseData(@"[{
		        ""sourceField"": {
		            ""displayName"": ""Control Number [Object Identifier]"",
		            ""isIdentifier"": false,
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
		    }]", "The unique identifier must be mapped.").SetName($"{nameof(ValidateAsyncUniqueIdentifierInvalidTest)}_SourceInvalid"),
			new TestCaseData(@"[{
		        ""sourceField"": {
		            ""displayName"": ""Control Number [Object Identifier]"",
		            ""isIdentifier"": true,
		            ""fieldIdentifier"": ""1003667"",
		            ""isRequired"": true
		        },
		        ""destinationField"": {
		            ""displayName"": ""Control Number [Object Identifier]"",
		            ""isIdentifier"": false,
		            ""fieldIdentifier"": ""1003668"",
		            ""isRequired"": true
		        },
		        ""fieldMapType"": ""Identifier""
		    }]", "Identifier must be mapped with another identifier.").SetName($"{nameof(ValidateAsyncUniqueIdentifierInvalidTest)}_DestinationInvalid"),
		};

		[Test]
		public async Task ValidateAsyncFieldOverlayBehaviorInvalidTest()
		{
			// Arrange
			SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, _TEST_SOURCE_FIELD_ARTIFACT_ID);
			SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, _TEST_DEST_FIELD_ARTIFACT_ID);

			_validationConfiguration.SetupGet(x => x.ImportOverwriteMode).Returns(ImportOverwriteMode.AppendOnly);
			_validationConfiguration.SetupGet(x => x.FieldOverlayBehavior).Returns(FieldOverlayBehavior.Replace);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.False(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage.Should().Contain("overlay behavior");

			VerifyObjectManagerQueryRequest();
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

		private void VerifyObjectManagerQueryRequest()
		{
			const int expectedFieldArtifactTypeId = 10;
			const string expectedObjectTypeName = "Field";

			string expectedDestQueryCondition = $"(('FieldArtifactTypeID' == {expectedFieldArtifactTypeId} AND 'ArtifactID' IN [{_TEST_DEST_FIELD_ARTIFACT_ID}]))";
			string expectedSourceQueryCondition = $"(('FieldArtifactTypeID' == {expectedFieldArtifactTypeId} AND 'ArtifactID' IN [{_TEST_SOURCE_FIELD_ARTIFACT_ID}]))";

			_objectManager.Verify(x => x.QueryAsync(It.Is<int>(y => y == _TEST_DEST_WORKSPACE_ARTIFACT_ID),
				It.Is<QueryRequest>(y => y.ObjectType.Name == expectedObjectTypeName && y.Condition == expectedDestQueryCondition && y.IncludeNameInQueryResult == true),
				It.Is<int>(y => y == 0), It.Is<int>(y => y == 1), It.Is<CancellationToken>(y => y == _cancellationToken), It.IsAny<IProgress<ProgressReport>>()));

			_objectManager.Verify(x => x.QueryAsync(It.Is<int>(y => y == _TEST_SOURCE_WORKSPACE_ARTIFACT_ID),
				It.Is<QueryRequest>(y => y.ObjectType.Name == expectedObjectTypeName && y.Condition == expectedSourceQueryCondition && y.IncludeNameInQueryResult == true),
				It.Is<int>(y => y == 0), It.Is<int>(y => y == 1), It.Is<CancellationToken>(y => y == _cancellationToken), It.IsAny<IProgress<ProgressReport>>()));
		}
	}
}