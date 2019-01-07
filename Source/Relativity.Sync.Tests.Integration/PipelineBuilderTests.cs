using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class PipelineBuilderTests
	{
		private SyncJobFactory _syncJobFactory;
		private List<Type> _executorTypes;

		[SetUp]
		public void SetUp()
		{
			_executorTypes = new List<Type>();
			_syncJobFactory = new SyncJobFactory();
		}

		[Test]
		public async Task PipelineStepsShouldBeInOrder()
		{
			List<Type[]> expectedOrder = ExpectedExecutionOrder();

			IContainer container = IntegrationTestsContainerBuilder.CreateContainer(_executorTypes);
			ISyncJob syncJob = _syncJobFactory.Create(container, new SyncJobParameters(1, 1));

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			AssertExecutionOrder(expectedOrder);
		}

		private void AssertExecutionOrder(List<Type[]> expectedOrder)
		{
			int counter = 0;
			foreach (Type[] types in expectedOrder)
			{
				foreach (var type in types)
				{
					bool isInOrder = false;
					for (int j = 0; j < types.Length; j++)
					{
						isInOrder |= type == _executorTypes[j + counter];
					}

					isInOrder.Should().BeTrue();
				}

				counter += types.Length;
			}
		}

		private static List<Type[]> ExpectedExecutionOrder()
		{
			return new List<Type[]>
			{
				new[] {typeof(IPermissionsCheckConfiguration)},
				new[] {typeof(IValidationConfiguration)},
				new[] {typeof(IPreviousRunCleanupConfiguration)},
				new[] {typeof(ITemporaryStorageInitializationConfiguration)},
				new[] {typeof(IDestinationWorkspaceObjectTypesCreationConfiguration)},
				new[] {typeof(IDataSourceSnapshotConfiguration)},
				new[]
				{
					typeof(ISourceWorkspaceTagsCreationConfiguration),
					typeof(IDestinationWorkspaceTagsCreationConfiguration),
					typeof(IDataDestinationInitializationConfiguration)
				},
				new[] {typeof(IDestinationWorkspaceSavedSearchCreationConfiguration)},
				new[] {typeof(ISnapshotPartitionConfiguration)},
				new[] {typeof(ISynchronizationConfiguration)},
				new[] {typeof(IDataDestinationFinalizationConfiguration)},
				new[] {typeof(IJobStatusConsolidationConfiguration)},
				new[] {typeof(IJobCleanupConfiguration)},
				new[] {typeof(INotificationConfiguration)}
			};
		}
	}
}