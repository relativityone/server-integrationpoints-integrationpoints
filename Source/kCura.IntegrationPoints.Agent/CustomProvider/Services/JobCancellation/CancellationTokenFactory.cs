using System;
using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobCancellation
{
    public class CancellationTokenFactory : ICancellationTokenFactory
    {
        private readonly IWindsorContainer _container;
        private readonly IManagerFactory _managerFactory;
        private readonly IJobService _jobService;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IAPILog _logger;

        public CancellationTokenFactory(IWindsorContainer container, IManagerFactory managerFactory,
            IJobService jobService, IJobHistoryService jobHistoryService, IAPILog logger)
        {
            _container = container;
            _managerFactory = managerFactory;
            _jobService = jobService;
            _jobHistoryService = jobHistoryService;
            _logger = logger;
        }

        public CompositeCancellationToken GetCancellationToken(Guid batchInstance, long jobId)
        {
            CancellationTokenSource stopTokenSource = new CancellationTokenSource();
            CancellationTokenSource drainStopTokenSource = new CancellationTokenSource();

            IJobStopManager jobStopManager = _managerFactory.CreateJobStopManager(
                _jobService,
                _jobHistoryService,
                batchInstance,
                jobId,
                supportsDrainStop: true,
                new EmptyDiagnosticLog(),
                stopCancellationTokenSource: stopTokenSource,
                drainStopCancellationTokenSource: drainStopTokenSource);

            _container.Register(Component.For<IJobStopManager>().Instance(jobStopManager).Named($"{nameof(jobStopManager)}-{Guid.NewGuid()}"));

            return new CompositeCancellationToken(stopTokenSource.Token, drainStopTokenSource.Token, _logger);
        }
    }
}
