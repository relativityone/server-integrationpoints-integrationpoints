using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.RelativitySync.Adapters;
using kCura.ScheduleQueue.Core;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.RelativitySync
{
	public sealed class RelativitySyncAdapter
	{
		private readonly IExtendedJob _job;
		private readonly IWindsorContainer _ripContainer;
		private readonly IAPILog _logger;
		private readonly IAPM _apmMetrics;
		private readonly Guid _correlationId;
		private readonly IntegrationPointToSyncConverter _converter;

		public RelativitySyncAdapter(IExtendedJob job, IWindsorContainer ripContainer, IAPILog logger, IAPM apmMetrics, IntegrationPointToSyncConverter converter)
		{
			_job = job;
			_ripContainer = ripContainer;
			_logger = logger;
			_apmMetrics = apmMetrics;
			_converter = converter;
			_correlationId = Guid.NewGuid();
		}

		public async Task<TaskResult> RunAsync()
		{
			TaskResult taskResult = new TaskResult { Status = TaskStatusEnum.Fail };
			SyncMetrics metrics = new SyncMetrics(_apmMetrics, _logger);
			try
			{
				CancellationToken cancellationToken = CancellationAdapter.GetCancellationToken(_job, _ripContainer);
				SyncConfiguration syncConfiguration = SyncConfigurationFactory.Create(_job, _ripContainer, _logger);
				using (IContainer container = InitializeSyncContainer(syncConfiguration))
				{
					metrics.MarkStartTime();
					await MarkJobAsStartedAsync().ConfigureAwait(false);

					ISyncJob syncJob = await CreateSyncJob(container, syncConfiguration).ConfigureAwait(false);
					Progress progress = new Progress();
					progress.SyncProgress += (sender, syncProgress) => UpdateJobStatusAsync(syncProgress.Id).ConfigureAwait(false).GetAwaiter().GetResult();
					await syncJob.ExecuteAsync(progress, cancellationToken).ConfigureAwait(false);

					if (cancellationToken.IsCancellationRequested)
					{
						await MarkJobAsStoppedAsync().ConfigureAwait(false);
					}
					else
					{
						await MarkJobAsCompletedAsync().ConfigureAwait(false);
					}

					taskResult = new TaskResult { Status = TaskStatusEnum.Success };
				}
			}
			catch (OperationCanceledException)
			{
				await MarkJobAsStoppedAsync().ConfigureAwait(false);
				taskResult = new TaskResult { Status = TaskStatusEnum.Fail };
			}
			catch (ValidationException ex)
			{
				await MarkJobAsValidationFailedAsync(ex).ConfigureAwait(false);
				taskResult = new TaskResult() { Status = TaskStatusEnum.Fail };
			}
			#pragma warning disable CA1031
			catch (Exception e)
			{
				await MarkJobAsFailedAsync(e).ConfigureAwait(false);
				taskResult = new TaskResult { Status = TaskStatusEnum.Fail };
			}
			#pragma warning restore CA1031
			finally
			{
				metrics.SendMetric(_correlationId, taskResult);
			}

			return taskResult;
		}

		private async Task MarkJobAsValidationFailedAsync(ValidationException ex)
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await JobHistoryHelper.MarkJobAsValidationFailedAsync(ex, _job, helper).ConfigureAwait(false);
			}
#pragma warning disable CA1031
			catch (Exception e)
			{
				helper.GetLoggerFactory().GetLogger().LogError(e, "Failed to mark job as validation failed.");
			}
#pragma warning restore CA1031
		}

		private async Task UpdateJobStatusAsync(string status)
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await JobHistoryHelper.UpdateJobStatusAsync(status, _job, helper).ConfigureAwait(false);
			}
#pragma warning disable CA1031
			catch (Exception e)
			{
				helper.GetLoggerFactory().GetLogger().LogError(e, "Failed to mark job as stopped.");
			}
#pragma warning restore CA1031
		}

		private async Task MarkJobAsStartedAsync()
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await JobHistoryHelper.MarkJobAsStartedAsync(_job, helper).ConfigureAwait(false);
			}
#pragma warning disable CA1031
			catch (Exception e)
			{
				helper.GetLoggerFactory().GetLogger().LogError(e, "Failed to mark job as stopped.");
			}
#pragma warning restore CA1031
		}

		private async Task MarkJobAsCompletedAsync()
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await JobHistoryHelper.MarkJobAsCompletedAsync(_job, helper).ConfigureAwait(false);
			}
