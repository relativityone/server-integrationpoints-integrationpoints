using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.ExecutorTests.TestsSetup;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.System.ExecutorTests
{
    internal class BatchDataSourcePreparationExecutorTests : SystemTest
    {
        private string _sourceWorkspaceName;
        private string _destinationWorkspaceName;

        private string _workspaceFileSharePath;

        [SetUp]
        public void SetUp()
        {
            _sourceWorkspaceName = "Source-e22514a8-9b8a-4a9a-926d-3feda0ada3a7";
            _destinationWorkspaceName = "Destination-b7cd0085-9cb2-4c33-8fae-3d51378eff77";

            _workspaceFileSharePath = Path.Combine(Path.GetTempPath(), _sourceWorkspaceName);

            Directory.CreateDirectory(_workspaceFileSharePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_workspaceFileSharePath))
            {
                Directory.Delete(_workspaceFileSharePath, true);
            }
        }

        [Test]
        public async Task ExecuteAsync_ShouldGenerateBasicLoadFile()
        {
            // Arrange
            FileShareServiceMock fileShareMock = new FileShareServiceMock(_workspaceFileSharePath);

            List<FieldMap> IdentifierFieldMap(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetDocumentIdentifierMappingAsync(sourceWorkspaceId, destinationWorkspaceId).GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspaceName, _destinationWorkspaceName)
                .ImportData(dataSet: Dataset.NativesAndExtractedText, extractedText: true)
                .SetupDocumentConfiguration(IdentifierFieldMap)
                .SetupContainer(b =>
                {
                    b.RegisterInstance<IFileShareService>(fileShareMock);
                })
                .PrepareBatches()
                .ExecutePreRequisteExecutor<IConfigureDocumentSynchronizationConfiguration>();

            IExecutor<IBatchDataSourcePreparationConfiguration> sut = setup.Container.Resolve<IExecutor<IBatchDataSourcePreparationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }
    }
}
