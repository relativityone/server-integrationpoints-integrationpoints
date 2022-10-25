using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.ExecutorTests.TestsSetup;

namespace Relativity.Sync.Tests.System.ExecutorTests
{
    internal class ConfigureDocumentSynchronizationExecutorTests : SystemTest
    {
        [Test]
        public async Task ExecuteAsync_ShouldCreateBasicIAPIv2Job()
        {
            // Arrange
            string sourceWorkspaceName = $"Source-{Guid.NewGuid()}";
            string destinationWorkspaceName = $"Destination-{Guid.NewGuid()}";

            List<FieldMap> IdentifierFieldMap(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetDocumentIdentifierMappingAsync(sourceWorkspaceId, destinationWorkspaceId).GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(sourceWorkspaceName, destinationWorkspaceName)
                .SetupDocumentConfiguration(IdentifierFieldMap)
                .SetupContainer()
                .ExecutePreRequisteExecutor<IDataSourceSnapshotConfiguration>();

            IExecutor<IConfigureDocumentSynchronizationConfiguration> sut = setup.Container.Resolve<IExecutor<IConfigureDocumentSynchronizationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }
    }
}
