using System.Threading;
using Autofac;
using Castle.Windsor;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	public static class RelativitySyncAdapter
	{
		public static TaskResult Run(Job job, IWindsorContainer ripContainer, IAPILog logger)
		{
			using (IContainer container = InitializeAutofacContainer(job, ripContainer, logger))
			{
				ISyncJob syncJob = CreateSyncJob(job, container, logger);
				CancellationToken cancellationToken = CancellationAdapter.GetCancellationToken(job, ripContainer);
				syncJob.ExecuteAsync(cancellationToken).GetAwaiter().GetResult();
				return new TaskResult {Status = TaskStatusEnum.Success};
			}
		}
		
		private static ISyncJob CreateSyncJob(Job job, IContainer container, IAPILog logger)
		{
			SyncJobFactory jobFactory = new SyncJobFactory();
			SyncJobParameters parameters = new SyncJobParameters((int) job.JobId, job.WorkspaceID);
			ISyncLog syncLog = new SyncLog(logger);
			ISyncJob syncJob = jobFactory.Create(container, parameters, syncLog);
			return syncJob;
		}

		private static IContainer InitializeAutofacContainer(Job job, IWindsorContainer ripContainer, IAPILog logger)
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(SyncConfigurationFactory.Create(job, ripContainer, logger)).AsImplementedInterfaces().SingleInstance();
			IContainer container = containerBuilder.Build();
			return container;
		}
	}
}
