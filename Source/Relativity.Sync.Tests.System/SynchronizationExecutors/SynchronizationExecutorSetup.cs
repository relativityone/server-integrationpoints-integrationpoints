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
using System;
using System.Collections.Generic;
using System.Linq;
using ImportJobFactory = Relativity.Sync.Tests.System.Core.Helpers.ImportJobFactory;

namespace Relativity.Sync.Tests.System.SynchronizationExecutors
{
    internal class SynchronizationExecutorSetup
	{
		public TestEnvironment Environment { get; }
		public ServiceFactory ServiceFactory { get; }
		public ConfigurationStub Configuration { get; }
		
		public IContainer Container { get; private set; }

		public WorkspaceRef SourceWorkspace { get; private set; }
		public WorkspaceRef DestinationWorkspace { get; private set; }

		public int TotalDataCount { get; private set; }

		public SynchronizationExecutorSetup(TestEnvironment environment, ServiceFactory serviceFactory)
		{
			Environment = environment;
			ServiceFactory = serviceFactory;
			Configuration = new ConfigurationStub();
		}

		public SynchronizationExecutorSetup ForWorkspaces(string sourceWorkspaceName, string destinationWorkspaceName)
		{
			SourceWorkspace = Environment.CreateWorkspaceWithFieldsAsync(sourceWorkspaceName).GetAwaiter().GetResult();
			DestinationWorkspace = Environment.CreateWorkspaceWithFieldsAsync(destinationWorkspaceName).GetAwaiter().GetResult();

			Configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
			Configuration.DestinationWorkspaceArtifactId = DestinationWorkspace.ArtifactID;

			return this;
		}

		public SynchronizationExecutorSetup ImportData(Dataset dataSet, bool extractedText = false, bool natives = false)
		{
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImportDataTable(dataSet, extractedText, natives);

			new ImportHelper(ServiceFactory).ImportDataAsync(SourceWorkspace.ArtifactID, dataTableWrapper).GetAwaiter().GetResult();
			
			TotalDataCount = dataTableWrapper.Data.Rows.Count;

			TridentHelper.UpdateFilePathToLocalIfNeeded(SourceWorkspace.ArtifactID, dataSet);

			return this;
		}

		public SynchronizationExecutorSetup ImportImageData(Dataset dataSet)
		{
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataSet);

			new ImportHelper(ServiceFactory).ImportDataAsync(SourceWorkspace.ArtifactID, dataTableWrapper).GetAwaiter().GetResult();
			
			TotalDataCount = dataTableWrapper.Data.Rows.Count;

			TridentHelper.UpdateFilePathToLocalIfNeeded(SourceWorkspace.ArtifactID, dataSet);

			return this;
		}

		public SynchronizationExecutorSetup ImportMetadata(ImportDataTableWrapper dataTableWrapper)
		{
			ImportBulkArtifactJob documentImportJob = ImportJobFactory.CreateNonNativesDocumentImportJob(
				SourceWorkspace.ArtifactID,
				Rdos.GetRootFolderInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID).GetAwaiter().GetResult(),
				dataTableWrapper);

			ImportJobExecutor.ExecuteAsync(documentImportJob).GetAwaiter().GetResult();

			TotalDataCount = dataTableWrapper.Data.Rows.Count;

