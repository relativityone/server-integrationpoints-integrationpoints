using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Tests;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Tests
{
    [TestFixture]
    public class JobServiceTests : TestBase
    {
        private IAgentService _agentService;

        private const string _MOCK_JOB_DETAILS = "There should be details";
        private const long _MOCK_JOB_ID = 123;
        private const long _ROOT_JOB_ID = 287;
        private const long _PARENT_JOB_ID = 756;
        private const int _AGENT_TYPE_ID = 4239;
        private const int _WORKSPACE_ID = 3429;
        private const int _RELATED_OBJECT_ARTIFACT_ID = 432;
        private const TaskType _TASK_TYPE = TaskType.ImportService;
        private static readonly DateTime _nextRunTime = new DateTime(2020, 12, 31);
        private static readonly DateTime _lastRunTime = new DateTime(2019, 1, 1);
        private const int _JOB_FLAGS = 48923;
        private static readonly DateTime _submittedDate = new DateTime(2016, 1, 6);
        private const int _SUBMITTED_BY = 4936;

        private int _goldFlowAgentTypeId = 54;
        private string _goldFlowAgentName = "AgentName";
        private string _goldFlowAgentNamespace = "Namespace";
        private readonly Guid _goldFlowAgentGuid = Guid.NewGuid();
        private readonly DateTime _mockScheduleRuleReturnDate = new DateTime(2020, 12, 31);
        private readonly IHelper _mockEmptyDbHelper = Substitute.For<IHelper>();
        private IJobServiceDataProvider _mockDataProvider;

        public override void FixtureSetUp()
        {
            base.FixtureSetUp();
            _agentService = Substitute.For<IAgentService>();
            _agentService.AgentTypeInformation.Returns(AgentTypeInformationHelper.
                CreateAgentTypeInformation(
                _goldFlowAgentTypeId, _goldFlowAgentName, _goldFlowAgentNamespace, _goldFlowAgentGuid));
        }

        public override void SetUp()
        {
            _mockDataProvider = Substitute.For<IJobServiceDataProvider>();
        }

        [Test]
        public void GetNextQueueJob_NoJobDataRowFound_ReturnsNull()
        {
            var agentId = 1;
            var resourceGroupIds = new[] { 1, 2 };
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            Job job = service.GetNextQueueJob(resourceGroupIds, agentId);

            _mockDataProvider.Received().GetNextQueueJob(agentId, Arg.Any<int>(), Arg.Any<int[]>());
            Assert.IsNull(job);
        }

        [Test]
        public void GetNextQueueJob_JobDataRowFound_ReturnsJob()
        {
            var agentId = 1;
            var resourceGroupIds = new[] { 1, 2 };
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            dataProvider.GetNextQueueJob(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int[]>()).Returns(GetMockJobDataRow());
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            Job job = service.GetNextQueueJob(resourceGroupIds, agentId);

            Assert.AreEqual(job.JobId, _MOCK_JOB_ID);
        }

        [Test]
        public void GetNextQueueJob_ResourceGroupIdsIsNull_ThrowsArgumentNullException()
        {
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            Assert.Throws<ArgumentNullException>(() => service.GetNextQueueJob(null, 1));
        }

        [Test]
        public void GetNextQueueJob_ResourceGroupIdsEmpty_ThrowsArgumentException()
        {
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            Assert.Throws<ArgumentException>(() => service.GetNextQueueJob(new List<int>(), 1));
        }

        [Test]
        public void GetJobNextUtcRunDateTime_ProperJobAndSchedule_DateEqualsScheduledDate()
        {
            Job job = GetMockJob();
            IScheduleRuleFactory scheduleRuleFactory = CreateScheduleRuleFactoryWithRuleReturning(_mockScheduleRuleReturnDate);
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            DateTime? returnedDate = service.GetJobNextUtcRunDateTime(job, scheduleRuleFactory, new TaskResult());

            Assert.AreEqual(returnedDate, _mockScheduleRuleReturnDate);
        }

        [Test]
        public void GetJobNextUtcRunDateTime_ScheduleRuleReturnsNullDate_ReturnsNull()
        {
            Job job = GetMockJob();
            IScheduleRuleFactory scheduleRuleFactory = CreateScheduleRuleFactoryWithRuleReturning(null);
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            DateTime? returnedDate = service.GetJobNextUtcRunDateTime(job, scheduleRuleFactory, new TaskResult());

            Assert.IsNull(returnedDate);
        }

        [Test]
        public void GetNextJobUtcRunDateTime_JobIsNull_ReturnsNull()
        {
            IScheduleRuleFactory scheduleRuleFactory = CreateScheduleRuleFactoryWithRuleReturning(_mockScheduleRuleReturnDate);
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            DateTime? returnedDate = service.GetJobNextUtcRunDateTime(null, scheduleRuleFactory, new TaskResult());

            Assert.IsNull(returnedDate);
        }

        [Test]
        public void GetNextJobUtcRunDateTime_ScheduleFactoryReturnsNull_ReturnsNull()
        {
            Job job = GetMockJob();
            var scheduleFactory = Substitute.For<IScheduleRuleFactory>();
            scheduleFactory.Deserialize(Arg.Any<Job>()).Returns(_ => null);
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            DateTime? returnedDate = service.GetJobNextUtcRunDateTime(job, scheduleFactory, new TaskResult());

            Assert.IsNull(returnedDate);
        }

        [Test]
        public void GetJobNextUtcRunDateTime_ScheduleRuleFactoryNull_ThrowsArgumentNullException()
        {
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            Assert.Throws<ArgumentNullException>(() => service.GetJobNextUtcRunDateTime(null, null, new TaskResult()));
        }

        [Test]
        public void FinalizeJob_ProperJobScheduleRuleReturnsDate_ReturnsStateModified()
        {
            Job job = GetMockJob();
            IScheduleRuleFactory scheduleRuleFactory = CreateScheduleRuleFactoryWithRuleReturning(_mockScheduleRuleReturnDate);
            
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            FinalizeJobResult result = service.FinalizeJob(job, scheduleRuleFactory, new TaskResult());

            _mockDataProvider.Received().UpdateScheduledJob(_MOCK_JOB_ID, _mockScheduleRuleReturnDate);
            Assert.AreEqual(result.JobState, JobLogState.Modified);
        }

        [Test]
        public void FinalizeJob_ProperJobScheduleRuleReturnsNull_ReturnsStateDeleted()
        {
            Job job = GetMockJob();
            IScheduleRuleFactory scheduleRuleFactory = CreateScheduleRuleFactoryWithRuleReturning(null);
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            FinalizeJobResult result = service.FinalizeJob(job, scheduleRuleFactory, new TaskResult());

            _mockDataProvider.Received().DeleteJob(_MOCK_JOB_ID);
            Assert.AreEqual(result.JobState, JobLogState.Deleted);
        }

        [Test]
        public void FinalizeJob_JobIsNull_ResultStateFinished()
        {
            IScheduleRuleFactory scheduleFactory = CreateScheduleRuleFactoryWithRuleReturning(_mockScheduleRuleReturnDate);
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            FinalizeJobResult result = service.FinalizeJob(null, scheduleFactory, new TaskResult());

            Assert.AreEqual(result.JobState, JobLogState.Finished);
        }

        [Test]
        public void FinalizeJob_ScheduleRuleFactoryNull_ThrowsArgumentNullException()
        {
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            Assert.Throws<ArgumentNullException>(() => service.FinalizeJob(null, null, new TaskResult()));
        }

        [Test]
        public void UnlockJobs_GoldFlow()
        {
            int agentId = 345;
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            service.UnlockJobs(agentId);

            _mockDataProvider.Received().UnlockScheduledJob(agentId);
        }

        [Test]
        public void CreateJob_ScheduleRuleReturnsDate_ReturnsValidJob()
        {
            string taskTypeString = _TASK_TYPE.ToString();
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            dataProvider.CreateScheduledJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, _nextRunTime,
                    _AGENT_TYPE_ID, null, null, _MOCK_JOB_DETAILS, _JOB_FLAGS, _SUBMITTED_BY, _ROOT_JOB_ID,
                    _PARENT_JOB_ID)
                .ReturnsForAnyArgs(GetMockJobDataRow());
            IScheduleRule scheduleRule = CreateScheduleRuleReturning(_mockScheduleRuleReturnDate);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);

            
            Job job = service.CreateJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, scheduleRule,
                _MOCK_JOB_DETAILS, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);


            Assert.AreEqual(job.JobDetails, _MOCK_JOB_DETAILS);
        }

        [Test]
        public void CreateJob_ScheduleRuleReturnsNullQueryReturnsJob_ReturnsJobDeletesJob()
        {
            string taskTypeString = _TASK_TYPE.ToString();
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            DataTable dataTable = GetDataTableWithRow();
            dataProvider.CreateScheduledJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, _nextRunTime,
                    _AGENT_TYPE_ID, null, null, _MOCK_JOB_DETAILS, _JOB_FLAGS, _SUBMITTED_BY, _ROOT_JOB_ID,
                    _PARENT_JOB_ID)
                .ReturnsForAnyArgs(GetMockJobDataRow());
            dataProvider.GetJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, Arg.Any<List<string>>())
                .ReturnsForAnyArgs(dataTable);
            IScheduleRule scheduleRule = CreateScheduleRuleReturning(null);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);


            Job job = service.CreateJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, scheduleRule,
                _MOCK_JOB_DETAILS, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);


            dataProvider.Received().DeleteJob(_MOCK_JOB_ID);
            Assert.AreEqual(job.JobDetails, _MOCK_JOB_DETAILS);
        }

        [Test]
        public void CreateJob_ScheduleRuleReturnsNullQueryNotReturnsJob_ReturnsNull()
        {
            string taskTypeString = _TASK_TYPE.ToString();
            var scheduleRule = CreateScheduleRuleReturning(null);
            DataTable dataTable = JobHelper.CreateEmptyJobDataTable();
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            dataProvider.GetJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, Arg.Any<List<string>>())
                .ReturnsForAnyArgs(dataTable);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);


            Job job = service.CreateJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, scheduleRule,
                _MOCK_JOB_DETAILS, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);


            dataProvider.DidNotReceive().DeleteJob(Arg.Any<long>());
            Assert.IsNull(job);
        }

        [Test]
        public void CreateJob_CreateJobReturnsJobDataRow_ReturnsProperJob()
        {
            string taskTypeString = _TASK_TYPE.ToString();
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            DataTable dataTable = GetDataTableWithRow();
            dataProvider.CreateScheduledJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, _nextRunTime,
                    _AGENT_TYPE_ID, null, null, _MOCK_JOB_DETAILS, _JOB_FLAGS, _SUBMITTED_BY, _ROOT_JOB_ID,
                    _PARENT_JOB_ID)
                .ReturnsForAnyArgs(GetMockJobDataRow());
            dataProvider.GetJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, Arg.Any<List<string>>())
                .ReturnsForAnyArgs(dataTable);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);


            Job job = service.CreateJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, _nextRunTime,
                _MOCK_JOB_DETAILS, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);


            Assert.AreEqual(job.JobId, _MOCK_JOB_ID);
        }

        [Test]
        public void CreateJob_CreateJobReturnsEmptyDataTable_ReturnsNull()
        {
            string taskTypeString = _TASK_TYPE.ToString();
            DataTable dataTable = JobHelper.CreateEmptyJobDataTable();
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            dataProvider.GetJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, Arg.Any<List<string>>())
                .ReturnsForAnyArgs(dataTable);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);

            Job job = service.CreateJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, _nextRunTime,
                _MOCK_JOB_DETAILS, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);

            Assert.IsNull(job);
        }

        [Test]
        public void DeleteJob_RunsDelete()
        {
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            service.DeleteJob(_MOCK_JOB_ID);

            _mockDataProvider.Received().DeleteJob(_MOCK_JOB_ID);
        }

        [Test]
        public void GetJob_QueryReturnsDataRow_ReturnsJob()
        {
            DataRow row = GetMockJobDataRow();
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            dataProvider.GetJob(_MOCK_JOB_ID).Returns(row);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);

            Job job = service.GetJob(_MOCK_JOB_ID);

            Assert.AreEqual(job.JobId, _MOCK_JOB_ID);
        }

        [Test]
        public void GetJob_QueryReturnsNull_ReturnsNull()
        {
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            dataProvider.GetJob(_MOCK_JOB_ID).Returns(_ => null);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);

            Job job = service.GetJob(_MOCK_JOB_ID);

            Assert.IsNull(job);
        }

        [Test]
        public void GetJobs_ProperIntegrationPointId_ReturnsListOfJobs()
        {
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            DataTable dataTable = GetDataTableWithRow();
            dataTable.Rows.Add(GetMockJobDataRow(dataTable));
            dataProvider.GetJobsByIntegrationPointId(_RELATED_OBJECT_ARTIFACT_ID)
                .ReturnsForAnyArgs(dataTable);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);

            IList<Job> result = service.GetJobs(3);

            Assert.AreEqual(result.Count, dataTable.Rows.Count);
        }

        [Test]
        public void GetScheduledJobs_QueryReturnedDataTableWithRows_ReturnsJobCollection()
        {
            var taskTypes = new List<string> {"TaskName"};
            DataTable dataTable = GetDataTableWithRow();
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            dataProvider.GetJobs(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<string>>()).Returns(dataTable);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);

            IEnumerable<Job> result = service.GetScheduledJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypes);

            Assert.IsTrue(result.Any());
        }

        [Test]
        public void GetScheduledJobs_QueryReturnedNoRows_ReturnsEmptyCollection()
        {
            var taskTypes = new List<string> {"TaskName"};
            DataTable dataTable = JobHelper.CreateEmptyJobDataTable();
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            dataProvider.GetJobs(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<string>>()).Returns(dataTable);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);

            IEnumerable<Job> result = service.GetScheduledJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypes);

            Assert.IsFalse(result.Any());
        }

	    [Test]
	    public void GetAllScheduledJobs()
		{
			DataTable dataTable = GetDataTableWithRow();
			IJobServiceDataProvider dataProvider = _mockDataProvider;
			dataProvider.GetAllJobs().Returns(dataTable);
			var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);

			// ACT
			IEnumerable<Job> result = service.GetAllScheduledJobs();

			Assert.IsTrue(result.Any());
		}

		[Test]
        public void UpdateStopState_RunsUpdateStopState()
        {
            var jobIds = new List<long> { 1, 2 };
            var stopState = StopState.None;
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            dataProvider.UpdateStopState(Arg.Any<IList<long>>(), Arg.Any<StopState>()).Returns(26);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);

            service.UpdateStopState(jobIds, stopState);

            dataProvider.Received().UpdateStopState(Arg.Any<List<long>>(), Arg.Any<StopState>());
        }

        [Test]
        public void UpdateStopState_EmptyJobList_LogsInfo()
        {
            var jobIds = new List<long>();
            var stopState = StopState.None;
            var service = new JobService(_agentService, _mockDataProvider, _mockEmptyDbHelper);

            service.UpdateStopState(jobIds, stopState);

            _mockDataProvider.DidNotReceiveWithAnyArgs().UpdateStopState(Arg.Any<List<long>>(), Arg.Any<StopState>());
        }

        [Test]
        public void UpdateStopState_QueryReturnsCountZero_LogsInfoAndError()
        {
            var jobIds = new List<long> { 1, 2 };
            var stopState = StopState.None;
            IJobServiceDataProvider dataProvider = _mockDataProvider;
            dataProvider.UpdateStopState(Arg.Any<IList<long>>(), Arg.Any<StopState>()).Returns(0);
            var service = new JobService(_agentService, dataProvider, _mockEmptyDbHelper);

            Assert.Throws<InvalidOperationException>(() => service.UpdateStopState(jobIds, stopState));
        }

        private static DataRow GetMockJobDataRow(DataTable dataTable = null)
        {
            if (dataTable == null)
            {
                dataTable = JobHelper.CreateEmptyJobDataTable();
            }

            return JobHelper.CreateJobDataRow(_MOCK_JOB_ID, _ROOT_JOB_ID, _PARENT_JOB_ID, _AGENT_TYPE_ID, 1,
                _WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, _TASK_TYPE, _nextRunTime, _lastRunTime, _MOCK_JOB_DETAILS,
                _JOB_FLAGS, _submittedDate, _SUBMITTED_BY, string.Empty, string.Empty, StopState.None, dataTable);
        }

        private static Job GetMockJob()
        {
            return new Job(GetMockJobDataRow());
        }

        private static IScheduleRuleFactory CreateScheduleRuleFactoryWithRuleReturning(DateTime? dateTime)
        {
            IScheduleRule scheduleRule = CreateScheduleRuleReturning(dateTime);
            var scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
            scheduleRuleFactory.Deserialize(Arg.Any<Job>()).Returns(scheduleRule);
            return scheduleRuleFactory;
        }

        private static IScheduleRule CreateScheduleRuleReturning(DateTime? dateTime)
        {
            var scheduleRule = Substitute.For<IScheduleRule>();
            scheduleRule.GetNextUTCRunDateTime().Returns(dateTime);
            return scheduleRule;
        }

        private static DataTable GetDataTableWithRow()
        {
            DataTable dataTable = JobHelper.CreateEmptyJobDataTable();
            dataTable.Rows.Add(GetMockJobDataRow(dataTable));
            return dataTable;
        }
    }
}
