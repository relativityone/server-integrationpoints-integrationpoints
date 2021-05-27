using System;
using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.ScheduleQueue.Core;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	public class CancellationAdapter : ICancellationAdapter
	{
		private readonly IWindsorContainer _container;
		private readonly IExtendedJob _job;
		private readonly IManagerFactory _managerFactory;
		private readonly IJobService _jobService;
		private readonly IJobHistoryService _jobHistoryService;

		public CancellationAdapter(IWindsorContainer container, IExtendedJob job, IManagerFactory managerFactory,
			IJobService jobService, IJobHistoryService jobHistoryService)
		{
			_container = container;
			_job = job;
			_managerFactory = managerFactory;
			_jobService = jobService;
			_jobHistoryService = jobHistoryService;
		}

		public CompositeCancellationToken GetCancellationToken()
		{
			CancellationTokenSource stopTokenSource = new CancellationTokenSource();
			CancellationTokenSource drainStopTokenSource = new CancellationTokenSource();
			IJobStopManager jobStopManager = _managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, _job.JobIdentifier, _job.JobId,
				supportsDrainStop: true, stopCancellationTokenSource: stopTokenSource, drainStopCancellationTokenSource: drainStopTokenSource);
			_container.Register(Component.For<IJobStopManager>().Instance(jobStopManager).Named($"{nameof(jobStopManager)}-{Guid.NewGuid()}"));
			
			return new CompositeCancellationToken(stopTokenSource.Token, drainStopTokenSource.Token);
		}
	}
}