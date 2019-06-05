using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	public class FieldManagerTests
	{
		private Mock<ISynchronizationConfiguration> _configuration;
		private Mock<IDocumentFieldRepository> _documentFieldRepository;
		private FieldManager _instance;
		private int[] _documentArtifactIds;
		private Mock<ISpecialFieldRowValuesBuilder> _rowValueBuilder1;
		private Mock<ISpecialFieldRowValuesBuilder> _rowValueBuilder2;
		private const RelativityDataType _NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE = RelativityDataType.WholeNumber;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 123;
		private const string _DOCUMENT_IDENTIFIER_FIELD_NAME = "Control Number [Identifier]";
		private const string _DOCUMENT_SPECIAL_FIELD_NAME = "DocumentSpecialField";
		private const string _NON_DOCUMENT_SPECIAL_FIELD_1_NAME = "NonDocumentSpecialField1";
		private const string _NON_DOCUMENT_SPECIAL_FIELD_2_NAME = "NonDocumentSpecialField2";
		private const string _MAPPED_FIELD_1_SOURCE_NAME = "MappedField1Source";
		private const string _MAPPED_FIELD_2_SOURCE_NAME = "MappedField2Source";
		private const string _MAPPED_FIELD_1_DESTINATION_NAME = "MappedField1Destination";
		private const string _MAPPED_FIELD_2_DESTINATION_NAME = "MappedField2Destination";
		private const int _FIRST_DOCUMENT = 1;
		private const int _SECOND_DOCUMENT = 2;
		private const int _THIRD_DOCUMENT = 3;

		[SetUp]
		public void SetUp()
		{
			_documentArtifactIds = new[] {_FIRST_DOCUMENT, _SECOND_DOCUMENT, _THIRD_DOCUMENT};

			FieldInfoDto documentIdentifierField = FieldInfoDto.DocumentField(_DOCUMENT_IDENTIFIER_FIELD_NAME, _DOCUMENT_IDENTIFIER_FIELD_NAME, true);
			FieldInfoDto documentSpecialField = FieldInfoDto.DocumentField(_DOCUMENT_SPECIAL_FIELD_NAME, _DOCUMENT_SPECIAL_FIELD_NAME, false);
			FieldInfoDto nonDocumentSpecialField1 = FieldInfoDto.GenericSpecialField(SpecialFieldType.None, _NON_DOCUMENT_SPECIAL_FIELD_1_NAME, _NON_DOCUMENT_SPECIAL_FIELD_1_NAME);
			FieldInfoDto nonDocumentSpecialField2 = FieldInfoDto.GenericSpecialField(SpecialFieldType.None, _NON_DOCUMENT_SPECIAL_FIELD_2_NAME, _NON_DOCUMENT_SPECIAL_FIELD_2_NAME);
			var mappedField1 = new FieldMap {SourceField = new FieldEntry {DisplayName = _MAPPED_FIELD_1_SOURCE_NAME}, DestinationField = new FieldEntry{ DisplayName = _MAPPED_FIELD_1_DESTINATION_NAME}};
			var mappedField2 = new FieldMap {SourceField = new FieldEntry {DisplayName = _MAPPED_FIELD_2_SOURCE_NAME}, DestinationField = new FieldEntry {DisplayName = _MAPPED_FIELD_2_DESTINATION_NAME}};
			
			_rowValueBuilder1 = new Mock<ISpecialFieldRowValuesBuilder>();
			var builder1 = new Mock<ISpecialFieldBuilder>();
			builder1.Setup(b => b.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _documentArtifactIds)).ReturnsAsync(_rowValueBuilder1.Object);
			builder1.Setup(b => b.BuildColumns()).Returns(new[] {nonDocumentSpecialField1, documentSpecialField, documentIdentifierField});

			_rowValueBuilder2 = new Mock<ISpecialFieldRowValuesBuilder>();
			var builder2 = new Mock<ISpecialFieldBuilder>();
			builder2.Setup(b => b.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _documentArtifactIds)).ReturnsAsync(_rowValueBuilder2.Object);
			builder2.Setup(b => b.BuildColumns()).Returns(new[] {nonDocumentSpecialField2});

			_configuration = new Mock<ISynchronizationConfiguration>();
			_configuration.Setup(c => c.FieldMappings).Returns(new[] { mappedField1, mappedField2 });
			_configuration.Setup(c => c.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);
			_documentFieldRepository = new Mock<IDocumentFieldRepository>();
			_documentFieldRepository.Setup(r => r.GetRelativityDataTypesForFieldsByFieldNameAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<ICollection<string>>(), CancellationToken.None))
				.ReturnsAsync<int, ICollection<string>, CancellationToken, IDocumentFieldRepository, IDictionary<string, RelativityDataType>>(
					(workspaceId, fieldNames, token) => fieldNames.ToDictionary(f => f, _ => _NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE));

			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, new[] {builder1.Object, builder2.Object});
		}

		[Test]
		public async Task ItShouldReturnDocumentIdentifierField()
		{
			// Act
			FieldInfoDto result = await _instance.GetObjectIdentifierFieldAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().NotBeNull();
			result.IsIdentifier.Should().BeTrue();
			result.IsDocumentField.Should().BeTrue();
			result.SourceFieldName.Should().Be(_DOCUMENT_IDENTIFIER_FIELD_NAME);
		}

		[Test]
		public void ItShouldReturnSpecialFields()
		{
			// Act
			IList<FieldInfoDto> result = _instance.GetSpecialFields().ToList();

			// Assert
			result.Should().Contain(f => f.SourceFieldName == _DOCUMENT_SPECIAL_FIELD_NAME);
			result.Should().Contain(f => f.SourceFieldName == _NON_DOCUMENT_SPECIAL_FIELD_1_NAME);
			result.Should().Contain(f => f.SourceFieldName == _NON_DOCUMENT_SPECIAL_FIELD_2_NAME);
			result.Should().NotContain(f => f.SourceFieldName == _MAPPED_FIELD_1_SOURCE_NAME);
			result.Should().NotContain(f => f.SourceFieldName == _MAPPED_FIELD_2_SOURCE_NAME);
		}

		[Test]
		public async Task ItShouldReturnDocumentFields()
		{
			// Act
			IList<FieldInfoDto> result = await _instance.GetDocumentFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().Contain(f => f.SourceFieldName == _DOCUMENT_SPECIAL_FIELD_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.DestinationFieldName == _DOCUMENT_SPECIAL_FIELD_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.SourceFieldName == _MAPPED_FIELD_1_SOURCE_NAME && f.DestinationFieldName == _MAPPED_FIELD_1_DESTINATION_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.SourceFieldName == _MAPPED_FIELD_2_SOURCE_NAME && f.DestinationFieldName == _MAPPED_FIELD_2_DESTINATION_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().NotContain(f => f.DestinationFieldName == _NON_DOCUMENT_SPECIAL_FIELD_1_NAME);
			result.Should().NotContain(f => f.DestinationFieldName == _NON_DOCUMENT_SPECIAL_FIELD_2_NAME);
			result.Select(f => f.DocumentFieldIndex).Should().ContainInOrder(Enumerable.Range(0, result.Count));
		}

		[Test]
		public async Task ItShouldReturnAllFields()
		{
			// Act
			IReadOnlyList<FieldInfoDto> result = await _instance.GetAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().Contain(f => f.SourceFieldName == _DOCUMENT_SPECIAL_FIELD_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.DestinationFieldName == _DOCUMENT_SPECIAL_FIELD_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);

			result.Should().Contain(f => f.SourceFieldName == _MAPPED_FIELD_1_SOURCE_NAME && f.DestinationFieldName == _MAPPED_FIELD_1_DESTINATION_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.SourceFieldName == _MAPPED_FIELD_1_SOURCE_NAME && f.DestinationFieldName == _MAPPED_FIELD_1_DESTINATION_NAME).Which.RelativityDataType.Should()
				.Be(_NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE);
			
			result.Should().Contain(f => f.SourceFieldName == _MAPPED_FIELD_2_SOURCE_NAME && f.DestinationFieldName == _MAPPED_FIELD_2_DESTINATION_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.SourceFieldName == _MAPPED_FIELD_2_SOURCE_NAME && f.DestinationFieldName == _MAPPED_FIELD_2_DESTINATION_NAME).Which.RelativityDataType.Should()
				.Be(_NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE);

			result.Should().Contain(f => f.DestinationFieldName == _NON_DOCUMENT_SPECIAL_FIELD_1_NAME).Which.DocumentFieldIndex.Should().Be(-1);

			result.Should().Contain(f => f.DestinationFieldName == _NON_DOCUMENT_SPECIAL_FIELD_2_NAME).Which.DocumentFieldIndex.Should().Be(-1);
		}

		[Test]
		public async Task ItShouldReturnSpecialFieldBuilders()
		{
			// Arrange 
			const SpecialFieldType rowValueBuilder1FieldType = SpecialFieldType.FolderPath;
			const SpecialFieldType rowValueBuilder2FieldType = SpecialFieldType.SourceWorkspace;

			_rowValueBuilder1.Setup(rb => rb.AllowedSpecialFieldTypes).Returns(new[] {rowValueBuilder1FieldType});
			_rowValueBuilder2.Setup(rb => rb.AllowedSpecialFieldTypes).Returns(new[] {rowValueBuilder2FieldType});
			
			// Act
			IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> result = await _instance.CreateSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _documentArtifactIds)
				.ConfigureAwait(false);

			// Assert
			result.Should().ContainKey(rowValueBuilder1FieldType).WhichValue.Should().Be(_rowValueBuilder1.Object);
			result.Should().ContainKey(rowValueBuilder2FieldType).WhichValue.Should().Be(_rowValueBuilder2.Object);
		}

		[Test]
		public async Task ItShouldThrowWhenRegisteringForTheSameSpecialFieldType()
		{
			// Arrange 
			const SpecialFieldType rowValueBuilder1FieldType = SpecialFieldType.NativeFileSize;
			const SpecialFieldType rowValueBuilder2FieldType = SpecialFieldType.NativeFileSize;

			_rowValueBuilder1.Setup(rb => rb.AllowedSpecialFieldTypes).Returns(new[] {rowValueBuilder1FieldType});
			_rowValueBuilder2.Setup(rb => rb.AllowedSpecialFieldTypes).Returns(new[] {rowValueBuilder2FieldType});
			
			// Act
			Func<Task<IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>>> action =
				() => _instance.CreateSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _documentArtifactIds);

			// Assert
			await action.Should().ThrowAsync<ArgumentException>().ConfigureAwait(false);
		}

		[Test]
		public async Task ItShouldNotThrowOnGetAllFieldsWhenNoSpecialFieldBuildersFound()
		{
			// Arrange
			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			// Act
			Func<Task<IReadOnlyList<FieldInfoDto>>> action = () => _instance.GetAllFieldsAsync(CancellationToken.None);

			// Assert
			await action.Should().NotThrowAsync().ConfigureAwait(false);
		}

		[Test]
		public async Task ItShouldNoFieldsIfThereAreNoSpecialFieldsAndNoMappedDocumentFields()
		{
			// Arrange
			_configuration = new Mock<ISynchronizationConfiguration>();
			_configuration.Setup(c => c.FieldMappings).Returns(new List<FieldMap>(0));
			_configuration.Setup(c => c.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);

			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			// Act
			IReadOnlyList<FieldInfoDto> result = await _instance.GetAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEmpty();
		}

		[Test]
		public void ItShouldNotThrowOnGetSpecialFieldsWhenNoSpecialFieldBuildersFound()
		{
			// Arrange
			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			// Act
			Func<IEnumerable<FieldInfoDto>> action = () => _instance.GetSpecialFields();

			// Assert
			action.Should().NotThrow();
		}

		[Test]
		public async Task ItShouldNotThrowOnGetDocumentFieldsWhenNoSpecialFieldBuildersFound()
		{
			// Arrange
			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			// Act
			Func<Task<IList<FieldInfoDto>>> action = () => _instance.GetDocumentFieldsAsync(CancellationToken.None);

			// Assert
			await action.Should().NotThrowAsync().ConfigureAwait(false);
		}
	}
}
