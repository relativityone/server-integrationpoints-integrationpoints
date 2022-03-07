using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration.FieldsMapping;

namespace Relativity.Sync.Tests.Unit.SyncConfiguration.FieldsMapping
{
	[TestFixture]
	internal class FieldsMappingBuilderTests
	{
		private Mock<IObjectManager> _objectManagerFake;
		private Mock<IFieldManager> _fieldManagerFake;

		private IFieldsMappingBuilder _sut;

		private const int _SOURCE_WORKSPACE_ID = 1;
		private const int _DESTINATION_WORKSPACE_ID = 2;
		private readonly int _sourceArtifactTypeId = 68;
		private readonly int _destinationArtifactTypeId = 78;

		[SetUp]
		public void SetUp()
		{
			_objectManagerFake = new Mock<IObjectManager>();
			_fieldManagerFake = new Mock<IFieldManager>();

			var syncServicesMgrFake = new Mock<ISourceServiceFactoryForAdmin>();
			syncServicesMgrFake.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.Returns(Task.FromResult(_objectManagerFake.Object));
			syncServicesMgrFake.Setup(x => x.CreateProxyAsync<IFieldManager>())
				.Returns(Task.FromResult(_fieldManagerFake.Object));

			_sut = new FieldsMappingBuilder(_SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, 
				_sourceArtifactTypeId, _destinationArtifactTypeId,
				syncServicesMgrFake.Object);
		}

