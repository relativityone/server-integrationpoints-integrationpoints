using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.ExecutorTests.TestsSetup;
using Relativity.Sync.Toggles;

namespace Relativity.Sync.Tests.System.ExecutorTests
{
    internal class ConfigureDocumentSynchronizationExecutorTests : SystemTest
    {
        private const string _sourceWorkspace = "ConfigureDocumentSynchronizationTests-Source";
        private const string _destinationWorkspace = "ConfigureDocumentSynchronizationTests-Destination";

        private TestSyncToggleProvider _syncToggleProvider;

        [OneTimeSetUp]
        public async Task OnetimeSetUp()
        {
            _syncToggleProvider = new TestSyncToggleProvider();
            await _syncToggleProvider.SetAsync<EnableIAPIv2Toggle>(true).ConfigureAwait(false);
        }

        [Test]
        [Ignore("REL-774348")]
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
                .SetupContainer(toggleProvider: _syncToggleProvider)
                .ExecutePreRequisteExecutor<IDataSourceSnapshotConfiguration>();

            IExecutor<IConfigureDocumentSynchronizationConfiguration> sut = setup.Container.Resolve<IExecutor<IConfigureDocumentSynchronizationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [TestCase(ImportNativeFileCopyMode.SetFileLinks)]
        [TestCase(ImportNativeFileCopyMode.CopyFiles)]
        [Ignore("REL-774348")]
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
                .SetupContainer(toggleProvider: _syncToggleProvider)
                .ExecutePreRequisteExecutor<IDataSourceSnapshotConfiguration>();

            IExecutor<IConfigureDocumentSynchronizationConfiguration> sut = setup.Container.Resolve<IExecutor<IConfigureDocumentSynchronizationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        [Ignore("REL-774348")]
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
                .SetupContainer(toggleProvider: _syncToggleProvider)
                .ExecutePreRequisteExecutor<IDataSourceSnapshotConfiguration>();

            IExecutor<IConfigureDocumentSynchronizationConfiguration> sut = setup.Container.Resolve<IExecutor<IConfigureDocumentSynchronizationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        [Ignore("REL-774348")]
        public async Task ExecueAsync_ShouldCreateIAPIJob_WithFolderReadFromField()
        {
            // Arrange
            List<FieldMap> MappedFields(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetFieldsMappingAsync(
                        sourceWorkspaceId,
                        destinationWorkspaceId,
                        new string[] { "MD5 Hash", "Original Folder Path" })
                    .GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspace, _destinationWorkspace)
                .SetupDocumentConfiguration(
                    MappedFields,
                    nativeFileCopyMode: ImportNativeFileCopyMode.DoNotImportNativeFiles,
                    folderStructure: DestinationFolderStructureBehavior.ReadFromField,
                    folderPathField: "Original Folder Path",
                    exportRunId: Guid.NewGuid())
                .SetupContainer(toggleProvider: _syncToggleProvider);

            IExecutor<IConfigureDocumentSynchronizationConfiguration> sut = setup.Container.Resolve<IExecutor<IConfigureDocumentSynchronizationConfiguration>>();

            // Act
            ExecutionResult result = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }
    }
}
