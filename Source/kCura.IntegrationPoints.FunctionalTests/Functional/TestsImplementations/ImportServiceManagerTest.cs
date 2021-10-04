using ARMTestServices.Services.Interfaces;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class ImportServiceManagerTest
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

        private readonly IRipApi _ripApi = 
            new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);

        private int _integrationPointTypeId;
        private int _sourceProviderId;
        private int _destinationProviderId;

        public Workspace SourceWorkspace => _testsImplementationTestFixture.Workspace;

        public ImportServiceManagerTest(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            if (RelativityFacade.Instance.Resolve<ILibraryApplicationService>().Get("ARM Test Services") == null)
            {
                InstallARMTestServicesToWorkspace();
            }

            // Preparing data for LoadFile and placing it in the right location
            string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFile();
            LoadFilesGenerator.UploadLoadFileToImportDirectory(_testsImplementationTestFixture.Workspace.ArtifactID, testDataPath).Wait();

            GetIntegrationPointsConstantsAsync().GetAwaiter().GetResult();
        }

        public void OnTearDownFixture()
        {
            // nothing for now, but probably some deleting integration point ? 
        }

        public async void RunIntegrationPointAndCheckCorectness()
        {
            IntegrationPointModel integrationPoint = await GetIntegrationPointAsync().ConfigureAwait(false);
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID)
                .ConfigureAwait(false);

            // FURTHER IMPLEMENTATION WILL GO HERE
        }

        public void RunTest()
        {
            RunIntegrationPointAndCheckCorectness();
        }

        // USED IN TWO CLASSES - MAYBE SEPARATE IT SOMEWHERE AND MAKE REUSABLE?
        private static void InstallARMTestServicesToWorkspace()
        {
            string rapFileLocation = TestConfig.ARMTestServicesRapFileLocation;

            RelativityFacade.Instance.Resolve<ILibraryApplicationService>()
                .InstallToLibrary(rapFileLocation, new LibraryApplicationInstallOptions
                {
                    CreateIfMissing = true
                });
        }

        
        public async Task<IntegrationPointModel> GetIntegrationPointAsync()
        {
            int rootFolderId = await GetRootFolderArtifactIdAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);

            // add configuration for source and destination!

            return new IntegrationPointModel
            {
                //SourceConfiguration = sourceConfiguration,
                //DestinationConfiguration = GetDestinationConfiguration(destinationWorkspace.ArtifactID, rootFolderId),
                Name = Const.INTEGRATION_POINT_NAME_FOR_LOADFILE_IMPORT_IMAGES,
                DestinationProvider = _destinationProviderId,
                SourceProvider = _sourceProviderId,
                Type = _integrationPointTypeId,
                EmailNotificationRecipients = "",
                ScheduleRule = new ScheduleModel()
            };
        }

        private async Task<int> GetRootFolderArtifactIdAsync(int workspaceId)
        {
            using (IFolderManager folderManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory
                .GetServiceProxy<IFolderManager>())
            {
                Folder rootFolder = await folderManager.GetWorkspaceRootAsync(workspaceId).ConfigureAwait(false);
                return rootFolder.ArtifactID;
            }
        }

        private async Task GetIntegrationPointsConstantsAsync()
        {
            _sourceProviderId = await GetSourceProviderAsync(SourceWorkspace.ArtifactID);
            _destinationProviderId = await GetDestinationProviderIdAsync(SourceWorkspace.ArtifactID);
            _integrationPointTypeId = await GetIntegrationPointTypeAsync(SourceWorkspace.ArtifactID, "Import");

        }

        private async Task<int> GetSourceProviderAsync(int workspaceId)
        {
            string identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE;

            QueryRequest query = new QueryRequest
            {
                Condition = $"'{SourceProviderFields.Identifier}' == '{identifier.ToLower()}'",
                ObjectType = new ObjectTypeRef { Guid = new Guid(ObjectTypeGuids.SourceProvider) },
                Fields = new FieldRef[] { new FieldRef { Name = "*" } }
            };

            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 1).ConfigureAwait(false);
                return result.Objects.Single().ArtifactID;
            }
        }

        private async Task<int> GetDestinationProviderIdAsync(int workspaceId,
            string identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders
                .RELATIVITY)
        {
            QueryRequest query = new QueryRequest
            {
                Condition = $"'{DestinationProviderFields.Identifier}' == '{identifier}'",
                ObjectType = new ObjectTypeRef { Guid = new Guid(ObjectTypeGuids.DestinationProvider) }
            };

            using (var objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory
                .GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 10000).ConfigureAwait(false);

                return result.Objects.Single().ArtifactID;
            }
        }

        private async Task<int> GetIntegrationPointTypeAsync(int workspaceId, string typeName)
        {
            QueryRequest query = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = ObjectTypeGuids.IntegrationPointTypeGuid
                },
                Condition = $"'Name' == '{typeName}'"
            };

            using(IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 1).ConfigureAwait(false);
                return result.Objects.Single().ArtifactID;
            }
        }

    }
}
