using System.Threading;
using Autofac;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class StorageInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<ProgressRepository>().As<IProgressRepository>();

			builder.RegisterType<ValidationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<PermissionsCheckConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<SnapshotPartitionConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<DocumentDataSourceSnapshotConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<DocumentRetryDataSourceSnapshotConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<ImageDataSourceSnapshotConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<ImageRetryDataSourceSnapshotConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<FieldConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<ImageRetrieveConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<DestinationWorkspaceSavedSearchCreationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<DestinationWorkspaceObjectTypesCreationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<DataDestinationInitializationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<DataDestinationFinalizationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<SynchronizationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<DestinationWorkspaceTagsCreationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<SourceWorkspaceTagsCreationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<JobCleanupConfiguration>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<AutomatedWorkflowTriggerConfiguration>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<JobEndMetricsConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<NotificationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<JobStatusConsolidationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<FieldMappings>().As<IFieldMappings>();
			builder.RegisterType<JobHistoryErrorRepository>().As<IJobHistoryErrorRepository>();
			builder.RegisterType<JobProgressUpdaterFactory>().As<IJobProgressUpdaterFactory>();
			builder.RegisterType<JobProgressHandlerFactory>().As<IJobProgressHandlerFactory>();

			builder.Register(CreateConfiguration).As<IConfiguration>().SingleInstance();
		}

		private IConfiguration CreateConfiguration(IComponentContext componentContext)
		{
			ISourceServiceFactoryForAdmin serviceFactory = componentContext.Resolve<ISourceServiceFactoryForAdmin>();
			SyncJobParameters syncJobParameters = componentContext.Resolve<SyncJobParameters>();
			ISyncLog logger = componentContext.Resolve<ISyncLog>();
			return Configuration.GetAsync(serviceFactory, syncJobParameters, logger, new SemaphoreSlimWrapper(new SemaphoreSlim(1))).ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}
}