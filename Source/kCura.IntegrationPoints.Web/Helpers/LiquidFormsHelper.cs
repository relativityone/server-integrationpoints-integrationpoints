using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Relativity;
using Relativity.API;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Web.Helpers
{
    public class LiquidFormsHelper : ILiquidFormsHelper
    {
        private readonly ConcurrentDictionary<int, bool> _isLiquidFormsDictionary = new ConcurrentDictionary<int, bool>();

        private readonly IServicesMgr _servicesManager;
        private readonly IAPILog _logger;

        public LiquidFormsHelper(IServicesMgr servicesManager, IAPILog logger) 
        {
            _servicesManager = servicesManager;
            _logger = logger;
        }

        public async Task<bool> IsLiquidForms(int workspaceArtifactId)
        {
            if (_isLiquidFormsDictionary.ContainsKey(workspaceArtifactId))
            {
                return _isLiquidFormsDictionary[workspaceArtifactId];
            }

            try
            {
                _logger.LogInformation("Querying for object type Integration Point for workspaceArtifactId - {workspaceArtifactId}", workspaceArtifactId);
                using (IObjectManager objectManager =
                       _servicesManager.CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
                using (IObjectTypeManager objectTypeManager =
                       _servicesManager.CreateProxy<IObjectTypeManager>(ExecutionIdentity.CurrentUser))
                {
                    QueryRequest queryRequest = new QueryRequest
                    {
                        ObjectType = new ObjectTypeRef
                        {
                            ArtifactTypeID = (int)ArtifactType.ObjectType
                        },
                        Condition = $"'Name' == 'Integration Point'"
                    };
                    QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 0, 1)
                        .ConfigureAwait(false);

                    if (queryResult.Objects.Count < 1)
                    {
                        return false;
                    }

                    ObjectTypeResponse result = await objectTypeManager.ReadAsync(workspaceArtifactId,
                        queryResult.Objects.First().ArtifactID);
                    bool isLiquidForms = result.UseRelativityForms ?? false;
                    _isLiquidFormsDictionary.TryAdd(workspaceArtifactId, isLiquidForms);

                    return isLiquidForms;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to get Integration Point type for workspace Artifact Id - {workspaceArtifactId}. Exception - {ex}", workspaceArtifactId, ex.Message);
            }

            return false;
        }
    }
}
