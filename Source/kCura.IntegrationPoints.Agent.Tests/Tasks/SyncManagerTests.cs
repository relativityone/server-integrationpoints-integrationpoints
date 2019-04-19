﻿using System;
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
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture]
	public class SyncManagerTests : TestBase
	{
		private readonly Guid _defaultGuidValue = new Guid("4258D67D-63D4-4902-A48A-B1B19649ABFA");
		private readonly Guid _jobGuidValue = new Guid("0D01AF2F-5AF5-4F4D-820C-90471AD75750");

		private IManagerFactory _managerFactory;
		private IContextContainerFactory _contextContainerFactory;
		private ICaseServiceContext _caseServiceContext;
		private IDataProviderFactory _dataProviderFactory;
		private IJobManager _jobManager;
		private IJobService _jobService;
		private IHelper _helper;
		private IIntegrationPointService _integrationPointService;
		private ISerializer _serializer;
		private IGuidService _guidService;
		private IJobHistoryService _jobHistoryService;
		private IScheduleRuleFactory _scheduleRuleFactory;
		private IBatchStatus _batchStatus;
		private SyncManager _instance;
		private Job _job;
		private SourceProvider _sourceProvider;
		private Data.IntegrationPoint _integrationPoint;
		private IDataSourceProvider _dataSourceProvider;
		private IDataReader _dataReader;
		private string _data;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private TestSyncManager _syncManagerEventHelper;
		private JobHistory _jobHistory;
		private IJobStopManager _jobStopManager;
		private Guid _batchInstance;
		private List<IBatchStatus> _batchStatuses;
		private TaskResult _taskResult;
		private IContextContainer _contextContainer;
		private IJobHistoryManager _jobHistoryManager;
		private IAgentValidator _agentValidator;

		[SetUp]
		public override void SetUp()
		{
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_dataProviderFactory = Substitute.For<IDataProviderFactory>();
			_jobManager = Substitute.For<IJobManager>();
			_jobService = Substitute.For<IJobService>();
			_helper = Substitute.For<IHelper>();
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_serializer = Substitute.For<ISerializer>();
			_guidService = Substitute.For<IGuidService>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_batchStatus = Substitute.For<IBatchStatus>();
			_batchStatuses = new List<IBatchStatus>() { _batchStatus };
			_dataSourceProvider = Substitute.For<IDataSourceProvider>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			_dataReader = Substitute.For<IDataReader>();
			_jobStopManager = Substitute.For<IJobStopManager>();
			_contextContainer = Substitute.For<IContextContainer>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_agentValidator = Substitute.For<IAgentValidator>();

			_job = GetJob(null);

			_sourceProvider = new SourceProvider()
			{
				ApplicationIdentifier = Guid.NewGuid().ToString(),
				Identifier = Guid.NewGuid().ToString()
			};
			_integrationPoint = new Data.IntegrationPoint()
			{
				SourceProvider = 8502,
				SourceConfiguration = "sourceConfiguration",
				SecuredConfiguration = "securedConfiguration",
				FieldMappings = "fields"
			};
			_jobHistory = new JobHistory()
			{
				ArtifactId = 1,
				JobStatus = JobStatusChoices.JobHistoryPending
			};
			_taskResult = new TaskResult();

			_batchInstance = Guid.NewGuid();
			_instance = new SyncManager(_caseServiceContext, _dataProviderFactory, _jobManager, _jobService, _helper,
				_integrationPointService, _serializer, _guidService, _jobHistoryService,
				_jobHistoryErrorService, _scheduleRuleFactory, _managerFactory,
				_contextContainerFactory, _batchStatuses, _agentValidator)
			{
				BatchInstance = _batchInstance,
				IntegrationPoint = _integrationPoint
			};

			_syncManagerEventHelper = new TestSyncManager(_caseServiceContext, _dataProviderFactory, _jobManager, _jobService, _helper,
			   _integrationPointService, _serializer, _guidService, _jobHistoryService,
			   _jobHistoryErrorService, _scheduleRuleFactory, _managerFactory,
			   _contextContainerFactory, _batchStatuses, _agentValidator);

			_data = "data";
			_caseServiceContext.RsapiService.RelativityObjectManager.Read<SourceProvider>(_integrationPoint.SourceProvider.Value).Returns(_sourceProvider);
			_dataProviderFactory.GetDataProvider(
					Arg.Is<Guid>(appGuid => appGuid == new Guid(_sourceProvider.ApplicationIdentifier)),
					Arg.Is<Guid>(providerGuid => providerGuid == new Guid(_sourceProvider.Identifier))).Returns(_dataSourceProvider);
			_dataSourceProvider.GetBatchableIds(Arg.Any<FieldEntry>(), Arg.Is<DataSourceProviderConfiguration>(
				x => x.Configuration.Equals(_integrationPoint.SourceConfiguration) && x.SecuredConfiguration.Equals(_integrationPoint.SecuredConfiguration))).Returns(_dataReader);
			_dataReader.Read().Returns(true, false);
			_dataReader.GetString(0).Returns(_data);
			_managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, _batchInstance, _job.JobId, true)
				.Returns(_jobStopManager);
			_jobService.GetJobNextUtcRunDateTime(_job, _scheduleRuleFactory, Arg.Any<TaskResult>()).Returns(DateTime.Now);
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_managerFactory.CreateJobHistoryManager(_contextContainer).Returns(_jobHistoryManager);
		}

		[Test]
		public void GetBatchInstance_NoJobDetails_CorrectOutput()
		{
			//ARRANGE
			var serializer = NSubstitute.Substitute.For<JSONSerializer>();
			var guidService = NSubstitute.Substitute.For<IGuidService>();
			guidService.NewGuid().Returns(_defaultGuidValue);
			SyncManager manager = new SyncManager(null, null, null, null, _helper, null, serializer, guidService, null, null, null, _managerFactory, _contextContainerFactory, null,
				_agentValidator);
			Job job = GetJob(null);

			//ACT
			Guid returnValue = manager.GetBatchInstance(job);

			//ASSERT
			Assert.AreEqual(_defaultGuidValue, returnValue);
		}

		[Test]
		public void GetBatchInstance_GuidInJobDetails_CorrectOutput()
		{
			//ARRANGE
			var serializer = NSubstitute.Substitute.For<JSONSerializer>();
			var guidService = NSubstitute.Substitute.For<IGuidService>();
			guidService.NewGuid().Returns(_defaultGuidValue);
			SyncManager manager = new SyncManager(null, null, null, null, _helper, null, serializer, guidService, null, null, null, _managerFactory, _contextContainerFactory, null, _agentValidator);
			Job job = GetJob(serializer.Serialize(new TaskParameters() { BatchInstance = _jobGuidValue }));

			//ACT
			Guid returnValue = manager.GetBatchInstance(job);

			//ASSERT
			Assert.AreEqual(_jobGuidValue, returnValue);
		}

		[Test]
		public void GetBatchInstance_BadGuidInJobDetails_CorrectOutput()
		{
			//ARRANGE
			var serializer = NSubstitute.Substitute.For<JSONSerializer>();
			var guidService = NSubstitute.Substitute.For<IGuidService>();
			guidService.NewGuid().Returns(_defaultGuidValue);
			SyncManager manager = new SyncManager(null, null, null, null, _helper, null, serializer, guidService, null, null, null, _managerFactory, _contextContainerFactory, null,
				_agentValidator);
			Job job = GetJob("BAD_GUID");

			//ACT

			Exception innerException = null;
			try
			{
				manager.GetBatchInstance(job);
			}
			catch (Exception ex)
			{
				innerException = ex;
			}

			//ASSERT
			Assert.AreEqual("Failed to get Batch Instance.", innerException.Message);
			Assert.IsNotNull(innerException.InnerException);
		}

		[Test]
		public void CreateBatchJob_GoldFlow()
		{
			// ARRANGE
			List<string> ids = new List<string>() { "id1", "id2" };

			// ACT
			_instance.CreateBatchJob(_job, ids);

			// ASSERT
			_jobManager.Received(1).CreateJobWithTracker(_job, Arg.Is<TaskParameters>(parameter => parameter.BatchInstance == _instance.BatchInstance && parameter.BatchParameters == (object)ids), TaskType.SyncWorker, _instance.BatchInstance.ToString());
			Assert.AreEqual(1, _instance.BatchJobCount);
		}

		[Test]
		public void CreateBatchJob_FailToCreateJob()
		{
			// ARRANGE
			List<string> ids = new List<string>() { "id1", "id2" };
			_jobManager.When(manager => manager.CreateJobWithTracker(_job, Arg.Is<TaskParameters>(parameter => parameter.BatchInstance == _instance.BatchInstance && parameter.BatchParameters == (object)ids), TaskType.SyncWorker, _instance.BatchInstance.ToString()))
				.Do(info => { throw new Exception(); });

			// ACT & ASSERT
			Assert.Throws<Exception>(() => _instance.CreateBatchJob(_job, ids));
			Assert.AreEqual(0, _instance.BatchJobCount);
		}

		[Test]
		public void GetUnbatchedIDs_GoldFlow()
		{
			// ARRANGE
			const string jobDetail = "new jobDetail";
			_serializer.Serialize(Arg.Is<TaskParameters>(parameter => parameter.BatchInstance == _instance.BatchInstance)).Returns(jobDetail);

			// ACT
			IEnumerable<String> ids = _instance.GetUnbatchedIDs(_job);

			// ASSERT
			_batchStatus.Received(1).OnJobStart(_job);
			_jobHistoryErrorService.Received(1).CommitErrors();
			Assert.IsNotNull(ids);
			Assert.IsTrue(ids.SequenceEqual(new[] { _data }));
		}

		[Test]
		public void GetUnbatchedIDs_FailToSerializeJobDetial()
		{
			// ARRANGE
			Exception exception = new Exception();
			_serializer.Serialize(Arg.Any<TaskParameters>()).Throws(exception);

			// ACT
			IEnumerable<String> ids = _instance.GetUnbatchedIDs(_job);

			// ASSERT
			_jobHistoryErrorService.Received(1).AddError(Arg.Is<Choice>(errorType => errorType.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)), exception);
			_jobHistoryErrorService.Received(1).CommitErrors();
			Assert.IsNotNull(ids);
			Assert.AreEqual(0, ids.Count());
		}

		[Test]
		public void RaiseJobPreExecute_GoldFlow()
		{
			// arrange
			PreJobExecutionGoldFlowSetup();

			// act
			_syncManagerEventHelper.RaisePreEvent(_job, _taskResult);

			// assert
			Assert.AreEqual(_batchInstance, _syncManagerEventHelper.BatchInstance);
			Assert.AreSame(_integrationPoint, _syncManagerEventHelper.IntegrationPoint);
			Assert.AreSame(_jobHistory, _syncManagerEventHelper.JobHistory);

			_jobHistoryErrorService.Received(1).IntegrationPoint = _integrationPoint;
			_jobHistoryErrorService.Received(1).JobHistory = _jobHistory;

			// We expect to update Start Time and State of JobHistory object
			_jobHistoryService.Received(2).UpdateRdo(_jobHistory);
			_managerFactory.Received(1).CreateJobStopManager(_jobService, _jobHistoryService, _batchInstance, _job.JobId, true);
		}

		[Test]
		public void RaiseJobPreExecute_FailToGetBatchInstance()
		{
			// arrange
			_job.JobDetails = "something something here";
			Exception exception = new Exception();
			_serializer.Deserialize<TaskParameters>(_job.JobDetails).Throws(exception);

			// act
			_syncManagerEventHelper.RaisePreEvent(_job, _taskResult);

			// assert
			_jobHistoryErrorService.Received(1).AddError(
				Arg.Is<Choice>(errorType => errorType.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)),
				Arg.Is<Exception>(ex => ex.Message == "Failed to get Batch Instance."));
			_jobHistoryErrorService.Received(1).CommitErrors();
		}

		[Test]
		public void RaiseJobPostExecute_GoldFlow()
		{
			// arrange
			int itemCount = 97894;
			_job.SerializedScheduleRule = null;
			PreJobExecutionGoldFlowSetup();
			_syncManagerEventHelper.RaisePreEvent(_job, _taskResult);
			_integrationPoint.NextScheduledRuntimeUTC = null;
			_jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(_jobHistory);
			// act
			_syncManagerEventHelper.RaisePostEvent(_job, _taskResult, itemCount);

			// assert
			_batchStatus.Received(1).OnJobComplete(_job);
			Assert.IsNull(_integrationPoint.NextScheduledRuntimeUTC);
			_caseServiceContext.RsapiService.RelativityObjectManager.Received(1).Update(_integrationPoint);
			_jobHistoryService.Received().UpdateRdo(_jobHistory);
			_jobHistoryErrorService.Received().CommitErrors();
		}

		[Test]
		public void RaiseJobPostExecute_GoldFlow_StopRequest()
		{
			// arrange
			int itemCount = 97894;
			_job.SerializedScheduleRule = "blah blah";
			PreJobExecutionGoldFlowSetup();
			_syncManagerEventHelper.RaisePreEvent(_job, _taskResult);
			_syncManagerEventHelper.BatchJobCount = 0;
			_integrationPoint.NextScheduledRuntimeUTC = null;
			_jobStopManager.IsStopRequested().Returns(true);
			_jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(_jobHistory);

			// act
			_syncManagerEventHelper.RaisePostEvent(_job, _taskResult, itemCount);

			// assert
			ValidatePostJobExecuteOnStoppingJob();
		}

		[Test]
		public void RaiseJobPostExecute_ErrorsOccur()
		{
			// arrange
			Exception exception1 = new Exception();
			Exception exception2 = new Exception();
			Exception exception3 = new Exception();
			Exception exception4 = new Exception();
			Exception exception5 = new Exception();
			_job.SerializedScheduleRule = "blah blah";
			PreJobExecutionGoldFlowSetup();
			_jobStopManager.IsStopRequested().Returns(true);

			_syncManagerEventHelper.RaisePreEvent(_job, _taskResult);
			_caseServiceContext.RsapiService.RelativityObjectManager.Update(_integrationPoint).Throws(exception1);
			_jobService.When(obj => obj.UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.None)).Do(info => { throw exception2; });
			_batchStatus.When(obj => obj.OnJobComplete(_job)).Do(info => { throw exception3; });
			_jobHistoryManager
				.When(obj => obj.SetErrorStatusesToExpired(_caseServiceContext.WorkspaceID, _jobHistory.ArtifactId))
				.Do(info =>
				{
					throw exception4;
				});
			_jobHistoryService.When(x => x.UpdateRdo(_jobHistory)).Do(x => { throw exception5;});
			_jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(_jobHistory);

			// act
			_syncManagerEventHelper.RaisePostEvent(_job, _taskResult, 0);

			// assert
			_jobHistoryErrorService.Received(1).AddError(
				Arg.Is<Choice>(errorType => errorType.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)),
				Arg.Is<Exception>((exception) => ValidatePostExecuteExceptions(exception, exception1, exception2, exception3, exception4, exception5)));
			_jobHistoryErrorService.Received().CommitErrors();
		}

		private bool ValidatePostExecuteExceptions(Exception exception, params Exception[] exceptions)
		{
			AggregateException ex = exception.InnerException as AggregateException;
			AggregateException finalizeExceptions = (AggregateException)ex.InnerExceptions[1];
			bool isValid = ex.InnerExceptions[0] == exceptions[0];
			isValid &= finalizeExceptions.InnerExceptions[0] == exceptions[1];
			isValid &= finalizeExceptions.InnerExceptions[1] == exceptions[2];
			isValid &= finalizeExceptions.InnerExceptions[2] == exceptions[3];
			isValid &= ex.InnerExceptions[2] == exceptions[4];
			return isValid;
		}

		[Test]
		public void JobPreExecute_OnStoppingJob()
		{
			// arrange
			PreJobExecutionGoldFlowSetup();
			_jobStopManager.When(obj => obj.ThrowIfStopRequested()).Do(info => { throw new OperationCanceledException(); });

			// act
			Assert.Throws<OperationCanceledException>(() => _syncManagerEventHelper.RaisePreEvent(_job, _taskResult));

			// assert
			_jobStopManager.Received(1).Dispose();
		}

		[Test]
		public void BatchTask_StopBeforeBatchingTask()
		{
			// arrange
			PreJobExecutionGoldFlowSetup();

			_syncManagerEventHelper.RaisePreEvent(_job, _taskResult);
			IEnumerable<string> ids = _syncManagerEventHelper.GetUnbatchedIDs(_job);
			_jobStopManager.When(obj => obj.ThrowIfStopRequested()).Do(info => { throw new OperationCanceledException(); });

			// act
			Assert.Throws<OperationCanceledException>(() => _syncManagerEventHelper.BatchTask(_job, ids));

			// assert
			_jobManager.DidNotReceive().CreateJobWithTracker(_job, Arg.Any<TaskParameters>(), TaskType.SyncWorker, Arg.Any<string>());
			Assert.AreEqual(0, _syncManagerEventHelper.BatchJobCount);
		}

		[Test]
		public void BatchTask_StopWhileCreatingTheFirstSubJob()
		{
			// arrange
			PreJobExecutionGoldFlowSetup();
			_dataReader.Read().Returns(true);
			_dataReader.GetString(0).Returns("1", "2", "3");

			_syncManagerEventHelper.RaisePreEvent(_job, _taskResult);
			IEnumerable<string> ids = _syncManagerEventHelper.GetUnbatchedIDs(_job);
			_jobStopManager.When(obj => obj.ThrowIfStopRequested())
				.Do(Callback.First(info => { })
				.Then(info => { throw new OperationCanceledException(); }));

			// act
			Assert.Throws<OperationCanceledException>(() => _syncManagerEventHelper.BatchTask(_job, ids));

			// assert
			_jobManager.DidNotReceive().CreateJobWithTracker(_job, Arg.Any<TaskParameters>(), TaskType.SyncWorker, Arg.Any<string>());
			Assert.AreEqual(0, _syncManagerEventHelper.BatchJobCount);
		}

		[Test]
		public void BatchTask_AfterCreatingAJob()
		{
			// arrange
			PreJobExecutionGoldFlowSetup();
			_dataReader.Read().Returns(true);
			_dataReader.GetString(0).Returns("1", "2", "3");

			_syncManagerEventHelper.RaisePreEvent(_job, _taskResult);
			IEnumerable<string> ids = _syncManagerEventHelper.GetUnbatchedIDs(_job);
			_jobStopManager.When(obj => obj.ThrowIfStopRequested())
				.Do(Callback.First(info => { }).Then(info => { })
				.Then(info => { }).Then(info => { })
				.Then(info => { throw new OperationCanceledException(); }));

			// act
			Assert.Throws<OperationCanceledException>(() => _syncManagerEventHelper.BatchTask(_job, ids));

			// assert
			_jobManager.Received(1).CreateJobWithTracker(_job, Arg.Is<TaskParameters>(param => ((List<string>)param.BatchParameters).SequenceEqual(new[] { "1", "2" })), TaskType.SyncWorker, Arg.Any<string>());
			Assert.AreEqual(1, _syncManagerEventHelper.BatchJobCount);
		}

		[Test]
		public void BatchTask_GoldFlow()
		{
			// arrange
			PreJobExecutionGoldFlowSetup();
			_dataReader.Read().Returns(true, true, true, false);
			_dataReader.GetString(0).Returns("1", "2", "3");

			_syncManagerEventHelper.RaisePreEvent(_job, _taskResult);
			IEnumerable<string> ids = _syncManagerEventHelper.GetUnbatchedIDs(_job);

			// act
			_syncManagerEventHelper.BatchTask(_job, ids);

			// assert
			_jobManager.Received(1).CreateJobWithTracker(_job, Arg.Is<TaskParameters>(param => ((List<string>)param.BatchParameters).SequenceEqual(new[] { "1", "2" })), TaskType.SyncWorker, Arg.Any<string>());
			_jobManager.Received(1).CreateJobWithTracker(_job, Arg.Is<TaskParameters>(param => ((List<string>)param.BatchParameters).SequenceEqual(new[] { "3" })), TaskType.SyncWorker, Arg.Any<string>());
			Assert.AreEqual(2, _syncManagerEventHelper.BatchJobCount);
		}

		[Test]
		public void ItShouldThrowValidationException()
		{
			// arrange

			PreJobExecutionGoldFlowSetup();
			_agentValidator.When(x => x.Validate(_integrationPoint, _job.SubmittedBy)).Do(x =>
				{
					throw new PermissionException();
				}
			);
			bool jobValidationFailedUpdated = false;
			_jobHistoryService.When(x => x.UpdateRdo(Arg.Is<JobHistory>( jh => jh.JobStatus.Guids.First() == JobStatusChoices.JobHistoryValidationFailed.Guids.First()))).Do(item =>
				{
					jobValidationFailedUpdated = true;
				}

			);

			// act
			PermissionException ex = Assert.Throws<PermissionException>(() => _syncManagerEventHelper.RaisePreEvent(_job, _taskResult));

			// assert
			Assert.That(_taskResult.Status, Is.EqualTo(TaskStatusEnum.Fail));

			// job status should be changed
			Assert.That(jobValidationFailedUpdated);
			_jobHistoryErrorService.Received(1).AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
		}

		private void ValidatePostJobExecuteOnStoppingJob()
		{
			_jobService.Received(1).UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.None);
			_batchStatus.Received(1).OnJobComplete(_job);
			_jobHistoryManager.Received(1).SetErrorStatusesToExpired(_caseServiceContext.WorkspaceID, _jobHistory.ArtifactId);
			Assert.IsNotNull(_integrationPoint.NextScheduledRuntimeUTC);
			_caseServiceContext.RsapiService.RelativityObjectManager.Received(1).Update(_integrationPoint);
			_jobHistoryService.Received().UpdateRdo(_jobHistory);
			_jobHistoryErrorService.Received().CommitErrors();
		}

		private void PreJobExecutionGoldFlowSetup()
		{
			_job.JobDetails = "something something here";
			_integrationPointService.ReadIntegrationPoint(_job.RelatedObjectArtifactID).Returns(_integrationPoint);
			_jobHistoryService.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _batchInstance, Arg.Any<DateTime>())
				.Returns(_jobHistory);
			_jobHistory.StartTimeUTC = null;
			_serializer.Deserialize<TaskParameters>(_job.JobDetails)
				.Returns((new TaskParameters() { BatchInstance = _batchInstance }));
		}

		private Job GetJob(string jobDetails)
		{
			return JobHelper.GetJob(1, null, null, 1, 1, 111, 222, TaskType.SyncEntityManagerWorker, new DateTime(), null, jobDetails,
				0, new DateTime(), 1, null, null);
		}

		/// <summary>
		/// Use to test pre and post events
		/// </summary>
		private class TestSyncManager : SyncManager
		{
			public TestSyncManager(ICaseServiceContext caseServiceContext,
				IDataProviderFactory providerFactory,
				IJobManager jobManager,
				IJobService jobService,
				IHelper helper,
				IIntegrationPointService integrationPointService,
				ISerializer serializer,
				IGuidService guidService,
				IJobHistoryService jobHistoryService,
				IJobHistoryErrorService jobHistoryErrorService,
				IScheduleRuleFactory scheduleRuleFactory,
				IManagerFactory managerFactory,
				IContextContainerFactory contextContainerFactory,
				IEnumerable<IBatchStatus> batchStatuses,
				IAgentValidator agentValidator) : base(caseServiceContext, providerFactory, jobManager, jobService, helper, integrationPointService, serializer, guidService,
				jobHistoryService, jobHistoryErrorService, scheduleRuleFactory, managerFactory, contextContainerFactory, batchStatuses, agentValidator)
			{
			}

			public void RaisePreEvent(Job job, TaskResult taskResult)
			{
				OnRaiseJobPreExecute(job, taskResult);
			}

			public void RaisePostEvent(Job job, TaskResult taskResult, int items)
			{
				OnRaiseJobPostExecute(job, taskResult, items);
			}

			public override int BatchSize => 2;
		}
	}
}
