﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.RelativitySync.Adapters;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.RelativitySync
{
	public sealed class RelativitySyncAdapter
	{
		private readonly JobHistoryHelper _jobHistoryHelper;
		private readonly IExtendedJob _job;
		private readonly IWindsorContainer _ripContainer;
		private readonly IAPILog _logger;
		private readonly IAPM _apmMetrics;
		private readonly Guid _correlationId;

		public RelativitySyncAdapter(IExtendedJob job, IWindsorContainer ripContainer, IAPILog logger, IAPM apmMetrics)
		{
			_jobHistoryHelper = new JobHistoryHelper();
			_job = job;
			_ripContainer = ripContainer;
			_logger = logger;
			_apmMetrics = apmMetrics;
			_correlationId = Guid.NewGuid();
		}

		public async Task<TaskResult> RunAsync()
		{
			SyncMetrics metrics = new SyncMetrics(_apmMetrics, _logger);
			TaskResult taskResult = new TaskResult {Status = TaskStatusEnum.Fail};
			try
			{
				CancellationToken cancellationToken = CancellationAdapter.GetCancellationToken(_job, _ripContainer);
				using (IContainer container = InitializeSyncContainer(metrics))
				{
					metrics.MarkStartTime();
					await MarkJobAsStartedAsync().ConfigureAwait(false);

					ISyncJob syncJob = CreateSyncJob(container);
					Progress progress = new Progress();
					progress.SyncProgress += (sender, syncProgress) => UpdateJobStatus(syncProgress.State).ConfigureAwait(false).GetAwaiter().GetResult();
					await syncJob.ExecuteAsync(progress, cancellationToken).ConfigureAwait(false);

					if (cancellationToken.IsCancellationRequested)
					{
						await MarkJobAsStoppedAsync().ConfigureAwait(false);
					}
					else
					{
						await MarkJobAsCompletedAsync().ConfigureAwait(false);
					}

					taskResult = new TaskResult {Status = TaskStatusEnum.Success};
				}
			}
			catch (OperationCanceledException)
			{
				await MarkJobAsStoppedAsync().ConfigureAwait(false);
				taskResult = new TaskResult {Status = TaskStatusEnum.Fail};
			}
			catch (Exception e)
			{
				await MarkJobAsFailedAsync(e).ConfigureAwait(false);
				taskResult = new TaskResult {Status = TaskStatusEnum.Fail};
			}
			finally
			{
				metrics.SendMetric(_correlationId, taskResult);
			}

			return taskResult;
		}

		private async Task UpdateJobStatus(string status)
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await _jobHistoryHelper.UpdateJobStatusAsync(status, _job, helper).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				helper.GetLoggerFactory().GetLogger().LogError(e, "Failed to mark job as stopped.");
			}
		}

		private async Task MarkJobAsStartedAsync()
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await _jobHistoryHelper.MarkJobAsStartedAsync(_job, helper).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				helper.GetLoggerFactory().GetLogger().LogError(e, "Failed to mark job as stopped.");
			}
		}

		private async Task MarkJobAsCompletedAsync()
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await _jobHistoryHelper.MarkJobAsCompletedAsync(_job, helper).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				helper.GetLoggerFactory().GetLogger().LogError(e, "Failed to mark job as stopped.");
			}
		}

		private async Task MarkJobAsStoppedAsync()
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await _jobHistoryHelper.MarkJobAsStoppedAsync(_job, helper).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as stopped.");
			}
		}

		private async Task MarkJobAsFailedAsync(Exception exception)
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await _jobHistoryHelper.MarkJobAsFailedAsync(_job, exception, helper).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as failed.");
			}
		}

		private ISyncJob CreateSyncJob(IContainer container)
		{
			SyncJobFactory jobFactory = new SyncJobFactory();
			SyncJobParameters parameters = new SyncJobParameters(_job.JobHistoryId, _job.WorkspaceId, _correlationId.ToString());
			ISyncLog syncLog = new SyncLog(_logger);
			ISyncJob syncJob = jobFactory.Create(container, parameters, syncLog);
			return syncJob;
		}

		private IContainer InitializeSyncContainer(SyncMetrics metrics)
		{
			// We are registering types directly related to adapting the new Relativity Sync workflow to the
			// existing RIP workflow. The Autofac container we are building will only resolve adapters and related
			// wrappers, and the Windsor container will only resolve existing RIP classes.

			var containerBuilder = new ContainerBuilder();
			SyncConfiguration syncConfiguration = SyncConfigurationFactory.Create(_job, _ripContainer, _logger);

			_ripContainer.Register(Component.For<SyncConfiguration>().Instance(syncConfiguration));

			containerBuilder.RegisterInstance(syncConfiguration).AsImplementedInterfaces().SingleInstance();
			containerBuilder.RegisterInstance(metrics).As<ISyncMetrics>().SingleInstance();

			containerBuilder.RegisterInstance(new DestinationWorkspaceSavedSearchCreation(_ripContainer))
				.As<IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>>()
				.As<IExecutionConstrains<IDestinationWorkspaceSavedSearchCreationConfiguration>>();

			containerBuilder.RegisterInstance(new DestinationWorkspaceObjectTypesCreation(_ripContainer))
				.As<IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration>>()
				.As<IExecutionConstrains<IDestinationWorkspaceObjectTypesCreationConfiguration>>();
			containerBuilder.RegisterInstance(new ValidationExecutorFactory(_ripContainer)).As<IValidationExecutorFactory>();
			containerBuilder.RegisterInstance(new RdoRepository(_ripContainer)).As<IRdoRepository>();
			containerBuilder.Register(context => new Validation(_ripContainer, context.Resolve<IValidationExecutorFactory>(), context.Resolve<IRdoRepository>()))
				.As<IExecutor<IValidationConfiguration>>()
				.As<IExecutionConstrains<IValidationConfiguration>>();
			containerBuilder.Register(context => new PermissionsCheck(_ripContainer, context.Resolve<IValidationExecutorFactory>(), context.Resolve<IRdoRepository>()))
				.As<IExecutor<IPermissionsCheckConfiguration>>()
				.As<IExecutionConstrains<IPermissionsCheckConfiguration>>();

			containerBuilder.RegisterInstance(new DestinationWorkspaceTagsCreation(_ripContainer))
				.As<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>()
				.As<IExecutionConstrains<IDestinationWorkspaceTagsCreationConfiguration>>();

			containerBuilder.RegisterInstance(new DestinationWorkspaceSavedSearchCreation(_ripContainer))
				.As<IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>>()
				.As<IExecutionConstrains<IDestinationWorkspaceSavedSearchCreationConfiguration>>();

			containerBuilder.RegisterInstance(new SourceWorkspaceTagsCreation(_ripContainer))
				.As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>()
				.As<IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>>();

			containerBuilder.RegisterInstance(new Synchronization(_ripContainer))
				.As<IExecutor<ISynchronizationConfiguration>>()
				.As<IExecutionConstrains<ISynchronizationConfiguration>>();

			containerBuilder.RegisterInstance(new Notification(_ripContainer))
				.As<IExecutor<INotificationConfiguration>>()
				.As<IExecutionConstrains<INotificationConfiguration>>();

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

			containerBuilder.RegisterType<SnapshotPartition>()
				.As<IExecutor<ISnapshotPartitionConfiguration>>()
				.As<IExecutionConstrains<ISnapshotPartitionConfiguration>>();

			containerBuilder.RegisterType<DataSourceSnapshot>()
				.As<IExecutor<IDataSourceSnapshotConfiguration>>()
				.As<IExecutionConstrains<IDataSourceSnapshotConfiguration>>();


			IContainer container = containerBuilder.Build();
			return container;
		}
	}
}