using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
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
            void ImportAction()
            {
                const int imagesCount = 10;
                
                string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFile();
                RelativityFacade.Instance.ImportImages(TestsImplementationTestFixture.Workspace, testDataPath, imagesCount);
            }

            OneTimeSetupExecution(ImportAction);
        }

        public async Task RunAndRetryIntegrationPoint()
        {
            void ImportAction(Workspace destinationWorkspace)
            {
                const int destinationWorkspaceInitialImportCount = 4;
                string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFileWithLimitedItems(destinationWorkspaceInitialImportCount);
                RelativityFacade.Instance.ImportImages(TestsImplementationTestFixture.Workspace, testDataPath, destinationWorkspaceInitialImportCount);
            }

            await RunAndRetryIntegrationPointExecution(ImportAction);
        }
    }
}
