using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Tests
{
    [TestFixture, Category("Unit")]
    public class JobServiceTests : TestBase
    {
        private IAgentService _agentService;
        private IJobServiceDataProvider _dataProviderMock;
        private IKubernetesMode _kubernetesModeMock;
        private const int _AGENT_TYPE_ID = 4239;
        private const int _JOB_FLAGS = 48923;
        private const int _RELATED_OBJECT_ARTIFACT_ID = 432;
        private const int _SUBMITTED_BY = 4936;
        private const int _WORKSPACE_ID = 3429;
        private const long _MOCK_JOB_ID = 123;
        private const long _PARENT_JOB_ID = 756;
        private const long _ROOT_JOB_ID = 287;

        private const string _MOCK_JOB_DETAILS = "There should be details";
        private const TaskType _TASK_TYPE = TaskType.ImportService;
        private readonly DateTime _mockScheduleRuleReturnDate = new DateTime(2020, 12, 31);
        private readonly Guid _goldFlowAgentGuid = Guid.NewGuid();
        private readonly IHelper _mockEmptyDbHelper = Substitute.For<IHelper>();

        private readonly int _goldFlowAgentTypeId = 54;
        private readonly string _goldFlowAgentName = "AgentName";
        private readonly string _goldFlowAgentNamespace = "Namespace";
        private static readonly DateTime _lastRunTime = new DateTime(2019, 1, 1);
        private static readonly DateTime _nextRunTime = new DateTime(2020, 12, 31);
        private static readonly DateTime _submittedDate = new DateTime(2016, 1, 6);

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
            _dataProviderMock = Substitute.For<IJobServiceDataProvider>();
            _kubernetesModeMock = Substitute.For<IKubernetesMode>();
            _kubernetesModeMock.IsEnabled().Returns(false);
        }

        [Test]
        public void GetNextQueueJob_NoJobDataRowFound_ReturnsNull()
        {
            int agentId = 1;
            int[] resourceGroupIds = new[] { 1, 2 };
            JobService service = PrepareSut();

            Job job = service.GetNextQueueJob(resourceGroupIds, agentId);

            _dataProviderMock.Received().GetNextQueueJob(agentId, Arg.Any<int>(), Arg.Any<int[]>());
            Assert.IsNull(job);
        }

        [Test]
        public void GetNextQueueJob_JobDataRowFound_ReturnsJob()
        {
            int agentId = 1;
            int[] resourceGroupIds = new[] { 1, 2 };
            IJobServiceDataProvider dataProvider = _dataProviderMock;
            dataProvider.GetNextQueueJob(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int[]>()).Returns(GetMockJobDataRow());
            JobService service = PrepareSut();

            Job job = service.GetNextQueueJob(resourceGroupIds, agentId);

            Assert.AreEqual(job.JobId, _MOCK_JOB_ID);
        }
        
        [Test]
        public void GetNextQueueJob_ResourceGroupIdsEmpty_ThrowsArgumentException()
        {
            JobService service = PrepareSut();

            Assert.Throws<ArgumentException>(() => service.GetNextQueueJob(new List<int>(), 1));
        }

        [Test]
        public void GetNextQueueJob_ShouldGetNextJob_WhenInKubernetesMode()
        {
            // arrange
            const int agentId = 1;
            _kubernetesModeMock.IsEnabled().Returns(true);
            JobService sut = PrepareSut();

            // act
            sut.GetNextQueueJob(null, agentId);

            // assert
            _dataProviderMock.Received().GetNextQueueJob(agentId, _goldFlowAgentTypeId);
        }

        [Test]
        public void GetJobNextUtcRunDateTime_ProperJobAndSchedule_DateEqualsScheduledDate()
        {
            Job job = GetMockJob();
            IScheduleRuleFactory scheduleRuleFactory = CreateScheduleRuleFactoryWithRuleReturning(_mockScheduleRuleReturnDate);
            JobService service = PrepareSut();

            DateTime? returnedDate = service.GetJobNextUtcRunDateTime(job, scheduleRuleFactory, new TaskResult());

            Assert.AreEqual(returnedDate, _mockScheduleRuleReturnDate);
        }

        [Test]
        public void GetJobNextUtcRunDateTime_ScheduleRuleReturnsNullDate_ReturnsNull()
        {
            Job job = GetMockJob();
            IScheduleRuleFactory scheduleRuleFactory = CreateScheduleRuleFactoryWithRuleReturning(null);
            JobService service = PrepareSut();

            DateTime? returnedDate = service.GetJobNextUtcRunDateTime(job, scheduleRuleFactory, new TaskResult());

            Assert.IsNull(returnedDate);
        }

        [Test]
        public void GetNextJobUtcRunDateTime_JobIsNull_ReturnsNull()
        {
            IScheduleRuleFactory scheduleRuleFactory = CreateScheduleRuleFactoryWithRuleReturning(_mockScheduleRuleReturnDate);
            JobService service = PrepareSut();

            DateTime? returnedDate = service.GetJobNextUtcRunDateTime(null, scheduleRuleFactory, new TaskResult());

            Assert.IsNull(returnedDate);
        }

        [Test]
        public void GetNextJobUtcRunDateTime_ScheduleFactoryReturnsNull_ReturnsNull()
        {
            Job job = GetMockJob();
            IScheduleRuleFactory scheduleFactory = Substitute.For<IScheduleRuleFactory>();
            scheduleFactory.Deserialize(Arg.Any<Job>()).Returns(_ => null);
            JobService service = PrepareSut();

            DateTime? returnedDate = service.GetJobNextUtcRunDateTime(job, scheduleFactory, new TaskResult());

            Assert.IsNull(returnedDate);
        }

        [Test]
        public void GetJobNextUtcRunDateTime_ScheduleRuleFactoryNull_ThrowsArgumentNullException()
        {
            JobService service = PrepareSut();

            Assert.Throws<ArgumentNullException>(() => service.GetJobNextUtcRunDateTime(null, null, new TaskResult()));
        }

        [Test]
        public void FinalizeJob_ProperJobScheduleRuleReturnsDate_ReturnsStateModified()
        {
            Job job = GetMockJob();
            IScheduleRuleFactory scheduleRuleFactory = CreateScheduleRuleFactoryWithRuleReturning(_mockScheduleRuleReturnDate);

            JobService service = PrepareSut();

            FinalizeJobResult result = service.FinalizeJob(job, scheduleRuleFactory, new TaskResult());

            _dataProviderMock.Received().CreateNewAndDeleteOldScheduledJob(_MOCK_JOB_ID, _WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, _TASK_TYPE.ToString(),
                _nextRunTime, _agentService.AgentTypeInformation.AgentTypeID, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), 0, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);

            Assert.AreEqual(result.JobState, JobLogState.Deleted);
        }

        [Test]
        public void FinalizeJob_ProperJobScheduleRuleReturnsNull_ReturnsStateDeleted()
        {
            Job job = GetMockJob();
            IScheduleRuleFactory scheduleRuleFactory = CreateScheduleRuleFactoryWithRuleReturning(null);
            JobService service = PrepareSut();

            FinalizeJobResult result = service.FinalizeJob(job, scheduleRuleFactory, new TaskResult());

            _dataProviderMock.Received().DeleteJob(_MOCK_JOB_ID);
            Assert.AreEqual(result.JobState, JobLogState.Deleted);
        }

        [Test]
        public void FinalizeJob_JobIsNull_ResultStateFinished()
        {
            IScheduleRuleFactory scheduleFactory = CreateScheduleRuleFactoryWithRuleReturning(_mockScheduleRuleReturnDate);
            JobService service = PrepareSut();

            FinalizeJobResult result = service.FinalizeJob(null, scheduleFactory, new TaskResult());

            Assert.AreEqual(result.JobState, JobLogState.Finished);
        }

        [Test]
        public void FinalizeJob_ScheduleRuleFactoryNull_ThrowsArgumentNullException()
        {
            JobService service = PrepareSut();

            Assert.Throws<ArgumentNullException>(() => service.FinalizeJob(null, null, new TaskResult()));
        }

        [Test]
        public void UnlockJobs_GoldFlow()
        {
            int agentId = 345;
            JobService service = PrepareSut();

            service.UnlockJobs(agentId);

            _dataProviderMock.Received().UnlockScheduledJob(agentId);
        }

        [Test]
        public void CreateJob_ScheduleRuleReturnsDate_ReturnsValidJob()
        {
            string taskTypeString = _TASK_TYPE.ToString();
            _dataProviderMock.CreateScheduledJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, _nextRunTime,
                    _AGENT_TYPE_ID, null, null, _MOCK_JOB_DETAILS, _JOB_FLAGS, _SUBMITTED_BY, _ROOT_JOB_ID,
                    _PARENT_JOB_ID)
                .ReturnsForAnyArgs(GetMockJobDataRow());
            IScheduleRule scheduleRule = CreateScheduleRuleReturning(_mockScheduleRuleReturnDate);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);


            Job job = service.CreateJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, scheduleRule,
                _MOCK_JOB_DETAILS, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);


            Assert.AreEqual(job.JobDetails, _MOCK_JOB_DETAILS);
        }

        [Test]
        public void CreateJob_ScheduleRuleReturnsNullQueryReturnsJob_ReturnsJobDeletesJob()
        {
            string taskTypeString = _TASK_TYPE.ToString();
            DataTable dataTable = GetDataTableWithRow();
            _dataProviderMock.CreateScheduledJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, _nextRunTime,
                    _AGENT_TYPE_ID, null, null, _MOCK_JOB_DETAILS, _JOB_FLAGS, _SUBMITTED_BY, _ROOT_JOB_ID,
                    _PARENT_JOB_ID)
                .ReturnsForAnyArgs(GetMockJobDataRow());
            _dataProviderMock.GetJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, Arg.Any<List<string>>())
                .ReturnsForAnyArgs(dataTable);
            IScheduleRule scheduleRule = CreateScheduleRuleReturning(null);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);


            Job job = service.CreateJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, scheduleRule,
                _MOCK_JOB_DETAILS, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);


            _dataProviderMock.Received().DeleteJob(_MOCK_JOB_ID);
            Assert.AreEqual(job.JobDetails, _MOCK_JOB_DETAILS);
        }

        [Test]
        public void CreateJob_ScheduleRuleReturnsNullQueryNotReturnsJob_ReturnsNull()
        {
            string taskTypeString = _TASK_TYPE.ToString();
            var scheduleRule = CreateScheduleRuleReturning(null);
            DataTable dataTable = JobHelper.CreateEmptyJobDataTable();
            _dataProviderMock.GetJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, Arg.Any<List<string>>())
                .ReturnsForAnyArgs(dataTable);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);


            Job job = service.CreateJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, scheduleRule,
                _MOCK_JOB_DETAILS, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);


            _dataProviderMock.DidNotReceive().DeleteJob(Arg.Any<long>());
            Assert.IsNull(job);
        }

        [Test]
        public void CreateJob_CreateJobReturnsJobDataRow_ReturnsProperJob()
        {
            string taskTypeString = _TASK_TYPE.ToString();
            DataTable dataTable = GetDataTableWithRow();
            _dataProviderMock.CreateScheduledJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, _nextRunTime,
                    _AGENT_TYPE_ID, null, null, _MOCK_JOB_DETAILS, _JOB_FLAGS, _SUBMITTED_BY, _ROOT_JOB_ID,
                    _PARENT_JOB_ID)
                .ReturnsForAnyArgs(GetMockJobDataRow());
            _dataProviderMock.GetJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, Arg.Any<List<string>>())
                .ReturnsForAnyArgs(dataTable);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);


            Job job = service.CreateJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, _nextRunTime,
                _MOCK_JOB_DETAILS, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);


            Assert.AreEqual(job.JobId, _MOCK_JOB_ID);
        }

        [Test]
        public void CreateJob_CreateJobReturnsEmptyDataTable_ReturnsNull()
        {
            string taskTypeString = _TASK_TYPE.ToString();
            DataTable dataTable = JobHelper.CreateEmptyJobDataTable();
            _dataProviderMock.GetJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, Arg.Any<List<string>>())
                .ReturnsForAnyArgs(dataTable);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);

            Job job = service.CreateJob(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypeString, _nextRunTime,
                _MOCK_JOB_DETAILS, _SUBMITTED_BY, _ROOT_JOB_ID, _PARENT_JOB_ID);

            Assert.IsNull(job);
        }

        [Test]
        public void DeleteJob_RunsDelete()
        {
            JobService service = PrepareSut();

            service.DeleteJob(_MOCK_JOB_ID);

            _dataProviderMock.Received().DeleteJob(_MOCK_JOB_ID);
        }

        [Test]
        public void GetJob_QueryReturnsDataRow_ReturnsJob()
        {
            DataRow row = GetMockJobDataRow();
            _dataProviderMock.GetJob(_MOCK_JOB_ID).Returns(row);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);

            Job job = service.GetJob(_MOCK_JOB_ID);

            Assert.AreEqual(job.JobId, _MOCK_JOB_ID);
        }

        [Test]
        public void GetJob_QueryReturnsNull_ReturnsNull()
        {
            _dataProviderMock.GetJob(_MOCK_JOB_ID).Returns(_ => null);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);

            Job job = service.GetJob(_MOCK_JOB_ID);

            Assert.IsNull(job);
        }

        [Test]
        public void GetJobs_ProperIntegrationPointId_ReturnsListOfJobs()
        {
            DataTable dataTable = GetDataTableWithRow();
            dataTable.Rows.Add(GetMockJobDataRow(dataTable));
            _dataProviderMock.GetJobsByIntegrationPointId(_RELATED_OBJECT_ARTIFACT_ID)
                .ReturnsForAnyArgs(dataTable);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);

            IList<Job> result = service.GetJobs(3);

            Assert.AreEqual(result.Count, dataTable.Rows.Count);
        }

        [Test]
        public void GetScheduledJobs_QueryReturnedDataTableWithRows_ReturnsJobCollection()
        {
            List<string> taskTypes = new List<string> { "TaskName" };
            DataTable dataTable = GetDataTableWithRow();
            _dataProviderMock.GetJobs(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<string>>()).Returns(dataTable);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);

            IEnumerable<Job> result = service.GetScheduledJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypes);

            Assert.IsTrue(result.Any());
        }

        [Test]
        public void GetScheduledJobs_QueryReturnedNoRows_ReturnsEmptyCollection()
        {
            List<string> taskTypes = new List<string> { "TaskName" };
            DataTable dataTable = JobHelper.CreateEmptyJobDataTable();
            _dataProviderMock.GetJobs(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<string>>()).Returns(dataTable);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);

            IEnumerable<Job> result = service.GetScheduledJobs(_WORKSPACE_ID, _RELATED_OBJECT_ARTIFACT_ID, taskTypes);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public void GetAllScheduledJobs()
        {
            DataTable dataTable = GetDataTableWithRow();
            _dataProviderMock.GetAllJobs().Returns(dataTable);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);

            // ACT
            IEnumerable<Job> result = service.GetAllScheduledJobs();

            Assert.IsTrue(result.Any());
        }

        [Test]
        public void UpdateStopState_RunsUpdateStopState()
        {
            List<long> jobIds = new List<long> { 1, 2 };
            StopState stopState = StopState.None;
            _dataProviderMock.UpdateStopState(Arg.Any<IList<long>>(), Arg.Any<StopState>()).Returns(26);
            JobService service = new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);

            service.UpdateStopState(jobIds, stopState);

            _dataProviderMock.Received().UpdateStopState(Arg.Any<List<long>>(), Arg.Any<StopState>());
        }

        [Test]
        public void UpdateStopState_EmptyJobList_LogsInfo()
        {
            List<long> jobIds = new List<long>();
            StopState stopState = StopState.None;
            JobService service = PrepareSut();

            service.UpdateStopState(jobIds, stopState);

            _dataProviderMock.DidNotReceiveWithAnyArgs().UpdateStopState(Arg.Any<List<long>>(), Arg.Any<StopState>());
        }

        [Test]
        public void UpdateStopState_QueryReturnsCountZero_LogsInfoAndError()
        {
            List<long> jobIds = new List<long> { 1, 2 };
            StopState stopState = StopState.None;
            _dataProviderMock.UpdateStopState(Arg.Any<IList<long>>(), Arg.Any<StopState>()).Returns(0);
            JobService service = PrepareSut();

            Assert.Throws<InvalidOperationException>(() => service.UpdateStopState(jobIds, stopState));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CleanupJobQueueTable_ShouldAlwaysCleanupScheduledJobsQueue(bool enableKubernetesMode)
        {
            // arrange
            _kubernetesModeMock.IsEnabled().Returns(enableKubernetesMode);
            JobService sut = PrepareSut();

            // act
            sut.CleanupJobQueueTable();

            // assert
            _dataProviderMock.Received().CleanupScheduledJobsQueue();
        }

        [Test]
        public void CleanupJobQueueTable_ShouldNotCleanupJobQueueTable_WhenInKubernetesMode()
        {
            // arrange
            _kubernetesModeMock.IsEnabled().Returns(true);
            JobService sut = PrepareSut();

            // act
            sut.CleanupJobQueueTable();

            // assert
            _dataProviderMock.DidNotReceive().CleanupJobQueueTable();
        }

        [Test]
        public void CleanupJobQueueTable_ShouldCleanupJobQueueTable_WhenNotInKubernetesMode()
        {
            // arrange
            _kubernetesModeMock.IsEnabled().Returns(false);
            JobService sut = PrepareSut();

            // act
            sut.CleanupJobQueueTable();

            // assert
            _dataProviderMock.Received().CleanupJobQueueTable();
        }

        private JobService PrepareSut()
        {
            return new JobService(_agentService, _dataProviderMock, _kubernetesModeMock, _mockEmptyDbHelper);
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
            IScheduleRuleFactory scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
            scheduleRuleFactory.Deserialize(Arg.Any<Job>()).Returns(scheduleRule);
            return scheduleRuleFactory;
        }

        private static IScheduleRule CreateScheduleRuleReturning(DateTime? dateTime)
        {
            IScheduleRule scheduleRule = Substitute.For<IScheduleRule>();
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
