using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.System.Core.Runner
{
	/// <summary>
	/// This class can be used to run sync job outside of Relativity Agent framework
	/// </summary>
	public class SyncRunner
	{
		private readonly ISyncServiceManager _serviceManager;
		private readonly Uri _relativityUri;
		private readonly ISyncLog _logger;
		private readonly IAPM _apmClient;

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="serviceManager">Authenticated service manager</param>
		/// <param name="relativityUri">Host name of relativity - no suffixes</param>
		/// <param name="apmClient">APM implementation</param>
		/// <param name="logger">Logger</param>
		public SyncRunner(ISyncServiceManager serviceManager, Uri relativityUri, IAPM apmClient, ISyncLog logger)
		{
			_serviceManager = serviceManager;
			_relativityUri = relativityUri;
			_logger = logger;
			_apmClient = apmClient;
		}

		/// <summary>
		/// Run job with set parameters
		/// </summary>
		/// <param name="parameters">Job parameters</param>
		/// <param name="progress">Progress to report to</param>
		/// <param name="userId">Id of user that submitted the job</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public Task RunAsync(SyncJobParameters parameters, int userId, IProgress<SyncJobState> progress, CancellationToken cancellationToken)
		{
			ISyncJob syncJob = CreateSyncJobAsync(parameters, userId, _relativityUri);
			return syncJob.ExecuteAsync(progress, cancellationToken);
		}

		/// <summary>
		/// Run job with set parameters and returns final job state
		/// </summary>
		/// <param name="parameters">Job parameters</param>
		/// <param name="userId">Id of user that submitted the job</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async Task<SyncJobState> RunAsync(SyncJobParameters parameters, int userId, CancellationToken cancellationToken)
		{
			SyncJobState result = null;
			var progress = new Progress<SyncJobState>(state =>
			{
				result = state;
			});

			await RunAsync(parameters, userId, progress, cancellationToken).ConfigureAwait(false);

			return result;
		}

		/// <summary>
		/// Run job with set parameters and returns final job state
		/// </summary>
		/// <param name="parameters">Job parameters</param>
		/// <param name="userId">Id of user that submitted the job</param>
		/// <returns></returns>
		public Task<SyncJobState> RunAsync(SyncJobParameters parameters, int userId) => RunAsync(parameters, userId, CancellationToken.None);


		private ISyncJob CreateSyncJobAsync(SyncJobParameters parameters, int userId, Uri relativityUri)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();


			containerBuilder.RegisterInstance(new SyncDataAndUserConfiguration(userId, 0)).AsImplementedInterfaces().SingleInstance();

			containerBuilder.RegisterType<EmptyExecutorConfiguration<IDataDestinationInitializationConfiguration>>()
				.As<IExecutor<IDataDestinationInitializationConfiguration>>()
				.As<IExecutionConstrains<IDataDestinationInitializationConfiguration>>();

			containerBuilder.RegisterType<EmptyExecutorConfiguration<IDataDestinationFinalizationConfiguration>>()
				.As<IExecutor<IDataDestinationFinalizationConfiguration>>()
				.As<IExecutionConstrains<IDataDestinationFinalizationConfiguration>>();

			var jobFactory = new SyncJobFactory();
			var relativityServices = new RelativityServices(_apmClient, _serviceManager, relativityUri);


			return jobFactory.Create(containerBuilder.Build(), parameters, relativityServices, _logger);
		}
	}
}
