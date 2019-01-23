using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal static class CancellationAdapter
	{
		public static CancellationToken GetCancellationToken(IExtendedJob job, IWindsorContainer ripContainer)
		{
			IManagerFactory managerFactory = ripContainer.Resolve<IManagerFactory>();
			IJobService jobService = ripContainer.Resolve<IJobService>();
			IJobHistoryService jobHistoryService = ripContainer.Resolve<IJobHistoryService>();

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			IJobStopManager jobStopManager = managerFactory.CreateJobStopManager(jobService, jobHistoryService, job.JobIdentifier, job.JobId, true, tokenSource);
			ripContainer.Register(Component.For<IJobStopManager>().Instance(jobStopManager));
			return tokenSource.Token;
		}
	}
}