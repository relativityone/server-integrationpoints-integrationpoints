using FluentAssertions;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
    internal class AzureADTestImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IRipApi _ripApi;

        private Workspace Workspace => _testsImplementationTestFixture.Workspace;

        public AzureADTestImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;

            _serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
            
            _ripApi = new RipApi(_serviceFactory);
        }

        public void OnSetupFixture()
        {
            var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

            int appId = applicationService.InstallToLibrary(TestConfig.AzureADProviderRapFileLocation, 
                new LibraryApplicationInstallOptions { IgnoreVersion = true });

            applicationService.InstallToWorkspace(Workspace.ArtifactID, appId);
        }

        public async Task ImportEntityWithAzureADProvider()
        {
            // Arrange
            AzureADIntegrationPointModelProvider modelProvider = new AzureADIntegrationPointModelProvider(_serviceFactory, Workspace);

            IntegrationPointModel integrationPoint = await modelProvider.ImportAzureADModel().ConfigureAwait(false);

            await _ripApi.CreateIntegrationPointAsync(integrationPoint, Workspace.ArtifactID).ConfigureAwait(false);

            // Act
            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, Workspace.ArtifactID).ConfigureAwait(false);

            await _ripApi.WaitForJobToFinishAsync(jobHistoryId, Workspace.ArtifactID, 
                expectedStatus: JobStatusChoices.JobHistoryCompletedWithErrors.Name).ConfigureAwait(false);

            // Assert
            int transferredItemsCount = await GetTransferredItems(jobHistoryId).ConfigureAwait(false);

            Entity[] entities = RelativityFacade.Instance.Resolve<IEntityService>().GetAll(Workspace.ArtifactID);

            entities.Should().HaveCount(transferredItemsCount);

            IEnumerable<object> managers = await GetManagersAsync().ConfigureAwait(false);

            managers.Should().Contain(x => x != null);
        }

        private async Task<int> GetTransferredItems(int jobHistoryId)
        {
            using(IObjectManager objectManager = _serviceFactory.GetServiceProxy<IObjectManager>())
            {
                ReadRequest request = new ReadRequest
                {
                    Object = new RelativityObjectRef { ArtifactID = jobHistoryId },
                    Fields = new[] { new FieldRef { Name = JobHistoryFields.ItemsTransferred } }
                };

                ReadResult jobHistory = await objectManager.ReadAsync(Workspace.ArtifactID, request).ConfigureAwait(false);

                return (int)jobHistory.Object.FieldValues[0].Value;
            }
        }

        private async Task<IEnumerable<object>> GetManagersAsync()
        {
            using(IObjectTypeManager objectTypeManager = _serviceFactory.GetServiceProxy<IObjectTypeManager>())
            using(IObjectManager objectManager = _serviceFactory.GetServiceProxy<IObjectManager>())
            {
                List<ObjectTypeIdentifier> artifactTypes = await objectTypeManager.GetAvailableParentObjectTypesAsync(Workspace.ArtifactID).ConfigureAwait(false);
                ObjectTypeIdentifier artifactType = artifactTypes.Single(x => x.Name == "Entity");

                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactID = artifactType.ArtifactID },
                    Fields = new[] { new FieldRef { Name = "Manager" } }
                };

                QueryResult result = await objectManager.QueryAsync(Workspace.ArtifactID, request, 0, int.MaxValue).ConfigureAwait(false);

                return result.Objects.Select(x => x.FieldValues.Single().Value);
            }
        }
    }
}
