using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal static class CancellationAdapter
	{
		public static CompositeCancellationToken GetCancellationToken(IExtendedJob job, IWindsorContainer ripContainer)
		{
			IManagerFactory managerFactory = ripContainer.Resolve<IManagerFactory>();
			IJobService jobService = ripContainer.Resolve<IJobService>();
			IJobHistoryService jobHistoryService = ripContainer.Resolve<IJobHistoryService>();

			CancellationTokenSource stopTokenSource = new CancellationTokenSource();
			CancellationTokenSource drainStopTokenSource = new CancellationTokenSource();
			IJobStopManager jobStopManager = managerFactory.CreateJobStopManager(jobService, jobHistoryService, job.JobIdentifier, job.JobId,
				supportsDrainStop: true, stopCancellationTokenSource: stopTokenSource, drainStopCancellationTokenSource: drainStopTokenSource);
			ripContainer.Register(Component.For<IJobStopManager>().Instance(jobStopManager));
			return new CompositeCancellationToken(stopTokenSource.Token, drainStopTokenSource.Token);
		}
	}
}