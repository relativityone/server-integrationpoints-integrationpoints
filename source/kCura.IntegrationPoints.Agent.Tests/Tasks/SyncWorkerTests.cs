using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture]
	public class SyncWorkerTests : TestBase
	{
		private ICaseServiceContext _caseServiceContext;
		private IHelper _helper;
		private IDataProviderFactory _dataProviderFactory;
		private ISerializer _serializer;
		private ISynchronizerFactory _appDomainRdoSynchronizerFactory;
		private IJobHistoryService _jobHistoryService;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IManagerFactory _managerFactory;
		private IContextContainerFactory _contextContainerFactory;
		private IJobService _jobService;
		private IJobManager _jobManager;
		private IBatchStatus _batchStatus;
		private SyncWorker _instance;
		private Job _job;
		private Data.IntegrationPoint _integrationPoint;
		private TaskParameters _taskParams;
		private JobHistory _jobHistory;
		private IJobStopManager _jobStopManager;
		private List<FieldMap> _fieldsMap;
		private SourceProvider _sourceProvider;
		private IDataSourceProvider _dataSourceProvider;
		private IDataReader _sourceDataReader;
		private DestinationProvider _destinationProvider;
		private IDataSynchronizer _dataSynchronizer;
		private List<Job> _associatedJobs;
		private IContextContainer _contextContainer;
		private IJobHistoryManager _jobHistoryManager;
		private IDataReaderWrapperFactory _dataReaderWrapperFactory;

		[SetUp]
		public override void SetUp()
		{
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_helper = Substitute.For<IHelper>();
			_dataProviderFactory = Substitute.For<IDataProviderFactory>();
			_serializer = Substitute.For<ISerializer>();
			_appDomainRdoSynchronizerFactory = Substitute.For<ISynchronizerFactory>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			_jobManager = Substitute.For<IJobManager>();
			_batchStatus = Substitute.For<IBatchStatus>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_jobService = Substitute.For<IJobService>();
			_jobStopManager = Substitute.For<IJobStopManager>();
			_dataSourceProvider = Substitute.For<IDataSourceProvider>();
			_sourceDataReader = Substitute.For<IDataReader>();
			_dataSynchronizer = Substitute.For<IDataSynchronizer>();
			_contextContainer = Substitute.For<IContextContainer>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_dataReaderWrapperFactory = Substitute.For<IDataReaderWrapperFactory>();

			_instance = new SyncWorker(_caseServiceContext, _helper, _dataProviderFactory, _serializer, 
				_appDomainRdoSynchronizerFactory, _jobHistoryService, _jobHistoryErrorService, _jobManager, new IBatchStatus[] { _batchStatus },
				null, _managerFactory, _dataReaderWrapperFactory, _contextContainerFactory, _jobService );

			_job = JobHelper.GetJob(1, null, null, 1, 1, 111, 222, TaskType.SyncCustodianManagerWorker, new DateTime(), null, "detail",
				0, new DateTime(), 1, null, null);
			_integrationPoint = new Data.IntegrationPoint()
			{
				SourceProvider = 852,
				DestinationProvider = 942,
				FieldMappings = "fields",
				SourceConfiguration = "source config",
				DestinationConfiguration = "dest config"
			};
			_sourceProvider = new SourceProvider() {Identifier = Guid.NewGuid().ToString(), ApplicationIdentifier = Guid.NewGuid().ToString() };
			_destinationProvider = new DestinationProvider() {Identifier = Guid.NewGuid().ToString()};
			_jobHistory = new JobHistory() {ArtifactId = 9876546};
			_taskParams = new TaskParameters()
			{
				BatchInstance = Guid.NewGuid(),
				BatchParameters = new List<String>() { "1", "2" }
			};
			_associatedJobs = new List<Job>() {_job};
			_fieldsMap = new List<FieldMap>();
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Read(_job.RelatedObjectArtifactID).Returns(_integrationPoint);
			_caseServiceContext.RsapiService.SourceProviderLibrary.Read(_integrationPoint.SourceProvider.Value).Returns(_sourceProvider);
			_caseServiceContext.RsapiService.DestinationProviderLibrary.Read(_integrationPoint.DestinationProvider.Value).Returns(_destinationProvider);
			_serializer.Deserialize<TaskParameters>(_job.JobDetails).Returns(_taskParams);
			_jobHistoryService.CreateRdo(_integrationPoint, _taskParams.BatchInstance, 
				JobTypeChoices.JobHistoryRun, Arg.Any<DateTime>()).Returns(_jobHistory);
			_managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, _taskParams.BatchInstance, _job.JobId, true)
				.Returns(_jobStopManager);
			_serializer.Deserialize<List<FieldMap>>(_integrationPoint.FieldMappings).Returns(_fieldsMap);
			_dataProviderFactory.GetDataProvider(new Guid(_sourceProvider.ApplicationIdentifier),
				new Guid(_sourceProvider.Identifier), _helper).Returns(_dataSourceProvider);
			_dataSourceProvider.GetData(Arg.Any<List<FieldEntry>>(), (List<string>) _taskParams.BatchParameters,
				_integrationPoint.SourceConfiguration).Returns(_sourceDataReader);
			_appDomainRdoSynchronizerFactory.CreateSynchronizer(new Guid(_destinationProvider.Identifier),
				_integrationPoint.DestinationConfiguration).Returns(_dataSynchronizer);
			_jobManager.CheckBatchOnJobComplete(_job, _taskParams.BatchInstance.ToString()).Returns(true);
			_jobManager.GetJobsByBatchInstanceId(_integrationPoint.ArtifactId, _taskParams.BatchInstance)
				.Returns(_associatedJobs);
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
		}

		[Test]
		public void Execute_GoldFlow()
		{
			// act
			_instance.Execute(_job);

			// assert
			_batchStatus.Received(1).OnJobStart(_job);
			EnsureToSetJobHistroyErrorServiceProperties();
			_dataSynchronizer.Received(1).SyncData(Arg.Any<IDataReader>(), Arg.Any<FieldMap[]>(), _integrationPoint.DestinationConfiguration);
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
			_dataSynchronizer.Received(1).SyncData(Arg.Any<IDataReader>(), Arg.Any<FieldMap[]>(), _integrationPoint.DestinationConfiguration);
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
			_dataSynchronizer.Received(1).SyncData(Arg.Any<IDataReader>(), Arg.Any<FieldMap[]>(), _integrationPoint.DestinationConfiguration);
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
			_dataSynchronizer.DidNotReceive().SyncData(Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(), Arg.Any<FieldMap[]>(), _integrationPoint.DestinationConfiguration);
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
			_dataSynchronizer.DidNotReceive().SyncData(Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(), Arg.Any<FieldMap[]>(), _integrationPoint.DestinationConfiguration);
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
			_dataSynchronizer.Received(1).SyncData(Arg.Any<IDataReader>(), Arg.Any<FieldMap[]>(), _integrationPoint.DestinationConfiguration);
			_batchStatus.Received(1).OnJobComplete(_job);
			_jobHistoryErrorService.Received(1).AddError(ErrorTypeChoices.JobHistoryErrorJob, exception);
			_jobHistoryErrorService.Received().CommitErrors();
			
		}

		[Test]
		public void Excute_EnsureToAlwaysDisposeJobStopManager()
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
			_jobHistoryErrorService.Received(1).IntegrationPoint = _integrationPoint;
			_jobHistoryErrorService.Received(1).JobStopManager = _jobStopManager;
			_jobHistoryErrorService.Received(1).SubscribeToBatchReporterEvents((object)_dataSynchronizer);
		}

		private void EnsureToUpdateTheStopStateBackToNone()
		{
			_jobStopManager.Received(1).Dispose();
			_jobService.Received(1).UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.None);
		}
	}
}