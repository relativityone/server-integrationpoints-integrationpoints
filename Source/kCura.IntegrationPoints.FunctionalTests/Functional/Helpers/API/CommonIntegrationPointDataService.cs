using Relativity.IntegrationPoints.Services;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.Folder;
using Relativity.Testing.Framework.Api.Kepler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.API
{
    internal interface ICommonIntegrationPointDataService
    {
        Task<int> GetDestinationProviderIdAsync(string identifier);
        Task<int> GetIntegrationPointTypeByAsync(string name);
        Task<int> GetOverwriteFieldsChoiceIdAsync(string name);
        Task<int> GetRootFolderArtifactIdAsync();
        Task<int> GetSourceProviderIdAsync(string identifier);
    }

    internal class CommonIntegrationPointDataService : ICommonIntegrationPointDataService
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly int _workspaceId;

        public CommonIntegrationPointDataService(IKeplerServiceFactory serviceFactory, int workspaceId)
        {
            _serviceFactory = serviceFactory;
            _workspaceId = workspaceId;
        }

        public async Task<int> GetSourceProviderIdAsync(string identifier)
        {
            using (IProviderManager providerManager = _serviceFactory.GetServiceProxy<IProviderManager>())
            {
                return await providerManager.GetSourceProviderArtifactIdAsync(_workspaceId, identifier).ConfigureAwait(false);
            }
        }

        public async Task<int> GetDestinationProviderIdAsync(string identifier)
        {
            using (IProviderManager providerManager = _serviceFactory.GetServiceProxy<IProviderManager>())
            {
                return await providerManager.GetDestinationProviderArtifactIdAsync(_workspaceId, identifier).ConfigureAwait(false);
            }
        }

        public async Task<int> GetIntegrationPointTypeByAsync(string name)
        {
            using (IIntegrationPointTypeManager integrationPointTypeManager = _serviceFactory.GetServiceProxy<IIntegrationPointTypeManager>())
            {
                IList<IntegrationPointTypeModel> types = await integrationPointTypeManager.GetIntegrationPointTypes(_workspaceId).ConfigureAwait(false);

                return types.Single(x => x.Name == name).ArtifactId;
            }
        }

        public async Task<int> GetOverwriteFieldsChoiceIdAsync(string name)
        {
            using (IArtifactGuidManager guidManager = _serviceFactory.GetServiceProxy<IArtifactGuidManager>())
            using (IChoiceQueryManager choiceManager = _serviceFactory.GetServiceProxy<IChoiceQueryManager>())
            {
                int overwriteFieldId = await guidManager.ReadSingleArtifactIdAsync(_workspaceId,
                       Guid.Parse(kCura.IntegrationPoints.Data.IntegrationPointFieldGuids.OverwriteFields))
                   .ConfigureAwait(false);

                List<Choice> choices = await choiceManager.QueryAsync(_workspaceId, overwriteFieldId).ConfigureAwait(false);

                return choices.Single(x => x.Name == name).ArtifactID;
            }
        }

        public async Task<int> GetRootFolderArtifactIdAsync()
        {
            using (IFolderManager folderManager = _serviceFactory.GetServiceProxy<IFolderManager>())
            {
                Folder rootFolder = await folderManager.GetWorkspaceRootAsync(_workspaceId).ConfigureAwait(false);
                
                return rootFolder.ArtifactID;
            }
        }
    }
}