#pragma warning disable CA1031
			catch (Exception e)
			{
				helper.GetLoggerFactory().GetLogger().LogError(e, "Failed to mark job as stopped.");
			}
#pragma warning restore CA1031
		}

		private async Task MarkJobAsStoppedAsync()
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await JobHistoryHelper.MarkJobAsStoppedAsync(_job, helper).ConfigureAwait(false);
			}
#pragma warning disable CA1031
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as stopped.");
			}
#pragma warning restore CA1031
		}

		private async Task MarkJobAsFailedAsync(Exception exception)
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await JobHistoryHelper.MarkJobAsFailedAsync(_job, exception, helper).ConfigureAwait(false);
			}
#pragma warning disable CA1031
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as failed.");
			}
#pragma warning restore CA1031
		}

		private async Task<ISyncJob> CreateSyncJob(IContainer container, SyncConfiguration syncConfiguration)
		{
			int syncConfigurationArtifactId;
			try
			{
				syncConfigurationArtifactId = await _converter.CreateSyncConfigurationAsync(_job, _ripContainer.Resolve<IHelper>()).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to create Sync Configuration RDO.");
				throw;
			}

			SyncJobFactory jobFactory = new SyncJobFactory();
			SyncJobParameters parameters = new SyncJobParameters(syncConfigurationArtifactId, _job.WorkspaceId, _job.IntegrationPointId, _correlationId.ToString(), syncConfiguration.ImportSettings);
			RelativityServices relativityServices = new RelativityServices(_apmMetrics, _ripContainer.Resolve<IHelper>().GetServicesManager(), _ripContainer.Resolve<Func<ISearchManager>>(), ExtensionPointServiceFinder.ServiceUriProvider.AuthenticationUri());
			ISyncLog syncLog = new SyncLog(_logger);
			ISyncJob syncJob = jobFactory.Create(container, parameters, relativityServices, syncLog);
			return syncJob;
		}

		private IContainer InitializeSyncContainer(SyncConfiguration syncConfiguration)
		{
			// We are registering types directly related to adapting the new Relativity Sync workflow to the
			// existing RIP workflow. The Autofac container we are building will only resolve adapters and related
			// wrappers, and the Windsor container will only resolve existing RIP classes.

			var containerBuilder = new ContainerBuilder();

			_ripContainer.Register(Component.For<SyncConfiguration>().Instance(syncConfiguration));

			containerBuilder.RegisterInstance(syncConfiguration).AsImplementedInterfaces().SingleInstance();

			containerBuilder.RegisterInstance(new DestinationWorkspaceObjectTypesCreation(_ripContainer))
				.As<IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration>>()
				.As<IExecutionConstrains<IDestinationWorkspaceObjectTypesCreationConfiguration>>();
			containerBuilder.RegisterInstance(new ValidationExecutorFactory(_ripContainer)).As<IValidationExecutorFactory>();
			containerBuilder.RegisterInstance(new RdoRepository(_ripContainer)).As<IRdoRepository>();

			containerBuilder.Register(context => new PermissionsCheck(_ripContainer, context.Resolve<IValidationExecutorFactory>(), context.Resolve<IRdoRepository>()))
				.As<IExecutor<IPermissionsCheckConfiguration>>()
				.As<IExecutionConstrains<IPermissionsCheckConfiguration>>();

			containerBuilder.RegisterType<DataDestinationFinalization>()
				.As<IExecutor<IDataDestinationFinalizationConfiguration>>()
				.As<IExecutionConstrains<IDataDestinationFinalizationConfiguration>>();

			containerBuilder.RegisterType<DataDestinationInitialization>()
				.As<IExecutor<IDataDestinationInitializationConfiguration>>()
				.As<IExecutionConstrains<IDataDestinationInitializationConfiguration>>();

			containerBuilder.RegisterType<JobCleanup>()
				.As<IExecutor<IJobCleanupConfiguration>>()
				.As<IExecutionConstrains<IJobCleanupConfiguration>>();

			containerBuilder.RegisterType<JobStatusConsolidation>()
				.As<IExecutor<IJobStatusConsolidationConfiguration>>()
				.As<IExecutionConstrains<IJobStatusConsolidationConfiguration>>();

			IContainer container = containerBuilder.Build();
			return container;
		}
	}
}