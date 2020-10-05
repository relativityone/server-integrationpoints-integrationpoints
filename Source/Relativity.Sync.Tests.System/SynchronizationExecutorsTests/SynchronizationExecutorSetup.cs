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
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImportJobFactory = Relativity.Sync.Tests.System.Core.Helpers.ImportJobFactory;

namespace Relativity.Sync.Tests.System.SynchronizationExecutorsTests
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

			//SourceWorkspace = new WorkspaceRef(1018075) { Name = sourceWorkspaceName };
			//DestinationWorkspace = new WorkspaceRef(1018076) { Name = destinationWorkspaceName };

			Configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
			Configuration.DestinationWorkspaceArtifactId = DestinationWorkspace.ArtifactID;

			return this;
		}

		public SynchronizationExecutorSetup ImportData(Dataset dataSet, bool extractedText = false, bool natives = false)
		{
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImportDataTable(dataSet, extractedText, natives);

			//ImportJobErrors importJobErrors = new ImportHelper(ServiceFactory)
			//	.ImportDataAsync(SourceWorkspace.ArtifactID, dataTableWrapper).GetAwaiter().GetResult();

			//Assert.IsTrue(importJobErrors.Success, $"IAPI errors: {string.Join(global::System.Environment.NewLine, importJobErrors.Errors)}");

			UpdateFilePathToLocalIfNeeded(dataSet);

			TotalDataCount = dataTableWrapper.Data.Rows.Count;

			return this;
		}

		public SynchronizationExecutorSetup ImportData(ImportDataTableWrapper table)
		{
			ImportBulkArtifactJob documentImportJob = ImportJobFactory.CreateNonNativesDocumentImportJob(
				SourceWorkspace.ArtifactID,
				Rdos.GetRootFolderInstance(ServiceFactory, SourceWorkspace.ArtifactID).GetAwaiter().GetResult(),
				table);

			ImportJobErrors importErrors = ImportJobExecutor.ExecuteAsync(documentImportJob).GetAwaiter().GetResult();

			Assert.IsTrue(importErrors.Success, $"{importErrors.Errors.Count} errors occurred during document upload: {importErrors}");

			TotalDataCount = table.Data.Rows.Count;

			return this;
		}

		public SynchronizationExecutorSetup SetupDocumentConfiguration(
			List<FieldMap> fieldMap,
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
			Configuration.DataSourceArtifactId = Rdos.GetSavedSearchInstance(ServiceFactory, SourceWorkspace.ArtifactID, savedSearchName).GetAwaiter().GetResult();
			Configuration.DestinationFolderStructureBehavior = folderStructure;

			Configuration.JobHistoryArtifactId = jobHistoryArtifactId;
			Configuration.DestinationFolderArtifactId = Rdos.GetRootFolderInstance(ServiceFactory, DestinationWorkspace.ArtifactID).GetAwaiter().GetResult();
			Configuration.SendEmails = false;

			Configuration.TotalRecordsCount = totalRecordsCount == 0 ? TotalDataCount : totalRecordsCount;
			Configuration.BatchSize = batchSize == 0 ? TotalDataCount : batchSize;
			Configuration.SyncConfigurationArtifactId = Rdos.CreateSyncConfigurationInstance(ServiceFactory, SourceWorkspace.ArtifactID, jobHistoryArtifactId, fieldMap).GetAwaiter().GetResult();
			Configuration.ImportOverwriteMode = overwriteMode;
			Configuration.FieldOverlayBehavior = overlayBehavior;
			Configuration.ImportNativeFileCopyMode = nativeFileCopyMode;

			Configuration.SetFieldMappings(fieldMap);

			return this;
		}

		public SynchronizationExecutorSetup SetupContainer()
		{
			Container = ContainerHelper.Create(Configuration, containerBuilder =>
			{
				containerBuilder.RegisterInstance(new ImportApiFactoryStub()).As<IImportApiFactory>();
			});

			return this;
		}

		public SynchronizationExecutorSetup SetDestinationWorkspaceTagArtifactId()
		{
			IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository = Container.Resolve<IDestinationWorkspaceTagRepository>();

			Configuration.DestinationWorkspaceTagArtifactId = destinationWorkspaceTagRepository.CreateAsync(
				SourceWorkspace.ArtifactID,
				DestinationWorkspace.ArtifactID,
				DestinationWorkspace.Name).GetAwaiter().GetResult().ArtifactId;

			return this;
		}

		public SynchronizationExecutorSetup CreateSourceTagsInDestinationWorkspace()
		{
			IExecutor<IDestinationWorkspaceTagsCreationConfiguration> destinationWorkspaceTagsCreationExecutor =
				Container.Resolve<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();

			ExecutionResult sourceWorkspaceTagsCreationExecutorResult = destinationWorkspaceTagsCreationExecutor.ExecuteAsync(Configuration, CancellationToken.None)
				.GetAwaiter().GetResult();

			Assert.AreEqual(ExecutionStatus.Completed, sourceWorkspaceTagsCreationExecutorResult.Status);

			return this;
		}

		public SynchronizationExecutorSetup CreateDocumentDataSourceSnapshot()
		{
			IExecutor<IDocumentDataSourceSnapshotConfiguration> dataSourceSnapshotExecutor = Container.Resolve<IExecutor<IDocumentDataSourceSnapshotConfiguration>>();

			ExecutionResult dataSourceSnapshotExecutorResult = dataSourceSnapshotExecutor.ExecuteAsync(Configuration, CancellationToken.None)
				.GetAwaiter().GetResult();

			Assert.AreEqual(ExecutionStatus.Completed, dataSourceSnapshotExecutorResult.Status);

			return this;
		}

		public SynchronizationExecutorSetup CreateImageDataSourceSnapshot()
		{
			IExecutor<IImageDataSourceSnapshotConfiguration> dataSourceSnapshotExecutor = Container.Resolve<IExecutor<IImageDataSourceSnapshotConfiguration>>();

			ExecutionResult dataSourceSnapshotExecutorResult = dataSourceSnapshotExecutor.ExecuteAsync(Configuration, CancellationToken.None)
				.GetAwaiter().GetResult();

			Assert.AreEqual(ExecutionStatus.Completed, dataSourceSnapshotExecutorResult.Status);

			return this;
		}

		public SynchronizationExecutorSetup PartitionDataSourceSnapshot()
		{
			IExecutor<ISnapshotPartitionConfiguration> snapshotPartitionExecutor = Container.Resolve<IExecutor<ISnapshotPartitionConfiguration>>();

			ExecutionResult snapshotPartitionExecutorResult = snapshotPartitionExecutor.ExecuteAsync(Configuration, CancellationToken.None)
				.GetAwaiter().GetResult();

			Assert.AreEqual(ExecutionStatus.Completed, snapshotPartitionExecutorResult.Status);

			return this;
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

		private void UpdateFilePathToLocalIfNeeded(Dataset dataSet)
		{
			if (AppSettings.IsSettingsFileSet)
			{
				#region Hopper Instance workaround explanation

				//This workaround was provided to omit Hopper Instance restrictions. IAPI which is executing on agent can't access file based on file location in database like '\\emttest\DefaultFileRepository\...'.
				//Hopper is closed for outside traffic so there is no possibility to access fileshare from Trident Agent. Jira related to this https://jira.kcura.com/browse/DEVOPS-70257.
				//If we decouple Sync from RIP and move it to RAP problem probably disappears. Right now as workaround we change on database this relative Fileshare path to local,
				//where out test data are stored. So we assume in testing that push is working correctly, but whole flow (metadata, etc.) is under tests.

				#endregion
				using (SqlConnection connection = CreateConnectionFromAppConfig(SourceWorkspace.ArtifactID))
				{
					connection.Open();

					const string sqlStatement =
						@"UPDATE [File] SET Location = CONCAT(@LocalFilePath, '\', [Filename])";
					SqlCommand command = new SqlCommand(sqlStatement, connection);
					command.Parameters.AddWithValue("LocalFilePath", dataSet.FolderPath);

					command.ExecuteNonQuery();
				}
			}
		}

		private static SqlConnection CreateConnectionFromAppConfig(int workspaceArtifactID)
		{
			SecureString password = new NetworkCredential("", AppSettings.SqlPassword).SecurePassword;
			password.MakeReadOnly();
			SqlCredential credential = new SqlCredential(AppSettings.SqlUsername, password);

			return new SqlConnection(
				GetWorkspaceConnectionString(workspaceArtifactID),
				credential);
		}

		private static string GetWorkspaceConnectionString(int workspaceArtifactID) => $"Data Source={AppSettings.SqlServer};Initial Catalog=EDDS{workspaceArtifactID}";

		#endregion
	}
}
