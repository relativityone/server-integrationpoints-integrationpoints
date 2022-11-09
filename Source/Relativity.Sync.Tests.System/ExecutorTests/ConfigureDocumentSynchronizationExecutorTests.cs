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
        private const string _sourceWorkspace = "ConfigureDocumentSynchronizationTests-Source";
        private const string _destinationWorkspace = "ConfigureDocumentSynchronizationTests-Destination";

        [Test]
        public async Task ExecuteAsync_ShouldCreateBasicIAPIv2Job()
        {
            // Arrange
            List<FieldMap> IdentifierFieldMap(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetDocumentIdentifierMappingAsync(sourceWorkspaceId, destinationWorkspaceId).GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspace, _destinationWorkspace)
                .SetupDocumentConfiguration(
                    IdentifierFieldMap,
                    nativeFileCopyMode: ImportNativeFileCopyMode.DoNotImportNativeFiles)
                .SetupContainer()
                .ExecutePreRequisteExecutor<IDataSourceSnapshotConfiguration>();

            IExecutor<IConfigureDocumentSynchronizationConfiguration> sut = setup.Container.Resolve<IExecutor<IConfigureDocumentSynchronizationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [TestCase(ImportNativeFileCopyMode.SetFileLinks)]
        [TestCase(ImportNativeFileCopyMode.CopyFiles)]
        public async Task ExecuteAsync_ShouldCreateIAPIJob_WithMativesConfigured(ImportNativeFileCopyMode fileCopyMode)
        {
            // Arrange
            List<FieldMap> IdentifierFieldMap(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetDocumentIdentifierMappingAsync(sourceWorkspaceId, destinationWorkspaceId).GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspace, _destinationWorkspace)
                .SetupDocumentConfiguration(
                    IdentifierFieldMap,
                    nativeFileCopyMode: fileCopyMode)
                .SetupContainer()
                .ExecutePreRequisteExecutor<IDataSourceSnapshotConfiguration>();

            IExecutor<IConfigureDocumentSynchronizationConfiguration> sut = setup.Container.Resolve<IExecutor<IConfigureDocumentSynchronizationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ExecuteAsync_ShouldCreateIAPIJob_WithMappingConfigured()
        {
            // Arrange
            List<FieldMap> MappedFields(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetFieldsMappingAsync(
                        sourceWorkspaceId,
                        destinationWorkspaceId,
                        new string[]
                        {
                            "Date Created",
                            "Conversation Family",
                            "MD5 Hash"
                        })
                    .GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspace, _destinationWorkspace)
                .SetupDocumentConfiguration(
                    MappedFields)
                .SetupContainer()
                .ExecutePreRequisteExecutor<IDataSourceSnapshotConfiguration>();

            IExecutor<IConfigureDocumentSynchronizationConfiguration> sut = setup.Container.Resolve<IExecutor<IConfigureDocumentSynchronizationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ExecueAsync_ShouldCreateIAPIJob_WithFolderReadFromField()
        {
            // Arrange
            List<FieldMap> MappedFields(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetFieldsMappingAsync(
                        sourceWorkspaceId,
                        destinationWorkspaceId,
                        new string[] { "Original Folder Path" })
                    .GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspace, _destinationWorkspace)
                .SetupDocumentConfiguration(
                    MappedFields,
                    folderStructure: DestinationFolderStructureBehavior.ReadFromField,
                    folderPathField: "Original Folder Path",
                    exportRunId: Guid.NewGuid())
                .SetupContainer();
                //.ExecutePreRequisteExecutor<IDataSourceSnapshotConfiguration>();

            IExecutor<IConfigureDocumentSynchronizationConfiguration> sut = setup.Container.Resolve<IExecutor<IConfigureDocumentSynchronizationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }
    }
}
