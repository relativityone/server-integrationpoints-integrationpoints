﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.Sync.Utils;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common.Attributes;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public class DocumentFieldMappingsValidatorTests
	{
		private CancellationToken _cancellationToken;

		private Mock<IObjectManager> _objectManager;

		private Mock<IValidationConfiguration> _validationConfiguration;

		private JSONSerializer _jsonSerializer;
		private List<FieldMap> _fieldMappings;

		private DocumentFieldMappingValidator _sut;

		private const int _TEST_DEST_WORKSPACE_ARTIFACT_ID = 202567;
		private const int _TEST_SOURCE_WORKSPACE_ARTIFACT_ID = 101234;
		private const int _IDENTIFIER_DEST_FIELD_ARTIFACT_ID = 1003668;
		private const int _IDENTIFIER_SOURCE_FIELD_ARTIFACT_ID = 1003667;
		private const string _IDENTIFIER_DEST_FIELD_NAME = "Control Number";
		private const string _IDENTIFIER_SOURCE_FIELD_NAME = "Control Number";


		private const string _TEST_DEST_FIELD_NAME = "Test";
		private const string _TEST_SOURCE_FIELD_NAME = "Test";


		private const int _TEST_DEST_FIELD_ARTIFACT_ID = 1003669;
		private const int _TEST_SOURCE_FIELD_ARTIFACT_ID = 10036670;


		private const string _TEST_FIELDS_MAP = @"[{
	        ""sourceField"": {
	            ""displayName"": ""Control Number"",
	            ""isIdentifier"": true,
	            ""fieldIdentifier"": ""1003667"",
	            ""isRequired"": true
	        },
	        ""destinationField"": {
	            ""displayName"": ""Control Number"",
	            ""isIdentifier"": true,
	            ""fieldIdentifier"": ""1003668"",
	            ""isRequired"": true
	        },
	        ""fieldMapType"": ""Identifier""
			},
		    {""sourceField"": {
	            ""displayName"": ""Test"",
	            ""isIdentifier"": false,
	            ""fieldIdentifier"": ""1003669"",
	        },
	        ""destinationField"": {
	            ""displayName"": ""Test"",
	            ""isIdentifier"": false,
	            ""fieldIdentifier"": ""1003670""
	        },
	        ""fieldMapType"": ""None""
	    }
		]";

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_jsonSerializer = new JSONSerializer();
			_fieldMappings = _jsonSerializer.Deserialize<List<FieldMap>>(_TEST_FIELDS_MAP);
		}

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;

			var destinationServiceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			var sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			_objectManager = new Mock<IObjectManager>();

			destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_validationConfiguration = new Mock<IValidationConfiguration>();
			_validationConfiguration.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_TEST_DEST_WORKSPACE_ARTIFACT_ID).Verifiable();
			_validationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID).Verifiable();
			_validationConfiguration.Setup(x => x.GetFieldMappings()).Returns(_fieldMappings).Verifiable();
			_validationConfiguration.SetupGet(x => x.ImportOverwriteMode).Returns(ImportOverwriteMode.AppendOverlay);
			_validationConfiguration.SetupGet(x => x.FieldOverlayBehavior).Returns(FieldOverlayBehavior.UseFieldSettings);


			SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, _fieldMappings.Select(x => x.SourceField));
			SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, _fieldMappings.Select(x => x.DestinationField));

			_sut = new DocumentFieldMappingValidator(sourceServiceFactoryForUser.Object, destinationServiceFactoryForUser.Object, new EmptyLogger());
		}


		[Test]
		public async Task ValidateAsync_ShouldPassGoldFlow()
		{
			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsTrue(actualResult.IsValid);

			VerifyObjectManagerQueryRequest();
			Mock.Verify(_validationConfiguration);
		}

		[Test]
		public async Task ValidateAsync_ShouldDeserializeThrowsException()
		{
			// Arrange
			_validationConfiguration.Setup(x => x.GetFieldMappings()).Throws<InvalidOperationException>().Verifiable();

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleDestinationFieldMissing()
		{
			// Arrange
			SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, Enumerable.Empty<FieldEntry>());

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			ValidationMessage actualMessage = actualResult.Messages.First();
			actualMessage.ErrorCode.Should().Be("20.005");
			actualMessage.ShortMessage.Should().StartWith("Destination field(s) mapped");

			VerifyObjectManagerQueryRequest();
			Mock.Verify(_validationConfiguration);
		}

		[Test]
		public async Task ValidateAsync_ShouldHAndleSourceFieldMissing()
		{
			// Arrange
			SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, Enumerable.Empty<FieldEntry>());

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage.Should().StartWith("Source field(s) mapped");

			VerifyObjectManagerQueryRequest();
			Mock.Verify(_validationConfiguration);
		}

		[Test]
		public async Task ValidateAsync_ShouldReturnInvalidMessage_WhenFieldInSourceWorkspaceHasBeenRenamed()
		{
			// Arrange
			var newFieldMappings = _jsonSerializer.Deserialize<List<FieldMap>>(_TEST_FIELDS_MAP);
			newFieldMappings[0].SourceField.DisplayName = "Control Number - RENAMED";

			SetUpObjectManagerQuery(_TEST_SOURCE_WORKSPACE_ARTIFACT_ID, newFieldMappings.Select(x => x.SourceField));
			
			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage
				.Should().StartWith("Source field(s) mapped")
				.And.Contain(_IDENTIFIER_SOURCE_FIELD_NAME);

			VerifyObjectManagerQueryRequest();
			Mock.Verify(_validationConfiguration);
		}

		[Test]
		public async Task ValidateAsync_ShouldReturnInvalidMessage_WhenFieldInDestinationWorkspaceHasBeenRenamed()
		{
			// Arrange
			var newFieldMappings = _jsonSerializer.Deserialize<List<FieldMap>>(_TEST_FIELDS_MAP);
			newFieldMappings[0].DestinationField.DisplayName = "Control Number - RENAMED";

			SetUpObjectManagerQuery(_TEST_DEST_WORKSPACE_ARTIFACT_ID, newFieldMappings.Select(x => x.DestinationField));

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);

			ValidationMessage actualMessage = actualResult.Messages.First();
			actualMessage.ErrorCode.Should().Be("20.005");
			actualMessage.ShortMessage
				.Should().StartWith("Destination field(s) mapped")
				.And.Contain(_IDENTIFIER_DEST_FIELD_NAME);

			VerifyObjectManagerQueryRequest();
			Mock.Verify(_validationConfiguration);
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleSourceObjectQuery_ThrowsException()
		{
			// Arrange
			_objectManager.Setup(x => x.QueryAsync(It.Is<int>(y => y == _TEST_SOURCE_WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(),
				It.IsAny<IProgress<ProgressReport>>())).Throws<InvalidOperationException>();

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage.Should().Be("Exception occurred during field mappings validation. See logs for more details.");

			VerifyObjectManagerQueryRequest();
			Mock.Verify(_validationConfiguration);
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleDestinationObjectQuery_ThrowsException()
		{
			// Arrange
			_objectManager.Setup(x => x.QueryAsync(It.Is<int>(y => y == _TEST_DEST_WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(),
				It.IsAny<IProgress<ProgressReport>>())).Throws<InvalidOperationException>();

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage.Should().Be("Exception occurred during field mappings validation. See logs for more details.");

			VerifyObjectManagerQueryRequest();
			Mock.Verify(_validationConfiguration);
		}

		[TestCaseSource(nameof(_invalidUniqueIdentifiersFieldMap))]
		public async Task ValidateAsync_ShouldHandleUniqueIdentifierInvalid(string testInvalidFieldMap, string expectedErrorMessage)
		{
			// Arrange
			List<FieldMap> fieldMap = _jsonSerializer.Deserialize<List<FieldMap>>(testInvalidFieldMap);
			_validationConfiguration.Setup(x => x.GetFieldMappings()).Returns(fieldMap).Verifiable();

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage.Should().Be(expectedErrorMessage);

			VerifyObjectManagerQueryRequest(fieldMap);
			Mock.Verify(_validationConfiguration);
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleFieldOverlayBehaviorInvalid()
		{
			// Arrange
			_validationConfiguration.SetupGet(x => x.ImportOverwriteMode).Returns(ImportOverwriteMode.AppendOnly);
			_validationConfiguration.SetupGet(x => x.FieldOverlayBehavior).Returns(FieldOverlayBehavior.ReplaceValues);

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(_validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.False(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);
			actualResult.Messages.First().ShortMessage.Should().Contain("overlay behavior");

			VerifyObjectManagerQueryRequest();
			Mock.Verify(_validationConfiguration);
		}

		[TestCase(typeof(SyncDocumentRunPipeline), true)]
		[TestCase(typeof(SyncDocumentRetryPipeline), true)]
		[EnsureAllPipelineTestCase(0)]
		public void ShouldExecute_ShouldReturnCorrectValue(Type pipelineType, bool expectedResult)
		{
			// Arrange
			ISyncPipeline pipelineObject = (ISyncPipeline)Activator.CreateInstance(pipelineType);

			// Act
			bool actualResult = _sut.ShouldValidate(pipelineObject);

			// Assert
			actualResult.Should().Be(expectedResult,
				$"ShouldValidate should return {expectedResult} for pipeline {pipelineType.Name}");
		}

		private void SetUpObjectManagerQuery(int workspaceArtifactId, IEnumerable<FieldEntry> fieldsAvailableInWorkspace)
		{
			var queryResult = new QueryResult
			{
				Objects = fieldsAvailableInWorkspace.Select(x => new RelativityObject
					{Name = x.DisplayName, ArtifactID = x.FieldIdentifier}).ToList()
			};

			_objectManager.Setup(x => x.QueryAsync(It.Is<int>(y => y == workspaceArtifactId), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(),
				It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);
		}

		private void VerifyObjectManagerQueryRequest(IEnumerable<FieldMap> mappingToVerify = null)
		{
			const int expectedFieldArtifactTypeId = (int)ArtifactType.Document;
			const string expectedObjectTypeName = "Field";

			mappingToVerify = mappingToVerify ?? _fieldMappings;

			string expectedDestQueryCondition = $"(('FieldArtifactTypeID' == {expectedFieldArtifactTypeId} AND 'ArtifactID' IN [{string.Join(",", mappingToVerify.Select(x => x.DestinationField.FieldIdentifier))}]))";
			string expectedSourceQueryCondition = $"(('FieldArtifactTypeID' == {expectedFieldArtifactTypeId} AND 'ArtifactID' IN [{string.Join(",", mappingToVerify.Select(x => x.SourceField.FieldIdentifier))}]))";

			_objectManager.Verify(x => x.QueryAsync(It.Is<int>(y => y == _TEST_DEST_WORKSPACE_ARTIFACT_ID),
				It.Is<QueryRequest>(y => y.ObjectType.Name == expectedObjectTypeName && y.Condition == expectedDestQueryCondition && y.IncludeNameInQueryResult == true),
				It.Is<int>(y => y == 0), It.Is<int>(y => y == mappingToVerify.Count()), It.Is<CancellationToken>(y => y == _cancellationToken), It.IsAny<IProgress<ProgressReport>>()));

			_objectManager.Verify(x => x.QueryAsync(It.Is<int>(y => y == _TEST_SOURCE_WORKSPACE_ARTIFACT_ID),
				It.Is<QueryRequest>(y => y.ObjectType.Name == expectedObjectTypeName && y.Condition == expectedSourceQueryCondition && y.IncludeNameInQueryResult == true),
				It.Is<int>(y => y == 0), It.Is<int>(y => y == mappingToVerify.Count()), It.Is<CancellationToken>(y => y == _cancellationToken), It.IsAny<IProgress<ProgressReport>>()));
		}

		private static IEnumerable<TestCaseData> _invalidUniqueIdentifiersFieldMap => new[]
		{
			new TestCaseData(@"[{
		        ""sourceField"": {
		            ""displayName"": ""Control Number"",
		            ""isIdentifier"": false,
		            ""fieldIdentifier"": ""1003667"",
		            ""isRequired"": true
		        },
		        ""destinationField"": {
		            ""displayName"": ""Control Number"",
		            ""isIdentifier"": true,
		            ""fieldIdentifier"": ""1003668"",
		            ""isRequired"": true
		        },
		        ""fieldMapType"": ""Identifier""
		    }]", "The unique identifier must be mapped.").SetName($"{nameof(ValidateAsync_ShouldHandleUniqueIdentifierInvalid)}_SourceInvalid"),
			new TestCaseData(@"[{
		        ""sourceField"": {
		            ""displayName"": ""Control Number"",
		            ""isIdentifier"": true,
		            ""fieldIdentifier"": ""1003667"",
		            ""isRequired"": true
		        },
		        ""destinationField"": {
		            ""displayName"": ""Control Number"",
		            ""isIdentifier"": false,
		            ""fieldIdentifier"": ""1003668"",
		            ""isRequired"": true
		        },
		        ""fieldMapType"": ""Identifier""
		    }]", "Identifier must be mapped with another identifier.").SetName($"{nameof(ValidateAsync_ShouldHandleUniqueIdentifierInvalid)}_DestinationInvalid"),
		};
	}
}