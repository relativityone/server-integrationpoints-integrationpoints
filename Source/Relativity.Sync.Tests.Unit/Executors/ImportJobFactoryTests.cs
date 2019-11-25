using System;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
	public class ImportJobFactoryTests
	{
		private Mock<IJobProgressHandlerFactory> _jobProgressHandlerFactory;
		private Mock<IJobProgressUpdaterFactory> _jobProgressUpdaterFactory;
		private Mock<ISourceWorkspaceDataReaderFactory> _dataReaderFactory;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;
		private Mock<IInstanceSettings> _instanceSettings;

		private Mock<IBatch> _batch;

		private ISyncLog _logger;

		[SetUp]
		public void SetUp()
		{
			_jobProgressUpdaterFactory = new Mock<IJobProgressUpdaterFactory>();
			Mock<IJobProgressHandler> jobProgressHandler = new Mock<IJobProgressHandler>();
			_jobProgressHandlerFactory = new Mock<IJobProgressHandlerFactory>();
			_jobProgressHandlerFactory.Setup(x => x.CreateJobProgressHandler(It.IsAny<IScheduler>())).Returns(jobProgressHandler.Object);
			Mock<ISourceWorkspaceDataReader> dataReader = new Mock<ISourceWorkspaceDataReader>();
			_dataReaderFactory = new Mock<ISourceWorkspaceDataReaderFactory>();
			_dataReaderFactory.Setup(x => x.CreateSourceWorkspaceDataReader(It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).Returns(dataReader.Object);
			_jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();
			_instanceSettings = new Mock<IInstanceSettings>();
			_instanceSettings.Setup(x => x.GetWebApiPathAsync(default(string))).ReturnsAsync("http://fake.uri");

			_logger = new EmptyLogger();

			_batch = new Mock<IBatch>(MockBehavior.Loose);
		}

		[Test]
		public async Task CreateImportJobAsync_ShouldPassGoldFlow()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);

			Mock<IImportApiFactory> importApiFactory = GetImportAPIFactoryMock();
			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			Sync.Executors.IImportJob result = await instance.CreateImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
			result.Dispose();

			// Assert
			result.Should().NotBeNull();
		}

		[Test]
		public async Task CreateImportJobAsync_HasExtractedFieldPath()
		{
			// Arrange

			var configuration = new Mock<ISynchronizationConfiguration>();

			Mock<IImportApiFactory> importApiFactory = GetImportAPIFactoryMock();
			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			Sync.Executors.IImportJob result = await instance.CreateImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
			result.Dispose();

			// Assert
			result.Should().NotBeNull();
		}

		[TestCase("relativeUri.com", "WebAPIPath relativeUri.com is invalid")]
		[TestCase("", "WebAPIPath doesn't exist")]
		public async Task CreateImportJobAsync_ShouldThrowException_WhenWebAPIPathIsInvalid(string invalidWebAPIPath, string expectedMessage)
		{
			// Arrange
			_instanceSettings.Setup(x => x.GetWebApiPathAsync(default(string))).ReturnsAsync(invalidWebAPIPath);

			var configuration = new Mock<ISynchronizationConfiguration>();
			Mock<IImportApiFactory> importApiFactory = GetImportAPIFactoryMock();
			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			Func<Task> action = async () => await instance.CreateImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<ImportFailedException>().ConfigureAwait(false))
				.Which.Message.Should().Be(expectedMessage);
		}

		[Test]
		public async Task CreateImportJobAsync_ShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0()
		{
			// Arrange
			Mock<ISynchronizationConfiguration> configurationStub = new Mock<ISynchronizationConfiguration>();
			Mock<IImportAPI> importApiStub = new Mock<IImportAPI>(MockBehavior.Loose);
			Mock<IImportApiFactory> importApiFactoryStub = new Mock<IImportApiFactory>();
			Mock<Field> fieldStub = new Mock<Field>();
			ImportBulkArtifactJob importBulkArtifactJobMock = new ImportBulkArtifactJob();


			importApiStub.Setup(x => x.NewNativeDocumentImportJob()).Returns(() => importBulkArtifactJobMock);
			importApiStub.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), It.IsAny<int>())).Returns(() => new[] { fieldStub.Object });
			importApiFactoryStub.Setup(x => x.CreateImportApiAsync(It.IsAny<Uri>())).ReturnsAsync(importApiStub.Object);

			const int batchStartingIndex = 250;
			_batch.SetupGet(x => x.StartingIndex).Returns(batchStartingIndex);

			ImportJobFactory instance = GetTestInstance(importApiFactoryStub);

			// Act
			Sync.Executors.IImportJob result = await instance.CreateImportJobAsync(configurationStub.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
			result.Dispose();

			// Assert
			importBulkArtifactJobMock.Settings.StartRecordNumber.Should().Be(0);
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
			var instance = new ImportJobFactory(importApiFactory.Object, _dataReaderFactory.Object,
				_jobHistoryErrorRepository.Object, _instanceSettings.Object, _logger);
			return instance;
		}
	}
}