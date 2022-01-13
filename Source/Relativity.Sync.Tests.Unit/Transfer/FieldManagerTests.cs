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
		private Mock<INativeSpecialFieldBuilder> _folderPathSpecialFieldBuilderFake;
		private Mock<INativeSpecialFieldBuilder> _emptySpecialFieldBuilder;

		private Mock<INativeSpecialFieldRowValuesBuilder> _nativeSpecialFieldRowValuesBuilderFake;
		private Mock<INativeSpecialFieldRowValuesBuilder> _folderPathSpecialFieldRowValuesBuilderFake;
		private Mock<INativeSpecialFieldRowValuesBuilder> _emptySpecialFieldRowValuesBuilderFake;

		private Mock<IImageSpecialFieldRowValuesBuilder> _imageSpecialFieldRowValuesBuilderFake;

		private Mock<IFieldConfiguration> _configuration;
		private Mock<IObjectFieldTypeRepository> _documentFieldRepository;

		private FieldManager _sut;

		#region Test Data

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 123;

		private const int _RDO_ARTIFACT_TYPE_ID = 420;

		private const string _FOLDER_PATH_FIELD_NAME = "Folder Path Field";

		private static readonly FieldInfoDto _DOCUMENT_IDENTIFIER_FIELD =
			new FieldInfoDto(SpecialFieldType.None, "Control Number Source [Identifier]", "Control Number Destination [Identifier]", true, true)
			{
				RelativityDataType = RelativityDataType.FixedLengthText
			};

		private static readonly FieldInfoDto _DOCUMENT_MAPPED_FIELD =
			FieldInfoDto.DocumentField("Mapped Source Field", "Mapped Destination Field", false);

		private static readonly FieldInfoDto _FOLDER_PATH_STRUCTURE_FIELD =
			FieldInfoDto.FolderPathFieldFromDocumentField(_FOLDER_PATH_FIELD_NAME);

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
			FieldInfoDto.RelativityNativeTypeField(),
			FieldInfoDto.SupportedByViewerField()
		};

		private readonly FieldInfoDto[] _FOLDER_PATH_SPECIAL_FIELDS = new FieldInfoDto[]
		{
			FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure()
		};

		private readonly FieldInfoDto[] _IMAGE_SPECIAL_FIELDS = new FieldInfoDto[]
		{
			FieldInfoDto.ImageFileNameField(),
			FieldInfoDto.ImageFileLocationField()
		};

		private static readonly FieldMap[] _MAPPED_FIELDS = new FieldMap[]
		{
			CreateFieldMap(_DOCUMENT_IDENTIFIER_FIELD, true),
			CreateFieldMap(_DOCUMENT_MAPPED_FIELD)
		};

		#endregion

		[SetUp]
		public void SetUp()
		{
			_configuration = new Mock<IFieldConfiguration>();
			_configuration.Setup(c => c.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);
			_configuration.Setup(c => c.GetFieldMappings()).Returns(_MAPPED_FIELDS);
			_configuration.SetupGet(c => c.RdoArtifactTypeId).Returns(_RDO_ARTIFACT_TYPE_ID);

			_documentFieldRepository = new Mock<IObjectFieldTypeRepository>();
			_documentFieldRepository.Setup(r => r.GetRelativityDataTypesForFieldsByFieldNameAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _RDO_ARTIFACT_TYPE_ID, It.IsAny<ICollection<string>>(), CancellationToken.None))
				.ReturnsAsync(_FIELD_TYPES);

			var nativeSpecialFieldBuilders = SetupNativeSpecialFieldBuilders();
			var imageSpecialFieldBuilders = SetupImageSpecialFieldBuilders();

			_sut = new FieldManager(_configuration.Object, _documentFieldRepository.Object, nativeSpecialFieldBuilders, imageSpecialFieldBuilders);
		}

		[Test]
		public async Task GetObjectIdentifierFieldAsync_ShouldReturnDocumentIdentifierField()
		{
			// Act
			FieldInfoDto result = await _sut.GetObjectIdentifierFieldAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEquivalentTo(_DOCUMENT_IDENTIFIER_FIELD, 
				options => options.ComparingByMembers<FieldInfoDto>().Excluding(x => x.DocumentFieldIndex));
		}

		[Test]
		public void GetNativeSpecialFields_ShouldReturnSpecialFields()
		{
			// Arrange
			var expectedFields = _NATIVE_SPECIAL_FIELDS.Concat(_FOLDER_PATH_SPECIAL_FIELDS);

			// Act
			IList<FieldInfoDto> result = _sut.GetNativeSpecialFields().ToList();

			// Assert
			result.Should().BeEquivalentTo(expectedFields);
		}

		[Test]
		public void GetImageSpecialFields_ShouldReturnSpecialFields()
		{
			// Act
			IList<FieldInfoDto> result = _sut.GetImageSpecialFields().ToList();

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
				_DOCUMENT_MAPPED_FIELD,
				FieldInfoDto.RelativityNativeTypeField(),
				FieldInfoDto.SupportedByViewerField()
			};

			// Act
			IList<FieldInfoDto> result = await _sut.GetDocumentTypeFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEquivalentTo(expectedFields,
				options => options.ComparingByMembers<FieldInfoDto>().Excluding(x => x.DocumentFieldIndex));
		}

		[Test]
		public async Task GetDocumentTypeFields_ShouldReturnDocumentFieldsWithFolderPathField_WhenDestinationFolderStructureIsSet()
		{
			// Arrange
			var expectedFields = new List<FieldInfoDto>
			{
				_DOCUMENT_IDENTIFIER_FIELD,
				_DOCUMENT_MAPPED_FIELD,
				FieldInfoDto.RelativityNativeTypeField(),
				FieldInfoDto.SupportedByViewerField(),
				_FOLDER_PATH_STRUCTURE_FIELD
			};

			_folderPathSpecialFieldBuilderFake.Setup(x => x.BuildColumns()).Returns(new FieldInfoDto[]
			{
				FieldInfoDto.FolderPathFieldFromDocumentField(_FOLDER_PATH_STRUCTURE_FIELD.SourceFieldName)
			});

			_configuration.Setup(c => c.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);

			// Act
			IList<FieldInfoDto> result = await _sut.GetDocumentTypeFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEquivalentTo(expectedFields,
				options => options.ComparingByMembers<FieldInfoDto>().Excluding(x => x.DocumentFieldIndex));
		}

		[Test]
		public async Task GetNativeAllFieldsAsync_ShouldReturnAllFields()
		{
			// Arrange
			var expectedFields = new List<FieldInfoDto>
			{
				_DOCUMENT_IDENTIFIER_FIELD,
				_DOCUMENT_MAPPED_FIELD,
				FieldInfoDto.NativeFileFilenameField(),
				FieldInfoDto.NativeFileLocationField(),
				FieldInfoDto.NativeFileSizeField(),
				FieldInfoDto.RelativityNativeTypeField(),
				FieldInfoDto.SupportedByViewerField(),
				FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure()
			};

			// Act
			IReadOnlyList<FieldInfoDto> result = await _sut.GetNativeAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEquivalentTo(expectedFields,
				options => options.ComparingByMembers<FieldInfoDto>().Excluding(x => x.DocumentFieldIndex));
		}

		[Test]
		public async Task GetImageAllFieldsAsync_ShouldReturnAllFields()
		{
			// Arrange
			var expectedFields = new List<FieldInfoDto>
			{
				_DOCUMENT_IDENTIFIER_FIELD,
				_DOCUMENT_MAPPED_FIELD,
				FieldInfoDto.ImageFileLocationField(),
				FieldInfoDto.ImageFileNameField()
			};

			// Act
			IReadOnlyList<FieldInfoDto> result = await _sut.GetImageAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEquivalentTo(expectedFields,
				options => options.ComparingByMembers<FieldInfoDto>().Excluding(x => x.DocumentFieldIndex));
		}

		[Test]
		public async Task CreateNativeSpecialFieldRowValueBuildersAsync_ShouldReturnSpecialFieldBuilders()
		{
			// Arrange 
			const SpecialFieldType nativeFileLocationType = SpecialFieldType.NativeFileLocation;
			const SpecialFieldType folderPathType = SpecialFieldType.FolderPath;

			_nativeSpecialFieldRowValuesBuilderFake.Setup(rb => rb.AllowedSpecialFieldTypes).Returns(new[] { nativeFileLocationType });
			_folderPathSpecialFieldRowValuesBuilderFake.Setup(rb => rb.AllowedSpecialFieldTypes).Returns(new[] { folderPathType });

			// Act
			IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder> result =
				await _sut.CreateNativeSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, Array.Empty<int>())
				.ConfigureAwait(false);

			// Assert
			result.Should().ContainKey(nativeFileLocationType).WhichValue.Should().Be(_nativeSpecialFieldRowValuesBuilderFake.Object);
			result.Should().ContainKey(folderPathType).WhichValue.Should().Be(_folderPathSpecialFieldRowValuesBuilderFake.Object);
		}

		[Test]
		public async Task CreateNativeSpecialFieldRowValueBuildersAsync_ShouldThrow_WhenRegisteringForTheSameSpecialFieldType()
		{
			// Arrange 
			const SpecialFieldType nativeFileLocationType = SpecialFieldType.NativeFileLocation;

			_nativeSpecialFieldRowValuesBuilderFake.Setup(rb => rb.AllowedSpecialFieldTypes)
				.Returns(new[] { nativeFileLocationType });
			_folderPathSpecialFieldRowValuesBuilderFake.Setup(rb => rb.AllowedSpecialFieldTypes)
				.Returns(new[] { nativeFileLocationType });

			// Act
			Func<Task<IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder>>> action =
				() => _sut.CreateNativeSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, Array.Empty<int>());

			// Assert
			await action.Should().ThrowAsync<ArgumentException>().ConfigureAwait(false);
		}

		[Test]
		public async Task CreateImageSpecialFieldRowValueBuildersAsync_ShouldReturnSpecialFieldBuilders()
		{
			// Arrange 
			const SpecialFieldType imageFileLocationType = SpecialFieldType.ImageFileLocation;
			const SpecialFieldType imageFileNameType = SpecialFieldType.ImageFileName;

			_imageSpecialFieldRowValuesBuilderFake.Setup(rb => rb.AllowedSpecialFieldTypes)
				.Returns(new[] { imageFileLocationType, imageFileNameType });

			// Act
			IDictionary<SpecialFieldType, IImageSpecialFieldRowValuesBuilder> result =
				await _sut.CreateImageSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, Array.Empty<int>())
				.ConfigureAwait(false);

			// Assert
			result.Should().ContainKey(imageFileNameType).WhichValue.Should().Be(_imageSpecialFieldRowValuesBuilderFake.Object);
			result.Should().ContainKey(imageFileLocationType).WhichValue.Should().Be(_imageSpecialFieldRowValuesBuilderFake.Object);
		}

		[Test]
		public async Task GetNativeAllFieldsAsync_ShouldNotThrow_WhenNoSpecialFieldBuildersFound()
		{
			// Arrange
			_sut = new FieldManager(_configuration.Object, _documentFieldRepository.Object,
				Enumerable.Empty<INativeSpecialFieldBuilder>(), Enumerable.Empty<IImageSpecialFieldBuilder>());

			// Act
			Func<Task<IReadOnlyList<FieldInfoDto>>> action = () => _sut.GetNativeAllFieldsAsync(CancellationToken.None);

			// Assert
			await action.Should().NotThrowAsync().ConfigureAwait(false);
		}

		[Test]
		public async Task GetNativeAllFieldsAsync_ShouldMergeDocumentSpecialFieldWithMappedField()
		{
			// Arrange
			FieldInfoDto sampleDocumentField = FieldInfoDto.DocumentField(_FOLDER_PATH_FIELD_NAME, "Sample Field", false);

			var expectedFields = new List<FieldInfoDto>
			{
				_DOCUMENT_IDENTIFIER_FIELD,
				_DOCUMENT_MAPPED_FIELD,
				sampleDocumentField,
				FieldInfoDto.NativeFileFilenameField(),
				FieldInfoDto.NativeFileLocationField(),
				FieldInfoDto.NativeFileSizeField(),
				FieldInfoDto.RelativityNativeTypeField(),
				FieldInfoDto.SupportedByViewerField(),
				_FOLDER_PATH_STRUCTURE_FIELD
			};

			var mappedFields = new FieldMap[]
			{
				CreateFieldMap(_DOCUMENT_IDENTIFIER_FIELD, true),
				CreateFieldMap(_DOCUMENT_MAPPED_FIELD),
				CreateFieldMap(sampleDocumentField)
			};

			_folderPathSpecialFieldBuilderFake.Setup(x => x.BuildColumns()).Returns(new FieldInfoDto[]
			{
				FieldInfoDto.FolderPathFieldFromDocumentField(_FOLDER_PATH_STRUCTURE_FIELD.SourceFieldName)
			});

			_configuration.Setup(c => c.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);
			_configuration.Setup(c => c.GetFieldMappings()).Returns(mappedFields);

			// Act
			IReadOnlyList<FieldInfoDto> result = await _sut.GetNativeAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEquivalentTo(expectedFields,
				options => options.ComparingByMembers<FieldInfoDto>().Excluding(x => x.DocumentFieldIndex));
		}

		[Test]
		public async Task GetNativeAllFieldsAsync_ShouldThrow_WhenSpecialFieldAndDocumentFieldHasSameDestinationNameAndDifferentSourceNames()
		{
			// Arrange
			FieldInfoDto specialField = new FieldInfoDto(SpecialFieldType.NativeFileSize, "Source Field - 1", "DESTINATION FIELD", false, true);
			FieldInfoDto mappedField = FieldInfoDto.DocumentField("SOURCE FIELD - 2", "Destination Field", false);

			SetupCustomSpecialFieldWithMappedField(specialField, mappedField);

			// Act
			Func<Task<IReadOnlyList<FieldInfoDto>>> action = () => _sut.GetNativeAllFieldsAsync(CancellationToken.None);

			// Assert
			(await action.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false))
				.Which.Message.Should().StartWith("Special field destination name conflicts with mapped field destination name.");
		}

		[Test]
		public async Task GetNativeAllFieldsAsync_ShouldThrow_WhenSpecialFieldIsNotDocumentFieldAndHasSameSourceAndDestinationFields()
		{
			// Arrange
			FieldInfoDto nonDocumentSpecialField = new FieldInfoDto(SpecialFieldType.NativeFileSize, "Source Field", "DESTINATION FIELD", false, false);
			FieldInfoDto mappedField = FieldInfoDto.DocumentField("SOURCE FIELD", "Destination Field", false);

			SetupCustomSpecialFieldWithMappedField(nonDocumentSpecialField, mappedField);

			// Act
			Func<Task<IReadOnlyList<FieldInfoDto>>> action = () => _sut.GetNativeAllFieldsAsync(CancellationToken.None);

			// Assert
			(await action.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false))
				.Which.Message.Should().StartWith("Special field destination name conflicts with mapped field destination name.");
		}

		[Test]
		public async Task GetNativeAllFieldsAsync_ShouldNotThrow_WhenSpecialFieldIsDocumentFieldAndHasSameSourceAndDestinationFields()
		{
			// Arrange
			FieldInfoDto specialField = FieldInfoDto.DocumentField("Source Field", "DESTINATION FIELD", false);
			FieldInfoDto mappedField = FieldInfoDto.DocumentField("SOURCE FIELD", "Destination Field", false);

			SetupCustomSpecialFieldWithMappedField(specialField, mappedField);

			// Act
			Func<Task<IReadOnlyList<FieldInfoDto>>> action = () => _sut.GetNativeAllFieldsAsync(CancellationToken.None);

			// Assert
			await action.Should().NotThrowAsync().ConfigureAwait(false);
		}

		[Test]
		public async Task GetNativeAllFieldsAsync_ShouldNotReturnFields_WhenThereAreNoSpecialFieldsAndNoMappedDocumentFields()
		{
			// Arrange
			_configuration = new Mock<IFieldConfiguration>();
			_configuration.Setup(c => c.GetFieldMappings()).Returns(new List<FieldMap>(0));
			_configuration.Setup(c => c.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);

			_sut = new FieldManager(_configuration.Object, _documentFieldRepository.Object,
				Enumerable.Empty<INativeSpecialFieldBuilder>(), Enumerable.Empty<IImageSpecialFieldBuilder>());

			// Act
			IReadOnlyList<FieldInfoDto> result = await _sut.GetNativeAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().BeEmpty();
		}

		[Test]
		public void GetNativeSpecialFields_ShouldNotThrow_WhenNoSpecialFieldBuildersFound()
		{
			// Arrange
			_sut = new FieldManager(_configuration.Object, _documentFieldRepository.Object,
				Enumerable.Empty<INativeSpecialFieldBuilder>(), Enumerable.Empty<IImageSpecialFieldBuilder>());

			// Act
			Func<IEnumerable<FieldInfoDto>> action = () => _sut.GetNativeSpecialFields();

			// Assert
			action.Should().NotThrow();
		}

		[Test]
		public void GetImageSpecialFields_ShouldNotThrow_WhenNoSpecialFieldBuildersFound()
		{
			// Arrange
			_sut = new FieldManager(_configuration.Object, _documentFieldRepository.Object,
				Enumerable.Empty<INativeSpecialFieldBuilder>(), Enumerable.Empty<IImageSpecialFieldBuilder>());

			// Act
			Func<IEnumerable<FieldInfoDto>> action = () => _sut.GetImageSpecialFields();

			// Assert
			action.Should().NotThrow();
		}

		[Test]
		public async Task GetDocumentTypeFieldsAsync_ShouldNotThrowOnGetDocumentFields_WhenNoSpecialFieldBuildersFound()
		{
			// Arrange
			_sut = new FieldManager(_configuration.Object, _documentFieldRepository.Object,
				Enumerable.Empty<INativeSpecialFieldBuilder>(), Enumerable.Empty<IImageSpecialFieldBuilder>());

			// Act
			Func<Task<IList<FieldInfoDto>>> action = () => _sut.GetDocumentTypeFieldsAsync(CancellationToken.None);

			// Assert
			await action.Should().NotThrowAsync().ConfigureAwait(false);
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

		private void SetupCustomSpecialFieldWithMappedField(FieldInfoDto specialField, FieldInfoDto mappedField)
		{
			var mappedFields = new FieldMap[]
			{
				CreateFieldMap(mappedField)
			};

			_emptySpecialFieldBuilder.Setup(b => b.BuildColumns()).Returns(new[] { specialField });

			_configuration.Setup(c => c.GetFieldMappings()).Returns(mappedFields);

			_documentFieldRepository.Setup(r => r.GetRelativityDataTypesForFieldsByFieldNameAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _RDO_ARTIFACT_TYPE_ID, It.IsAny<ICollection<string>>(), CancellationToken.None))
				.ReturnsAsync(new Dictionary<string, RelativityDataType>
				{
					{ mappedField.SourceFieldName, RelativityDataType.FixedLengthText }
				});
		}

		private INativeSpecialFieldBuilder[] SetupNativeSpecialFieldBuilders()
		{
			_nativeSpecialFieldRowValuesBuilderFake = new Mock<INativeSpecialFieldRowValuesBuilder>();
			_folderPathSpecialFieldRowValuesBuilderFake = new Mock<INativeSpecialFieldRowValuesBuilder>();
			_emptySpecialFieldRowValuesBuilderFake = new Mock<INativeSpecialFieldRowValuesBuilder>();

			var nativeSpecialFieldBuilder = new Mock<INativeSpecialFieldBuilder>();
			nativeSpecialFieldBuilder.Setup(b => b.BuildColumns()).Returns(_NATIVE_SPECIAL_FIELDS);
			nativeSpecialFieldBuilder.Setup(b => b.GetRowValuesBuilderAsync(It.IsAny<int>(), It.IsAny<int[]>()))
				.ReturnsAsync(_nativeSpecialFieldRowValuesBuilderFake.Object);

			_folderPathSpecialFieldBuilderFake = new Mock<INativeSpecialFieldBuilder>();
			_folderPathSpecialFieldBuilderFake.Setup(b => b.BuildColumns()).Returns(_FOLDER_PATH_SPECIAL_FIELDS);
			_folderPathSpecialFieldBuilderFake.Setup(b => b.GetRowValuesBuilderAsync(It.IsAny<int>(), It.IsAny<int[]>()))
				.ReturnsAsync(_folderPathSpecialFieldRowValuesBuilderFake.Object);

			_emptySpecialFieldBuilder = new Mock<INativeSpecialFieldBuilder>();
			_emptySpecialFieldBuilder.Setup(b => b.BuildColumns()).Returns(Enumerable.Empty<FieldInfoDto>());
			_emptySpecialFieldBuilder.Setup(b => b.GetRowValuesBuilderAsync(It.IsAny<int>(), It.IsAny<int[]>()))
				.ReturnsAsync(_emptySpecialFieldRowValuesBuilderFake.Object);

			return new INativeSpecialFieldBuilder[]
			{
				nativeSpecialFieldBuilder.Object,
				_folderPathSpecialFieldBuilderFake.Object,
				_emptySpecialFieldBuilder.Object
			};
		}

		private IImageSpecialFieldBuilder[] SetupImageSpecialFieldBuilders()
		{
			_imageSpecialFieldRowValuesBuilderFake = new Mock<IImageSpecialFieldRowValuesBuilder>();

			var imageSpecialFieldBuilder = new Mock<IImageSpecialFieldBuilder>();
			imageSpecialFieldBuilder.Setup(b => b.BuildColumns()).Returns(_IMAGE_SPECIAL_FIELDS);
			imageSpecialFieldBuilder.Setup(b => b.GetRowValuesBuilderAsync(It.IsAny<int>(), It.IsAny<int[]>()))
				.ReturnsAsync(_imageSpecialFieldRowValuesBuilderFake.Object);

			return new IImageSpecialFieldBuilder[]
			{
				imageSpecialFieldBuilder.Object
			};
		}
	}
}
