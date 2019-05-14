using System.Data;
using System.Text;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class ImportJobFactoryTests
	{
		private Mock<IBatchProgressHandlerFactory> _batchProgressHandlerFactory;
		private Mock<IDataReader> _dataReader;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;
		private Mock<ISemaphoreSlim> _semaphoreSlim;

		private Mock<IBatch> _batch;

		private ISyncLog _logger;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_batchProgressHandlerFactory = new Mock<IBatchProgressHandlerFactory>();
			_dataReader = new Mock<IDataReader>();
			_jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();
			_semaphoreSlim = new Mock<ISemaphoreSlim>();

			_logger = new EmptyLogger();

			_batch = new Mock<IBatch>(MockBehavior.Loose);
		}

		[Test]
		public void CreateImportJobGoldFlowTest()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			configuration.SetupGet(x => x.ImportSettings).Returns(() => new ImportSettingsDto());

			Mock<IImportAPI> importApi = GetImportApiMock();
			ImportJobFactory instance = GetTestInstance(importApi);

			// Act
			Sync.Executors.IImportJob result = instance.CreateImportJob(configuration.Object, _batch.Object);
			result.Dispose();

			// Assert
			Assert.IsNotNull(result);

			_semaphoreSlim.Verify(x => x.Dispose(), Times.AtLeastOnce);
		}

		[Test]
		public void CreateImportJobHasExtractedFieldPathTest()
		{
			// Arrange
			var importSettingsDto = new ImportSettingsDto
			{
				ExtractedTextFieldContainsFilePath = true,
				ExtractedTextFileEncoding = Encoding.Unicode.EncodingName
			};

			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			configuration.SetupGet(x => x.ImportSettings).Returns(importSettingsDto);

			Mock<IImportAPI> importApi = GetImportApiMock();
			ImportJobFactory instance = GetTestInstance(importApi);

			// Act
			Sync.Executors.IImportJob result = instance.CreateImportJob(configuration.Object, _batch.Object);
			result.Dispose();

			// Assert
			Assert.IsNotNull(result);

			_semaphoreSlim.Verify(x => x.Dispose(), Times.AtLeastOnce);
		}

		private Mock<IImportAPI> GetImportApiMock()
		{
			var importApi = new Mock<IImportAPI>(MockBehavior.Loose);
			importApi.Setup(x => x.NewNativeDocumentImportJob()).Returns(() => new ImportBulkArtifactJob());

			var field = new Mock<Field>();
			importApi.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), It.IsAny<int>())).Returns(() => new[] { field.Object });

			return importApi;
		}

		private ImportJobFactory GetTestInstance(Mock<IImportAPI> importApi)
		{
			var instance = new ImportJobFactory(importApi.Object, _dataReader.Object,
				_batchProgressHandlerFactory.Object, _jobHistoryErrorRepository.Object, _semaphoreSlim.Object, _logger);
			return instance;
		}
	}
}