using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Choice = Relativity.Services.ChoiceQuery.Choice;
using Relativity.Testing.Framework.Models;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    public class JobRetryApiTests 
    {       
        private const string SavedSearchName = "AllDocuments";
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private Workspace destinationWorkspace;
        private int integrationPoint;

        private readonly IRipApi _ripApi = new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);

        private int savedSearchId;
        private int destinationProviderId;
        private int sourceProviderId;
        private int integrationPointType;
        private List<Choice> choices;
        private const string JOB_RETRY_TEST_WORKSPACE_NAME = "RIP Job Retry Test";
        public Workspace SourceWorkspace => _testsImplementationTestFixture.Workspace;

        [OneTimeSetUp]
        public void OneTimeSetup() 
        {
            //import docs to Source Workspace
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);

            //create Destination Workspace
            RelativityFacade.Instance.CreateWorkspace(JOB_RETRY_TEST_WORKSPACE_NAME);

            
        }

        [OneTimeTearDown]
        public void OneTimeTeardown() { }

        //[Test]
        //public async Task JobRetryTest()
        //{

        //}

       

    }
}
