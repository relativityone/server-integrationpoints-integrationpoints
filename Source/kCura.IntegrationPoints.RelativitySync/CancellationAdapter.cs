using System;
using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using Relativity.API;
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
        private readonly IAPILog _log;

        public CancellationAdapter(
            IWindsorContainer container,
            IExtendedJob job,
            IManagerFactory managerFactory,
            IJobService jobService,
            IJobHistoryService jobHistoryService,
            IAPILog log)
        {
            _container = container;
            _job = job;
            _managerFactory = managerFactory;
            _jobService = jobService;
            _jobHistoryService = jobHistoryService;
            _log = log;
        }

        public CompositeCancellationToken GetCancellationToken(Action drainStopTokenCallback = null)
        {
            CancellationTokenSource stopTokenSource = new CancellationTokenSource();
            CancellationTokenSource drainStopTokenSource = new CancellationTokenSource();
            IJobStopManager jobStopManager = _managerFactory.CreateJobStopManager(
                _jobService,
                _jobHistoryService,
                _job.JobIdentifier,
                _job.JobId,
                supportsDrainStop: true,
                new EmptyDiagnosticLog(),
                stopCancellationTokenSource: stopTokenSource,
                drainStopCancellationTokenSource: drainStopTokenSource);
            _container.Register(Component.For<IJobStopManager>().Instance(jobStopManager).Named($"{nameof(jobStopManager)}-{Guid.NewGuid()}"));

            if (drainStopTokenCallback != null)
            {
                drainStopTokenSource.Token.Register(drainStopTokenCallback);
            }

            return new CompositeCancellationToken(stopTokenSource.Token, drainStopTokenSource.Token, _log);
        }
    }
}
