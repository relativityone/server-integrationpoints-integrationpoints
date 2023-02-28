using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Choice;
using Relativity.Services.ResourcePool;
using IResourcePoolManagerSvc = Relativity.Services.ResourcePool.IResourcePoolManager;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class ResourcePoolRepository : IResourcePoolRepository
    {
        #region Fields

        private readonly IHelper _helper;

        #endregion // Fields

        #region Constructors

        public ResourcePoolRepository(IHelper helper)
        {
            _helper = helper;
        }

        #endregion // Constructors

        #region Methods

        public List<ProcessingSourceLocationDTO> GetProcessingSourceLocationsByResourcePool(int resourcePoolId)
        {
            using (IResourcePoolManagerSvc resourcePoolManagerSvcProxy =
                    _helper.GetServicesManager().CreateProxy<IResourcePoolManagerSvc>(ExecutionIdentity.System))
            {
                List<ChoiceRef> choiceRefs = resourcePoolManagerSvcProxy.GetProcessingSourceLocationsAsync(
                    new ResourcePoolRef(resourcePoolId)).ConfigureAwait(false).GetAwaiter().GetResult();

                return choiceRefs.Select(item => new ProcessingSourceLocationDTO
                {
                    ArtifactId = item.ArtifactID,
                    Location = item.Name
                }).ToList();
            }
        }

        #endregion Methods
    }
}
