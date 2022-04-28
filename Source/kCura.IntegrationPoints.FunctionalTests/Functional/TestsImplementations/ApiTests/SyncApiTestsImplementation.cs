using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
    internal class SyncApiTestsImplementation : SyncApiTestsImplementationBase
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

        public SyncApiTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture) : 
            base(testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public override void OneTimeSetup()
        {
            void ImportAction()
            {
                RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace,
                    LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);
            }

            OneTimeSetupExecution(ImportAction);
        }

        public override async Task RunAndRetryIntegrationPoint()
        {
            void ImportAction(Workspace destinationWorkspace)
            {
                const int destinationWorkspaceInitialImportCount = 4;
                RelativityFacade.Instance.
                    ImportDocumentsFromCsv(destinationWorkspace, LoadFilesGenerator.CreateNativesLoadFileWithLimitedItems(destinationWorkspaceInitialImportCount), overwriteMode: DocumentOverwriteMode.AppendOverlay);
            }

            await RunAndRetryIntegrationPointExecution(ImportAction).ConfigureAwait(false);
        }
    }
}