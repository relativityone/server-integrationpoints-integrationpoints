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
		private Mock<ISpecialFieldBuilder> _folderPathSpecialFieldBuilderMock;


		private Mock<IFieldConfiguration> _configuration;
		private Mock<IDocumentFieldRepository> _documentFieldRepository;
		private FieldManager _instance;
		private int[] _documentArtifactIds;
		private const RelativityDataType _NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE = RelativityDataType.WholeNumber;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 123;
		private const string _DOCUMENT_IDENTIFIER_FIELD_NAME = "Control Number [Identifier]";
		private const string _DOCUMENT_SPECIAL_FIELD_NAME = "DocumentSpecialField";
		private const string _NON_DOCUMENT_SPECIAL_FIELD_1_NAME = "NonDocumentSpecialField1";
		private const string _NON_DOCUMENT_SPECIAL_FIELD_2_NAME = "NonDocumentSpecialField2";
		private const string _MAPPED_FIELD_1_SOURCE_NAME = "MappedField1Source";
		private const string _MAPPED_FIELD_1_SOURCE_NAME_LOWER_CASE = "mappedfield1source";
		private const string _MAPPED_FIELD_2_SOURCE_NAME = "MappedField2Source";
		private const string _MAPPED_FIELD_1_DESTINATION_NAME = "MappedField1Destination";
		private const string _MAPPED_FIELD_1_DESTINATION_NAME_LOWER_CASE = "mappedfield1destination";
		private const string _MAPPED_FIELD_2_DESTINATION_NAME = "MappedField2Destination";
		private const int _FIRST_DOCUMENT = 1;
		private const int _SECOND_DOCUMENT = 2;
		private const int _THIRD_DOCUMENT = 3;

		private static readonly FieldInfoDto _DOCUMENT_IDENTIFIER_FIELD = 
			new FieldInfoDto(SpecialFieldType.None, "Control Number Source [Identifier]", "Control Number Destination [Identifier]", true, true)
			{
				RelativityDataType = RelativityDataType.FixedLengthText
			};

		private static readonly FieldInfoDto _DOCUMENT_MAPPED_FIELD =
			FieldInfoDto.DocumentField("Mapped Source Field", "Mapped Destination Field", false);

		private static readonly FieldInfoDto _FOLDER_PATH_STRUCTURE_FIELD =
			FieldInfoDto.FolderPathFieldFromDocumentField("Folder Path Field");


		private readonly IDictionary<string, RelativityDataType> _FIELD_TYPES = new Dictionary<string, RelativityDataType>
		{
			{ _DOCUMENT_IDENTIFIER_FIELD.SourceFieldName, _DOCUMENT_IDENTIFIER_FIELD.RelativityDataType },
			{ _DOCUMENT_MAPPED_FIELD.SourceFieldName, _DOCUMENT_MAPPED_FIELD.RelativityDataType },
			{ _FOLDER_PATH_STRUCTURE_FIELD.SourceFieldName, RelativityDataType.FixedLengthText }
		};

		private readonly FieldInfoDto[] _NATIVE_SPECIAL_FIELDS = new FieldInfoDto[]
		{
			FieldInfoDto.NativeFileFilenameField(),
			FieldInfoDto.NativeFileLocationField(),
			FieldInfoDto.NativeFileSizeField(),
			FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure()
		};

		private readonly FieldInfoDto[] _IMAGE_SPECIAL_FIELDS = new FieldInfoDto[]
		{
			FieldInfoDto.ImageFileNameField(),
			FieldInfoDto.ImageFileLocationField()
		};

		[SetUp]
		public void SetUp()
		{
			_documentArtifactIds = new[] {_FIRST_DOCUMENT, _SECOND_DOCUMENT, _THIRD_DOCUMENT};


			_configuration = new Mock<IFieldConfiguration>();
			_configuration.Setup(c => c.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);

			_documentFieldRepository = new Mock<IDocumentFieldRepository>();
			_documentFieldRepository.Setup(r => r.GetRelativityDataTypesForFieldsByFieldNameAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<ICollection<string>>(), CancellationToken.None))
				.ReturnsAsync(_FIELD_TYPES);

			var specialFieldBuilders = SetupSpecialFieldBuilders();

			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, specialFieldBuilders);
		}

		[Test]
		public async Task GetObjectIdentifierFieldAsync_ShouldReturnDocumentIdentifierField()
		{
			// Arrange
			var mappedFields = new FieldMap[]
			{
				CreateFieldMap(_DOCUMENT_IDENTIFIER_FIELD, true),
				CreateFieldMap(_DOCUMENT_MAPPED_FIELD)
			};

			_configuration.Setup(c => c.GetFieldMappings()).Returns(mappedFields);

			// Act
			FieldInfoDto result = await _instance.GetObjectIdentifierFieldAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().Be(_DOCUMENT_IDENTIFIER_FIELD);
		}

		[Test]
		public void GetNativeSpecialFields_ShouldReturnSpecialFields()
		{
			// Act
			IList<FieldInfoDto> result = _instance.GetNativeSpecialFields().ToList();

			// Assert
			result.Should().BeEquivalentTo(_NATIVE_SPECIAL_FIELDS);
		}

		[Test]
		public void GetImageSpecialFields_ShouldReturnSpecialFields()
		{
			// Act
			IList<FieldInfoDto> result = _instance.GetImageSpecialFields().ToList();

			// Assert
			result.Should().BeEquivalentTo(_IMAGE_SPECIAL_FIELDS);
		}

		[Test]
		public async Task GetDocumentTypeFields_ShouldReturnDocumentFields()
		{
			// Arrange
			var expectedFields = new List<FieldInfoDto>
			{
				_DOCUMENT_IDENTIFIER_FIELD,
				_DOCUMENT_MAPPED_FIELD
			};

			var mappedFields = new FieldMap[]
			{
				CreateFieldMap(_DOCUMENT_IDENTIFIER_FIELD, true),
				CreateFieldMap(_DOCUMENT_MAPPED_FIELD)
			};

			_configuration.Setup(c => c.GetFieldMappings()).Returns(mappedFields);

			// Act
			IList<FieldInfoDto> result = await _instance.GetDocumentTypeFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEquivalentTo(expectedFields);
		}

		[Test]
		public async Task GetDocumentTypeFields_ShouldReturnDocumentFieldsWithFolderPathField_WhenDestinationFolderStructureIsSet()
		{
			// Arrange
			var expectedFields = new List<FieldInfoDto>
			{
				_DOCUMENT_IDENTIFIER_FIELD,
				_DOCUMENT_MAPPED_FIELD,
				_FOLDER_PATH_STRUCTURE_FIELD
			};

			var mappedFields = new FieldMap[]
			{
				CreateFieldMap(_DOCUMENT_IDENTIFIER_FIELD, true),
				CreateFieldMap(_DOCUMENT_MAPPED_FIELD),
				CreateSpecialFieldMap(_FOLDER_PATH_STRUCTURE_FIELD, FieldMapType.FolderPathInformation)
			};

			_configuration.Setup(c => c.GetFieldMappings()).Returns(mappedFields);
			_configuration.Setup(c => c.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);

			// Act
			IList<FieldInfoDto> result = await _instance.GetDocumentTypeFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEquivalentTo(expectedFields);
		}

		[Test]
		public async Task ItShouldReturnAllFields()
		{
			// Act
			IReadOnlyList<FieldInfoDto> result = await _instance.GetNativeAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().Contain(f => f.SourceFieldName == _DOCUMENT_SPECIAL_FIELD_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.DestinationFieldName == _DOCUMENT_SPECIAL_FIELD_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);

			result.Should().Contain(f => f.SourceFieldName == _MAPPED_FIELD_1_SOURCE_NAME && f.DestinationFieldName == _MAPPED_FIELD_1_DESTINATION_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.SourceFieldName == _MAPPED_FIELD_1_SOURCE_NAME && f.DestinationFieldName == _MAPPED_FIELD_1_DESTINATION_NAME).Which.RelativityDataType.Should()
				.Be(_NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE);
			
			result.Should().Contain(f => f.SourceFieldName == _MAPPED_FIELD_2_SOURCE_NAME && f.DestinationFieldName == _MAPPED_FIELD_2_DESTINATION_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.SourceFieldName == _MAPPED_FIELD_2_SOURCE_NAME && f.DestinationFieldName == _MAPPED_FIELD_2_DESTINATION_NAME).Which.RelativityDataType.Should()
				.Be(_NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE);
		}

		//[Test]
		//public async Task ItShouldReturnSpecialFieldBuilders()
		//{
		//	// Arrange 
		//	const SpecialFieldType rowValueBuilder1FieldType = SpecialFieldType.FolderPath;
		//	const SpecialFieldType rowValueBuilder2FieldType = SpecialFieldType.NativeFileLocation;

		//	_rowValueBuilder1.Setup(rb => rb.AllowedSpecialFieldTypes).Returns(new[] {rowValueBuilder1FieldType});
		//	_rowValueBuilder2.Setup(rb => rb.AllowedSpecialFieldTypes).Returns(new[] {rowValueBuilder2FieldType});
			
		//	// Act
		//	IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> result = await _instance.CreateNativeSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _documentArtifactIds)
		//		.ConfigureAwait(false);

		//	// Assert
		//	result.Should().ContainKey(rowValueBuilder1FieldType).WhichValue.Should().Be(_rowValueBuilder1.Object);
		//	result.Should().ContainKey(rowValueBuilder2FieldType).WhichValue.Should().Be(_rowValueBuilder2.Object);
		//}

		//[Test]
		//public async Task ItShouldThrowWhenRegisteringForTheSameSpecialFieldType()
		//{
		//	// Arrange 
		//	const SpecialFieldType rowValueBuilder1FieldType = SpecialFieldType.NativeFileSize;
		//	const SpecialFieldType rowValueBuilder2FieldType = SpecialFieldType.NativeFileSize;

		//	_rowValueBuilder1.Setup(rb => rb.AllowedSpecialFieldTypes).Returns(new[] {rowValueBuilder1FieldType});
		//	_rowValueBuilder2.Setup(rb => rb.AllowedSpecialFieldTypes).Returns(new[] {rowValueBuilder2FieldType});
			
		//	// Act
		//	Func<Task<IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>>> action =
		//		() => _instance.CreateNativeSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _documentArtifactIds);

		//	// Assert
		//	await action.Should().ThrowAsync<ArgumentException>().ConfigureAwait(false);
		//}

		[Test]
		public async Task ItShouldNotThrowOnGetAllFieldsWhenNoSpecialFieldBuildersFound()
		{
			// Arrange
			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			// Act
			Func<Task<IReadOnlyList<FieldInfoDto>>> action = () => _instance.GetNativeAllFieldsAsync(CancellationToken.None);

			// Assert
			await action.Should().NotThrowAsync().ConfigureAwait(false);
		}

		private static IEnumerable<TestCaseData> SpecialFieldAndDocumentFieldHasSameDestinationAndSourceNamesTestCases()
		{
			yield return new TestCaseData(_MAPPED_FIELD_1_SOURCE_NAME, _MAPPED_FIELD_1_DESTINATION_NAME, _MAPPED_FIELD_1_SOURCE_NAME, _MAPPED_FIELD_1_DESTINATION_NAME);
			yield return new TestCaseData(_MAPPED_FIELD_1_SOURCE_NAME_LOWER_CASE, _MAPPED_FIELD_1_DESTINATION_NAME_LOWER_CASE, _MAPPED_FIELD_1_SOURCE_NAME, _MAPPED_FIELD_1_DESTINATION_NAME);
		}

		[TestCaseSource(nameof(SpecialFieldAndDocumentFieldHasSameDestinationAndSourceNamesTestCases))]
		public async Task ItShouldMergeDocumentSpecialFieldWithMappedField(string mappedFieldSourceName, string mappedFieldDestinationName, string specialFieldSourceName, string specialFieldDestinationName)
		{
			// Arrange
			const bool isSpecialDocumentField = true;
			const SpecialFieldType specialFieldType = SpecialFieldType.FolderPath;

			MockFieldMappingsToReturnOneMapping(mappedFieldSourceName, mappedFieldDestinationName);

			FieldInfoDto specialField = new FieldInfoDto(specialFieldType, specialFieldSourceName, specialFieldDestinationName, false, isSpecialDocumentField);
			Mock<ISpecialFieldBuilder> builder = CreateSpecialFieldBuilderMockWithBuildColumnsReturning(new[] { specialField });

			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, new[] { builder.Object });

			// Act
			IReadOnlyList<FieldInfoDto> result = await _instance.GetNativeAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			FieldInfoDto returnedField = result.Single();
			returnedField.SourceFieldName.Should().Be(mappedFieldSourceName);
			returnedField.DestinationFieldName.Should().Be(mappedFieldDestinationName);
			returnedField.IsDocumentField.Should().Be(isSpecialDocumentField);
			returnedField.SpecialFieldType.Should().Be(SpecialFieldType.FolderPath);
		}

		[Test]
		public async Task ItShouldAddSpecialFieldAndDocumentMappedField()
		{
			// Arrange
			const int expectedFieldCount = 2;
			const bool isSpecialDocumentField = true;

			MockFieldMappingsToReturnOneMapping(_MAPPED_FIELD_1_SOURCE_NAME, _MAPPED_FIELD_1_DESTINATION_NAME);

			FieldInfoDto specialField = new FieldInfoDto(SpecialFieldType.FolderPath, _MAPPED_FIELD_1_SOURCE_NAME, _DOCUMENT_SPECIAL_FIELD_NAME, false, isSpecialDocumentField);
			Mock<ISpecialFieldBuilder> builder = CreateSpecialFieldBuilderMockWithBuildColumnsReturning(new[] { specialField });

			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, new[] { builder.Object });

			// Act
			IReadOnlyList<FieldInfoDto> result = await _instance.GetNativeAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().HaveCount(expectedFieldCount);
			result.Should().Contain(f =>
				f.SourceFieldName == _MAPPED_FIELD_1_SOURCE_NAME &&
				f.DestinationFieldName == _MAPPED_FIELD_1_DESTINATION_NAME &&
				f.IsDocumentField &&
				f.SpecialFieldType == SpecialFieldType.None);
			result.Should().Contain(specialField);
		}

		private static IEnumerable<TestCaseData> SpecialFieldAndDocumentFieldHasSameDestinationNameAndDifferentSourceNamesTestCases()
		{
			yield return new TestCaseData(_MAPPED_FIELD_1_SOURCE_NAME, _MAPPED_FIELD_1_DESTINATION_NAME, _DOCUMENT_SPECIAL_FIELD_NAME, _MAPPED_FIELD_1_DESTINATION_NAME);
			yield return new TestCaseData(_MAPPED_FIELD_1_SOURCE_NAME_LOWER_CASE, _MAPPED_FIELD_1_DESTINATION_NAME_LOWER_CASE, _DOCUMENT_SPECIAL_FIELD_NAME, _MAPPED_FIELD_1_DESTINATION_NAME);
		}

		[TestCaseSource(nameof(SpecialFieldAndDocumentFieldHasSameDestinationNameAndDifferentSourceNamesTestCases))]
		public async Task ItShouldThrowWhenSpecialFieldAndDocumentFieldHasSameDestinationNameAndDifferentSourceNames(string mappedFieldSourceName, string mappedFieldDestinationName, string specialFieldSourceName, string specialFieldDestinationName)
		{
			// Arrange
			MockFieldMappingsToReturnOneMapping(mappedFieldSourceName, mappedFieldDestinationName);

			FieldInfoDto specialField = FieldInfoDto.GenericSpecialField(SpecialFieldType.None, specialFieldSourceName, specialFieldDestinationName);
			Mock<ISpecialFieldBuilder> builder = CreateSpecialFieldBuilderMockWithBuildColumnsReturning(new[] {specialField});

			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, new[] { builder.Object });

			// Act
			Func<Task<IReadOnlyList<FieldInfoDto>>> action = () => _instance.GetNativeAllFieldsAsync(CancellationToken.None);

			// Assert
			(await action.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false)).Which.Message.Should().StartWith("Special field destination name conflicts with mapped field destination name.");
		}

		[TestCaseSource(nameof(SpecialFieldAndDocumentFieldHasSameDestinationAndSourceNamesTestCases))]
		public async Task ItShouldThrowWhenSpecialFieldIsNotDocumentFieldAndHasSameSourceAndDestinationFields(string mappedFieldSourceName, string mappedFieldDestinationName, string specialFieldSourceName, string specialFieldDestinationName)
		{
			// Arrange
			const bool isSpecialDocumentField = false;

			MockFieldMappingsToReturnOneMapping(mappedFieldSourceName, mappedFieldDestinationName);

			FieldInfoDto specialField = new FieldInfoDto(SpecialFieldType.None, specialFieldSourceName, specialFieldDestinationName, false, isSpecialDocumentField);
			Mock<ISpecialFieldBuilder> builder = CreateSpecialFieldBuilderMockWithBuildColumnsReturning(new[] {specialField});

			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, new[] { builder.Object });

			// Act
			Func<Task<IReadOnlyList<FieldInfoDto>>> action = () => _instance.GetNativeAllFieldsAsync(CancellationToken.None);

			// Assert
			(await action.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false)).Which.Message.Should().StartWith("Special field destination name conflicts with mapped field destination name.");
		}

		[TestCaseSource(nameof(SpecialFieldAndDocumentFieldHasSameDestinationAndSourceNamesTestCases))]
		public async Task ItShouldNotThrowWhenSpecialFieldIsDocumentFieldAndHasSameSourceAndDestinationFields(string mappedFieldSourceName, string mappedFieldDestinationName, string specialFieldSourceName, string specialFieldDestinationName)
		{
			// Arrange
			const bool isSpecialDocumentField = true;

			MockFieldMappingsToReturnOneMapping(mappedFieldSourceName, mappedFieldDestinationName);

			FieldInfoDto specialField = new FieldInfoDto(SpecialFieldType.None, specialFieldSourceName, specialFieldDestinationName, false, isSpecialDocumentField);
			Mock<ISpecialFieldBuilder> builder = CreateSpecialFieldBuilderMockWithBuildColumnsReturning(new[] {specialField});

			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, new[] { builder.Object });

			// Act
			Func<Task<IReadOnlyList<FieldInfoDto>>> action = () => _instance.GetNativeAllFieldsAsync(CancellationToken.None);

			// Assert
			await action.Should().NotThrowAsync().ConfigureAwait(false);
		}

		[Test]
		public async Task ItShouldNoFieldsIfThereAreNoSpecialFieldsAndNoMappedDocumentFields()
		{
			// Arrange
			_configuration = new Mock<IFieldConfiguration>();
			_configuration.Setup(c => c.GetFieldMappings()).Returns(new List<FieldMap>(0));
			_configuration.Setup(c => c.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);

			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			// Act
			IReadOnlyList<FieldInfoDto> result = await _instance.GetNativeAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEmpty();
		}

		[Test]
		public void ItShouldNotThrowOnGetSpecialFieldsWhenNoSpecialFieldBuildersFound()
		{
			// Arrange
			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			// Act
			Func<IEnumerable<FieldInfoDto>> action = () => _instance.GetNativeSpecialFields();

			// Assert
			action.Should().NotThrow();
		}

		[Test]
		public async Task ItShouldNotThrowOnGetDocumentFieldsWhenNoSpecialFieldBuildersFound()
		{
			// Arrange
			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			// Act
			Func<Task<IList<FieldInfoDto>>> action = () => _instance.GetDocumentTypeFieldsAsync(CancellationToken.None);

			// Assert
			await action.Should().NotThrowAsync().ConfigureAwait(false);
		}

		private void MockFieldMappingsToReturnOneMapping(string sourceFieldName, string destinationFieldName)
		{
			FieldMap mapping = CreateFieldMapping(sourceFieldName, destinationFieldName);
			_configuration.Setup(c => c.GetFieldMappings()).Returns(new[] { mapping });
		}

		private static FieldMap CreateFieldMapping(string sourceFieldName, string destinationFieldName)
		{
			return new FieldMap
			{
				SourceField = new FieldEntry { DisplayName = sourceFieldName },
				DestinationField = new FieldEntry { DisplayName = destinationFieldName }
			};
		}

		private static Mock<ISpecialFieldBuilder> CreateSpecialFieldBuilderMockWithBuildColumnsReturning(IEnumerable<FieldInfoDto> fieldsToReturn)
		{
			Mock<ISpecialFieldBuilder> builder = new Mock<ISpecialFieldBuilder>();
			builder.Setup(b => b.BuildColumns()).Returns(fieldsToReturn);
			return builder;
		}

		private static FieldMap CreateFieldMap(FieldInfoDto fieldInfo, bool isIdentifier = false)
			=> new FieldMap
			{
				SourceField = new FieldEntry
				{
					DisplayName = fieldInfo.SourceFieldName,
					IsIdentifier = fieldInfo.IsIdentifier
				},
				DestinationField = new FieldEntry
				{
					DisplayName = fieldInfo.DestinationFieldName,
					IsIdentifier = fieldInfo.IsIdentifier
				},
				FieldMapType = isIdentifier ? FieldMapType.Identifier : FieldMapType.None
			};

		private static FieldMap CreateSpecialFieldMap(FieldInfoDto fieldInfo, FieldMapType mapType)
			=> new FieldMap
			{
				SourceField = new FieldEntry
				{
					DisplayName = fieldInfo.SourceFieldName,
					IsIdentifier = fieldInfo.IsIdentifier
				},
				DestinationField = new FieldEntry(),
				FieldMapType = mapType
			};

		private ISpecialFieldBuilder[] SetupSpecialFieldBuilders()
		{
			var nativeSpecialFieldBuilder = new Mock<ISpecialFieldBuilder>();
			nativeSpecialFieldBuilder.Setup(b => b.BuildColumns()).Returns(_NATIVE_SPECIAL_FIELDS);

			var imageSpecialFieldBuilder = new Mock<ISpecialFieldBuilder>();
			imageSpecialFieldBuilder.Setup(b => b.BuildColumns()).Returns(_IMAGE_SPECIAL_FIELDS);

			_folderPathSpecialFieldBuilderMock = new Mock<ISpecialFieldBuilder>();
			_folderPathSpecialFieldBuilderMock.Setup(b => b.BuildColumns()).Returns(_IMAGE_SPECIAL_FIELDS);

			return new ISpecialFieldBuilder[] 
			{ 
				nativeSpecialFieldBuilder.Object,
				imageSpecialFieldBuilder.Object
			};
		}
	}
}
