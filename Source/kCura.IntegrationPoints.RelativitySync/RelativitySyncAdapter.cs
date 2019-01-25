﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Castle.Windsor;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	public sealed class RelativitySyncAdapter
	{
		private readonly JobHistoryHelper _jobHistoryHelper;
		private readonly IExtendedJob _job;
		private readonly IWindsorContainer _ripContainer;
		private readonly IAPILog _logger;

		public RelativitySyncAdapter(IExtendedJob job, IWindsorContainer ripContainer, IAPILog logger)
		{
			_jobHistoryHelper = new JobHistoryHelper();
			_job = job;
			_ripContainer = ripContainer;
			_logger = logger;
		}

		public async Task<TaskResult> RunAsync()
		{
			try
			{
				CancellationToken cancellationToken = CancellationAdapter.GetCancellationToken(_job, _ripContainer);
				using (IContainer container = InitializeAutofacContainer())
				{
					ISyncJob syncJob = CreateSyncJob(container);
					await syncJob.ExecuteAsync(cancellationToken).ConfigureAwait(false);
					if (cancellationToken.IsCancellationRequested)
					{
						await MarkJobAsStoppedAsync().ConfigureAwait(false);
					}

					return new TaskResult {Status = TaskStatusEnum.Success};
				}
			}
			catch (OperationCanceledException)
			{
				await MarkJobAsStoppedAsync().ConfigureAwait(false);
				return new TaskResult {Status = TaskStatusEnum.Fail};
			}
			catch (Exception e)
			{
				await MarkJobAsFailed(e).ConfigureAwait(false);
				return new TaskResult {Status = TaskStatusEnum.Fail};
			}
		}

		private async Task MarkJobAsStoppedAsync()
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await _jobHistoryHelper.MarkJobAsStopped(_job, helper).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as stopped.");
			}
		}

		private async Task MarkJobAsFailed(Exception exception)
		{
			IHelper helper = _ripContainer.Resolve<IHelper>();
			try
			{
				await _jobHistoryHelper.MarkJobAsFailed(_job, exception, helper).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to mark job as failed.");
			}
		}

		private ISyncJob CreateSyncJob(IContainer container)
		{
			SyncJobFactory jobFactory = new SyncJobFactory();
			SyncJobParameters parameters = new SyncJobParameters(_job.JobHistoryId, _job.WorkspaceId);
			ISyncLog syncLog = new SyncLog(_logger);
			ISyncJob syncJob = jobFactory.Create(container, parameters, syncLog);
			return syncJob;
		}

		private IContainer InitializeAutofacContainer()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(SyncConfigurationFactory.Create(_job, _ripContainer, _logger)).AsImplementedInterfaces().SingleInstance();
			IContainer container = containerBuilder.Build();
			return container;
		}
	}
}