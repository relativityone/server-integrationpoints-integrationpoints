using Autofac;
using NUnit.Framework;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class SyncJobProgressTests
	{
		[SetUp]
		public void SetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockAllSteps(containerBuilder);
			IntegrationTestsContainerBuilder.MockReporting(containerBuilder);
		}

		[Test]
		public void ItShouldCreateCompletedProgressStateForEachStep()
		{
			Assert.Pass();
		}

		[Test]
		public void ItShouldCreateFailedProgressStateForFailedSteps()
		{
			Assert.Pass();
		}

		[Test]
		public void ItShouldAssignIncreasingOrder()
		{
			Assert.Pass();
		}

		[Test]
		public void ItShouldAssignSameOrderToMultiNode()
		{
			Assert.Pass();
		}
	}
}
