using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;

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
            void ImportAction()
            {
                RelativityFacade.Instance.ImportDocumentsFromCsv(TestsImplementationTestFixture.Workspace,
                    LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);
            }

            OneTimeSetupExecution(ImportAction);
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