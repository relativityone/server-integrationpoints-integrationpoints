using System.Linq;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

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
			IntegrationTestsContainerBuilder.MockReporting(containerBuilder);
			_progressRepository = new ProgressRepositoryStub();
			containerBuilder.RegisterInstance(_progressRepository).As<IProgressRepository>();
			_container = containerBuilder.Build();
			_instance = _container.Resolve<ISyncJob>();
		}

		[Test]
		public async Task ItShouldAssignIncreasingOrder()
		{
			// ACT
			await _instance.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			// Checks progress objects in creation order - if final value is null
			// then a progress object was created with a lower order than a previous one.
			int? maxOrder = _progressRepository.ProgressObjects.Aggregate(
				(int?)-1,
				(lastOrder, progress) => lastOrder.HasValue && lastOrder <= progress.Order ? (int?)progress.Order : null);
			Assert.IsNotNull(maxOrder);
		}

		[Test]
		public async Task ItShouldAssignSameOrderToMultiNodeChildren()
		{
			// ACT
			await _instance.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			string[] multiNodeChildIds =
			{
				_container.ResolveNode<IDocumentJobStartMetricsConfiguration>().Id,
				_container.ResolveNode<IDestinationWorkspaceTagsCreationConfiguration>().Id,
				_container.ResolveNode<ISourceWorkspaceTagsCreationConfiguration>().Id,
				_container.ResolveNode<IDataDestinationInitializationConfiguration>().Id
			};

			int[] multiNodeChildOrders = _progressRepository.ProgressObjects
				.Where(p => multiNodeChildIds.Contains(p.Name))
				.Select(p => p.Order)
				.ToArray();

			Assert.AreEqual(multiNodeChildIds.Length, multiNodeChildOrders.Length);
			Assert.True(multiNodeChildOrders.All(x => x == multiNodeChildOrders[0]));
		}
	}
}
