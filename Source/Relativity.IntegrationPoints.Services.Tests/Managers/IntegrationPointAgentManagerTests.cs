using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Managers;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.Logging;
using WorkloadDiscovery;

namespace Relativity.IntegrationPoints.Services.Tests.Managers
{
    [TestFixture, Category("Unit")]
	public class IntegrationPointAgentManagerTests : TestBase
	{
		private Mock<ILog> _loggerFake;
		private Mock<IPermissionRepositoryFactory> _permissionsFake;
		private Mock<IWindsorContainer> _containerFake;
		private Mock<IQueueQueryManager> _queueQueryManagerFake;
		private Mock<IInstanceSettingsManager> _instanceSettingsManagerFake;		

		public override void SetUp()
		{
			_loggerFake = new Mock<ILog>();
			_permissionsFake = new Mock<IPermissionRepositoryFactory>();
			_containerFake = new Mock<IWindsorContainer>();			

			_queueQueryManagerFake = new Mock<IQueueQueryManager>();
			_containerFake.Setup(x => x.Resolve<IQueueQueryManager>()).Returns(_queueQueryManagerFake.Object);			

			_instanceSettingsManagerFake = new Mock<IInstanceSettingsManager>();
			_containerFake.Setup(x => x.Resolve<IInstanceSettingsManager>()).Returns(_instanceSettingsManagerFake.Object);
		}

		private IntegrationPointAgentManager PrepareSut(int jobsCount = 0, string workloadSizeInstanceSettingValue = "", int blockedJobs = 0)
		{			
			Mock<IQuery<DataTable>> fakeGetQueueDetails = new Mock<IQuery<DataTable>>();
			Mock<IQuery<DataRow>> fakeAgentInfoRow = new Mock<IQuery<DataRow>>();			
			
			DataTable fakeQueueDetails = PrepareFakeQueueDetails(jobsCount, blockedJobs);
			DataRow fakeAgentInfoData = PrepareFakeAgentInfoDataRow();
			
			fakeGetQueueDetails.Setup(x => x.Execute()).Returns(fakeQueueDetails);
			fakeAgentInfoRow.Setup(x => x.Execute()).Returns(fakeAgentInfoData);			
			_queueQueryManagerFake.Setup(x => x.GetJobsQueueDetails(It.IsAny<int>())).Returns(fakeGetQueueDetails.Object);
			_queueQueryManagerFake.Setup(x => x.GetAgentTypeInformation(It.IsAny<Guid>())).Returns(fakeAgentInfoRow.Object);			

			_instanceSettingsManagerFake.Setup(x => x.GetWorkloadSizeSettings()).Returns(workloadSizeInstanceSettingValue);
			return new IntegrationPointAgentManager(_loggerFake.Object, _permissionsFake.Object, _containerFake.Object);
		}

        private DataTable PrepareFakeQueueDetails(int jobsCount, int blockedJobs)
        {
			DataTable dt = new DataTable();
			dt.Columns.Add("Total", typeof(int));
			dt.Columns.Add("Blocked", typeof(int));
			dt.Rows.Add(new Object[] { jobsCount, blockedJobs });
			return dt;
        }

		private DataRow PrepareFakeAgentInfoDataRow()
        {
			DataTable dt = new DataTable();		
			dt.Columns.Add("AgentTypeID", typeof(int));
			dt.Columns.Add("Name", typeof(string));
			dt.Columns.Add("Fullnamespace", typeof(string));
			dt.Columns.Add("Guid", typeof(Guid));
			dt.Rows.Add(new Object[] { 123, "TestName", "TestNameSpace", new Guid() });
			return dt.Rows[0];
		}

        [TestCase(0, WorkloadSize.None)]
		[TestCase(1, WorkloadSize.One)]
		[TestCase(2, WorkloadSize.S)]
		[TestCase(3, WorkloadSize.S)]
		[TestCase(4, WorkloadSize.M)]
		[TestCase(5, WorkloadSize.M)]
		[TestCase(7, WorkloadSize.M)]
		[TestCase(8, WorkloadSize.L)]
		[TestCase(15, WorkloadSize.L)]
		[TestCase(23, WorkloadSize.L)]
		[TestCase(24, WorkloadSize.XL)]
		[TestCase(28, WorkloadSize.XL)]
		[TestCase(31, WorkloadSize.XL)]
		[TestCase(32, WorkloadSize.XXL)]
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
	}
}