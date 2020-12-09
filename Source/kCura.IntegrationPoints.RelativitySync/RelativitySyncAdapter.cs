using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.RelativitySync.Metrics;
using kCura.IntegrationPoints.RelativitySync.RipOverride;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Executors.Validation;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.RelativitySync
{
#pragma warning disable CA1031
	public sealed class RelativitySyncAdapter
	{
		private readonly IExtendedJob _job;
		private readonly IWindsorContainer _ripContainer;
		private readonly IAPILog _logger;
		private readonly IAPM _apmMetrics;
		private readonly ISyncJobMetric _jobMetric;
		private readonly Guid _correlationId;
		private readonly IntegrationPointToSyncConverter _converter;

		private readonly IHelper _helper;
		private readonly IJobHistorySyncService _jobHistorySyncService;

		public RelativitySyncAdapter(IExtendedJob job, IWindsorContainer ripContainer, IAPILog logger, IAPM apmMetrics,
			ISyncJobMetric jobMetric, IntegrationPointToSyncConverter converter)
		{
			_job = job;
			_ripContainer = ripContainer;
			_logger = logger;
			_apmMetrics = apmMetrics;
			_jobMetric = jobMetric;
			_converter = converter;
			_correlationId = Guid.NewGuid();

			_helper = _ripContainer.Resolve<IHelper>();
			_jobHistorySyncService = new JobHistorySyncService(_helper);
		}

		public async Task<TaskResult> RunAsync()
		{
			TaskResult taskResult = new TaskResult { Status = TaskStatusEnum.Fail };
			SyncMetrics metrics = new SyncMetrics(_apmMetrics, _logger);
			try
			{
				CancellationToken cancellationToken = CancellationAdapter.GetCancellationToken(_job, _ripContainer);
				SyncConfiguration syncConfiguration = new SyncConfiguration(_job.SubmittedById);
				using (IContainer container = InitializeSyncContainer(syncConfiguration))
				{
					metrics.MarkStartTime();
					await MarkJobAsStartedAsync().ConfigureAwait(false);

					ISyncJob syncJob = await CreateSyncJobAsync(container).ConfigureAwait(false);
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
			catch (Exception e)
			{
				await MarkJobAsFailedAsync(e).ConfigureAwait(false);
				taskResult = new TaskResult { Status = TaskStatusEnum.Fail };
			}
			finally
			{
				metrics.SendMetric(_correlationId, taskResult);
			}

			return taskResult;
		}

		private async Task MarkJobAsValidationFailedAsync(ValidationException ex)
		{
			try
			{
				await _jobHistorySyncService.MarkJobAsValidationFailedAsync(ex, _job).ConfigureAwait(false);
				await _jobMetric.SendJobFailedAsync(_job.Job).ConfigureAwait(false);
			}
			catch (SyncMetricException e)
			{
				_logger.LogError(e, "Failed to send job failed metric");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as validation failed.");
			}
		}

		private async Task UpdateJobStatusAsync(string status)
		{
			try
			{
				await _jobHistorySyncService.UpdateJobStatusAsync(status, _job).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to update job status.");
			}
		}

		private async Task MarkJobAsStartedAsync()
		{
			try
			{
				await _jobHistorySyncService.MarkJobAsStartedAsync(_job).ConfigureAwait(false);
				await _jobMetric.SendJobStartedAsync(_job.Job).ConfigureAwait(false);
			}
			catch (SyncMetricException e)
			{
				_logger.LogError(e, "Failed to send job started metric");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as started.");
			}
		}

		private async Task MarkJobAsCompletedAsync()
		{
			try
			{
				await _jobHistorySyncService.MarkJobAsCompletedAsync(_job).ConfigureAwait(false);
				await _jobMetric.SendJobCompletedAsync(_job.Job).ConfigureAwait(false);
			}
			catch (SyncMetricException e)
			{
				_logger.LogError(e, "Failed to send job completed metric");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as completed.");
			}
		}

		private async Task MarkJobAsStoppedAsync()
		{
			try
			{
				await _jobHistorySyncService.MarkJobAsStoppedAsync(_job).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as stopped.");
			}
		}

		private async Task MarkJobAsFailedAsync(Exception exception)
		{
			try
			{
				await _jobHistorySyncService.MarkJobAsFailedAsync(_job, exception).ConfigureAwait(false);
				await _jobMetric.SendJobFailedAsync(_job.Job).ConfigureAwait(false);
			}
			catch (SyncMetricException e)
			{
				_logger.LogError(e, "Failed to send job failed metric");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as failed.");
			}
		}

		private async Task<ISyncJob> CreateSyncJobAsync(IContainer container)
		{
			int syncConfigurationArtifactId;
			try
			{
				syncConfigurationArtifactId = await _converter.CreateSyncConfigurationAsync(_job, _helper, _jobHistorySyncService).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to create Sync Configuration RDO.");
				throw;
			}

			SyncJobFactory jobFactory = new SyncJobFactory();
			SyncJobParameters parameters = new SyncJobParameters(syncConfigurationArtifactId, _job.WorkspaceId, _job.JobHistoryId)
											{
												TriggerValue = "rip"
											};

			SyncServiceManagerForRip serviceManager = new SyncServiceManagerForRip(_ripContainer.Resolve<IHelper>().GetServicesManager());

			RelativityServices relativityServices = new RelativityServices(_apmMetrics, serviceManager, ExtensionPointServiceFinder.ServiceUriProvider.AuthenticationUri());
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

			IContainer container = containerBuilder.Build();
			return container;
		}
	}
#pragma warning restore CA1031
}
