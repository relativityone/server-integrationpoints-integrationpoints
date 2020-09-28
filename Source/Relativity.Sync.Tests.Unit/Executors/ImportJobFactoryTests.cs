﻿using System;
using System.Reactive.Concurrency;
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
		private Mock<ISourceWorkspaceDataReaderFactory> _dataReaderFactory;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;
		private Mock<IInstanceSettings> _instanceSettings;
		private SyncJobParameters _syncJobParameters;
		private Mock<IBatch> _batch;

		private ISyncLog _logger;

		[SetUp]
		public void SetUp()
		{
			Mock<IJobProgressHandler> jobProgressHandler = new Mock<IJobProgressHandler>();
			_jobProgressHandlerFactory = new Mock<IJobProgressHandlerFactory>();
			_jobProgressHandlerFactory.Setup(x => x.CreateJobProgressHandler(It.IsAny<IScheduler>())).Returns(jobProgressHandler.Object);
			Mock<ISourceWorkspaceDataReader> dataReader = new Mock<ISourceWorkspaceDataReader>();
			_dataReaderFactory = new Mock<ISourceWorkspaceDataReaderFactory>();
			_dataReaderFactory.Setup(x => x.CreateNativeSourceWorkspaceDataReader(It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).Returns(dataReader.Object);
			_dataReaderFactory.Setup(x => x.CreateImageSourceWorkspaceDataReader(It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).Returns(dataReader.Object);
			_jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();
			_instanceSettings = new Mock<IInstanceSettings>();
			_instanceSettings.Setup(x => x.GetWebApiPathAsync(default(string))).ReturnsAsync("http://fake.uri");
			_syncJobParameters = new SyncJobParameters(0, 0, 0)
			{
				SyncApplicationName = "Test App"
			};
			_logger = new EmptyLogger();

			_batch = new Mock<IBatch>(MockBehavior.Loose);
		}

		[Test]
		public async Task CreateNativeImportJobAsync_ShouldPassGoldFlow()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);

			Mock<IImportApiFactory> importApiFactory = GetNativesImportAPIFactoryMock();
			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
			result.Dispose();

			// Assert
			result.Should().NotBeNull();
		}

		[Test]
		public async Task CreateImageImportJobAsync_ShouldPassGoldFlow()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);

			Mock<IImportApiFactory> importApiFactory = GetImagesImportAPIFactoryMock();
			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			Sync.Executors.IImportJob result = await instance.CreateImageImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
			result.Dispose();

			// Assert
			result.Should().NotBeNull();
		}

		[Test]
		public async Task CreateNativeImportJobAsync_HasExtractedFieldPath()
		{
			// Arrange

			var configuration = new Mock<ISynchronizationConfiguration>();

			Mock<IImportApiFactory> importApiFactory = GetNativesImportAPIFactoryMock();
			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
			result.Dispose();

			// Assert
			result.Should().NotBeNull();
		}

		[TestCase("relativeUri.com", "WebAPIPath relativeUri.com is invalid")]
		[TestCase("", "WebAPIPath doesn't exist")]
		[TestCase(null, "WebAPIPath doesn't exist")]
		public async Task CreateNativeImportJobAsync_ShouldThrowException_WhenWebAPIPathIsInvalid(string invalidWebAPIPath, string expectedMessage)
		{
			// Arrange
			_instanceSettings.Setup(x => x.GetWebApiPathAsync(default(string))).ReturnsAsync(invalidWebAPIPath);

			var configurationStub = new Mock<ISynchronizationConfiguration>();
			Mock<IImportApiFactory> importApiFactoryMock = GetNativesImportAPIFactoryMock();
			ImportJobFactory instance = GetTestInstance(importApiFactoryMock);

			// Act
			Func<Task> action = () => instance.CreateNativeImportJobAsync(configurationStub.Object, _batch.Object, CancellationToken.None);

			// Assert
			(await action.Should().ThrowAsync<ImportFailedException>().ConfigureAwait(false))
				.Which.Message.Should().Be(expectedMessage);
		}

		[TestCase("relativeUri.com", "WebAPIPath relativeUri.com is invalid")]
		[TestCase("", "WebAPIPath doesn't exist")]
		[TestCase(null, "WebAPIPath doesn't exist")]
		public async Task CreateImageImportJobAsync_ShouldThrowException_WhenWebAPIPathIsInvalid(string invalidWebAPIPath, string expectedMessage)
		{
			// Arrange
			_instanceSettings.Setup(x => x.GetWebApiPathAsync(default(string))).ReturnsAsync(invalidWebAPIPath);

			var configurationStub = new Mock<ISynchronizationConfiguration>();
			Mock<IImportApiFactory> importApiFactoryMock = GetNativesImportAPIFactoryMock();
			ImportJobFactory instance = GetTestInstance(importApiFactoryMock);

			// Act
			Func<Task> action = () => instance.CreateImageImportJobAsync(configurationStub.Object, _batch.Object, CancellationToken.None);

			// Assert
			(await action.Should().ThrowAsync<ImportFailedException>().ConfigureAwait(false))
				.Which.Message.Should().Be(expectedMessage);
		}

		[Test]
		public async Task CreateNativeImportJobAsync_ShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0()
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
			Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(configurationStub.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
			result.Dispose();

			// Assert
			importBulkArtifactJobMock.Settings.StartRecordNumber.Should().Be(0);
		}

		[Test]
		public async Task CreateImageImportJobAsync_ShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0()
		{
			// Arrange
			Mock<ISynchronizationConfiguration> configurationStub = new Mock<ISynchronizationConfiguration>();
			Mock<IImportAPI> importApiStub = new Mock<IImportAPI>(MockBehavior.Loose);
			Mock<IImportApiFactory> importApiFactoryStub = new Mock<IImportApiFactory>();
			Mock<Field> fieldStub = new Mock<Field>();
			ImageImportBulkArtifactJob importBulkArtifactJobMock = new ImageImportBulkArtifactJob();

			importApiStub.Setup(x => x.NewImageImportJob()).Returns(() => importBulkArtifactJobMock);
			importApiStub.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), It.IsAny<int>())).Returns(() => new[] { fieldStub.Object });
			importApiFactoryStub.Setup(x => x.CreateImportApiAsync(It.IsAny<Uri>())).ReturnsAsync(importApiStub.Object);

			const int batchStartingIndex = 250;
			_batch.SetupGet(x => x.StartingIndex).Returns(batchStartingIndex);

			ImportJobFactory instance = GetTestInstance(importApiFactoryStub);

			// Act
			Sync.Executors.IImportJob result = await instance.CreateImageImportJobAsync(configurationStub.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
			result.Dispose();

			// Assert
			importBulkArtifactJobMock.Settings.StartRecordNumber.Should().Be(0);
		}

		[Test]
		public async Task CreateNativeImportJob_ShouldSetBillableToTrue_WhenCopyingNatives()
		{
			// Arrange

			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			configuration.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.CopyFiles);

			var importBulkArtifactJob = new ImportBulkArtifactJob();
			Mock<IImportApiFactory> importApiFactory = GetNativesImportAPIFactoryMock(importBulkArtifactJob);

			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			await instance.CreateNativeImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			importBulkArtifactJob.Settings.Billable.Should().Be(true);
		}

		[Test]
		public async Task CreateImageImportJob_ShouldSetBillableToTrue_WhenCopyingImages()
		{
			// Arrange

			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			configuration.SetupGet(x => x.ImportImageFileCopyMode).Returns(ImportImageFileCopyMode.CopyFiles);

			var importBulkArtifactJob = new ImageImportBulkArtifactJob();
			Mock<IImportApiFactory> importApiFactory = GetImagesImportAPIFactoryMock(importBulkArtifactJob);

			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			await instance.CreateImageImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			importBulkArtifactJob.Settings.Billable.Should().Be(true);
		}

		[Test]
		public async Task CreateNativeImportJob_ShouldSetBillableToFalse_WhenUsingLinksOnly()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			configuration.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.SetFileLinks);

			var importBulkArtifactJob = new ImportBulkArtifactJob();
			Mock<IImportApiFactory> importApiFactory = GetNativesImportAPIFactoryMock(importBulkArtifactJob);

			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			await instance.CreateNativeImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			importBulkArtifactJob.Settings.Billable.Should().Be(false);
		}

		[Test]
		public async Task CreateImageImportJob_ShouldSetBillableToFalse_WhenLinkingImages()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			configuration.SetupGet(x => x.ImportImageFileCopyMode).Returns(ImportImageFileCopyMode.SetFileLinks);

			var importBulkArtifactJob = new ImageImportBulkArtifactJob();
			Mock<IImportApiFactory> importApiFactory = GetImagesImportAPIFactoryMock(importBulkArtifactJob);

			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			await instance.CreateImageImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			importBulkArtifactJob.Settings.Billable.Should().Be(false);
		}
		
		[Test]
		public async Task CreateImageImportJob_ShouldSetImageFilePathSourceFieldName()
		{
			// Arrange
			const string fakePath = "//fake/path.jpg";
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			configuration.SetupGet(x => x.ImageFilePathSourceFieldName).Returns(fakePath);

			var importBulkArtifactJob = new ImageImportBulkArtifactJob();
			Mock<IImportApiFactory> importApiFactory = GetImagesImportAPIFactoryMock(importBulkArtifactJob);

			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			await instance.CreateImageImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			importBulkArtifactJob.Settings.ImageFilePathSourceFieldName.Should().Be(fakePath);
		}


		[Test]
		public async Task CreateNativeImportJob_ShouldSetBillableToFalse_WhenNotCopyingNatives()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			configuration.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);

			var importBulkArtifactJob = new ImportBulkArtifactJob();
			Mock<IImportApiFactory> importApiFactory = GetNativesImportAPIFactoryMock(importBulkArtifactJob);

			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			await instance.CreateNativeImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			importBulkArtifactJob.Settings.Billable.Should().Be(false);
		}

		[Test]
		public async Task CreateNativeImportJob_ShouldSetApplicationName()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);

			var importBulkArtifactJob = new ImportBulkArtifactJob();
			Mock<IImportApiFactory> importApiFactory = GetNativesImportAPIFactoryMock(importBulkArtifactJob);

			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			await instance.CreateNativeImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			importBulkArtifactJob.Settings.ApplicationName.Should().Be(_syncJobParameters.SyncApplicationName);
		}

		[Test]
		public async Task CreateImagesImportJob_ShouldSetApplicationName()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);

			var importBulkArtifactJob = new ImageImportBulkArtifactJob();
			Mock<IImportApiFactory> importApiFactory = GetImagesImportAPIFactoryMock(importBulkArtifactJob);

			ImportJobFactory instance = GetTestInstance(importApiFactory);

			// Act
			await instance.CreateImageImportJobAsync(configuration.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			importBulkArtifactJob.Settings.ApplicationName.Should().Be(_syncJobParameters.SyncApplicationName);
		}

		private Mock<IImportApiFactory> GetNativesImportAPIFactoryMock(ImportBulkArtifactJob importBulkArtifactJob = null)
		{
			var importApi = new Mock<IImportAPI>(MockBehavior.Loose);
			importApi.Setup(x => x.NewNativeDocumentImportJob()).Returns(() => importBulkArtifactJob ?? new ImportBulkArtifactJob());

			var field = new Mock<Field>();
			importApi.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), It.IsAny<int>())).Returns(() => new[] { field.Object });

			var importApiFactory = new Mock<IImportApiFactory>();
			importApiFactory.Setup(x => x.CreateImportApiAsync(It.IsAny<Uri>())).ReturnsAsync(importApi.Object);

			return importApiFactory;
		}

		private Mock<IImportApiFactory> GetImagesImportAPIFactoryMock(ImageImportBulkArtifactJob job = null)
		{
			var importApi = new Mock<IImportAPI>(MockBehavior.Loose);
			importApi.Setup(x => x.NewImageImportJob()).Returns(() => job ?? new ImageImportBulkArtifactJob());

			var field = new Mock<Field>();
			importApi.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), It.IsAny<int>())).Returns(() => new[] { field.Object });

			var importApiFactory = new Mock<IImportApiFactory>();
			importApiFactory.Setup(x => x.CreateImportApiAsync(It.IsAny<Uri>())).ReturnsAsync(importApi.Object);

			return importApiFactory;
		}

		private ImportJobFactory GetTestInstance(Mock<IImportApiFactory> importApiFactory)
		{
			var instance = new ImportJobFactory(importApiFactory.Object, _dataReaderFactory.Object,
				_jobHistoryErrorRepository.Object, _instanceSettings.Object, _syncJobParameters, _logger);
			return instance;
		}
	}
}