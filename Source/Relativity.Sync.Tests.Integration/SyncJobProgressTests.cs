using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class SyncJobProgressTests
	{
		private ISyncJob _instance;
		private IContainer _container;
		private ProgressRepositoryStub _progressRepository;

		[SetUp]
		public void SetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockAllSteps(containerBuilder);
			containerBuilder.RegisterInstance(Mock.Of<ISyncMetrics>()).As<ISyncMetrics>();
			_progressRepository = new ProgressRepositoryStub();
			containerBuilder.RegisterInstance(_progressRepository).As<IProgressRepository>();
			_container = containerBuilder.Build();
			_instance = _container.Resolve<ISyncJob>();
		}

		[Test]
		public async Task ItShouldAssignIncreasingOrder()
		{
			// ACT
			await _instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			// Checks progress objects in creation order - if final value is null
			// then a progress object was created with a lower order than a previous one.
			int? maxOrder = _progressRepository.ProgressObjects.Aggregate(
				(int?)-1,
				(lastOrder, progress) => lastOrder.HasValue && lastOrder <= progress.Order ? (int?)progress.Order : null);
			Assert.IsNotNull(maxOrder);
		}

		// This _should_ theoretically assign the same order to these children, but we don't have a mechanism
		// to do that at this time.
		[Test]
		public async Task ItShouldNotAssignSameOrderToMultiNodeChildren()
		{
			// ACT
			await _instance.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			string[] multiNodeChildIds =
			{
				_container.ResolveNode<IDestinationWorkspaceTagsCreationConfiguration>().Id,
				_container.ResolveNode<ISourceWorkspaceTagsCreationConfiguration>().Id,
				_container.ResolveNode<IDataDestinationInitializationConfiguration>().Id
			};

			int[] multiNodeChildOrders = _progressRepository.ProgressObjects
				.Where(p => multiNodeChildIds.Contains(p.Name))
				.Select(p => p.Order)
				.ToArray();

			Assert.AreEqual(multiNodeChildIds.Length, multiNodeChildOrders.Length);
			Assert.False(multiNodeChildOrders.All(x => x == multiNodeChildOrders[0]));
		}
	}
}
