using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core.Data;
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

		private IntegrationPointAgentManager PrepareSut(int pendingJobsCount = 0, string workloadSizeInstanceSettingValue = "")
		{
			Mock<IQuery<int>> fakeGetPendingJobsCount = new Mock<IQuery<int>>();
			fakeGetPendingJobsCount.Setup(x => x.Execute()).Returns(pendingJobsCount);
			_queueQueryManagerFake.Setup(x => x.GetPendingJobsCount()).Returns(fakeGetPendingJobsCount.Object);

			_instanceSettingsManagerFake.Setup(x => x.GetWorkloadSizeSettings()).Returns(workloadSizeInstanceSettingValue);

			return new IntegrationPointAgentManager(_loggerFake.Object, _permissionsFake.Object, _containerFake.Object);
		}
		
		[TestCase(0, WorkloadSize.None)]
		[TestCase(1, WorkloadSize.One)]
		[TestCase(2, WorkloadSize.S)]
		[TestCase(3, WorkloadSize.M)]
		[TestCase(4, WorkloadSize.M)]
		[TestCase(5, WorkloadSize.L)]
		[TestCase(6, WorkloadSize.L)]
		[TestCase(9999, WorkloadSize.L)]
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
			IntegrationPointAgentManager sut = PrepareSut(pendingJobsCount: 4, workloadSizeInstanceSettingValue: JsonConvert.SerializeObject(customSettings));

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
			IntegrationPointAgentManager sut = PrepareSut(pendingJobsCount: 1, workloadSizeInstanceSettingValue: JsonConvert.SerializeObject(customSettings));

			// Act
			Workload workload = await sut.GetWorkloadAsync().ConfigureAwait(false);

			// Assert
			workload.Size.Should().Be(WorkloadSize.One);
		}

		[Test]
		public async Task GetWorkloadAsync_ShouldReturnDefaultValue_WhenRetrievingInstanceSettingValueFails()
		{
			// Arrange
			IntegrationPointAgentManager sut = PrepareSut(pendingJobsCount: 1, workloadSizeInstanceSettingValue: "[{Invalid Json]");

			// Act
			Workload workload = await sut.GetWorkloadAsync().ConfigureAwait(false);

			// Assert
			workload.Size.Should().Be(WorkloadSize.One);
		}
	}
}