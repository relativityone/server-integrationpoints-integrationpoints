using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
    internal class SyncImageApiTestsImplementation : SyncApiTestsImplementationBase
    {
        public SyncImageApiTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture) : 
            base(testsImplementationTestFixture)
        {
        }

        public void OneTimeSetup()
        {
            ImageImportOptions imageImportOptions = new ImageImportOptions
            {
                ExtractedTextFieldContainsFilePath = false,
                OverwriteMode = DocumentOverwriteMode.AppendOverlay,
                OverlayBehavior = DocumentOverlayBehavior.UseRelativityDefaults,
                FileLocationField = "FileLocation",
                IdentityFieldId = 0,
                DocumentIdentifierField = "DocumentIdentifier",
                ExtractedTextEncoding = null,
                BatesNumberField = "BatesNumber"
            };

            _sourceWorkspace = _testsImplementationTestFixture.Workspace;

            const int imagesCount = 10;
            string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFile();
            RelativityFacade.Instance.ImportImages(_testsImplementationTestFixture.Workspace, testDataPath
                , imageImportOptions, imagesCount);

            CreateSavedSearch(_testsImplementationTestFixture.Workspace.ArtifactID);

            _sourceWorkspaceDataService = new CommonIntegrationPointDataService(_serviceFactory, _sourceWorkspace.ArtifactID);
        }

        public async Task RunAndRetryIntegrationPoint()
        {
            //0. Arrange test
            const int destinationWorkspaceInitialImportCount = 4;

            Workspace destinationWorkspace = RelativityFacade.Instance.CreateWorkspace($"SYNC - {Guid.NewGuid()}", _testsImplementationTestFixture.Workspace.Name);
            _destinationWorkspaces.Add(destinationWorkspace);

            ImageImportOptions imageImportOptions = new ImageImportOptions
            {
                ExtractedTextFieldContainsFilePath = false,
                OverwriteMode = DocumentOverwriteMode.AppendOverlay,
                OverlayBehavior = DocumentOverlayBehavior.UseRelativityDefaults,
                FileLocationField = "FileLocation",
                IdentityFieldId = 0,
                DocumentIdentifierField = "DocumentIdentifier",
                ExtractedTextEncoding = null,
                BatesNumberField = "BatesNumber"
            };
            
            string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFileWithLimitedItems(destinationWorkspaceInitialImportCount);
            RelativityFacade.Instance.ImportImages(_testsImplementationTestFixture.Workspace, testDataPath
                , imageImportOptions, destinationWorkspaceInitialImportCount);

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
