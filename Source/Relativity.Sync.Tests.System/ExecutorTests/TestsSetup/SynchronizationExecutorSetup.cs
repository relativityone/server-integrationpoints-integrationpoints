using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using kCura.Relativity.DataReaderClient;
using NUnit.Framework;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Sync.Transfer;
using ImportJobFactory = Relativity.Sync.Tests.System.Core.Helpers.ImportJobFactory;

namespace Relativity.Sync.Tests.System.ExecutorTests.TestsSetup
{
    internal class ExecutorTestSetup
    {
        public ExecutorTestSetup(TestEnvironment environment, ServiceFactory serviceFactory)
        {
            Environment = environment;
            ServiceFactory = serviceFactory;
            Configuration = new ConfigurationStub();
        }

        public TestEnvironment Environment { get; }

        public ServiceFactory ServiceFactory { get; }

        public ConfigurationStub Configuration { get; }

        public IContainer Container { get; private set; }

        public WorkspaceRef SourceWorkspace { get; private set; }

        public WorkspaceRef DestinationWorkspace { get; private set; }

        public int TotalDataCount { get; private set; }

        public ExecutorTestSetup ForWorkspaces(string sourceWorkspaceName, string destinationWorkspaceName)
        {
            SourceWorkspace = Environment.CreateWorkspaceWithFieldsAsync(sourceWorkspaceName).GetAwaiter().GetResult();
            DestinationWorkspace = Environment.CreateWorkspaceWithFieldsAsync(destinationWorkspaceName).GetAwaiter().GetResult();

            Configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
            Configuration.DestinationWorkspaceArtifactId = DestinationWorkspace.ArtifactID;

            return this;
        }

        public ExecutorTestSetup ImportData(Dataset dataSet, bool extractedText = false, bool natives = false)
        {
            ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImportDataTable(dataSet, extractedText, natives);

            new ImportHelper(ServiceFactory).ImportDataAsync(SourceWorkspace.ArtifactID, dataTableWrapper).GetAwaiter().GetResult();

            TotalDataCount = dataTableWrapper.Data.Rows.Count;

            TridentHelper.UpdateFilePathToLocalIfNeeded(SourceWorkspace.ArtifactID, dataSet, natives);

            return this;
        }

        public ExecutorTestSetup ImportImageData(Dataset dataSet)
        {
            ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataSet);

            new ImportHelper(ServiceFactory).ImportDataAsync(SourceWorkspace.ArtifactID, dataTableWrapper).GetAwaiter().GetResult();

            TotalDataCount = dataTableWrapper.Data.Rows.Count;

            TridentHelper.UpdateFilePathToLocalIfNeeded(SourceWorkspace.ArtifactID, dataSet);

            return this;
        }

        public ExecutorTestSetup ImportMetadata(ImportDataTableWrapper dataTableWrapper)
        {
            ImportBulkArtifactJob documentImportJob = ImportJobFactory.CreateNonNativesDocumentImportJob(
                SourceWorkspace.ArtifactID,
                Rdos.GetRootFolderInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID).GetAwaiter().GetResult(),
                dataTableWrapper);

            ImportJobExecutor.ExecuteAsync(documentImportJob).GetAwaiter().GetResult();

            TotalDataCount = dataTableWrapper.Data.Rows.Count;

