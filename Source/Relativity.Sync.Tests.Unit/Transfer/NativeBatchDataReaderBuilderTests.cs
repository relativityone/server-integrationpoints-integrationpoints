using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	public sealed class NativeBatchDataReaderBuilderTests
	{
		private List<FieldInfoDto> _getAllFieldsResult;
		private Mock<IFieldManager> _fieldManagerMock;
		private Mock<IExportDataSanitizer> _exportDataSanitizerFake;
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
			_exportDataSanitizerFake = new Mock<IExportDataSanitizer>();
			_exportDataSanitizerFake.Setup(s => s.ShouldSanitize(It.IsAny<RelativityDataType>())).Returns(false);
			_fieldManagerMock = new Mock<IFieldManager>();
			_fieldManagerMock.Setup(fm => fm.GetNativeAllFieldsAsync(CancellationToken.None)).ReturnsAsync(_getAllFieldsResult);
			_fieldManagerMock.Setup(fm => fm.GetObjectIdentifierFieldAsync(CancellationToken.None)).ReturnsAsync(_secondDocumentField);
		}

		[Test]
		public async Task BuildAsync_ShouldReturnDataReaderWithProperlyOrderedColumnsAndValues()
		{
			// Arrange
			NativeBatchDataReaderBuilder builder = PrepareSut();

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeTrue();
			GetColumnCount(result).Should().Be(_getAllFieldsResult.Count);

			result.GetName(_FIRST_DOCUMENT_FIELD_INDEX_IN_READER).Should().Be(_FIRST_DOCUMENT_FIELD_NAME);
			result.GetName(_SECOND_DOCUMENT_FIELD_INDEX_IN_READER).Should().Be(_SECOND_DOCUMENT_FIELD_NAME);

			result[_FIRST_DOCUMENT_FIELD_NAME].Should().Be(_FIRST_DOCUMENT_FIELD_VALUE.ToString(CultureInfo.InvariantCulture));
			result[_SECOND_DOCUMENT_FIELD_NAME].Should().Be(_SECOND_DOCUMENT_FIELD_VALUE);
		}
		
		private int GetColumnCount(IDataReader reader)
		{
			const int tmpObjectsTableSize = 10;
			var tmp = new object[tmpObjectsTableSize];
			return reader.GetValues(tmp);
		}

		[Test]
		public async Task BuildAsync_ShouldReturnDataReaderWithProperRowCount()
		{
			// Arrange
			RelativityObjectSlim[] batchWithTwoRows = {_batchObject, _batchObject};
			NativeBatchDataReaderBuilder builder = PrepareSut();

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, batchWithTwoRows, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeTrue();
			result.Read().Should().BeTrue();
			result.Read().Should().BeFalse();
		}

		[Test]
		public async Task BuildAsync_ShouldReturnDataReaderWithSanitizedValue()
		{
			// Arrange
			const string valueAfterSanitization = "Value Sanitized!";
			_exportDataSanitizerFake.Setup(s => s.ShouldSanitize(It.IsAny<RelativityDataType>())).Returns(true);
			_exportDataSanitizerFake.Setup(s => s.SanitizeAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _SECOND_DOCUMENT_FIELD_NAME, _SECOND_DOCUMENT_FIELD_VALUE, _firstDocumentField, It.IsAny<object>()))
				.ReturnsAsync(valueAfterSanitization);

			NativeBatchDataReaderBuilder builder = PrepareSut();

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);
			result.Read();

			// Assert
			result[_FIRST_DOCUMENT_FIELD_INDEX_IN_READER].Should().Be(valueAfterSanitization);
		}

		[Test]
		public async Task BuildAsync_ShouldCheckThatFieldShouldBeSanitized()
		{
			// Arrange
			_firstDocumentField.RelativityDataType = RelativityDataType.Currency;
			_secondDocumentField.RelativityDataType = RelativityDataType.Date;

			_exportDataSanitizerFake.Setup(s => s.ShouldSanitize(_firstDocumentField.RelativityDataType)).Returns(true);
			_exportDataSanitizerFake.Setup(s => s.ShouldSanitize(_secondDocumentField.RelativityDataType)).Returns(false);

			const string valueAfterSanitization = "Value Sanitized!";
			_exportDataSanitizerFake.Setup(s => s.SanitizeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FieldInfoDto>(), It.IsAny<object>()))
				.ReturnsAsync(valueAfterSanitization);

			NativeBatchDataReaderBuilder builder = PrepareSut();

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);
			result.Read();

			// Assert
			result[_FIRST_DOCUMENT_FIELD_INDEX_IN_READER].Should().Be(valueAfterSanitization);
			result[_SECOND_DOCUMENT_FIELD_INDEX_IN_BATCH].Should().Be(_SECOND_DOCUMENT_FIELD_VALUE);
		}

		[Test]
		public async Task BuildAsync_ShouldReturnEmptyDataReader_WhenBatchEmpty()
		{
			// Arrange
			RelativityObjectSlim[] emptyBatch = Array.Empty<RelativityObjectSlim>();
			NativeBatchDataReaderBuilder builder = PrepareSut();

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, emptyBatch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeFalse();
		}

		[Test]
		public async Task BuildAsync_ShouldReturnDataReaderWithSpecialColumn()
		{
			// Arrange
			const SpecialFieldType differentSpecialFieldType = SpecialFieldType.FolderPath;
			const SpecialFieldType specialFieldType = SpecialFieldType.NativeFileLocation;
			const string specialFieldName = "Special field";
			var specialFieldValue = new Guid("56C1128A-64B7-4F67-A57F-0932CBAE1747");
			FieldInfoDto specialFieldDto = FieldInfoDto.GenericSpecialField(specialFieldType, specialFieldName, specialFieldName);
			
			_getAllFieldsResult.Add(specialFieldDto);

			var specialFieldValueBuilder = new Mock<INativeSpecialFieldRowValuesBuilder>();
			specialFieldValueBuilder.Setup(b => b.BuildRowValue(specialFieldDto, It.IsAny<RelativityObjectSlim>(), It.IsAny<object>())).Returns(specialFieldValue);
			var buildersDictionary = new Dictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder>
			{
				{specialFieldType, specialFieldValueBuilder.Object},
				{differentSpecialFieldType, Mock.Of<INativeSpecialFieldRowValuesBuilder>()}
			};
			_fieldManagerMock.Setup(fm => fm.CreateNativeSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<int[]>())).ReturnsAsync(buildersDictionary);
			NativeBatchDataReaderBuilder builder = PrepareSut();

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeTrue();
			result[_FIRST_DOCUMENT_FIELD_NAME].Should().Be(_FIRST_DOCUMENT_FIELD_VALUE.ToString(CultureInfo.InvariantCulture));
			result[_SECOND_DOCUMENT_FIELD_NAME].Should().Be(_SECOND_DOCUMENT_FIELD_VALUE);
			result[specialFieldName].Should().Be(specialFieldValue.ToString());
		}

		[Test]
		public async Task BuildAsync_ShouldRaiseItemLevelError_WhenExportFieldSanitizationFails()
		{
			// Arrange
			_firstDocumentField.RelativityDataType = RelativityDataType.User;

			_exportDataSanitizerFake.Setup(s => s.ShouldSanitize(_firstDocumentField.RelativityDataType)).Returns(true);
			_exportDataSanitizerFake.Setup(s => s.SanitizeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FieldInfoDto>(), It.IsAny<object>()))
				.Throws<InvalidExportFieldValueException>();

			NativeBatchDataReaderBuilder builder = PrepareSut();
			Mock<Action<string, string>> itemLevelErrorHandlerMock = new Mock<Action<string, string>>();
			builder.ItemLevelErrorHandler = itemLevelErrorHandlerMock.Object;

			// Act
			IDataReader reader = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);
			reader.Read();

			// Assert
			itemLevelErrorHandlerMock.Verify(x => x.Invoke(It.IsAny<string>(), It.IsAny<string>()));
		}

		[Test]
		public async Task BuildAsync_ShouldInvokeGetNativeAllFieldsAsync()
		{
			// Arrange
			NativeBatchDataReaderBuilder builder = PrepareSut();

			// Act
			await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_fieldManagerMock.Verify(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()));
			_fieldManagerMock.Verify(x => x.GetImageAllFieldsAsync(It.IsAny<CancellationToken>()), Times.Never);
		}

		[Test]
		public async Task Read_ShouldThrow_WhenNoSpecialFieldBuilderFound()
		{
			// Arrange
			const SpecialFieldType differentSpecialFieldType = SpecialFieldType.FolderPath;
			const SpecialFieldType specialFieldType = SpecialFieldType.SupportedByViewer;
			const string specialFieldName = "Special field";
			FieldInfoDto specialFieldDto = FieldInfoDto.GenericSpecialField(specialFieldType, specialFieldName, specialFieldName);
			
			_getAllFieldsResult.Add(specialFieldDto);

			var buildersDictionary = new Dictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder>
			{
				{differentSpecialFieldType, Mock.Of<INativeSpecialFieldRowValuesBuilder>()}
			};
			
			_fieldManagerMock.Setup(fm => fm.CreateNativeSpecialFieldRowValueBuildersAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<int[]>())).ReturnsAsync(buildersDictionary);
			NativeBatchDataReaderBuilder builder = PrepareSut();
			IDataReader reader = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);

			// Act
			Func<bool> action = () => reader.Read();

			// Assert
			action.Should().Throw<SourceDataReaderException>();
		}

		private NativeBatchDataReaderBuilder PrepareSut()
		{
			return new NativeBatchDataReaderBuilder(_fieldManagerMock.Object, _exportDataSanitizerFake.Object, new EmptyLogger());
		}
	}
}
