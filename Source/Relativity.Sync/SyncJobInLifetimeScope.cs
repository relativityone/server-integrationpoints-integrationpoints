using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Autofac;
using Relativity.API;

namespace Relativity.Sync
{
	internal sealed class SyncJobInLifetimeScope : ISyncJob
	{
		private readonly IContainerFactory _containerFactory;
		private readonly IContainer _container;
		private readonly SyncJobParameters _syncJobParameters;
		private readonly IRelativityServices _relativityServices;
		private readonly SyncJobExecutionConfiguration _configuration;
        private readonly IAPILog _logger;
		
		public SyncJobInLifetimeScope(IContainerFactory containerFactory, IContainer container, SyncJobParameters syncJobParameters, IRelativityServices relativityServices,
			SyncJobExecutionConfiguration configuration, IAPILog logger)
		{
			_containerFactory = containerFactory;
			_container = container;
			_syncJobParameters = syncJobParameters;
			_relativityServices = relativityServices;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task ExecuteAsync(CompositeCancellationToken token)
		{
            using (EnrichLogger())
			using (ILifetimeScope scope = BeginLifetimeScope())
			{
				ISyncJob syncJob = CreateSyncJob(scope);
				await syncJob.ExecuteAsync(token).ConfigureAwait(false);
			}
		}

		public async Task ExecuteAsync(IProgress<SyncJobState> progress, CompositeCancellationToken token)
		{
            using (EnrichLogger())
			using (ILifetimeScope scope = BeginLifetimeScope())
			{
				ISyncJob syncJob = CreateSyncJob(scope);
				await syncJob.ExecuteAsync(progress, token).ConfigureAwait(false);
			}
		}
		
		private ILifetimeScope BeginLifetimeScope()
        {
			return _container.BeginLifetimeScope(builder => _containerFactory.RegisterSyncDependencies(builder, _syncJobParameters, _relativityServices, _configuration, _logger));
		}

        private IDisposable EnrichLogger()
        {
            CompositeDisposable disposables = new CompositeDisposable(
                _logger.LogContextPushProperty(nameof(SyncJobParameters.WorkflowId), _syncJobParameters.WorkflowId),
                _logger.LogContextPushProperty(nameof(SyncJobParameters.SyncConfigurationArtifactId), _syncJobParameters.SyncConfigurationArtifactId),
                _logger.LogContextPushProperty(nameof(SyncJobParameters.SyncBuildVersion), _syncJobParameters.SyncBuildVersion)
            );

            return disposables;
        }

		private ISyncJob CreateSyncJob(ILifetimeScope scope)
		{
			try
			{
				return scope.Resolve<ISyncJob>();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to create Sync job {workflowId}.", _syncJobParameters.WorkflowId);
				throw new SyncException("Unable to create Sync job. See inner exception for more details.", ex, _syncJobParameters.WorkflowId);
			}
		}
	}
}