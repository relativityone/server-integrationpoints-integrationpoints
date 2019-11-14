using System.Linq;
using System.Reflection;
using System.Threading;
using Autofac;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.ExecutionConstrains.SumReporting;
using Relativity.Sync.Executors.PermissionCheck;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class ExecutorsInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<SourceCaseTagService>().As<ISourceCaseTagService>();
			builder.RegisterType<RelativitySourceCaseTagRepository>().As<IRelativitySourceCaseTagRepository>();
			builder.RegisterType<SourceJobTagService>().As<ISourceJobTagService>();
			builder.RegisterType<JobHistoryNameQuery>().As<IJobHistoryNameQuery>();
			builder.RegisterType<RelativitySourceJobTagRepository>().As<IRelativitySourceJobTagRepository>();
			builder.RegisterType<DestinationWorkspaceTagRepository>().As<IDestinationWorkspaceTagRepository>();
			builder.RegisterType<SyncObjectTypeManager>().As<ISyncObjectTypeManager>();
			builder.RegisterType<SyncFieldManager>().As<ISyncFieldManager>();
			builder.RegisterType<SourceWorkspaceTagRepository>().As<ISourceWorkspaceTagRepository>();
			builder.RegisterType<DocumentTagRepository>().As<IDocumentTagRepository>();
			builder.RegisterType<DestinationWorkspaceTagLinker>().As<IDestinationWorkspaceTagsLinker>();
			builder.RegisterType<FederatedInstance>().As<IFederatedInstance>();
			builder.RegisterType<WorkspaceNameQuery>().As<IWorkspaceNameQuery>();
			builder.RegisterType<TagNameFormatter>().As<ITagNameFormatter>();
			builder.RegisterType<WorkspaceNameValidator>().As<IWorkspaceNameValidator>();
			builder.RegisterType<TagSavedSearch>().As<ITagSavedSearch>();
			builder.RegisterType<TagSavedSearchFolder>().As<ITagSavedSearchFolder>();
			builder.RegisterType<BatchProgressHandlerFactory>().As<IBatchProgressHandlerFactory>();
			builder.Register(c => new BatchProgressUpdater(c.Resolve<ISyncLog>(), new SemaphoreSlimWrapper(new SemaphoreSlim(1)))).As<IBatchProgressUpdater>();
			builder.RegisterType<ImportJobFactory>().As<IImportJobFactory>();
			builder.RegisterType<ImportApiFactory>().As<IImportApiFactory>();

			builder.RegisterType<JobStartMetricsExecutorConstrains>().As<IExecutionConstrains<ISumReporterConfiguration>>();
			builder.RegisterType<JobStartMetricsExecutor>().As<IExecutor<ISumReporterConfiguration>>();
			builder.RegisterType<SourceWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<SourceWorkspaceTagsCreationExecutor>().As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<IDestinationWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceTagsCreationExecutor>().As<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceObjectTypesCreationExecutorConstrains>().As<IExecutionConstrains<IDestinationWorkspaceObjectTypesCreationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceObjectTypesCreationExecutor>().As<IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration>>();
			builder.RegisterType<ValidationExecutionConstrains>().As<IExecutionConstrains<IValidationConfiguration>>();
			builder.RegisterType<ValidationExecutor>().As<IExecutor<IValidationConfiguration>>();
			builder.RegisterType<PermissionCheckExecutionConstrains>().As<IExecutionConstrains<IPermissionsCheckConfiguration>>();
			builder.RegisterType<PermissionCheckExecutor>().As<IExecutor<IPermissionsCheckConfiguration>>();
			builder.RegisterType<DestinationWorkspaceSavedSearchCreationExecutionConstrains>().As<IExecutionConstrains<IDestinationWorkspaceSavedSearchCreationConfiguration>>();
			builder.RegisterType<DestinationWorkspaceSavedSearchCreationExecutor>().As<IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>>();
			builder.RegisterType<DataSourceSnapshotExecutor>().As<IExecutor<IDataSourceSnapshotConfiguration>>();
			builder.RegisterType<DataSourceSnapshotExecutionConstrains>().As<IExecutionConstrains<IDataSourceSnapshotConfiguration>>();
			builder.RegisterType<SnapshotPartitionExecutionConstrains>().As<IExecutionConstrains<ISnapshotPartitionConfiguration>>();
			builder.RegisterType<SnapshotPartitionExecutor>().As<IExecutor<ISnapshotPartitionConfiguration>>();
			builder.RegisterType<SynchronizationExecutionConstrains>().As<IExecutionConstrains<ISynchronizationConfiguration>>();
			builder.RegisterType<SynchronizationExecutor>().As<IExecutor<ISynchronizationConfiguration>>();
			builder.RegisterType<DataDestinationInitializationExecutor>().As<IExecutor<IDataDestinationInitializationConfiguration>>();
			builder.RegisterType<DataDestinationInitializationExecutionConstrains>().As<IExecutionConstrains<IDataDestinationInitializationConfiguration>>();
			builder.RegisterType<DataDestinationFinalizationExecutor>().As<IExecutor<IDataDestinationFinalizationConfiguration>>();
			builder.RegisterType<DataDestinationFinalizationExecutionConstrains>().As<IExecutionConstrains<IDataDestinationFinalizationConfiguration>>();
			builder.RegisterType<NotificationExecutionConstrains>().As<IExecutionConstrains<INotificationConfiguration>>();
			builder.RegisterType<NotificationExecutor>().As<IExecutor<INotificationConfiguration>>();
			builder.RegisterType<JobStatusConsolidationExecutionConstrains>().As<IExecutionConstrains<IJobStatusConsolidationConfiguration>>();
			builder.RegisterType<JobStatusConsolidationExecutor>().As<IExecutor<IJobStatusConsolidationConfiguration>>();
			builder.RegisterType<JobCleanupExecutorConstrains>().As<IExecutionConstrains<IJobCleanupConfiguration>>();
			builder.RegisterType<JobCleanupExecutor>().As<IExecutor<IJobCleanupConfiguration>>();

			builder.RegisterTypes(Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsAssignableTo<IPermissionCheck>()).ToArray()).As<IPermissionCheck>();

			builder.RegisterType<BatchRepository>().As<IBatchRepository>();
			builder.RegisterType<ProgressRepository>().As<IProgressRepository>();
			builder.RegisterType<SemaphoreSlimWrapper>().As<ISemaphoreSlim>();
		}
	}
}