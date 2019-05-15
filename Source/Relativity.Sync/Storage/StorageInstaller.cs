using System.Threading;
using Autofac;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class StorageInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<ProgressRepository>().As<IProgressRepository>();

			builder.RegisterType<ValidationConfiguration>().As<IValidationConfiguration>();
			builder.RegisterType<DataSourceSnapshotConfiguration>().As<IDataSourceSnapshotConfiguration>();
			builder.Register(CreateSynchronizationConfiguration).As<ISynchronizationConfiguration>();
			builder.RegisterType<FieldMappings>().As<IFieldMappings>();

			builder.Register(CreateConfiguration).As<IConfiguration>();
		}

		private IConfiguration CreateConfiguration(IComponentContext componentContext)
		{
			ISourceServiceFactoryForAdmin serviceFactory = componentContext.Resolve<ISourceServiceFactoryForAdmin>();
			SyncJobParameters syncJobParameters = componentContext.Resolve<SyncJobParameters>();
			ISyncLog logger = componentContext.Resolve<ISyncLog>();
			return Configuration.GetAsync(serviceFactory, syncJobParameters, logger, new SemaphoreSlimWrapper(new SemaphoreSlim(1))).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private ISynchronizationConfiguration CreateSynchronizationConfiguration(IComponentContext context)
		{
			IConfiguration configuration = context.Resolve<IConfiguration>();
			SyncJobParameters syncJobParameters = context.Resolve<SyncJobParameters>();
			IFieldMappings fieldMappings = context.Resolve<IFieldMappings>();
			int jobHistoryTagArtifactId = context.Resolve<ISynchronizationConfiguration>().JobHistoryTagArtifactId;
			return new SynchronizationConfiguration(configuration, syncJobParameters, fieldMappings, jobHistoryTagArtifactId);
		}
	}
}