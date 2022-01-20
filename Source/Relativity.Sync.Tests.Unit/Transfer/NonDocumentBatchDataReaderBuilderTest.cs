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
    public sealed class NonDocumentBatchDataReaderBuilderTest
    {
		private List<FieldInfoDto> _getAllFieldsResult;
		private Mock<IFieldManager> _fieldManagerMock;
		private Mock<IExportDataSanitizer> _exportDataSanitizerFake;
		private RelativityObjectSlim[] _batch;
		private RelativityObjectSlim _batchObject;
		private FieldInfoDto _firstDocumentField;
		private FieldInfoDto _secondDocumentField;

		private const int _FIRST_RDO_FIELD_INDEX_IN_BATCH = 0;
		private const int _FIRST_RDO_FIELD_INDEX_IN_READER = 0;
		private const string _FIRST_RDO_FIELD_NAME = "Test Field 1";
		private const int _FIRST_RDO_FIELD_VALUE = 987;

		private const int _SECOND_RDO_FIELD_INDEX_IN_BATCH = 1;
		private const int _SECOND_RDO_FIELD_INDEX_IN_READER = 1;
		private const string _SECOND_RDO_FIELD_NAME = "Test Field 2";
		private const string _SECOND_RDO_FIELD_VALUE = "test";

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1234;

		[SetUp]
		public void SetUp()
		{
			_firstDocumentField = FieldInfoDto.DocumentField(_FIRST_RDO_FIELD_NAME, _FIRST_RDO_FIELD_NAME, false);
			_firstDocumentField.DocumentFieldIndex = _FIRST_RDO_FIELD_INDEX_IN_BATCH;
			_secondDocumentField = FieldInfoDto.DocumentField(_SECOND_RDO_FIELD_NAME, _SECOND_RDO_FIELD_NAME, false);
			_secondDocumentField.DocumentFieldIndex = _SECOND_RDO_FIELD_INDEX_IN_BATCH;
			_getAllFieldsResult = new List<FieldInfoDto> { _firstDocumentField, _secondDocumentField };
			_batchObject = new RelativityObjectSlim { Values = new List<object> { _FIRST_RDO_FIELD_VALUE, _SECOND_RDO_FIELD_VALUE } };
			_batch = new[] { _batchObject };
			_exportDataSanitizerFake = new Mock<IExportDataSanitizer>();
			_exportDataSanitizerFake.Setup(s => s.ShouldSanitize(It.IsAny<RelativityDataType>())).Returns(false);
			_fieldManagerMock = new Mock<IFieldManager>();
			_fieldManagerMock.Setup(fm => fm.GetMappedFieldNonDocumentLinklessAsync(CancellationToken.None)).ReturnsAsync(_getAllFieldsResult);
			_fieldManagerMock.Setup(fm => fm.GetObjectIdentifierFieldAsync(CancellationToken.None)).ReturnsAsync(_secondDocumentField);
		}

		[Test]
		public async Task BuildAsync_ShouldInvokeGetNonDocumentAllFieldsAsync()
		{
			// Arrange
			var builder = new NonDocumentBatchDataReaderBuilder(_fieldManagerMock.Object, _exportDataSanitizerFake.Object, new EmptyLogger());

			// Act
			IDataReader dataReader = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_fieldManagerMock.Verify(x => x.GetImageAllFieldsAsync(It.IsAny<CancellationToken>()), Times.Never);
			_fieldManagerMock.Verify(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()), Times.Never);
			_fieldManagerMock.Verify(x => x.GetMappedFieldNonDocumentLinklessAsync(It.IsAny<CancellationToken>()));
			dataReader.Should().BeOfType<NonDocumentBatchDataReader>();
		}

		[Test]
		public async Task BuildAsync_ShouldReturnDataReaderWithProperlyOrderedColumnsAndValues()
		{
			// Arrange
			NonDocumentBatchDataReaderBuilder builder = new NonDocumentBatchDataReaderBuilder(_fieldManagerMock.Object, _exportDataSanitizerFake.Object, new EmptyLogger());

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeTrue();

			result.GetName(_FIRST_RDO_FIELD_INDEX_IN_READER).Should().Be(_FIRST_RDO_FIELD_NAME);
			result.GetName(_SECOND_RDO_FIELD_INDEX_IN_READER).Should().Be(_SECOND_RDO_FIELD_NAME);

			result[_FIRST_RDO_FIELD_NAME].Should().Be(_FIRST_RDO_FIELD_VALUE.ToString(CultureInfo.InvariantCulture));
			result[_SECOND_RDO_FIELD_NAME].Should().Be(_SECOND_RDO_FIELD_VALUE);
		}

		[Test]
		public async Task BuildAsync_ShouldReturnDataReaderWithProperRowCount()
		{
			// Arrange
			RelativityObjectSlim[] batchWithTwoRows = { _batchObject, _batchObject };
			NonDocumentBatchDataReaderBuilder builder = new NonDocumentBatchDataReaderBuilder(_fieldManagerMock.Object, _exportDataSanitizerFake.Object, new EmptyLogger());

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, batchWithTwoRows, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeTrue();
			result.Read().Should().BeTrue();
			result.Read().Should().BeFalse();
		}

		[Test]
		public async Task BuildAsync_ShouldReturnNonDocumentBatchDataReader_WhichCanBeCanceled()
		{
			// Arrange
			NonDocumentBatchDataReaderBuilder builder = new NonDocumentBatchDataReaderBuilder(_fieldManagerMock.Object, _exportDataSanitizerFake.Object, new EmptyLogger());

			// Act
			IBatchDataReader nonDocBatchDataReader = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			nonDocBatchDataReader.CanCancel.Should().BeTrue();
		}

		[Test]
		public async Task BuildAsync_ShouldReturnDataReaderWithSanitizedValue()
		{
			// Arrange
			const string valueAfterSanitization = "Value Sanitized!";
			_exportDataSanitizerFake.Setup(s => s.ShouldSanitize(It.IsAny<RelativityDataType>())).Returns(true);
			_exportDataSanitizerFake.Setup(s => s.SanitizeAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _SECOND_RDO_FIELD_NAME, _SECOND_RDO_FIELD_VALUE, _firstDocumentField, It.IsAny<object>()))
				.ReturnsAsync(valueAfterSanitization);

			NonDocumentBatchDataReaderBuilder builder = new NonDocumentBatchDataReaderBuilder(_fieldManagerMock.Object, _exportDataSanitizerFake.Object, new EmptyLogger());

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);
			result.Read();

			// Assert
			result[_FIRST_RDO_FIELD_INDEX_IN_READER].Should().Be(valueAfterSanitization);
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

			NonDocumentBatchDataReaderBuilder builder = new NonDocumentBatchDataReaderBuilder(_fieldManagerMock.Object, _exportDataSanitizerFake.Object, new EmptyLogger());

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);
			result.Read();

			// Assert
			result[_FIRST_RDO_FIELD_INDEX_IN_READER].Should().Be(valueAfterSanitization);
			result[_SECOND_RDO_FIELD_INDEX_IN_BATCH].Should().Be(_SECOND_RDO_FIELD_VALUE);
		}

		[Test]
		public async Task BuildAsync_ShouldReturnEmptyDataReader_WhenBatchEmpty()
		{
			// Arrange
			RelativityObjectSlim[] emptyBatch = Array.Empty<RelativityObjectSlim>();
			NonDocumentBatchDataReaderBuilder builder = new NonDocumentBatchDataReaderBuilder(_fieldManagerMock.Object, _exportDataSanitizerFake.Object, new EmptyLogger());

			// Act
			IDataReader result = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, emptyBatch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Read().Should().BeFalse();
		}

	}
}