            return this;
        }

        public ExecutorTestSetup SetupDocumentConfiguration(
            Func<int, int, List<FieldMap>> fieldMapProvider,
            string savedSearchName = "All Documents",
            ImportOverwriteMode overwriteMode = ImportOverwriteMode.AppendOnly,
            FieldOverlayBehavior overlayBehavior = FieldOverlayBehavior.UseFieldSettings,
            ImportNativeFileCopyMode nativeFileCopyMode = ImportNativeFileCopyMode.CopyFiles,
            DestinationFolderStructureBehavior folderStructure = DestinationFolderStructureBehavior.None,
            int batchSize = 0,
            int totalRecordsCount = 0,
            string folderPathField = "",
            Guid exportRunId = default)
        {
            int jobHistoryArtifactId = Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, $"JobHistory.{Guid.NewGuid()}").GetAwaiter().GetResult();

            Configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
            Configuration.DestinationWorkspaceArtifactId = DestinationWorkspace.ArtifactID;
            Configuration.DataSourceArtifactId = Rdos.GetSavedSearchInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, savedSearchName).GetAwaiter().GetResult();
            Configuration.DestinationFolderStructureBehavior = folderStructure;
            Configuration.FolderPathField = folderPathField;

            Configuration.ExportRunId = exportRunId;

            Configuration.JobHistoryArtifactId = jobHistoryArtifactId;
            Configuration.DestinationFolderArtifactId = Rdos.GetRootFolderInstanceAsync(ServiceFactory, DestinationWorkspace.ArtifactID).GetAwaiter().GetResult();
            Configuration.SendEmails = false;

            Configuration.TotalRecordsCount = totalRecordsCount == 0 ? TotalDataCount : totalRecordsCount;
            Configuration.SyncBatchSize = batchSize == 0 ? TotalDataCount : batchSize;
            Configuration.ImportOverwriteMode = overwriteMode;
            Configuration.FieldOverlayBehavior = overlayBehavior;

            // Native Configuration
            Configuration.ImportNativeFileCopyMode = nativeFileCopyMode;

            Configuration.SetFieldMappings(fieldMapProvider(SourceWorkspace.ArtifactID, DestinationWorkspace.ArtifactID));

            return this;
        }

        public ExecutorTestSetup SetupImageConfiguration(
            Func<int, int, List<FieldMap>> fieldMapProvider,
            string savedSearchName = "All Documents",
            ImportOverwriteMode overwriteMode = ImportOverwriteMode.AppendOnly,
            FieldOverlayBehavior overlayBehavior = FieldOverlayBehavior.UseFieldSettings,
            ImportImageFileCopyMode imageFileCopyMode = ImportImageFileCopyMode.CopyFiles,
            int totalRecordsCount = 0,
            int batchSize = 0)
        {
            int jobHistoryArtifactId = Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, $"JobHistory.{Guid.NewGuid()}").GetAwaiter().GetResult();

            Configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
            Configuration.DestinationWorkspaceArtifactId = DestinationWorkspace.ArtifactID;
            Configuration.DataSourceArtifactId = Rdos.GetSavedSearchInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, savedSearchName).GetAwaiter().GetResult();

            Configuration.JobHistoryArtifactId = jobHistoryArtifactId;
            Configuration.DestinationFolderArtifactId = Rdos.GetRootFolderInstanceAsync(ServiceFactory, DestinationWorkspace.ArtifactID).GetAwaiter().GetResult();
            Configuration.SendEmails = false;

            Configuration.TotalRecordsCount = totalRecordsCount == 0 ? TotalDataCount : totalRecordsCount;
            Configuration.SyncBatchSize = batchSize == 0 ? TotalDataCount : batchSize;
            Configuration.ImportOverwriteMode = overwriteMode;
            Configuration.FieldOverlayBehavior = overlayBehavior;

            // Image Configuration
            Configuration.ImportImageFileCopyMode = imageFileCopyMode;
            Configuration.ImageImport = true;

            Configuration.SetFieldMappings(fieldMapProvider(SourceWorkspace.ArtifactID, DestinationWorkspace.ArtifactID));

            return this;
        }

        public ExecutorTestSetup SetupContainer(Action<ContainerBuilder> registerAction = null)
        {
            Configuration.SyncConfigurationArtifactId =
                Rdos.CreateSyncConfigurationRdoAsync(
                    SourceWorkspace.ArtifactID,
                    Configuration,
                    TestLogHelper.GetLogger())
                .GetAwaiter().GetResult();

            Container = ContainerHelper.Create(Configuration, toggleProvider: null, containerBuilder =>
            {
                if (registerAction != null)
                {
                    registerAction(containerBuilder);
                }

                containerBuilder.RegisterInstance(new ImportApiFactoryStub()).As<IImportApiFactory>();
            });

            return this;
        }

        public ExecutorTestSetup PrepareBatches()
        {
            return this
                .ExecutePreRequisteExecutor<IDataSourceSnapshotConfiguration>()
                .ExecutePreRequisteExecutor<ISnapshotPartitionConfiguration>();
        }

        public ExecutorTestSetup SetupDestinationWorkspaceTag()
        {
            return this
                .SetDestinationWorkspaceTagArtifactId()
                .ExecutePreRequisteExecutor<IDestinationWorkspaceTagsCreationConfiguration>();
        }

        public ExecutorTestSetup ExecutePreRequisteExecutor<T>() where T : class, Configuration.IConfiguration
        {
            IExecutor<T> executor = Container.Resolve<IExecutor<T>>();

            ExecutionResult sourceWorkspaceTagsCreationExecutorResult =
                executor.ExecuteAsync(
                    Configuration as T,
                    CompositeCancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(ExecutionStatus.Completed, sourceWorkspaceTagsCreationExecutorResult.Status);

            return this;
        }

        public ExecutorTestSetup SetDocumentTracking()
        {
            // Replacing DocumentTagRepository with TrackingDocumentTagRepository. I need a shower when I'm done...
            ContainerBuilder overrideContainerBuilder = new ContainerBuilder();
            Container.ComponentRegistry.Registrations.Where(cr => cr.Activator.LimitType != typeof(DocumentTagRepository)).ForEach(cr => overrideContainerBuilder.RegisterComponent(cr));
            Container.ComponentRegistry.Sources.ForEach(rs => overrideContainerBuilder.RegisterSource(rs));
            overrideContainerBuilder.RegisterTypes(typeof(TrackingDocumentTagRepository)).As<IDocumentTagRepository>();

            Container = overrideContainerBuilder.Build();

            return this;
        }

        public ExecutorTestSetup SetSupportedByViewer()
        {
            // Replacing FileInfoFieldsBuilder with NullSupportedByViewerFileInfoFieldsBuilder. Kids, don't do it in your code.
            ContainerBuilder overrideContainerBuilder = new ContainerBuilder();
            Container.ComponentRegistry.Registrations.Where(cr => cr.Activator.LimitType != typeof(NativeInfoFieldsBuilder)).ForEach(cr => overrideContainerBuilder.RegisterComponent(cr));
            Container.ComponentRegistry.Sources.ForEach(rs => overrideContainerBuilder.RegisterSource(rs));
            overrideContainerBuilder.RegisterTypes(typeof(NullSupportedByViewerFileInfoFieldsBuilder)).As<INativeSpecialFieldBuilder>();

            Container = overrideContainerBuilder.Build();

            return this;
        }

        private ExecutorTestSetup SetDestinationWorkspaceTagArtifactId()
        {
            IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository = Container.Resolve<IDestinationWorkspaceTagRepository>();

            Configuration.DestinationWorkspaceTagArtifactId = destinationWorkspaceTagRepository.CreateAsync(
                SourceWorkspace.ArtifactID,
                DestinationWorkspace.ArtifactID,
                DestinationWorkspace.Name).GetAwaiter().GetResult().ArtifactId;

            return this;
        }
    }
}
