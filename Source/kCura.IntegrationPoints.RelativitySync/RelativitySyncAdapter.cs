using System.Threading;
using Autofac;
using Castle.Windsor;
using kCura.ScheduleQueue.Core;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	public static class RelativitySyncAdapter
	{
		public static TaskResult Run(Job job, IWindsorContainer ripContainer)
		{
			using (IContainer container = InitializeAutofacContainer())
			{
				ISyncJob syncJob = CreateSyncJob(job, container);
				CancellationToken cancellationToken = CancellationAdapter.GetCancellationToken(job, ripContainer);
				syncJob.ExecuteAsync(cancellationToken).GetAwaiter().GetResult();
				return new TaskResult {Status = TaskStatusEnum.Success};
			}
		}

		private static ISyncJob CreateSyncJob(Job job, IContainer container)
		{
			SyncJobFactory jobFactory = new SyncJobFactory();
			SyncJobParameters parameters = new SyncJobParameters((int) job.JobId, job.WorkspaceID);
			ISyncJob syncJob = jobFactory.Create(container, parameters);
			return syncJob;
		}

		private static IContainer InitializeAutofacContainer()
		{
			var containerBuilder = new ContainerBuilder();
			IContainer container = containerBuilder.Build();
			return container;
		}
	}
}