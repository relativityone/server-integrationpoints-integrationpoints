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

			builder.Register(CreateConfiguration).As<IConfiguration>();
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