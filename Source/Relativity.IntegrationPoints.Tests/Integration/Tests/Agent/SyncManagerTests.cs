using kCura.IntegrationPoints.Agent.Tasks;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[IdentifiedTestFixture("9D111D3B-2E16-4652-82C9-6F8B7A8F2AD5")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class SyncManagerTests : TestsBase
	{
		private SyncManager PrepareSut()
		{
			SyncManager sut = Container.Resolve<SyncManager>();

			return sut;
		}

		[IdentifiedTest("D7134532-7560-4F63-B695-384FCD464F11")]
		public void SyncManager_ShouldSplitJobIntoBatches()
		{
			// Arrange
			SyncManager sut = PrepareSut();
		}
	}
}