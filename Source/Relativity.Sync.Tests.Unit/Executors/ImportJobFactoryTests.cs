using System;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class ImportJobFactoryTests
	{
		private Mock<IBatchProgressHandlerFactory> _batchProgressHandlerFactory;
		private Mock<ISourceWorkspaceDataReader> _dataReader;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;

		private Mock<IBatch> _batch;

		private ISyncLog _logger;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_batchProgressHandlerFactory = new Mock<IBatchProgressHandlerFactory>();
			_dataReader = new Mock<ISourceWorkspaceDataReader>();
			_jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();

			_logger = new EmptyLogger();

			_batch = new Mock<IBatch>(MockBehavior.Loose);
		}

		[Test]
		public async Task CreateImportJobGoldFlowTest()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			configuration.SetupGet(x => x.ImportSettings).Returns(() => new ImportSettingsDto());

			Mock<IImportApiFactory> importApiFactory = GetImportAPIFactoryMock();
			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			Sync.Executors.IImportJob result = await instance.CreateImportJobAsync(configuration.Object, _batch.Object).ConfigureAwait(false);
			result.Dispose();

			// Assert
			Assert.IsNotNull(result);
		}

		[Test]
		public async Task CreateImportJobHasExtractedFieldPathTest()
		{
			// Arrange
			var importSettingsDto = new ImportSettingsDto
			{
				ExtractedTextFieldContainsFilePath = true,
				ExtractedTextFileEncoding = Encoding.Unicode.EncodingName
			};

			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			configuration.SetupGet(x => x.ImportSettings).Returns(importSettingsDto);

			Mock<IImportApiFactory> importApiFactory = GetImportAPIFactoryMock();
			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			Sync.Executors.IImportJob result = await instance.CreateImportJobAsync(configuration.Object, _batch.Object).ConfigureAwait(false);
			result.Dispose();

			// Assert
			Assert.IsNotNull(result);
		}

		private Mock<IImportApiFactory> GetImportAPIFactoryMock()
		{
			var importApi = new Mock<IImportAPI>(MockBehavior.Loose);
			importApi.Setup(x => x.NewNativeDocumentImportJob()).Returns(() => new ImportBulkArtifactJob());

			var field = new Mock<Field>();
			importApi.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), It.IsAny<int>())).Returns(() => new[] { field.Object });

			var importApiFactory = new Mock<IImportApiFactory>();
			importApiFactory.Setup(x => x.CreateImportApiAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>())).ReturnsAsync(importApi.Object);

			return importApiFactory;
		}

		private ImportJobFactory GetTestInstance(Mock<IImportApiFactory> importApiFactory)
		{
			var instance = new ImportJobFactory(importApiFactory.Object, _dataReader.Object,
				_batchProgressHandlerFactory.Object, _jobHistoryErrorRepository.Object, _logger);
			return instance;
		}
	}
}