		[Test]
		public void WithIdentifier_ShouldAddIdentifierToFieldsMapping()
		{
			// Arrange
			RelativityObject identifierField = new RelativityObject() {ArtifactID = 1, Name = "Test"};

			List<FieldMap> expectedFieldsMap = new List<FieldMap>
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						DisplayName = identifierField.Name,
						FieldIdentifier = identifierField.ArtifactID,
						IsIdentifier = true
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = identifierField.Name,
						FieldIdentifier = identifierField.ArtifactID,
						IsIdentifier = true
					},
					FieldMapType = FieldMapType.Identifier
				}
			};

			SetupIdentifierField(identifierField, _sourceArtifactTypeId, _SOURCE_WORKSPACE_ID);
			SetupIdentifierField(identifierField, _destinationArtifactTypeId ,_DESTINATION_WORKSPACE_ID);

			// Act
			var fieldsMapping = _sut.WithIdentifier().FieldsMapping;

			// Assert
			fieldsMapping.Should().BeEquivalentTo(expectedFieldsMap);
		}

		[Test]
		public void WithIdentifier_ShouldThrowException_WhenWithIdentifierWasCalledTwice()
		{
			// Arrange
			RelativityObject identifierField = new RelativityObject() { ArtifactID = 1, Name = "Test" };

			SetupIdentifierField(identifierField, _sourceArtifactTypeId, _SOURCE_WORKSPACE_ID);
			SetupIdentifierField(identifierField, _destinationArtifactTypeId, _DESTINATION_WORKSPACE_ID);

			// Act
			Action action = () => _sut.WithIdentifier().WithIdentifier();

			// Assert
			action.Should().Throw<InvalidFieldsMappingException>()
				.WithMessage(InvalidFieldsMappingException.IdentifierMappedTwice().Message);
		}

		[Test]
		public void WithField_ShouldAddFieldToFieldsMappingWhenFieldIds()
		{
			// Arrange
			const int sourceFieldId = 100;
			const int destinationFieldId = 200;

			FieldResponse sourceField = new FieldResponse {IsIdentifier = false, Name = "Test", ArtifactID = sourceFieldId};
			FieldResponse destinationField = new FieldResponse {IsIdentifier = false, Name = "Test", ArtifactID = destinationFieldId };

			List<FieldMap> expectedFieldsMap = new List<FieldMap>
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						DisplayName = sourceField.Name,
						FieldIdentifier = sourceField.ArtifactID,
						IsIdentifier = false
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = destinationField.Name,
						FieldIdentifier = destinationField.ArtifactID,
						IsIdentifier = false
					},
					FieldMapType = FieldMapType.None
				}
			};

			SetupField(sourceField, sourceFieldId, _SOURCE_WORKSPACE_ID);
			SetupField(destinationField, destinationFieldId, _DESTINATION_WORKSPACE_ID);

			// Act
			var fieldsMapping = _sut.WithField(sourceFieldId, destinationFieldId).FieldsMapping;

			// Assert
			fieldsMapping.Should().BeEquivalentTo(expectedFieldsMap);
		}

		[Test]
		public void WithField_ShouldAddFieldToFieldsMappingWhenFieldNames()
		{
			// Arrange
			const string sourceFieldName = "Field 1";
			const int sourceFieldId = 1;
			const string destinationFieldName = "Field 2";
			const int destinationFieldId = 2;

			RelativityObject sourceField = new RelativityObject
			{
				Name = sourceFieldName, 
				ArtifactID = sourceFieldId,
				FieldValues = new List<FieldValuePair> { new FieldValuePair { Value = false }}
			};
			RelativityObject destinationField = new RelativityObject
			{
				Name = destinationFieldName,
				ArtifactID = destinationFieldId,
				FieldValues = new List<FieldValuePair> { new FieldValuePair { Value = false } }
			};

			List<FieldMap> expectedFieldsMap = new List<FieldMap>
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						DisplayName = sourceField.Name,
						FieldIdentifier = sourceField.ArtifactID,
						IsIdentifier = false
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = destinationField.Name,
						FieldIdentifier = destinationField.ArtifactID,
						IsIdentifier = false
					},
					FieldMapType = FieldMapType.None
				}
			};

			SetupField(sourceField, sourceFieldName, _sourceArtifactTypeId ,_SOURCE_WORKSPACE_ID);
			SetupField(destinationField, destinationFieldName, _destinationArtifactTypeId, _DESTINATION_WORKSPACE_ID);

			// Act
			var fieldsMapping = _sut
				.WithField(sourceFieldName, destinationFieldName)
				.FieldsMapping;

			// Assert
			fieldsMapping.Should().BeEquivalentTo(expectedFieldsMap);
		}

		[TestCase(true, false)]
		[TestCase(false, true)]
		public void WithField_ShouldThrowException_WhenOneOfFieldsIsIdentifier(
			bool isSourceIdentifier, bool isDestinationIdentifier)
		{
			// Arrange
			const int identifierId = 10;

			int sourceFieldId = isSourceIdentifier ? identifierId : 100;
			int destinationFieldId = isDestinationIdentifier ? identifierId : 200;

			FieldResponse sourceField = new FieldResponse { IsIdentifier = isSourceIdentifier, Name = "Test", ArtifactID = sourceFieldId };
			FieldResponse destinationField = new FieldResponse { IsIdentifier = isDestinationIdentifier, Name = "Test", ArtifactID = destinationFieldId };
			
			SetupField(sourceField, sourceFieldId, _SOURCE_WORKSPACE_ID);
			SetupField(destinationField, destinationFieldId, _DESTINATION_WORKSPACE_ID);

			// Act
			Action action = () => _sut.WithField(sourceFieldId, destinationFieldId);

			// Assert
			action.Should().Throw<InvalidFieldsMappingException>()
				.WithMessage(InvalidFieldsMappingException.FieldIsIdentifier(identifierId).Message);
		}

		private void SetupIdentifierField(RelativityObject expectedIdentifier,int rdoArtifactTypeId ,int workspaceId)
		{
			_objectManagerFake.Setup(x =>
					x.QueryAsync(workspaceId, It.Is<QueryRequest>(q => 
						q.IncludeNameInQueryResult == true && q.Condition.Contains($"'FieldArtifactTypeID' == {rdoArtifactTypeId} AND 'Is Identifier' == true")),
						It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new QueryResult
				{
					ResultCount = 1,
					Objects = new List<RelativityObject>() {expectedIdentifier},
				});
		}

		private void SetupField(FieldResponse expectedField, int fieldId, int workspaceId)
		{
			_fieldManagerFake.Setup(x => x.ReadAsync(workspaceId, fieldId))
				.ReturnsAsync(expectedField);
		}

		private void SetupField(RelativityObject expectedField, string fieldName, int rdoArtifactTypeId, int workspaceId)
		{
			_objectManagerFake.Setup(x =>
					x.QueryAsync(workspaceId, It.Is<QueryRequest>(q =>
						q.IncludeNameInQueryResult == true 
						&& q.Condition.Contains($"'FieldArtifactTypeID' == {rdoArtifactTypeId} AND 'Name' == '{fieldName}'")
						&& q.Fields.Any(f => f.Name == "Is Identifier")),
						It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new QueryResult
				{
					ResultCount = 1,
					Objects = new List<RelativityObject>() { expectedField },
				});
		}
	}
}
