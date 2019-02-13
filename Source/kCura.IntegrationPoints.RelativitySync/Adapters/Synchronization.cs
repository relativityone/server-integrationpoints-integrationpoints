using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.RelativitySync.RipOverride;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class Synchronization : IExecutor<ISynchronizationConfiguration>, IExecutionConstrains<ISynchronizationConfiguration>
	{
		private readonly IWindsorContainer _container;

		public Synchronization(IWindsorContainer container)
		{
			_container = container;
			OverrideContainerRegistrations(_container);
		}

		private void OverrideContainerRegistrations(IWindsorContainer container)
		{
			// We have to register those dependencies after creation of RelativitySyncAdapter because we have to have SyncConfiguration registered
			container.Register(Component.For<ITagsCreator>().ImplementedBy<SyncTagsInjector>().Named(Guid.NewGuid().ToString()).IsDefault());
			container.Register(Component.For<ISourceWorkspaceTagCreator>().ImplementedBy<SourceWorkspaceTagInjector>().Named(Guid.NewGuid().ToString()).IsDefault());
			container.Register(Component.For<ITagSavedSearchManager>().ImplementedBy<EmptyTagSavedSearchManager>().Named(Guid.NewGuid().ToString()).IsDefault());
			container.Register(Component.For<IManagerFactory>().ImplementedBy<SyncManagerFactory>().Named(Guid.NewGuid().ToString()).IsDefault());
			container.Register(Component.For<IJobHistoryService>().ImplementedBy<SyncJobHistoryService>().Named(Guid.NewGuid().ToString()).IsDefault());
			container.Register(Component.For<IAgentValidator>().ImplementedBy<EmptyAgentValidator>().Named(Guid.NewGuid().ToString()).IsDefault());
		}

		public Task<bool> CanExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}

		public async Task ExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			await Task.Yield();

			IExportServiceManager exportServiceManager = _container.Resolve<IExportServiceManager>();
			IExtendedJob extendedJob = _container.Resolve<IExtendedJob>();

			exportServiceManager.Execute(extendedJob.Job);
		}
	}
}