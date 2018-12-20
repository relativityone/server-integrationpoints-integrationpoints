using System.Reflection;
using Autofac;

namespace Relativity.Sync
{
	internal sealed class SyncJobFactory : ISyncJobFactory
	{
		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters)
		{
			return Create(container, syncJobParameters, new SyncConfiguration(), new EmptyLogger());
		}

		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, ISyncLog logger)
		{
			return Create(container, syncJobParameters, new SyncConfiguration(), logger);
		}

		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, SyncConfiguration configuration)
		{
			return Create(container, syncJobParameters, configuration, new EmptyLogger());
		}

		public ISyncJob Create(IContainer container, SyncJobParameters syncJobParameters, SyncConfiguration configuration, ISyncLog logger)
		{
			using (ILifetimeScope scope = container.BeginLifetimeScope(builder => RegisterDependencies(builder, syncJobParameters, configuration, logger)))
			{
				return scope.Resolve<ISyncJob>();
			}
		}

		private static void RegisterDependencies(ContainerBuilder builder, SyncJobParameters syncJobParameters, SyncConfiguration configuration, ISyncLog logger)
		{
			CorrelationId correlationId = new CorrelationId(syncJobParameters.CorrelationId);
			builder.RegisterType<SyncJob>().As<ISyncJob>();
			builder.RegisterInstance(new ContextLogger(correlationId, logger)).As<ISyncLog>();
			builder.RegisterInstance(syncJobParameters).As<SyncJobParameters>();
			builder.RegisterInstance(configuration).As<SyncConfiguration>();

			builder.RegisterGeneric(typeof(Command<>)).As(typeof(ICommand<>));
			builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsImplementedInterfaces();
		}
	}
}