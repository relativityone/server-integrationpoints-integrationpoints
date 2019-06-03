using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	public sealed class BatchDataReaderBuilderTests
	{
		private List<FieldInfoDto> _getAllFieldsResult;
		private Mock<IFieldManager> _fieldManager;
		private Mock<IFieldValueSanitizer> _fieldValueSanitizer;
		private RelativityObjectSlim[] _batch;
		private RelativityObjectSlim _batchObject;
		private FieldInfoDto _firstDocumentField;
		private FieldInfoDto _secondDocumentField;
		private const int _FIRST_DOCUMENT_FIELD_INDEX_IN_BATCH = 0;
		private const int _FIRST_DOCUMENT_FIELD_INDEX_IN_READER = 0;
		private const int _FIRST_DOCUMENT_FIELD_VALUE = 987;
		private const int _SECOND_DOCUMENT_FIELD_INDEX_IN_BATCH = 1;
		private const int _SECOND_DOCUMENT_FIELD_INDEX_IN_READER = 1;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1234;
		private const string _FIRST_DOCUMENT_FIELD_NAME = "Test Field 1";
		private const string _SECOND_DOCUMENT_FIELD_NAME = "Test Field 2";
		private const string _SECOND_DOCUMENT_FIELD_VALUE = "test";


		[SetUp]
		public void SetUp()
		{
			_firstDocumentField = FieldInfoDto.DocumentField(_FIRST_DOCUMENT_FIELD_NAME, _FIRST_DOCUMENT_FIELD_NAME, false);
			_firstDocumentField.DocumentFieldIndex = _FIRST_DOCUMENT_FIELD_INDEX_IN_BATCH;
			_secondDocumentField = FieldInfoDto.DocumentField(_SECOND_DOCUMENT_FIELD_NAME, _SECOND_DOCUMENT_FIELD_NAME, false);
			_secondDocumentField.DocumentFieldIndex = _SECOND_DOCUMENT_FIELD_INDEX_IN_BATCH;
			_getAllFieldsResult = new List<FieldInfoDto> {_firstDocumentField, _secondDocumentField};
			_batchObject = new RelativityObjectSlim {Values = new List<object> {_FIRST_DOCUMENT_FIELD_VALUE, _SECOND_DOCUMENT_FIELD_VALUE}};
			_batch = new[] {_batchObject};
			_fieldValueSanitizer = new Mock<IFieldValueSanitizer>();
			_fieldValueSanitizer.Setup(s => s.ShouldBeSanitized(It.IsAny<RelativityDataType>())).Returns(false);
			_fieldManager = new Mock<IFieldManager>();
			_fieldManager.Setup(fm => fm.GetAllFieldsAsync(CancellationToken.None)).ReturnsAsync(_getAllFieldsResult);
			_fieldManager.Setup(fm => fm.GetObjectIdentifierFieldAsync(CancellationToken.None)).ReturnsAsync(_secondDocumentField);
		}

		[Test]
		public async Task ItShouldReturnDataReaderWithProperlyOrderedColumnsAndValues()
		{
			// Arrange
			BatchDataReaderBuilder builder = new BatchDataReaderBuilder(_fieldManager.Object, _fieldValueSanitizer.Object);

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeTrue();
			GetColumnCount(result).Should().Be(_getAllFieldsResult.Count);

			result.GetName(_FIRST_DOCUMENT_FIELD_INDEX_IN_READER).Should().Be(_FIRST_DOCUMENT_FIELD_NAME);
			result.GetName(_SECOND_DOCUMENT_FIELD_INDEX_IN_READER).Should().Be(_SECOND_DOCUMENT_FIELD_NAME);
			result[_FIRST_DOCUMENT_FIELD_NAME].Should().Be(_FIRST_DOCUMENT_FIELD_VALUE);
			result[_SECOND_DOCUMENT_FIELD_NAME].Should().Be(_SECOND_DOCUMENT_FIELD_VALUE);
		}
		
		private int GetColumnCount(IDataReader reader)
		{
			const int tmpObjectsTableSize = 10;
			var tmp = new object[tmpObjectsTableSize];
			return reader.GetValues(tmp);
		}

		[Test]
		public async Task ItShouldReturnDataReaderWithProperRowCount()
		{
			// Arrange
			RelativityObjectSlim[] batchWithTwoRows = new[] {_batchObject, _batchObject};
			BatchDataReaderBuilder builder = new BatchDataReaderBuilder(_fieldManager.Object, _fieldValueSanitizer.Object);

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, batchWithTwoRows, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeTrue();
			result.Read().Should().BeTrue();
			result.Read().Should().BeFalse();
		}

		[Test]
		public async Task ItShouldReturnDataReaderWithSanitizedValue()
		{
			// Arrange
			const string valueAfterSanitization = "Value Sanitized!";
			_fieldValueSanitizer.Setup(s => s.ShouldBeSanitized(It.IsAny<RelativityDataType>())).Returns(true);
			_fieldValueSanitizer.Setup(s => s.SanitizeAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _SECOND_DOCUMENT_FIELD_NAME, _SECOND_DOCUMENT_FIELD_VALUE, _firstDocumentField, It.IsAny<object>()))
				.ReturnsAsync(valueAfterSanitization);
			
			BatchDataReaderBuilder builder = new BatchDataReaderBuilder(_fieldManager.Object, _fieldValueSanitizer.Object);

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);
			result.Read();

			// Assert
			result[_FIRST_DOCUMENT_FIELD_INDEX_IN_READER].Should().Be(valueAfterSanitization);
		}

		[Test]
		public async Task ItShouldReturnEmptyDataReaderWhenBatchEmpty()
		{
			// Arrange
			RelativityObjectSlim[] emptyBatch = Array.Empty<RelativityObjectSlim>();
			BatchDataReaderBuilder builder = new BatchDataReaderBuilder(_fieldManager.Object, _fieldValueSanitizer.Object);

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, emptyBatch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeFalse();
		}

		[Test]
		public async Task ItShouldReturnDataReaderWithSpecialColumn()
		{
			// Arrange
			const SpecialFieldType differentSpecialFieldType = SpecialFieldType.FolderPath;
			const SpecialFieldType specialFieldType = SpecialFieldType.SourceWorkspace;
			const string specialFieldName = "Special field";
			Guid specialFieldValue = new Guid("56C1128A-64B7-4F67-A57F-0932CBAE1747");
			FieldInfoDto specialFieldDto = FieldInfoDto.GenericSpecialField(specialFieldType, specialFieldName, specialFieldName);
			
			_getAllFieldsResult.Add(specialFieldDto);

			Mock<ISpecialFieldRowValuesBuilder> specialFieldValueBuilder = new Mock<ISpecialFieldRowValuesBuilder>();
			specialFieldValueBuilder.Setup(b => b.BuildRowValue(specialFieldDto, It.IsAny<RelativityObjectSlim>(), It.IsAny<object>())).Returns(specialFieldValue);
			Dictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> buildersDictionary = new Dictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>
			{
				{specialFieldType, specialFieldValueBuilder.Object},
				{differentSpecialFieldType, Mock.Of<ISpecialFieldRowValuesBuilder>()}
			};
			_fieldManager.Setup(fm => fm.CreateSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<ICollection<int>>())).ReturnsAsync(buildersDictionary);
			BatchDataReaderBuilder builder = new BatchDataReaderBuilder(_fieldManager.Object, _fieldValueSanitizer.Object);

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeTrue();
			result[_FIRST_DOCUMENT_FIELD_NAME].Should().Be(_FIRST_DOCUMENT_FIELD_VALUE);
			result[_SECOND_DOCUMENT_FIELD_NAME].Should().Be(_SECOND_DOCUMENT_FIELD_VALUE);
			result[specialFieldName].Should().Be(specialFieldValue);
		}

		[Test]
		public async Task ItShouldThrowWhenNoSpecialFieldBuilderFound()
		{
			// Arrange
			const SpecialFieldType differentSpecialFieldType = SpecialFieldType.FolderPath;
			const SpecialFieldType specialFieldType = SpecialFieldType.SourceWorkspace;
			const string specialFieldName = "Special field";
			FieldInfoDto specialFieldDto = FieldInfoDto.GenericSpecialField(specialFieldType, specialFieldName, specialFieldName);
			
			_getAllFieldsResult.Add(specialFieldDto);

			Dictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> buildersDictionary = new Dictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>
			{
				{differentSpecialFieldType, Mock.Of<ISpecialFieldRowValuesBuilder>()}
			};
			
			_fieldManager.Setup(fm => fm.CreateSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<ICollection<int>>())).ReturnsAsync(buildersDictionary);
			BatchDataReaderBuilder builder = new BatchDataReaderBuilder(_fieldManager.Object, _fieldValueSanitizer.Object);

			// Act
			Func<Task<IDataReader>> action = () => builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None);

			// Assert
			await action.Should().ThrowAsync<SourceDataReaderException>().ConfigureAwait(false);
		}
	}
}
