using Autofac;
using Relativity.AutomatedWorkflows.SDK;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.ExecutionConstrains.SumReporting;
using Relativity.Sync.Executors.DocumentTaggers;
using Relativity.Sync.Executors.PermissionCheck;
using Relativity.Sync.Executors.PreValidation;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Extensions;
using Relativity.Sync.HttpClient;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Toggles.Service;
using Relativity.Sync.Transfer.ADLS;
using Relativity.Sync.Transfer.FileMovementService;

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
            builder.RegisterType<DocumentTagger>().As<IDocumentTagger>();
            builder.RegisterType<ImportJobFactory>().As<IImportJobFactory>();
            builder.RegisterType<ImportApiFactory>().As<IImportApiFactory>();
            builder.RegisterType<ExtendedImportAPI>().As<IExtendedImportAPI>();

            builder.RegisterType<AutomatedWorkflowsManager>().As<IAutomatedWorkflowsManager>();

            builder.RegisterType<FieldMappingSummary>().As<IFieldMappingSummary>();
            builder.RegisterType<DocumentJobStartMetricsExecutorConstrains>().As<IExecutionConstrains<IDocumentJobStartMetricsConfiguration>>();
            builder.RegisterType<DocumentJobStartMetricsExecutor>().As<IExecutor<IDocumentJobStartMetricsConfiguration>>();
            builder.RegisterType<ImageJobStartMetricsExecutorConstrains>().As<IExecutionConstrains<IImageJobStartMetricsConfiguration>>();
            builder.RegisterType<ImageJobStartMetricsExecutor>().As<IExecutor<IImageJobStartMetricsConfiguration>>();

            builder.RegisterType<SourceWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>>();
            builder.RegisterType<SourceWorkspaceTagsCreationExecutor>().As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();
            builder.RegisterType<DestinationWorkspaceTagsCreationExecutionConstrains>().As<IExecutionConstrains<IDestinationWorkspaceTagsCreationConfiguration>>();
            builder.RegisterType<DestinationWorkspaceTagsCreationExecutor>().As<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();
            builder.RegisterType<DestinationWorkspaceObjectTypesCreationExecutorConstrains>().As<IExecutionConstrains<IDestinationWorkspaceObjectTypesCreationConfiguration>>();
            builder.RegisterType<DestinationWorkspaceObjectTypesCreationExecutor>().As<IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration>>();
            builder.RegisterType<PreValidationExecutionConstrains>().As<IExecutionConstrains<IPreValidationConfiguration>>();
            builder.RegisterType<PreValidationExecutor>().As<IExecutor<IPreValidationConfiguration>>();
            builder.RegisterType<ValidationExecutionConstrains>().As<IExecutionConstrains<IValidationConfiguration>>();
            builder.RegisterType<ValidationExecutor>().As<IExecutor<IValidationConfiguration>>();
            builder.RegisterType<PermissionCheckExecutionConstrains>().As<IExecutionConstrains<IPermissionsCheckConfiguration>>();
            builder.RegisterType<PermissionCheckExecutor>().As<IExecutor<IPermissionsCheckConfiguration>>();
            builder.RegisterType<DestinationWorkspaceSavedSearchCreationExecutionConstrains>().As<IExecutionConstrains<IDestinationWorkspaceSavedSearchCreationConfiguration>>();
            builder.RegisterType<DestinationWorkspaceSavedSearchCreationExecutor>().As<IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>>();

            builder.RegisterType<DataSourceSnapshotExecutor>().As<IExecutor<IDataSourceSnapshotConfiguration>>();
            builder.RegisterType<RetryDataSourceSnapshotExecutor>().As<IExecutor<IRetryDataSourceSnapshotConfiguration>>();

            builder.RegisterType<DataSourceSnapshotExecutionConstrains>().As<IExecutionConstrains<IDataSourceSnapshotConfiguration>>();
            builder.RegisterType<RetryDataSourceSnapshotExecutionConstrains>().As<IExecutionConstrains<IRetryDataSourceSnapshotConfiguration>>();

            builder.RegisterType<SnapshotPartitionExecutionConstrains>().As<IExecutionConstrains<ISnapshotPartitionConfiguration>>();
            builder.RegisterType<SnapshotPartitionExecutor>().As<IExecutor<ISnapshotPartitionConfiguration>>();

            builder.RegisterType<DocumentSynchronizationExecutionConstrains>().As<IExecutionConstrains<IDocumentSynchronizationConfiguration>>();
            builder.RegisterType<ImageSynchronizationExecutionConstrains>().As<IExecutionConstrains<IImageSynchronizationConfiguration>>();
            builder.RegisterType<DocumentSynchronizationExecutor>().As<IExecutor<IDocumentSynchronizationConfiguration>>();
            builder.RegisterType<ImageSynchronizationExecutor>().As<IExecutor<IImageSynchronizationConfiguration>>();

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
            builder.RegisterType<AutomatedWorkflowExecutorConstrains>().As<IExecutionConstrains<IAutomatedWorkflowTriggerConfiguration>>();
            builder.RegisterType<AutomatedWorkflowExecutor>().As<IExecutor<IAutomatedWorkflowTriggerConfiguration>>();

            builder.RegisterType<ConfigureDocumentSynchronizationExecutor>().As<IExecutor<IConfigureDocumentSynchronizationConfiguration>>();
            builder.RegisterType<ConfigureDocumentSynchronizationExecutionConstrains>().As<IExecutionConstrains<IConfigureDocumentSynchronizationConfiguration>>();
            builder.RegisterType<BatchDataSourcePreparationExecutor>().As<IExecutor<IBatchDataSourcePreparationConfiguration>>();
            builder.RegisterType<BatchDataSourcePreparationExecutionConstrains>().As<IExecutionConstrains<IBatchDataSourcePreparationConfiguration>>();
            builder.RegisterType<DocumentSynchronizationMonitorExecutor>().As<IExecutor<IDocumentSynchronizationMonitorConfiguration>>();
            builder.RegisterType<DocumentSynchronizationMonitorExecutionConstrains>().As<IExecutionConstrains<IDocumentSynchronizationMonitorConfiguration>>();

            builder.RegisterTypesInExecutingAssembly<IPermissionCheck>();
            builder.RegisterTypesInExecutingAssembly<IPreValidator>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<SyncToggles>().As<ISyncToggles>().SingleInstance();
            builder.RegisterType<AdlsMigrationStatus>().As<IAdlsMigrationStatus>().SingleInstance();
            builder.RegisterType<IsAdfTransferEnabled>().As<IIsAdfTransferEnabled>().SingleInstance();
            builder.RegisterType<HelperWrapper>().As<IHelperWrapper>().SingleInstance();
            builder.RegisterType<AdlsUploader>().As<IAdlsUploader>().SingleInstance();

            builder.RegisterType<BatchRepository>().As<IBatchRepository>();
            builder.RegisterType<ProgressRepository>().As<IProgressRepository>();
            builder.RegisterType<SemaphoreSlimWrapper>().As<ISemaphoreSlim>();

            builder.RegisterType<SharedServiceHttpClientFactory>().As<ISharedServiceHttpClientFactory>();
            builder.RegisterType<HttpClientRetryPolicyProvider>().As<IHttpClientRetryPolicyProvider>();
            builder.RegisterType<FmsInstanceSettingsService>().As<IFmsInstanceSettingsService>();
            builder.RegisterType<FmsClient>().As<IFmsClient>();
            builder.RegisterType<FmsRunner>().As<IFmsRunner>();
            builder.RegisterType<LoadFileGenerator>().As<ILoadFileGenerator>();
            builder.RegisterType<ItemLevelErrorHandler>().As<IItemLevelErrorHandler>();
            builder.RegisterType<ImportApiItemLevelErrorHandler>().As<IImportApiItemLevelErrorHandler>();
            builder.RegisterType<ItemLevelErrorHandlerFactory>().As<IItemLevelErrorHandlerFactory>();

            RegisterNonDocumentFlowComponents(builder);
        }

        private void RegisterNonDocumentFlowComponents(ContainerBuilder builder)
        {
            builder.RegisterType<NonDocumentObjectDataSourceSnapshotExecutionConstrains>().As<IExecutionConstrains<INonDocumentDataSourceSnapshotConfiguration>>();
            builder.RegisterType<NonDocumentObjectDataSourceSnapshotExecutor>().As<IExecutor<INonDocumentDataSourceSnapshotConfiguration>>();

            builder.RegisterType<ObjectLinkingSnapshotPartitionExecutionConstrains>().As<IExecutionConstrains<IObjectLinkingSnapshotPartitionConfiguration>>();
            builder.RegisterType<ObjectLinkingSnapshotPartitionExecutor>().As<IExecutor<IObjectLinkingSnapshotPartitionConfiguration>>();

            builder.RegisterType<NonDocumentJobStartMetricsExecutorConstrains>().As<IExecutionConstrains<INonDocumentJobStartMetricsConfiguration>>();
            builder.RegisterType<NonDocumentJobStartMetricsExecutor>().As<IExecutor<INonDocumentJobStartMetricsConfiguration>>();

            builder.RegisterType<NonDocumentSynchronizationExecutionConstrains>().As<IExecutionConstrains<INonDocumentSynchronizationConfiguration>>();
            builder.RegisterType<NonDocumentSynchronizationExecutor>().As<IExecutor<INonDocumentSynchronizationConfiguration>>();

            builder.RegisterType<NonDocumentObjectLinkingExecutionConstrains>().As<IExecutionConstrains<INonDocumentObjectLinkingConfiguration>>();
            builder.RegisterType<NonDocumentObjectLinkingExecutor>().As<IExecutor<INonDocumentObjectLinkingConfiguration>>();
        }
    }
}
