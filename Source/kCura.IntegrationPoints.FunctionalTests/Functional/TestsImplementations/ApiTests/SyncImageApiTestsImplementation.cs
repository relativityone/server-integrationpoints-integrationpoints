using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
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
            void ImportAction(Workspace destinationWorkspace)
            {
                const int destinationWorkspaceInitialImportCount = 4;
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
            }

            await RunAndRetryIntegrationPointExecution(ImportAction);
        }
    }
}
