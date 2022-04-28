using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Services;
using Relativity.Services.Objects.DataContracts;
using kCura.IntegrationPoints.Data;
using FluentAssertions;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
    internal class SyncApiTestsImplementation : SyncApiTestsImplementationBase
    {
        public SyncApiTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture) : 
            base(testsImplementationTestFixture)
        {
        }

        public void OneTimeSetup()
        {
            _sourceWorkspace = _testsImplementationTestFixture.Workspace;
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace,
                LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);

            CreateSavedSearch(_testsImplementationTestFixture.Workspace.ArtifactID);

            _sourceWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, _sourceWorkspace.ArtifactID);
        }

        public async Task RunAndRetryIntegrationPoint()
        {
            //0. Arrange test
            const int destinationWorkspaceInitialImportCount = 4;

            Workspace destinationWorkspace = RelativityFacade.Instance.CreateWorkspace($"SYNC - {Guid.NewGuid()}", _testsImplementationTestFixture.Workspace.Name);
            _destinationWorkspaces.Add(destinationWorkspace);

            RelativityFacade.Instance.ImportDocumentsFromCsv(destinationWorkspace,
                LoadFilesGenerator.CreateNativesLoadFileWithLimitedItems(destinationWorkspaceInitialImportCount),
                overwriteMode: DocumentOverwriteMode.AppendOverlay);

            ICommonIntegrationPointDataService destinationWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, destinationWorkspace.ArtifactID);

            string integrationPointName = $"{nameof(RunAndRetryIntegrationPoint)} - {Guid.NewGuid()}";

            //1. Job first run:

            // Arrange
            List<RelativityObject> sourceWorkspaceAlldocs = await GetDocumentsFromWorkspace(_sourceWorkspace.ArtifactID).ConfigureAwait(false);
            List<RelativityObject> destinationWorkspaceAllDocs = await GetDocumentsFromWorkspace(destinationWorkspace.ArtifactID).ConfigureAwait(false);

            IntegrationPointModel integrationPoint = await PrepareIntegrationPointModel(integrationPointName,
                    ImportOverwriteModeEnum.OverlayOnly, destinationWorkspaceDataService)
                .ConfigureAwait(false);
            
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            int expectedItemErrorsToRetry = sourceWorkspaceAlldocs.Count() - destinationWorkspaceAllDocs.Count();

            // Act
            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

            // Assert
            await _ripApi.WaitForJobToFinishAsync(jobHistoryId, _sourceWorkspace.ArtifactID,
                expectedStatus: JobStatusChoices.JobHistoryCompletedWithErrors.Name).ConfigureAwait(false);

            (int RunTransferredItems, int RunItemsWithErrors) = await GetTransferredItemsFromJobHistory(jobHistoryId).ConfigureAwait(false);

            RunItemsWithErrors.Should().Be(expectedItemErrorsToRetry);

            //2. Job retry:

            // Act
            int retryJobHistoryId = await _ripApi.RetryIntegrationPointAsync(integrationPoint, _sourceWorkspace.ArtifactID, true);

            // Assert
            await _ripApi.WaitForJobToFinishAsync(retryJobHistoryId, _sourceWorkspace.ArtifactID,
                expectedStatus: JobStatusChoices.JobHistoryCompleted.Name).ConfigureAwait(false);

            (int RetryTransferredItems, int RetryItemsWithErrors) = await GetTransferredItemsFromJobHistory(retryJobHistoryId).ConfigureAwait(false);

            RetryItemsWithErrors.Should().Be(0);
            RetryTransferredItems.Should().Be(expectedItemErrorsToRetry);
        }
    }
}