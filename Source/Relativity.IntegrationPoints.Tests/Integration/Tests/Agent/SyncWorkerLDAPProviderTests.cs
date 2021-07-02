using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System.Linq;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[IdentifiedTestFixture("55CAA9A3-B9CE-4A69-9CC9-ED931EE9EB81")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class SyncWorkerLDAPProviderTests : TestsBase
	{
		[IdentifiedTest("46988B61-878E-4F9F-95BA-3775E13F492E")]
		public void SyncWorker_SouldImportLDAPData()
		{
			// Arrange
			IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportEntityFromLdapIntegrationPoint();

			Helper.SecretStore.Setup(SourceWorkspace, integrationPoint);

			FakeRelativityInstance.Helpers.JobHelper.ScheduleSyncWorkerJob(SourceWorkspace, integrationPoint);

			Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport((importJob) => { importJob.Complete(4); })).LifestyleSingleton());

			FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container);

			// Act
			sut.Execute();

			// Assert
			int jobHistoryId = integrationPoint.JobHistory.Single();
			JobHistoryTest jobHistory = SourceWorkspace.JobHistory.Single(x => x.ArtifactId == jobHistoryId);
			jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistoryCompletedGuid);

			FakeRelativityInstance.JobsInQueue.Should().BeEmpty();
		}

		[IdentifiedTest("F83AF76A-50C5-4F4E-A097-74C5FB57350A")]
		public void SyncWorker_SouldImportLDAPDataAndSubmitLinkManagersJob_WhenLinkManagersWasEnabled()
		{
			// Arrange
			IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportEntityFromLdapIntegrationPoint(true);

			Helper.SecretStore.Setup(SourceWorkspace, integrationPoint);

			FakeRelativityInstance.Helpers.JobHelper.ScheduleSyncWorkerJob(SourceWorkspace, integrationPoint);

			Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport((importJob) => { importJob.Complete(4); })).LifestyleSingleton());

			FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container, false);

			// Act
			sut.Execute();

			// Assert
			int jobHistoryId = integrationPoint.JobHistory.Single();
			JobHistoryTest jobHistory = SourceWorkspace.JobHistory.Single(x => x.ArtifactId == jobHistoryId);
			jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistoryCompletedGuid);

			FakeRelativityInstance.JobsInQueue.Should().ContainSingle(x => x.TaskType == "SyncEntityManagerWorker");
		}
	}
}