			return this;
		}

		public SynchronizationExecutorSetup SetupDocumentConfiguration(
			Func<int, int, List<FieldMap>> fieldMapProvider,
			string savedSearchName = "All Documents",
			ImportOverwriteMode overwriteMode = ImportOverwriteMode.AppendOnly,
			FieldOverlayBehavior overlayBehavior = FieldOverlayBehavior.UseFieldSettings,
			ImportNativeFileCopyMode nativeFileCopyMode = ImportNativeFileCopyMode.CopyFiles,
			DestinationFolderStructureBehavior folderStructure = DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure,
			int batchSize = 0,
			int totalRecordsCount = 0)
		{
			int jobHistoryArtifactId = Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, $"JobHistory.{Guid.NewGuid()}").GetAwaiter().GetResult();

			Configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
			Configuration.DestinationWorkspaceArtifactId = DestinationWorkspace.ArtifactID;
			Configuration.DataSourceArtifactId = Rdos.GetSavedSearchInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, savedSearchName).GetAwaiter().GetResult();
			Configuration.DestinationFolderStructureBehavior = folderStructure;

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

		public SynchronizationExecutorSetup SetupImageConfiguration(
			Func<int, int, List<FieldMap>> fieldMapProvider,
			string savedSearchName = "All Documents",
			ImportOverwriteMode overwriteMode = ImportOverwriteMode.AppendOnly,
			FieldOverlayBehavior overlayBehavior = FieldOverlayBehavior.UseFieldSettings,
			ImportImageFileCopyMode imageFileCopyMode = ImportImageFileCopyMode.CopyFiles,
			int totalRecordsCount = 0,
			int batchSize = 0
			)
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
			
			//Image Configuration
			Configuration.ImportImageFileCopyMode = imageFileCopyMode;
			Configuration.ImageImport = true;

			Configuration.SetFieldMappings(fieldMapProvider(SourceWorkspace.ArtifactID, DestinationWorkspace.ArtifactID));

			return this;
		}

		public SynchronizationExecutorSetup SetupContainer()
		{
			Configuration.SyncConfigurationArtifactId = Rdos.CreateSyncConfigurationRdoAsync(SourceWorkspace.ArtifactID,
				Configuration, TestLogHelper.GetLogger()).GetAwaiter().GetResult();
			
			Container = ContainerHelper.Create(Configuration, toggleProvider: null, containerBuilder =>
			{
				containerBuilder.RegisterInstance(new ImportApiFactoryStub()).As<IImportApiFactory>();
			});

			return this;
		}

		public SynchronizationExecutorSetup ExecutePreSynchronizationExecutors()
		{
			return this
				.SetDestinationWorkspaceTagArtifactId()
				.CreateSourceTagsInDestinationWorkspace()
				.CreateDataSourceSnapshot()
				.PartitionDataSourceSnapshot();
		}

		public SynchronizationExecutorSetup SetDocumentTracking()
		{
			// Replacing DocumentTagRepository with TrackingDocumentTagRepository. I need a shower when I'm done...
			ContainerBuilder overrideContainerBuilder = new ContainerBuilder();
			Container.ComponentRegistry.Registrations.Where(cr => cr.Activator.LimitType != typeof(DocumentTagRepository)).ForEach(cr => overrideContainerBuilder.RegisterComponent(cr));
			Container.ComponentRegistry.Sources.ForEach(rs => overrideContainerBuilder.RegisterSource(rs));
			overrideContainerBuilder.RegisterTypes(typeof(TrackingDocumentTagRepository)).As<IDocumentTagRepository>();

			Container = overrideContainerBuilder.Build();

			return this;
		}

		public SynchronizationExecutorSetup SetSupportedByViewer()
		{
			// Replacing FileInfoFieldsBuilder with NullSupportedByViewerFileInfoFieldsBuilder. Kids, don't do it in your code.
			ContainerBuilder overrideContainerBuilder = new ContainerBuilder();
			Container.ComponentRegistry.Registrations.Where(cr => cr.Activator.LimitType != typeof(NativeInfoFieldsBuilder)).ForEach(cr => overrideContainerBuilder.RegisterComponent(cr));
			Container.ComponentRegistry.Sources.ForEach(rs => overrideContainerBuilder.RegisterSource(rs));
			overrideContainerBuilder.RegisterTypes(typeof(NullSupportedByViewerFileInfoFieldsBuilder)).As<INativeSpecialFieldBuilder>();

			Container = overrideContainerBuilder.Build();

			return this;
		}

		#region Helper Methods

		private SynchronizationExecutorSetup SetDestinationWorkspaceTagArtifactId()
		{
			IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository = Container.Resolve<IDestinationWorkspaceTagRepository>();

			Configuration.DestinationWorkspaceTagArtifactId = destinationWorkspaceTagRepository.CreateAsync(
				SourceWorkspace.ArtifactID,
				DestinationWorkspace.ArtifactID,
				DestinationWorkspace.Name).GetAwaiter().GetResult().ArtifactId;

			return this;
		}

		private SynchronizationExecutorSetup CreateSourceTagsInDestinationWorkspace()
		{
			IExecutor<IDestinationWorkspaceTagsCreationConfiguration> destinationWorkspaceTagsCreationExecutor =
				Container.Resolve<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();

			ExecutionResult sourceWorkspaceTagsCreationExecutorResult = destinationWorkspaceTagsCreationExecutor.ExecuteAsync(Configuration, CompositeCancellationToken.None)
				.GetAwaiter().GetResult();

			Assert.AreEqual(ExecutionStatus.Completed, sourceWorkspaceTagsCreationExecutorResult.Status);

			return this;
		}

		private SynchronizationExecutorSetup CreateDataSourceSnapshot()
		{
			IExecutor<IDataSourceSnapshotConfiguration> dataSourceSnapshotExecutor = Container.Resolve<IExecutor<IDataSourceSnapshotConfiguration>>();

			ExecutionResult dataSourceSnapshotExecutorResult = dataSourceSnapshotExecutor.ExecuteAsync(Configuration, CompositeCancellationToken.None)
				.GetAwaiter().GetResult();

			Assert.AreEqual(ExecutionStatus.Completed, dataSourceSnapshotExecutorResult.Status);

			return this;
		}

		public SynchronizationExecutorSetup PartitionDataSourceSnapshot()
		{
			IExecutor<ISnapshotPartitionConfiguration> snapshotPartitionExecutor = Container.Resolve<IExecutor<ISnapshotPartitionConfiguration>>();

			ExecutionResult snapshotPartitionExecutorResult = snapshotPartitionExecutor.ExecuteAsync(Configuration, CompositeCancellationToken.None)
				.GetAwaiter().GetResult();

			Assert.AreEqual(ExecutionStatus.Completed, snapshotPartitionExecutorResult.Status);

			return this;
		}
		#endregion
	}
}
