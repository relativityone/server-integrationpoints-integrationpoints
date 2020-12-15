using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration;

namespace Relativity.Sync.Tests.Unit.SyncConfiguration
{
	[TestFixture]
	internal class FieldsMappingBuilderTests
	{
		private Mock<IObjectManager> _objectManagerFake;
		private Mock<IFieldManager> _fieldManagerFake;

		private IFieldsMappingBuilder _sut;

		private const int _SOURCE_WORKSPACE_ID = 1;
		private const int _DESTINATION_WORKSPACE_ID = 2;

		[SetUp]
		public void SetUp()
		{
			_objectManagerFake = new Mock<IObjectManager>();
			_fieldManagerFake = new Mock<IFieldManager>();

			var syncServicesMgrFake = new Mock<ISyncServiceManager>();
			syncServicesMgrFake.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_objectManagerFake.Object);
			syncServicesMgrFake.Setup(x => x.CreateProxy<IFieldManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_fieldManagerFake.Object);

			_sut = new FieldsMappingBuilder(_SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID,
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

			SetupIdentifierField(identifierField, _SOURCE_WORKSPACE_ID);
			SetupIdentifierField(identifierField, _DESTINATION_WORKSPACE_ID);

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

			SetupIdentifierField(identifierField, _SOURCE_WORKSPACE_ID);
			SetupIdentifierField(identifierField, _DESTINATION_WORKSPACE_ID);

			// Act
			Action action = () => _sut.WithIdentifier().WithIdentifier();

			// Assert
			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void WithField_ShouldAddFieldToFieldsMapping()
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

		[TestCase(true, false)]
		[TestCase(false, true)]
		public void WithField_ShouldThrowException_WhenOneOfFieldsIsIdentifier(
			bool isSourceIdentifier, bool isDestinationIdentifier)
		{
			// Arrange
			const int sourceFieldId = 100;
			const int destinationFieldId = 200;

			FieldResponse sourceField = new FieldResponse { IsIdentifier = isSourceIdentifier, Name = "Test", ArtifactID = sourceFieldId };
			FieldResponse destinationField = new FieldResponse { IsIdentifier = isDestinationIdentifier, Name = "Test", ArtifactID = destinationFieldId };
			
			SetupField(sourceField, sourceFieldId, _SOURCE_WORKSPACE_ID);
			SetupField(destinationField, destinationFieldId, _DESTINATION_WORKSPACE_ID);

			// Act
			Action action = () => _sut.WithField(sourceFieldId, destinationFieldId);

			// Assert
			action.Should().Throw<ArgumentException>();
		}

		private void SetupIdentifierField(RelativityObject expectedIdentifier, int workspaceId)
		{
			_objectManagerFake.Setup(x =>
					x.QueryAsync(workspaceId, It.Is<QueryRequest>(q => q.IncludeNameInQueryResult == true), 0, 1))
				.ReturnsAsync(new QueryResult
				{
					Objects = new List<RelativityObject>() {expectedIdentifier},
				});
		}

		private void SetupField(FieldResponse expectedField, int fieldId, int workspaceId)
		{
			_fieldManagerFake.Setup(x => x.ReadAsync(workspaceId, fieldId))
				.ReturnsAsync(expectedField);
		}
	}
}
