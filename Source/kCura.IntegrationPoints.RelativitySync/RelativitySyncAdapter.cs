using System.Threading;
using Autofac;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	public class RelativitySyncAdapter
	{
		public static TaskResult Run(Job job, IAPILog logger)
		{
			using (IContainer container = InitializeAutofacContainer())
			{
				ISyncJob syncJob = CreateSyncJob(job, container, logger);
				syncJob.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
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

		private static IContainer InitializeAutofacContainer()
		{
			var containerBuilder = new ContainerBuilder();
			IContainer container = containerBuilder.Build();
			return container;
		}
	}
}