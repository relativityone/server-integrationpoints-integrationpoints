using System;
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
        private bool? _isLiquidForms;

        private readonly IServicesMgr _servicesManager;
        private readonly IAPILog _logger;

        public LiquidFormsHelper(IServicesMgr servicesManager, IAPILog logger) 
        {
            _servicesManager = servicesManager;
            _logger = logger;
        }

        public async Task<bool> IsLiquidForms(int workspaceArtifactId)
        {
            if (_isLiquidForms.HasValue)
            {
                return (bool)_isLiquidForms;
            }

            try
            {
                _logger.LogInformation("Querying for object type Integration Point for workspaceArtifactId - {workspaceArtifactId}", workspaceArtifactId);
                using (IObjectManager objectManager =
                       _servicesManager.CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
                {
                    QueryRequest queryRequest = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef()
                        {
                            ArtifactTypeID = (int)ArtifactType.ObjectType
                        },
                        Condition = $"'Name' == 'Integration Point'"
                    };
                    QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 0, 1)
                        .ConfigureAwait(false);

                    using (IObjectTypeManager objectTypeManager =
                           _servicesManager.CreateProxy<IObjectTypeManager>(ExecutionIdentity.CurrentUser))
                    {
                        ObjectTypeResponse result = await objectTypeManager.ReadAsync(workspaceArtifactId,
                            queryResult.Objects.First().ArtifactID);
                        _isLiquidForms = result.UseRelativityForms;
                        return _isLiquidForms ?? false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to get Integration Point type for workspace Artifact Id - {workspaceArtifactId}. Exception - {ex}", workspaceArtifactId, ex);
            }

            return false;
        }
    }
}
