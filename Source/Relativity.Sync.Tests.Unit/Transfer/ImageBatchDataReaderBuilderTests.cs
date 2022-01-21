using System.Collections.Generic;
using System.Data;
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
	public class ImageBatchDataReaderBuilderTests
	{
		private List<FieldInfoDto> _getAllFieldsResult;
		private Mock<IFieldManager> _fieldManagerMock;
		private Mock<IExportDataSanitizer> _exportDataSanitizerFake;
		private RelativityObjectSlim[] _batch;
		private RelativityObjectSlim _batchObject;
		private FieldInfoDto _firstDocumentField;
		private FieldInfoDto _secondDocumentField;

		private const int _FIRST_DOCUMENT_FIELD_INDEX_IN_BATCH = 0;
		private const int _FIRST_DOCUMENT_FIELD_VALUE = 987;
		private const int _SECOND_DOCUMENT_FIELD_INDEX_IN_BATCH = 1;
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
			_getAllFieldsResult = new List<FieldInfoDto> { _firstDocumentField, _secondDocumentField };
			_batchObject = new RelativityObjectSlim { Values = new List<object> { _FIRST_DOCUMENT_FIELD_VALUE, _SECOND_DOCUMENT_FIELD_VALUE } };
			_batch = new[] { _batchObject };
			_exportDataSanitizerFake = new Mock<IExportDataSanitizer>();
			_exportDataSanitizerFake.Setup(s => s.ShouldSanitize(It.IsAny<RelativityDataType>())).Returns(false);
			_fieldManagerMock = new Mock<IFieldManager>();
			_fieldManagerMock.Setup(fm => fm.GetImageAllFieldsAsync(CancellationToken.None)).ReturnsAsync(_getAllFieldsResult);
			_fieldManagerMock.Setup(fm => fm.GetObjectIdentifierFieldAsync(CancellationToken.None)).ReturnsAsync(_secondDocumentField);
		}
		
		[Test]
		public async Task BuildAsync_ShouldInvokeGetImageAllFieldsAsync()
		{
			// Arrange
			var builder = new ImageBatchDataReaderBuilder(_fieldManagerMock.Object, _exportDataSanitizerFake.Object, new EmptyLogger());

			// Act
			IDataReader dataReader = await builder.BuildAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, _batch, CancellationToken.None).ConfigureAwait(false);

			// Assert
			_fieldManagerMock.Verify(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>()), Times.Never);
			_fieldManagerMock.Verify(x => x.GetMappedFieldNonDocumentWithoutLinksAsync(It.IsAny<CancellationToken>()), Times.Never);
			_fieldManagerMock.Verify(x => x.GetImageAllFieldsAsync(It.IsAny<CancellationToken>()));
			dataReader.Should().BeOfType<ImageBatchDataReader>();
		}
	}
}