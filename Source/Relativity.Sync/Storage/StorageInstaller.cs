﻿using System.Threading;
using Autofac;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Storage.RdoGuidsProviders;

namespace Relativity.Sync.Storage
{
	internal sealed class StorageInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<ProgressRepository>().As<IProgressRepository>();

			builder.RegisterType<PreValidationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<ValidationConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<PermissionsCheckConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<SnapshotPartitionConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<DocumentJobStartMetricsConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<ImageJobStartMetricsConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<DataSourceSnapshotConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<RetryDataSourceSnapshotConfiguration>().AsImplementedInterfaces();
			builder.RegisterType<SnapshotQueryConfiguration>().AsImplementedInterfaces();
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
			
			builder.RegisterType<RdoGuidConfiguration>().AsImplementedInterfaces();
			
			builder.RegisterType<FieldMappings>().As<IFieldMappings>();
			builder.RegisterType<JobHistoryErrorRepository>().As<IJobHistoryErrorRepository>();
			builder.RegisterType<JobProgressUpdaterFactory>().As<IJobProgressUpdaterFactory>();
			builder.RegisterType<JobProgressHandlerFactory>().As<IJobProgressHandlerFactory>();

			builder.Register(CreateConfiguration).As<IConfiguration>().SingleInstance();
		}

		private IConfiguration CreateConfiguration(IComponentContext componentContext)
		{
			SyncJobParameters syncJobParameters = componentContext.Resolve<SyncJobParameters>();
			ISyncLog logger = componentContext.Resolve<ISyncLog>();
			IRdoManager rdoManager = componentContext.Resolve<IRdoManager>();
			
			return Configuration.GetAsync(syncJobParameters, logger, new SemaphoreSlimWrapper(new SemaphoreSlim(1)), rdoManager).ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}
}