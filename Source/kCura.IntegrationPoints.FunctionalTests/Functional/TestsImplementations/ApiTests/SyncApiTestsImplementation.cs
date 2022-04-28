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
            void ImportAction(Workspace destinationWorkspace)
            {
                const int destinationWorkspaceInitialImportCount = 4;
                RelativityFacade.Instance.
                    ImportDocumentsFromCsv(destinationWorkspace, LoadFilesGenerator.CreateNativesLoadFileWithLimitedItems(destinationWorkspaceInitialImportCount), overwriteMode: DocumentOverwriteMode.AppendOverlay);
            }

            await RunAndRetryIntegrationPointExecution(ImportAction);
        }
    }
}