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
		private Mock<IJobProgressHandlerFactory> _jobProgressHandlerFactory;
		private Mock<IJobProgressUpdaterFactory> _jobProgressUpdaterFactory;
		private Mock<ISourceWorkspaceDataReaderFactory> _dataReaderFactory;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;

		private Mock<ISourceWorkspaceDataReader> _dataReader;
		private Mock<IBatch> _batch;

		private ISyncLog _logger;

		[SetUp]
		public void SetUp()
		{
			_batchProgressHandlerFactory = new Mock<IBatchProgressHandlerFactory>();
			_jobProgressUpdaterFactory = new Mock<IJobProgressUpdaterFactory>();
			Mock<IJobProgressHandler> jobProgressHandler = new Mock<IJobProgressHandler>();
			_jobProgressHandlerFactory = new Mock<IJobProgressHandlerFactory>();
			_jobProgressHandlerFactory.Setup(x => x.CreateJobProgressHandler(It.IsAny<IJobProgressUpdater>())).Returns(jobProgressHandler.Object);
			_dataReader = new Mock<ISourceWorkspaceDataReader>();
			_dataReaderFactory = new Mock<ISourceWorkspaceDataReaderFactory>();
			_dataReaderFactory.Setup(x => x.CreateSourceWorkspaceDataReader(It.IsAny<IBatch>())).Returns(_dataReader.Object);
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
			importApiFactory.Setup(x => x.CreateImportApiAsync(It.IsAny<Uri>())).ReturnsAsync(importApi.Object);

			return importApiFactory;
		}

		private ImportJobFactory GetTestInstance(Mock<IImportApiFactory> importApiFactory)
		{
			var instance = new ImportJobFactory(importApiFactory.Object, _dataReaderFactory.Object, _batchProgressHandlerFactory.Object, 
				_jobProgressHandlerFactory.Object, _jobProgressUpdaterFactory.Object,
				_jobHistoryErrorRepository.Object, _logger);
			return instance;
		}
	}
}