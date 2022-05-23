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
    public class LiquidFormsHelper
    {
        private bool? _isLiquidForms;

        private readonly IServicesMgr _serviceManager;
        private readonly IAPILog _logger;

        public LiquidFormsHelper(IServicesMgr serviceManager, IAPILog logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
        }

        public async Task<bool> IsLiquidForms(int workspaceArtifactId)
        {
            if (_isLiquidForms.HasValue)
            {
                return (bool)_isLiquidForms;
            }

            _logger.LogVerbose("Querying for object type Integration Point");
            using (IObjectManager objectManager = _serviceManager.CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                QueryRequest queryRequest = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int)ArtifactType.ObjectType
                    },
                    Condition = $"'Name' == 'Integration Point'"
                };
                QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 0, 1).ConfigureAwait(false);

                using (IObjectTypeManager objectTypeManager = _serviceManager.CreateProxy<IObjectTypeManager>(ExecutionIdentity.CurrentUser))
                {
                    ObjectTypeResponse result = await objectTypeManager.ReadAsync(workspaceArtifactId, queryResult.Objects.First().ArtifactID);
                    _isLiquidForms = result.UseRelativityForms;
                    return _isLiquidForms ?? false;
                }
            }
        }
    }
}
