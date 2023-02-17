using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.Logging;
using System.Linq;
using kCura.ScheduleQueue.Core.Interfaces;
using WorkloadDiscovery;

namespace Relativity.IntegrationPoints.Services.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointAgentManagerTests : TestBase
    {
        private int _AGENT_TYPE_ID = 123;
        private int _WORKSPACE_ID = 100001;
        private int _RELATED_OBJ_ARTIFACT_ID = 45678;
        private int _JOB_SUBMITTED_BY = 111;
        private Mock<ILog> _loggerFake;
        private Mock<IPermissionRepositoryFactory> _permissionsFake;
        private Mock<IWindsorContainer> _containerFake;
        private Mock<IQueueQueryManager> _queueQueryManagerFake;
        private Mock<IInstanceSettingsManager> _instanceSettingsManagerFake;
        private Mock<IJobService> _jobServiceFake;

        public override void SetUp()
        {
            _loggerFake = new Mock<ILog>();
            _permissionsFake = new Mock<IPermissionRepositoryFactory>();
            _containerFake = new Mock<IWindsorContainer>();

            _queueQueryManagerFake = new Mock<IQueueQueryManager>();
            _containerFake.Setup(x => x.Resolve<IQueueQueryManager>()).Returns(_queueQueryManagerFake.Object);

            _instanceSettingsManagerFake = new Mock<IInstanceSettingsManager>();
            _containerFake.Setup(x => x.Resolve<IInstanceSettingsManager>()).Returns(_instanceSettingsManagerFake.Object);

            _jobServiceFake = new Mock<IJobService>();
            _containerFake.Setup(x => x.Resolve<IJobService>()).Returns(_jobServiceFake.Object);
        }

        [TestCase(3, 0, 3, WorkloadSize.None)]
        [TestCase(5, 3, 2, WorkloadSize.One)]
        [TestCase(5, 2, 1, WorkloadSize.S)]
        [TestCase(3, 0, 0, WorkloadSize.S)]
        [TestCase(5, 0, 1, WorkloadSize.M)]
        [TestCase(10, 1, 4, WorkloadSize.M)]
        [TestCase(10, 1, 1, WorkloadSize.L)]
        [TestCase(30, 0, 2, WorkloadSize.L)]
        public async Task GetWorkloadAsync_ShouldReturnProperWorkloadSize_WhenExcludedJobsAreInQueue(int queueCount,  int jobsExcludedByPriority, int excludedFromProcessingByTimeCondition, WorkloadSize expectedWorkloadSize)
        {
            // Arrange
            IntegrationPointAgentManager sut = PrepareSut(queueCount, string.Empty, jobsExcludedByPriority, excludedFromProcessingByTimeCondition);

            // Act
            Workload workload = await sut.GetWorkloadAsync().ConfigureAwait(false);

            // Assert
            workload.Size.Should().Be(expectedWorkloadSize);
        }

        [TestCase(0, WorkloadSize.None)]
        [TestCase(1, WorkloadSize.One)]
        [TestCase(2, WorkloadSize.S)]
        [TestCase(3, WorkloadSize.S)]
        [TestCase(4, WorkloadSize.M)]
        [TestCase(7, WorkloadSize.M)]
        [TestCase(8, WorkloadSize.L)]
        [TestCase(30, WorkloadSize.L)]
        public async Task GetWorkloadAsync_ShouldReturnProperWorkloadSize_WhenUsingDefaultSettings(int pendingJobsCount, WorkloadSize expectedWorkloadSize)
        {
            // Arrange
            IntegrationPointAgentManager sut = PrepareSut(pendingJobsCount);

            // Act
            Workload workload = await sut.GetWorkloadAsync().ConfigureAwait(false);

            // Assert
            workload.Size.Should().Be(expectedWorkloadSize);
        }

        [Test]
        public async Task GetWorkloadAsync_ShouldReturnProperWorkloadSize_WhenUsingCustomInstanceSettingValue()
        {
            // Arrange
            List<IntegrationPointAgentManager.WorkloadSizeDefinition> customSettings = new List<IntegrationPointAgentManager.WorkloadSizeDefinition>()
            {
                new IntegrationPointAgentManager.WorkloadSizeDefinition(minJobsCount: 3, maxJobsCount: 6, workloadSize: WorkloadSize.S)
            };
            IntegrationPointAgentManager sut = PrepareSut(jobsCount: 4, workloadSizeInstanceSettingValue: JsonConvert.SerializeObject(customSettings));

            // Act
            Workload workload = await sut.GetWorkloadAsync().ConfigureAwait(false);

            // Assert
            workload.Size.Should().Be(WorkloadSize.S);
        }

        [Test]
        public async Task GetWorkloadAsync_ShouldReturnDefaultValue_WhenMatchingWorkloadSizeDefinitionNotFoundInInstanceSettingValue()
        {
            // Arrange
            List<IntegrationPointAgentManager.WorkloadSizeDefinition> customSettings = new List<IntegrationPointAgentManager.WorkloadSizeDefinition>()
            {
                new IntegrationPointAgentManager.WorkloadSizeDefinition(minJobsCount: 3, maxJobsCount: 4, workloadSize: WorkloadSize.S)
            };
            IntegrationPointAgentManager sut = PrepareSut(jobsCount: 1, workloadSizeInstanceSettingValue: JsonConvert.SerializeObject(customSettings));

            // Act
            Workload workload = await sut.GetWorkloadAsync().ConfigureAwait(false);

            // Assert
            workload.Size.Should().Be(WorkloadSize.One);
        }

        [Test]
        public async Task GetWorkloadAsync_ShouldReturnDefaultValue_WhenRetrievingInstanceSettingValueFails()
        {
            // Arrange
            IntegrationPointAgentManager sut = PrepareSut(jobsCount: 1, workloadSizeInstanceSettingValue: "[{Invalid Json]");

            // Act
            Workload workload = await sut.GetWorkloadAsync().ConfigureAwait(false);

            // Assert
            workload.Size.Should().Be(WorkloadSize.One);
        }

        private IntegrationPointAgentManager PrepareSut(int jobsCount = 0, string workloadSizeInstanceSettingValue = "", int excludedFromProcessingByPriority = 0, int excludedFromProcessingByTimeCondition = 0)
        {
            Mock<IQuery<DataRow>> fakeAgentInfoRow = new Mock<IQuery<DataRow>>();

            IEnumerable<Job> fakeQueueState = GetFakeQueue(jobsCount, excludedFromProcessingByPriority, excludedFromProcessingByTimeCondition);
            DataRow fakeAgentInfoData = PrepareFakeAgentInfoDataRow();

            fakeAgentInfoRow.Setup(x => x.Execute()).Returns(fakeAgentInfoData);
            _jobServiceFake.Setup(x => x.GetAllScheduledJobs()).Returns(fakeQueueState);
            _queueQueryManagerFake.Setup(x => x.GetAgentTypeInformation(It.IsAny<Guid>())).Returns(fakeAgentInfoRow.Object);

            _instanceSettingsManagerFake.Setup(x => x.GetWorkloadSizeSettings()).Returns(workloadSizeInstanceSettingValue);
            return new IntegrationPointAgentManager(_loggerFake.Object, _permissionsFake.Object, _containerFake.Object);
        }

        private List<Job> GetFakeQueue(int jobsCount, int excludedFromProcessingByPriority, int excludedFromProcessingByTimeCondition)
        {
            DataTable dt = PrepareFakeDbTableState(jobsCount, excludedFromProcessingByPriority, excludedFromProcessingByTimeCondition);
            return dt.Rows.Cast<DataRow>().Select(row => new Job(row)).ToList();
        }

        private DataTable PrepareFakeDbTableState(int jobsCount, int excludedFromProcessingByPriority, int excludedFromProcessingByTimeCondition)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("JobID", typeof(long));
            dt.Columns.Add("RootJobId", typeof(long));
            dt.Columns.Add("ParentJobId", typeof(long));
            dt.Columns.Add("AgentTypeID", typeof(int));
            dt.Columns.Add("LockedByAgentID", typeof(int));
            dt.Columns.Add("WorkspaceID", typeof(int));
            dt.Columns.Add("RelatedObjectArtifactID", typeof(int));
            dt.Columns.Add("TaskType", typeof(string));
            dt.Columns.Add("NextRunTime", typeof(DateTime));
            dt.Columns.Add("LastRunTime", typeof(DateTime));
            dt.Columns.Add("JobDetails", typeof(string));
            dt.Columns.Add("JobFlags", typeof(int));
            dt.Columns.Add("SubmittedDate", typeof(DateTime));
            dt.Columns.Add("SubmittedBy", typeof(int));
            dt.Columns.Add("ScheduleRuleType", typeof(string));
            dt.Columns.Add("ScheduleRule", typeof(string));
            dt.Columns.Add("StopState", typeof(int));
            dt.Columns.Add("Heartbeat", typeof(DateTime));

            if (excludedFromProcessingByPriority > 0)
            {
                dt.Rows.Add(GetTestRow(dt.Rows.Count + 1, 1, dt.Rows.Count, nameof(TaskType.SyncWorker), 0));
                AddFakeRowsToDataTable(dt, excludedFromProcessingByPriority, GetTestRow(dt.Rows.Count + 1, 1, dt.Rows.Count, nameof(TaskType.SyncEntityManagerWorker), 0));
            }
            if (excludedFromProcessingByTimeCondition > 0)
            {
                AddFakeRowsToDataTable(dt, excludedFromProcessingByTimeCondition,
                    GetTestRow(dt.Rows.Count + 1, 0, dt.Rows.Count, nameof(TaskType.SyncEntityManagerWorker), 0, nextRunTimeDiff: 10));
            }
            int jobsForProcessingCount = jobsCount - dt.Rows.Count;
            if (jobsForProcessingCount > 0)
            {
                AddFakeRowsToDataTable(dt, jobsForProcessingCount, GetTestRow(dt.Rows.Count + 1, 0, dt.Rows.Count, nameof(TaskType.SyncEntityManagerWorker), 0));
            }
            return dt;
        }

        private void AddFakeRowsToDataTable(DataTable dt, int itemsCount, object[] data)
        {
            while (itemsCount > 0)
            {
                dt.Rows.Add(data);
                itemsCount--;
            }
        }

        private object[] GetTestRow(long id, long? rootId, long? parentId, string taskTypeName, int stopState, int nextRunTimeDiff = -1)
        {
            return new object[]
                        {id,
                        rootId,
                        parentId,
                        _AGENT_TYPE_ID,
                        null,
                        _WORKSPACE_ID,
                        _RELATED_OBJ_ARTIFACT_ID,
                        taskTypeName,
                        DateTime.UtcNow.AddMinutes(nextRunTimeDiff),
                        null,
                        string.Empty,
                        0,
                        DateTime.UtcNow.AddHours(-1),
                        _JOB_SUBMITTED_BY,
                        string.Empty,
                        string.Empty,
                        stopState};
        }

        private DataRow PrepareFakeAgentInfoDataRow()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AgentTypeID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Fullnamespace", typeof(string));
            dt.Columns.Add("Guid", typeof(Guid));
            dt.Rows.Add(new Object[] { _AGENT_TYPE_ID, "TestName", "TestNameSpace", new Guid() });
            return dt.Rows[0];
        }
    }
}
