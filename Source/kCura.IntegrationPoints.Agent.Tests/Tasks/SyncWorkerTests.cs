using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
    [TestFixture]
    [Category("Unit")]
    public class SyncWorkerTests : TestBase
    {
        private ICaseServiceContext _caseServiceContext;
        private IJobHistoryErrorService _jobHistoryErrorService;
        private IJobService _jobService;
        private IJobManager _jobManager;
        private IBatchStatus _batchStatus;
        private SyncWorker _instance;
        private Job _job;
        private IntegrationPointDto _integrationPoint;
        private TaskParameters _taskParams;
        private JobHistory _jobHistory;
        private IJobStopManager _jobStopManager;
        private IDataSynchronizer _dataSynchronizer;
        private IJobHistoryManager _jobHistoryManager;

        [SetUp]
        public override void SetUp()
        {
            _caseServiceContext = Substitute.For<ICaseServiceContext>();
            IHelper helper = Substitute.For<IHelper>();
            IDataProviderFactory dataProviderFactory = Substitute.For<IDataProviderFactory>();
            ISerializer serializer = Substitute.For<ISerializer>();
            ISynchronizerFactory appDomainRdoSynchronizerFactory = Substitute.For<ISynchronizerFactory>();
            IJobHistoryService jobHistoryService = Substitute.For<IJobHistoryService>();
            _jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
            _jobManager = Substitute.For<IJobManager>();
            _batchStatus = Substitute.For<IBatchStatus>();
            JobStatisticsService statisticsService = Substitute.For<JobStatisticsService>();
            IManagerFactory managerFactory = Substitute.For<IManagerFactory>();
            _jobService = Substitute.For<IJobService>();
            _jobStopManager = Substitute.For<IJobStopManager>();
            IDataSourceProvider dataSourceProvider = Substitute.For<IDataSourceProvider>();
            IDataReader sourceDataReader = Substitute.For<IDataReader>();
            _dataSynchronizer = Substitute.For<IDataSynchronizer>();
            _jobHistoryManager = Substitute.For<IJobHistoryManager>();
            IProviderTypeService providerTypeService = Substitute.For<IProviderTypeService>();
            IIntegrationPointService integrationPointService = Substitute.For<IIntegrationPointService>();

            _instance = new SyncWorker(
                _caseServiceContext,
                helper,
                dataProviderFactory,
                serializer,
                appDomainRdoSynchronizerFactory,
                jobHistoryService,
                _jobHistoryErrorService,
                _jobManager,
                new IBatchStatus[] { _batchStatus },
                statisticsService,
                managerFactory,
                _jobService,
                providerTypeService,
                integrationPointService,
                new EmptyDiagnosticLog());

            _job = JobHelper.GetFakeJobOfTaskType(TaskType.SyncEntityManagerWorker);

            _integrationPoint = new IntegrationPointDto()
            {
                SourceProvider = 852,
                DestinationProvider = 942,
                SourceConfiguration = "source config",
                DestinationConfiguration = new DestinationConfiguration(),
                SecuredConfiguration = "sec config",
                FieldMappings = new List<FieldMap>(),
            };
            SourceProvider sourceProvider = new SourceProvider() { Identifier = Guid.NewGuid().ToString(), ApplicationIdentifier = Guid.NewGuid().ToString() };
            DestinationProvider destinationProvider = new DestinationProvider() { Identifier = Guid.NewGuid().ToString() };
            _jobHistory = new JobHistory() { ArtifactId = 9876546, BatchInstance = Guid.Empty.ToString() };
            List<string> recordIds = new List<string> { "1", "2" };
            _dataSynchronizer.TotalRowsProcessed.Returns(recordIds.Count);

            _taskParams = new TaskParameters()
            {
                BatchInstance = Guid.NewGuid(),
                BatchParameters = recordIds
            };
            var associatedJobs = new List<Job>() { _job };
            integrationPointService.Read(_job.RelatedObjectArtifactID).Returns(_integrationPoint);
            _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(_integrationPoint.SourceProvider).Returns(sourceProvider);
            _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<DestinationProvider>(_integrationPoint.DestinationProvider).Returns(destinationProvider);
            serializer.Deserialize<TaskParameters>(_job.JobDetails).Returns(_taskParams);
            jobHistoryService.GetRdoWithoutDocuments(_taskParams.BatchInstance).Returns(_jobHistory);

            jobHistoryService.GetRdoWithoutDocuments(Guid.Empty).Returns(_jobHistory);

            managerFactory.CreateJobStopManager(_jobService, jobHistoryService, _taskParams.BatchInstance, _job.JobId, Arg.Any<bool>(), Arg.Any<IDiagnosticLog>())
                .Returns(_jobStopManager);

            serializer.Deserialize<List<string>>(Arg.Is<string>(_taskParams.BatchParameters.ToString())).Returns(recordIds);

            dataProviderFactory.GetDataProvider(
                new Guid(sourceProvider.ApplicationIdentifier),
                new Guid(sourceProvider.Identifier))
                .Returns(dataSourceProvider);
            dataSourceProvider.GetData(Arg.Any<List<FieldEntry>>(), Arg.Any<List<string>>(), new DataSourceProviderConfiguration(_integrationPoint.SourceConfiguration, _integrationPoint.SecuredConfiguration)).Returns(sourceDataReader);
            appDomainRdoSynchronizerFactory.CreateSynchronizer(
                new Guid(destinationProvider.Identifier),
                _integrationPoint.DestinationConfiguration)
                .Returns(_dataSynchronizer);
            _jobManager.CheckBatchOnJobComplete(_job, _taskParams.BatchInstance.ToString()).Returns(true);
            _jobManager.GetJobsByBatchInstanceId(_integrationPoint.ArtifactId, _taskParams.BatchInstance)
                .Returns(associatedJobs);
            _jobManager.GetBatchesStatuses(_job, _taskParams.BatchInstance.ToString())
                .Returns(new BatchStatusQueryResult { ProcessingCount = 1 });

            _jobService.GetJob(_job.JobId).Returns(_job);
        }

        [Test]
        public void Execute_GoldFlow()
        {
            // act
            _instance.Execute(_job);

            // assert
            _batchStatus.Received(1).OnJobStart(_job);
            EnsureToSetJobHistroyErrorServiceProperties();
            _dataSynchronizer.Received(1).SyncData(Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(), Arg.Any<FieldMap[]>(), Arg.Any<ImportSettings>(), _jobStopManager, Arg.Any<IDiagnosticLog>());
            _batchStatus.Received(1).OnJobComplete(_job);
            _jobHistoryErrorService.Received().CommitErrors();
            EnsureToUpdateTheStopStateBackToNone();
        }

        [Test]
        public void Execute_IgnoreErrorWhenFailToUpdateStopStateToUnstoppable()
        {
            // arrange
            _jobStopManager.IsStopRequested().Returns(true);
            _jobService.When(manager => manager
                .UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.Unstoppable))
                .Do(info => { throw new Exception(); });

            // act
            _instance.Execute(_job);

            // assert
            _batchStatus.Received(1).OnJobStart(_job);
            EnsureToSetJobHistroyErrorServiceProperties();
            _dataSynchronizer
                .Received(1)
                .SyncData(
                    Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(),
                    Arg.Any<FieldMap[]>(),
                    Arg.Any<ImportSettings>(),
                    _jobStopManager,
                    Arg.Any<IDiagnosticLog>());
            _batchStatus.Received(1).OnJobComplete(_job);
            _jobHistoryErrorService.Received().CommitErrors();
            EnsureToUpdateTheStopStateBackToNone();
        }

        [Test]
        public void Execute_IgnoreErrorWhenFailToSetErrorStatusesToExpired()
        {
            // arrange
            _jobStopManager.IsStopRequested().Returns(true);
            _jobHistoryManager.When(manager => manager.SetErrorStatusesToExpired(_caseServiceContext.WorkspaceUserID, _jobHistory.ArtifactId)).Do(info => { throw new Exception(); });

            // act
            _instance.Execute(_job);

            // assert
            _batchStatus.Received(1).OnJobStart(_job);
            EnsureToSetJobHistroyErrorServiceProperties();
            _dataSynchronizer
                .Received(1)
                .SyncData(
                    Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(),
                    Arg.Any<FieldMap[]>(),
                    Arg.Any<ImportSettings>(),
                    _jobStopManager,
                    Arg.Any<IDiagnosticLog>());
            _batchStatus.Received(1).OnJobComplete(_job);
            _jobHistoryErrorService.Received().CommitErrors();
            EnsureToUpdateTheStopStateBackToNone();
        }

        [Test]
        public void Execute_StopBeforeImportingData()
        {
            // arrange
            _jobStopManager.When(manager => manager.ThrowIfStopRequested()).Do(info => { throw new OperationCanceledException(); });
            _jobStopManager.IsStopRequested().Returns(true);

            // act
            _instance.Execute(_job);

            // assert
            _dataSynchronizer.DidNotReceive().SyncData(Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(), Arg.Any<FieldMap[]>(), new ImportSettings(_integrationPoint.DestinationConfiguration), _jobStopManager, null);
            _jobService.Received(1).UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.Unstoppable); // mark job as unstoppable while finalizing the job.
            EnsureToUpdateTheStopStateBackToNone();
        }

        [Test]
        public void Execute_StopBeforeCallSyncData()
        {
            // arrange
            _jobStopManager
                .When(manager => manager.ThrowIfStopRequested())
                .Do(Callback.First(x => { })
                    .Then(x => { }).Then(x => { })
                    .Then(info => { throw new OperationCanceledException(); }));
            _jobStopManager.IsStopRequested().Returns(true);

            // act
            _instance.Execute(_job);

            // assert
            _dataSynchronizer.DidNotReceive().SyncData(Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(), Arg.Any<FieldMap[]>(), new ImportSettings(_integrationPoint.DestinationConfiguration), _jobStopManager, null);
            _jobService.Received(1).UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.Unstoppable); // mark job as unstoppable while finalizing the job.
            EnsureToUpdateTheStopStateBackToNone();
        }

        [Test]
        public void Execute_FailToUpdateStopStateBackToNone()
        {
            // arrange
            Exception exception = new Exception();
            _jobStopManager.IsStopRequested().Returns(true);
            _jobService.When(manager => manager
                .UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.None))
                .Do(info => { throw exception; });

            // act
            _instance.Execute(_job);

            // assert
            _batchStatus.Received(1).OnJobStart(_job);
            EnsureToSetJobHistroyErrorServiceProperties();
            _dataSynchronizer
                .Received(1)
                .SyncData(
                    Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(),
                    Arg.Any<FieldMap[]>(),
                    Arg.Any<ImportSettings>(),
                    _jobStopManager,
                    Arg.Any<IDiagnosticLog>());
            _batchStatus.Received(1).OnJobComplete(_job);
            _jobHistoryErrorService.Received().AddError(ErrorTypeChoices.JobHistoryErrorJob, exception);
            _jobHistoryErrorService.Received().CommitErrors();
        }

        [Test]
        public void Execute_EnsureToAlwaysDisposeJobStopManager()
        {
            // arrange
            _jobManager.CheckBatchOnJobComplete(_job, _taskParams.BatchInstance.ToString()).Returns(false);

            // act
            _instance.Execute(_job);

            // assert
            _jobStopManager.Received(1).Dispose();
        }

        private void EnsureToSetJobHistroyErrorServiceProperties()
        {
            _jobHistoryErrorService.Received(1).JobHistory = _jobHistory;
            _jobHistoryErrorService.Received(1).IntegrationPointDto = _integrationPoint;
            _jobHistoryErrorService.Received(1).JobStopManager = _jobStopManager;
            _jobHistoryErrorService.Received(1).SubscribeToBatchReporterEvents((object)_dataSynchronizer);
        }

        private void EnsureToUpdateTheStopStateBackToNone()
        {
            _jobStopManager.Received(1).Dispose();
            _jobService.Received().UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.None);
        }
    }
}
