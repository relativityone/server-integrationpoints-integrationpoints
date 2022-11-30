using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Executors.DocumentTaggers;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    internal sealed class ImageSynchronizationExecutorTests
    {
        private ConfigurationStub _config;
        private IExecutor<IImageSynchronizationConfiguration> _executor;
        private ISourceWorkspaceDataReaderFactory _dataReaderFactory;
        private Mock<IObjectManager> _objectManagerMock;
        private Mock<IFolderManager> _folderManagerMock;
        private Mock<ISyncImportBulkArtifactJob> _importBulkArtifactJob;
        private Mock<IImageFileRepository> _imageFileRepository;
        private Mock<IRdoManager> _rdoManagerMock;

        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 10001;
        private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 20002;

        private static readonly ImportApiJobStatistics _emptyJobStatistsics = new ImportApiJobStatistics(0, 0, 0, 0);

        private static readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
        private static readonly Guid JobHistoryErrorObjectTypeGuid = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");

        [SetUp]
        public void SetUp()
        {
            ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            IntegrationTestsContainerBuilder.MockStepsExcept<IImageSynchronizationConfiguration>(containerBuilder);
            containerBuilder.RegisterType<ImportJobFactoryStub>().As<IImportJobFactory>();

            _importBulkArtifactJob = new Mock<ISyncImportBulkArtifactJob>();
            containerBuilder.RegisterInstance(_importBulkArtifactJob.Object).As<ISyncImportBulkArtifactJob>();

            Mock<ISyncMetrics> syncMetrics = new Mock<ISyncMetrics>();
            containerBuilder.RegisterInstance(syncMetrics.Object).As<ISyncMetrics>();

            Mock<ISemaphoreSlim> semaphoreSlim = new Mock<ISemaphoreSlim>();
            containerBuilder.RegisterInstance(semaphoreSlim.Object).As<ISemaphoreSlim>();

            _rdoManagerMock = new Mock<IRdoManager>();
            containerBuilder.RegisterInstance(_rdoManagerMock.Object).As<IRdoManager>();

            IList<FieldMap> fieldMaps = new List<FieldMap>()
            {
                new FieldMap()
                {
                    SourceField = new FieldEntry()
                    {
                        DisplayName = "Control Number",
                        IsIdentifier = true,
                    },
                    DestinationField = new FieldEntry
                    {
                        DisplayName = "Control Number",
                        IsIdentifier = true,
                    }
                }
            };

            _config = new ConfigurationStub
            {
                DestinationWorkspaceArtifactId = _DESTINATION_WORKSPACE_ARTIFACT_ID,
                SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID,
            };
            _config.SetFieldMappings(fieldMaps);
            containerBuilder.RegisterInstance(_config).AsImplementedInterfaces();

            _objectManagerMock = new Mock<IObjectManager>();
            _folderManagerMock = new Mock<IFolderManager>();
            var destinationServiceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
            var sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            var sourceServiceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();

            var fieldMappings = new Mock<IFieldMappings>();
            fieldMappings.Setup(x => x.GetFieldMappings()).Returns(fieldMaps);

            _imageFileRepository = new Mock<IImageFileRepository>();

            destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManagerMock.Object);
            sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManagerMock.Object);
            sourceServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManagerMock.Object);
            sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IFolderManager>()).ReturnsAsync(_folderManagerMock.Object);

            containerBuilder.RegisterInstance(destinationServiceFactoryForUser.Object).As<IDestinationServiceFactoryForUser>();
            containerBuilder.RegisterInstance(sourceServiceFactoryForUser.Object).As<ISourceServiceFactoryForUser>();
            containerBuilder.RegisterInstance(sourceServiceFactoryForAdmin.Object).As<ISourceServiceFactoryForAdmin>();
            containerBuilder.RegisterType<DocumentTagger>().As<IDocumentTagger>();
            containerBuilder.RegisterType<ImageSynchronizationExecutor>().As<IExecutor<IImageSynchronizationConfiguration>>();

            containerBuilder.RegisterInstance(new EmptyLogger()).As<IAPILog>();
            containerBuilder.RegisterInstance(fieldMappings.Object).As<IFieldMappings>();
            containerBuilder.RegisterInstance(_imageFileRepository.Object).As<IImageFileRepository>();

            IContainer container = containerBuilder.Build();
            _dataReaderFactory = container.Resolve<ISourceWorkspaceDataReaderFactory>();
            _executor = container.Resolve<IExecutor<IImageSynchronizationConfiguration>>();
        }

        [Test]
        public async Task ItShouldSuccessfullyRunImportAndTagDocuments()
        {
            const int newBatchArtifactId = 1001;
            const int folderArtifactId = 1002;
            const int totalItemsCount = 10;
            const int startIndex = 0;

            Mock<IBatch> batch = SetupNewBatch(newBatchArtifactId, totalItemsCount);

            IList<int> documentIds = Enumerable.Range(startIndex, totalItemsCount).ToList();
            CreateExportBlock(documentIds);

            ISourceWorkspaceDataReader dataReader = _dataReaderFactory.CreateImageSourceWorkspaceDataReader(batch.Object, CancellationToken.None);
            _importBulkArtifactJob.SetupGet(x => x.ItemStatusMonitor).Returns(dataReader.ItemStatusMonitor);
            _importBulkArtifactJob.Setup(x => x.Execute()).Callback(() =>
            {
                for (int i = 0; i < totalItemsCount; i++)
                {
                    dataReader.Read();
                }
                _importBulkArtifactJob.Raise(x => x.OnComplete += null, _emptyJobStatistsics);
            });

            SetupFieldQueryResult();
            SetupFolderPaths(folderArtifactId);
            SetupTaggingOfDocuments(totalItemsCount);
            SetupImageFileRepository(documentIds);

            // act
            ExecutionResult result = await _executor.ExecuteAsync(_config, CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            _objectManagerMock.Verify();
            _objectManagerMock.Verify(x => x.CreateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID,
                It.Is<CreateRequest>(cr => cr.ObjectType.Guid == JobHistoryErrorObjectTypeGuid)), Times.Never);
            _importBulkArtifactJob.Verify(x => x.Execute(), Times.Once);
            _rdoManagerMock.Verify();
        }

        [Test]
        public async Task ItShouldReportItemLevelErrors()
        {
            const int newBatchArtifactId = 1001;
            const int folderArtifactId = 1002;
            const int jobHistoryErrorArtifactId = 1003;
            const int totalItemsCount = 10;
            const int numberOfErrors = 4;
            int completedItems = totalItemsCount - numberOfErrors;
            const int startIndex = 0;

            Mock<IBatch> batch = SetupNewBatch(newBatchArtifactId, totalItemsCount);

            IList<int> documentIds = Enumerable.Range(startIndex, totalItemsCount).ToList();
            RelativityObjectSlim[] exportBlock = CreateExportBlock(documentIds);

            batch.SetupGet(x => x.TotalDocumentsCount).Returns(totalItemsCount);
            ISourceWorkspaceDataReader dataReader = _dataReaderFactory.CreateImageSourceWorkspaceDataReader(batch.Object, CancellationToken.None);
            List<RelativityObjectSlim> failedDocuments = exportBlock.Take(numberOfErrors).ToList();
            _importBulkArtifactJob.SetupGet(x => x.ItemStatusMonitor).Returns(dataReader.ItemStatusMonitor);
            _importBulkArtifactJob.Setup(x => x.Execute()).Callback(() =>
            {
                foreach (RelativityObjectSlim document in failedDocuments)
                {
                    dataReader.Read();
                    _importBulkArtifactJob.Raise(x => x.OnItemLevelError += null, new ItemLevelError(
                        document.Values[0].ToString(),
                        "Some weird error message."
                    ));
                }

                for (int i = 0; i < completedItems; i++)
                {
                    dataReader.Read();
                }

                _importBulkArtifactJob.Raise(x => x.OnComplete += null, _emptyJobStatistsics);
            });

            SetupFieldQueryResult();
            SetupFolderPaths(folderArtifactId);
            SetupTaggingOfDocuments(completedItems);
            SetupImageFileRepository(documentIds);

            MassCreateResult massCreateResult = new MassCreateResult()
            {
                Success = true,
                Objects = new List<RelativityObjectRef>()
                {
                    new RelativityObjectRef()
                    {
                        ArtifactID = jobHistoryErrorArtifactId
                    }
                }
            };
            _objectManagerMock.Setup(x => x.CreateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID,
                    It.Is<MassCreateRequest>(cr => cr.ValueLists.Count == failedDocuments.Count))).ReturnsAsync(massCreateResult)
                    .Verifiable();

            // act
            ExecutionResult result = await _executor.ExecuteAsync(_config, CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            result.Status.Should().Be(ExecutionStatus.CompletedWithErrors);
            _importBulkArtifactJob.Verify(x => x.Execute(), Times.Once);
            _objectManagerMock.Verify();
            _rdoManagerMock.Verify();
        }

        [Test]
        public async Task ItShouldReportJobLevelError()
        {
            const int newBatchArtifactId = 1001;
            const int jobHistoryErrorArtifactId = 1003;
            const int totalItemsCount = 10;

            SetupNewBatch(newBatchArtifactId, totalItemsCount);

            ItemStatusMonitor itemStatusMonitor = new ItemStatusMonitor();
            _importBulkArtifactJob.SetupGet(x => x.ItemStatusMonitor).Returns(itemStatusMonitor);
            _importBulkArtifactJob.Setup(x => x.Execute()).Callback(() =>
            {
                _importBulkArtifactJob.Raise(x => x.OnFatalException += null, _emptyJobStatistsics);
                _importBulkArtifactJob.Raise(x => x.OnComplete += null, _emptyJobStatistsics);
            });

            MassCreateResult massCreateResult = new MassCreateResult()
            {
                Success = true,
                Objects = new List<RelativityObjectRef>()
                {
                    new RelativityObjectRef()
                    {
                        ArtifactID = jobHistoryErrorArtifactId
                    }
                }
            };
            _objectManagerMock.Setup(x => x.CreateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID,
                    It.Is<MassCreateRequest>(cr => cr.ValueLists.Count == 1))).ReturnsAsync(massCreateResult)
                    .Verifiable();

            // act
            ExecutionResult result = await _executor.ExecuteAsync(_config, CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            result.Status.Should().Be(ExecutionStatus.Failed);
            _importBulkArtifactJob.Verify(x => x.Execute(), Times.Once);
            _objectManagerMock.Verify();
            _rdoManagerMock.Verify();
        }

        [Test]
        public async Task ItShouldCancelJob()
        {
            const int newBatchArtifactId = 1001;
            const int totalItemsCount = 10;

            SetupNewBatch(newBatchArtifactId, totalItemsCount);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CompositeCancellationToken compositeCancellationToken = new CompositeCancellationToken(tokenSource.Token, CancellationToken.None, new EmptyLogger());

            // act
            tokenSource.Cancel();
            ExecutionResult result = await _executor.ExecuteAsync(_config, compositeCancellationToken).ConfigureAwait(false);

            // assert
            _importBulkArtifactJob.Verify(x => x.Execute(), Times.Never);
            _rdoManagerMock.Verify(x => x.GetAsync<SyncBatchRdo>(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            result.Status.Should().Be(ExecutionStatus.Canceled);
        }

        private void SetupTaggingOfDocuments(int numberOfDocumentsToTag)
        {
            var taggingResult = new MassUpdateResult
            {
                Success = true
            };
            _objectManagerMock.Setup(x => x.UpdateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID,
                    It.Is<MassUpdateByObjectIdentifiersRequest>(request => request.Objects.Count == numberOfDocumentsToTag),
                    It.Is<MassUpdateOptions>(options => options.UpdateBehavior == FieldUpdateBehavior.Merge), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(taggingResult)
                    .Verifiable();
            _objectManagerMock.Setup(x => x.UpdateAsync(_DESTINATION_WORKSPACE_ARTIFACT_ID,
                    It.IsAny<MassUpdateByCriteriaRequest>(),
                    It.Is<MassUpdateOptions>(options => options.UpdateBehavior == FieldUpdateBehavior.Merge), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(taggingResult)
                    .Verifiable();
        }

        private void SetupFolderPaths(int folderArtifactId)
        {
            var folderPaths = new List<FolderPath>()
            {
                new FolderPath()
                {
                    ArtifactID = folderArtifactId,
                    FullPath = "/"
                }
            };
            _folderManagerMock.Setup(x => x.GetFullPathListAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<List<int>>()))
                .ReturnsAsync(folderPaths);
        }

        private void SetupFieldQueryResult()
        {
            var fieldQueryResult = new QueryResultSlim()
            {
                Objects = new List<RelativityObjectSlim>()
                {
                    new RelativityObjectSlim()
                    {
                        Values = new List<object>()
                        {
                            "Control Number",
                            "Fixed-Length Text"
                        }
                    }
                }
            };
            _objectManagerMock.Setup(x => x.QuerySlimAsync(_SOURCE_WORKSPACE_ARTIFACT_ID,
                    It.Is<QueryRequest>(qr => qr.Condition.Contains("Control Number")),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                    .ReturnsAsync(fieldQueryResult)
                    .Verifiable();
        }

        private RelativityObjectSlim[] CreateExportBlock(IList<int> documentIds)
        {
            RelativityObjectSlim[] exportBlock = documentIds.Select(x => new RelativityObjectSlim()
            {
                ArtifactID = x,
                Values = new List<object>()
                {
                    // Order does matter.
                    $"CONTROL_NUMBER_{x}",
                    true,
                    "HTML File"
                }
            }).ToArray();

            _objectManagerMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(_SOURCE_WORKSPACE_ARTIFACT_ID,
                    It.IsAny<Guid>(),
                    documentIds.Count,
                    It.IsAny<int>()))
                    .ReturnsAsync(exportBlock)
                    .Verifiable();

            return exportBlock;
        }

        private Mock<IBatch> SetupNewBatch(int newBatchArtifactId, int totalItemsCount)
        {
            Mock<IBatch> batch = new Mock<IBatch>();
            batch.SetupGet(x => x.TotalDocumentsCount).Returns(totalItemsCount);

            const int numberOfNewBatches = 1;
            QueryResult queryResultForNewBatches = new QueryResult()
            {
                Objects = new List<RelativityObject>()
                {
                    new RelativityObject
                    {
                        ArtifactID = newBatchArtifactId
                    }
                },
                TotalCount = numberOfNewBatches
            };

            _objectManagerMock.Setup(x => x.QueryAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(q => q.ObjectType.Guid == BatchObjectTypeGuid && q.Condition.Contains("New")), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(queryResultForNewBatches)
                .Verifiable();

            _objectManagerMock.Setup(x => x.QueryAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(q => q.ObjectType.Guid == BatchObjectTypeGuid && q.Condition.Contains("Paused")), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResult { Objects = new List<RelativityObject>(), ResultCount = 0, TotalCount = 0 })
                .Verifiable();

            _objectManagerMock.Setup(x => x.QuerySlimAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(q => q.ObjectType.Guid == BatchObjectTypeGuid && q.Condition.Contains("Completed")), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResultSlim { Objects = new List<RelativityObjectSlim>(), ResultCount = 0, TotalCount = 0 })
                .Verifiable();

            _objectManagerMock.Setup(x => x.QuerySlimAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(q => q.ObjectType.Guid == BatchObjectTypeGuid && q.Condition.Contains("Completed With Errors")), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResultSlim { Objects = new List<RelativityObjectSlim>(), ResultCount = 0, TotalCount = 0 })
                .Verifiable();

            _rdoManagerMock.Setup(x => x.GetAsync<SyncBatchRdo>(_SOURCE_WORKSPACE_ARTIFACT_ID, newBatchArtifactId))
                .ReturnsAsync(new SyncBatchRdo
                {
                    ArtifactId = newBatchArtifactId,
                    TotalDocumentsCount = totalItemsCount
                })
                .Verifiable();

            return batch;
        }

        private void SetupImageFileRepository(IList<int> documentIds)
        {
            var imageFiles = documentIds.Select(x => new ImageFile(x, x.ToString(), $"\\\\location{x}", $"name{x}", 100)).ToList();

            _imageFileRepository.Setup(x => x.QueryImagesForDocumentsAsync(_SOURCE_WORKSPACE_ARTIFACT_ID,
                It.Is<int[]>(d => d.Length == documentIds.Count), It.IsAny<QueryImagesOptions>()))
                .ReturnsAsync(imageFiles);
        }
    }
}
