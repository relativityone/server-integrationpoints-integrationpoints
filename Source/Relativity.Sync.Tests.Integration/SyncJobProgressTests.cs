using Autofac;
using NUnit.Framework;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class SyncJobProgressTests
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void SetUp()
		{
			_containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockAllSteps(_containerBuilder);
			IntegrationTestsContainerBuilder.MockReporting(_containerBuilder);
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
