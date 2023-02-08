using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    public class SourceProviderService : ISourceProviderService
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IDataProviderFactory _providerFactory;
        private readonly IAPILog _logger;

        public SourceProviderService(IKeplerServiceFactory serviceFactory, IDataProviderFactory providerFactory, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _providerFactory = providerFactory;
            _logger = logger;
        }

        public async Task<IDataSourceProvider> GetSourceProviderAsync(int workspaceId, int sourceProviderId)
        {
            try
            {
                using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    QueryRequest queryRequest = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef()
                        {
                            Guid = ObjectTypeGuids.SourceProviderGuid
                        },
                        Condition = $"'ArtifactID' == {sourceProviderId}",
                        Fields = new []
                        {
                            new FieldRef()
                            {
                                Name = "*"
                            }
                        }
                    };

                    QueryResult queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, 0, 1).ConfigureAwait(false);

                    if (!queryResult.Objects.Any())
                    {
                        throw new Exception($"Could not find Source Provider ID: {sourceProviderId}");
                    }

                    RelativityObject sourceProvider = queryResult.Objects.Single();
                    Guid appGuid = new Guid(sourceProvider[SourceProviderFieldGuids.ApplicationIdentifierGuid].Value.ToString());
                    Guid providerGuid = new Guid(sourceProvider[SourceProviderFieldGuids.IdentifierGuid].Value.ToString());

                    IDataSourceProvider provider = _providerFactory.GetDataProvider(appGuid, providerGuid);
                    return provider;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query for Source Provider ID: {sourceProviderId} in Workspace {workspaceId}", sourceProviderId, workspaceId);
                throw;
            }
        }
    }
}