using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Moq;
using NUnit.Framework;
using Relativity.Sync.AntiMalware.SDK;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Progress;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;
using Relativity.Sync.Transfer;
namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class ImportJobFactoryTests
    {
        private IAPILog _logger;
        private Mock<IBatch> _batch;

        private Mock<IDocumentSynchronizationConfiguration> _documentConfigurationMock;
        private Mock<IImageSynchronizationConfiguration> _imageConfigurationMock;
        private Mock<INonDocumentSynchronizationConfiguration> _nonDocumentConfigurationMock;
        private Mock<IInstanceSettings> _instanceSettings;
        private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepository;
        private Mock<IJobProgressHandlerFactory> _jobProgressHandlerFactory;
        private Mock<ISourceWorkspaceDataReaderFactory> _dataReaderFactory;
        private Mock<IFieldMappings> _fieldMappingsMock;
        private Mock<IAntiMalwareEventHelper> _antiMalwareEventHelperMock;
        private Mock<ISyncToggles> _syncTogglesMock;
        private SyncJobParameters _syncJobParameters;
        private const string _IMAGE_IDENTIFIER_DISPLAY_NAME = "ImageIdentifier";
        private const int _DEST_RDO_ARTIFACT_TYPE = 1234567;

        private static readonly FieldInfoDto _DOCUMENT_IDENTIFIER_FIELD =
            new FieldInfoDto(SpecialFieldType.None, "Control Number Source [Identifier]", "Control Number Destination [Identifier]", true, true)
            {
                RelativityDataType = RelativityDataType.FixedLengthText
            };

        private static readonly FieldMap[] _MAPPED_FIELDS = new FieldMap[]
        {
            CreateFieldMap(_DOCUMENT_IDENTIFIER_FIELD, true),
        };

        [SetUp]
        public void SetUp()
        {
            _documentConfigurationMock = new Mock<IDocumentSynchronizationConfiguration>();
            _imageConfigurationMock = new Mock<IImageSynchronizationConfiguration>();
            _nonDocumentConfigurationMock = new Mock<INonDocumentSynchronizationConfiguration>();
            Mock<IJobProgressHandler> jobProgressHandler = new Mock<IJobProgressHandler>();
            _jobProgressHandlerFactory = new Mock<IJobProgressHandlerFactory>();
            _jobProgressHandlerFactory.Setup(x => x.CreateJobProgressHandler(Enumerable.Empty<IBatch>(), It.IsAny<IScheduler>())).Returns(jobProgressHandler.Object);
            Mock<ISourceWorkspaceDataReader> dataReader = new Mock<ISourceWorkspaceDataReader>();
            _dataReaderFactory = new Mock<ISourceWorkspaceDataReaderFactory>();
            _dataReaderFactory.Setup(x => x.CreateNativeSourceWorkspaceDataReader(It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).Returns(dataReader.Object);
            _dataReaderFactory.Setup(x => x.CreateImageSourceWorkspaceDataReader(It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).Returns(dataReader.Object);
            _dataReaderFactory.Setup(x => x.CreateNonDocumentSourceWorkspaceDataReader(It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).Returns(dataReader.Object);
            _dataReaderFactory.Setup(x => x.CreateNonDocumentObjectLinkingSourceWorkspaceDataReader(It.IsAny<IBatch>(), It.IsAny<CancellationToken>())).Returns(dataReader.Object);

            _jobHistoryErrorRepository = new Mock<IJobHistoryErrorRepository>();
            _fieldMappingsMock = new Mock<IFieldMappings>();
            _fieldMappingsMock.Setup(x => x.GetFieldMappings()).Returns(_MAPPED_FIELDS);
            _instanceSettings = new Mock<IInstanceSettings>();
            _instanceSettings.Setup(x => x.GetShouldForceADFTransferAsync(default(bool))).ReturnsAsync(false);
            _syncJobParameters = FakeHelper.CreateSyncJobParameters();
            _antiMalwareEventHelperMock = new Mock<IAntiMalwareEventHelper>();
            _syncTogglesMock = new Mock<ISyncToggles>();
            _logger = new EmptyLogger();

            _batch = new Mock<IBatch>(MockBehavior.Loose);

            _imageConfigurationMock.SetupGet(x => x.IdentifierColumn).Returns(_IMAGE_IDENTIFIER_DISPLAY_NAME);
            _documentConfigurationMock.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);
            _nonDocumentConfigurationMock.SetupGet(x => x.DestinationRdoArtifactTypeId).Returns(_DEST_RDO_ARTIFACT_TYPE);
        }

        [Test]
        public async Task CreateNativeImportJobAsync_ShouldPassGoldFlow()
        {
            // Arrange
            ImportJobFactory instance = GetTestInstance(GetNativesImportAPIFactoryMock());

            // Act
            Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
            result.Dispose();

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task CreateNonDocumentImportJobAsync_ShouldPassGoldFlow()
        {
            // Arrange
            ImportBulkArtifactJob importBulkArtifactJob = new ImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetNonDocumentImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            Sync.Executors.IImportJob result = await instance.CreateRdoImportJobAsync(_nonDocumentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
            result.Dispose();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeNull();
            importBulkArtifactJob.Settings.OverwriteMode.Should().Be(_nonDocumentConfigurationMock.Object.ImportOverwriteMode);
            importBulkArtifactJob.Settings.ArtifactTypeId.Should().Be(_DEST_RDO_ARTIFACT_TYPE);
            importBulkArtifactJob.Settings.NativeFileCopyMode.Should()
                .Be(NativeFileCopyModeEnum.DoNotImportNativeFiles);
            importBulkArtifactJob.Settings.SelectedIdentifierFieldName.Should()
                .Be(_DOCUMENT_IDENTIFIER_FIELD.DestinationFieldName);
        }

        [Test]
        public async Task CreateNativeImportJobAsync_ShouldPassGoldFlow_WhenDoNotImporNatives()
        {
            // Arrange
            ImportJobFactory instance = GetTestInstance(GetNativesImportAPIFactoryMock());
            _documentConfigurationMock.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);

            // Act
            Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
            result.Dispose();

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task CreateImageImportJobAsync_ShouldPassGoldFlow()
        {
            // Arrange
            ImportJobFactory instance = GetTestInstance(GetImagesImportAPIFactoryMock());

            // Act
            Sync.Executors.IImportJob result = await instance.CreateImageImportJobAsync(_imageConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
            result.Dispose();

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task CreateNativeImportJobAsync_HasExtractedFieldPath()
        {
            // Arrange
            Mock<IImportApiFactory> importApiFactory = GetNativesImportAPIFactoryMock();
            ImportJobFactory instance = GetTestInstance(importApiFactory);

            // Act
            Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
            result.Dispose();

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task CreateNativeImportJobAsync_HasExtractedFieldPath_WhenDoNotImporNatives()
        {
            // Arrange
            Mock<IImportApiFactory> importApiFactory = GetNativesImportAPIFactoryMock();
            _documentConfigurationMock.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);
            ImportJobFactory instance = GetTestInstance(importApiFactory);

            // Act
            Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
            result.Dispose();

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task CreateNativeImportJobAsync_ShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0()
        {
            // Arrange
            ImportBulkArtifactJob importBulkArtifactJobMock = new ImportBulkArtifactJob();
            ImportJobFactory instance =
                PrepareInstanceForShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0(x => x.NewNativeDocumentImportJob(), importBulkArtifactJobMock);

            // Act
            Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
            result.Dispose();

            // Assert
            AssertStartRecordNumberForShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0(importBulkArtifactJobMock.Settings);
        }

        [Test]
        public async Task CreateNativeImportJobAsync_ShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0_WhenDoNotImporNatives()
        {
            // Arrange
            ImportBulkArtifactJob importBulkArtifactJobMock = new ImportBulkArtifactJob();
            _documentConfigurationMock.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);
            ImportJobFactory instance =
                PrepareInstanceForShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0(x => x.NewNativeDocumentImportJob(), importBulkArtifactJobMock);

            // Act
            Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
            result.Dispose();

            // Assert
            AssertStartRecordNumberForShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0(importBulkArtifactJobMock.Settings);
        }

        [Test]
        public async Task CreateImageImportJobAsync_ShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0()
        {
            // Arrange
            ImageImportBulkArtifactJob imageImportBulkArtifactJob = new ImageImportBulkArtifactJob();
            ImportJobFactory instance =
                PrepareInstanceForShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0(x => x.NewImageImportJob(), imageImportBulkArtifactJob);

            // Act
            Sync.Executors.IImportJob result = await instance.CreateImageImportJobAsync(_imageConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);
            result.Dispose();

            // Assert
            AssertStartRecordNumberForShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0(imageImportBulkArtifactJob.Settings);
        }

        [Test]
        public async Task CreateNativeImportJob_ShouldSetBillableToTrue_WhenCopyingNatives()
        {
            // Arrange
            _documentConfigurationMock.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.CopyFiles);

            var importBulkArtifactJob = new ImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetNativesImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            importBulkArtifactJob.Settings.Billable.Should().Be(true);
        }

        [Test]
        public async Task CreateImageImportJob_ShouldSetBillableToTrue_WhenCopyingImages()
        {
            // Arrange
            _imageConfigurationMock.SetupGet(x => x.ImportImageFileCopyMode).Returns(ImportImageFileCopyMode.CopyFiles);

            var importBulkArtifactJob = new ImageImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetImagesImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            await instance.CreateImageImportJobAsync(_imageConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            importBulkArtifactJob.Settings.Billable.Should().Be(true);
        }

        [Test]
        public async Task CreateNativeImportJob_ShouldSetBillableToFalse_WhenUsingLinksOnly()
        {
            // Arrange
            _documentConfigurationMock.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.SetFileLinks);

            var importBulkArtifactJob = new ImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetNativesImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            importBulkArtifactJob.Settings.Billable.Should().Be(false);
        }

        [Test]
        public async Task CreateImageImportJob_ShouldSetBillableToFalse_WhenLinkingImages()
        {
            // Arrange
            _imageConfigurationMock.SetupGet(x => x.ImportImageFileCopyMode).Returns(ImportImageFileCopyMode.SetFileLinks);

            var importBulkArtifactJob = new ImageImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetImagesImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            await instance.CreateImageImportJobAsync(_imageConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            importBulkArtifactJob.Settings.Billable.Should().Be(false);
        }

        [Test]
        public async Task CreateNativeImportJob_ShouldSetBillableToFalse_WhenNotCopyingNatives()
        {
            // Arrange
            _documentConfigurationMock.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);

            var importBulkArtifactJob = new ImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetNativesImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            importBulkArtifactJob.Settings.Billable.Should().Be(false);
        }

        [Test]
        public async Task CreateNativeImportJob_ShouldSetApplicationName()
        {
            // Arrange
            var importBulkArtifactJob = new ImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetNativesImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            AssertApplicationName(importBulkArtifactJob.Settings);
        }

        [Test]
        public async Task CreateImagesImportJob_ShouldSetApplicationName()
        {
            // Arrange
            var importBulkArtifactJob = new ImageImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetImagesImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            await instance.CreateImageImportJobAsync(_imageConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            AssertApplicationName(importBulkArtifactJob.Settings);
        }

        [Test]
        public async Task CreateImagesImportJob_ShouldSetBatesNumberFieldToImageIdentifier()
        {
            // Arrange
            var importBulkArtifactJob = new ImageImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetImagesImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            await instance.CreateImageImportJobAsync(_imageConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            importBulkArtifactJob.Settings.BatesNumberField.Should().Be(_imageConfigurationMock.Object.IdentifierColumn);
        }

        [Test]
        public async Task CreateImagesImportJob_ShouldSetImageFileName()
        {
            // Arrange
            _imageConfigurationMock.SetupGet(x => x.FileNameColumn).Returns("MyCustomImageFileNameColumn");
            var importBulkArtifactJob = new ImageImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetImagesImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            await instance.CreateImageImportJobAsync(_imageConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            importBulkArtifactJob.Settings.FileNameField.Should().Be(_imageConfigurationMock.Object.FileNameColumn);
        }

        [Test]
        public async Task CreateRdoLinkingJobAsync_ShouldSetCorrectValues()
        {
            // Arrange
            ImportBulkArtifactJob importBulkArtifactJob = new ImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetNonDocumentImportAPIFactoryMock(importBulkArtifactJob));

            // Act
            Sync.Executors.IImportJob result = await instance.CreateRdoLinkingJobAsync(_nonDocumentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            importBulkArtifactJob.Settings.OverwriteMode.Should().Be(OverwriteModeEnum.Overlay);
            importBulkArtifactJob.Settings.ArtifactTypeId.Should().Be(_DEST_RDO_ARTIFACT_TYPE);
            importBulkArtifactJob.Settings.NativeFileCopyMode.Should()
                .Be(NativeFileCopyModeEnum.DoNotImportNativeFiles);
            importBulkArtifactJob.Settings.SelectedIdentifierFieldName.Should()
                .Be(_DOCUMENT_IDENTIFIER_FIELD.DestinationFieldName);
        }

        [Test]
        public async Task CreateNativeImportJobAsync_ShouldSetCorrectValues()
        {
            // Arrange
            ImportBulkArtifactJob importBulkArtifactJob = new ImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetNativesImportAPIFactoryMock(importBulkArtifactJob));

            _documentConfigurationMock.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.CopyFiles);

            // Act
            Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            importBulkArtifactJob.Settings.NativeFileCopyMode.Should().Be(NativeFileCopyModeEnum.CopyFiles);
            importBulkArtifactJob.Settings.DisableNativeLocationValidation.Should().BeFalse();
        }

        [Test]
        public async Task CreateNativeImportJobAsync_ShouldSetCorrectValues_WhenAuditToggleIsFalse()
        {
            // Arrange
            ImportBulkArtifactJob importBulkArtifactJob = new ImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetNativesImportAPIFactoryMock(importBulkArtifactJob));

            _documentConfigurationMock.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.CopyFiles);

            _syncTogglesMock.Setup(t => t.IsEnabled<EnableAuditToggle>()).Returns(false);

            // Act
            Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            importBulkArtifactJob.Settings.AuditLevel.Should().Be(ImportAuditLevel.NoAudit);
        }

        [Test]
        public async Task CreateNativeImportJobAsync_ShouldSetCorrectValues_WhenAuditToggleIsTrue()
        {
            // Arrange
            ImportBulkArtifactJob importBulkArtifactJob = new ImportBulkArtifactJob();
            ImportJobFactory instance = GetTestInstance(GetNativesImportAPIFactoryMock(importBulkArtifactJob));

            _documentConfigurationMock.SetupGet(x => x.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.CopyFiles);

            _syncTogglesMock.Setup(t => t.IsEnabled<EnableAuditToggle>()).Returns(true);

            // Act
            Sync.Executors.IImportJob result = await instance.CreateNativeImportJobAsync(_documentConfigurationMock.Object, _batch.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            importBulkArtifactJob.Settings.AuditLevel.Should().Be(ImportAuditLevel.FullAudit);
        }

        private ImportJobFactory PrepareInstanceForShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0<T>(Expression<Func<IImportAPI, T>> setupAction, T mockObject)
        {
            Mock<IImportAPI> importApiStub = new Mock<IImportAPI>(MockBehavior.Loose);
            Mock<IImportApiFactory> importApiFactoryStub = new Mock<IImportApiFactory>();
            Mock<Field> fieldStub = new Mock<Field>();

            importApiStub.Setup(setupAction).Returns(mockObject);
            importApiStub.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), It.IsAny<int>())).Returns(() => new[] { fieldStub.Object });
            importApiFactoryStub.Setup(x => x.CreateImportApiAsync()).ReturnsAsync(importApiStub.Object);

            const int batchStartingIndex = 250;
            _batch.SetupGet(x => x.StartingIndex).Returns(batchStartingIndex);

            ImportJobFactory instance = GetTestInstance(importApiFactoryStub);
            return instance;
        }

        private void AssertStartRecordNumberForShouldCreateBulkJobWithStartingIndexAlwaysEqualTo0(ImportSettingsBase settings)
        {
            settings.StartRecordNumber.Should().Be(0);
        }

        private void AssertApplicationName(ImportSettingsBase settings)
        {
            settings.ApplicationName.Should().Be(_syncJobParameters.SyncApplicationName);
        }

        private Mock<IImportApiFactory> GetNativesImportAPIFactoryMock(ImportBulkArtifactJob job = null)
        {
            return GetImportAPIFactoryMock(iapi => iapi.NewNativeDocumentImportJob(), job ?? new ImportBulkArtifactJob());
        }

        private Mock<IImportApiFactory> GetNonDocumentImportAPIFactoryMock(ImportBulkArtifactJob job = null)
        {
            return GetImportAPIFactoryMock(iapi => iapi.NewObjectImportJob(_DEST_RDO_ARTIFACT_TYPE), job ?? new ImportBulkArtifactJob());
        }

        private Mock<IImportApiFactory> GetImagesImportAPIFactoryMock(ImageImportBulkArtifactJob job = null)
        {
            return GetImportAPIFactoryMock(iapi => iapi.NewImageImportJob(), job ?? new ImageImportBulkArtifactJob());
        }

        private Mock<IImportApiFactory> GetImportAPIFactoryMock<T>(Expression<Func<IImportAPI, T>> setupAction, T mockObject)
        {
            var importApi = new Mock<IImportAPI>(MockBehavior.Loose);
            importApi.Setup(setupAction).Returns(() => mockObject);

            var field = new Mock<Field>();
            importApi.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), It.IsAny<int>())).Returns(() => new[] { field.Object });

            var importApiFactory = new Mock<IImportApiFactory>();
            importApiFactory.Setup(x => x.CreateImportApiAsync()).ReturnsAsync(importApi.Object);

            return importApiFactory;
        }

        private ImportJobFactory GetTestInstance(Mock<IImportApiFactory> importApiFactory)
        {
            var instance = new ImportJobFactory(importApiFactory.Object, _dataReaderFactory.Object,
                _jobHistoryErrorRepository.Object, _syncJobParameters,
                _fieldMappingsMock.Object, _antiMalwareEventHelperMock.Object,
                _syncTogglesMock.Object, _logger);
            return instance;
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
    }
}
