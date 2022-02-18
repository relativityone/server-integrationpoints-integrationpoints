using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public class RdoFilter : IRdoFilter
    {
        private readonly List<string> _systemRdo = new List<string>
        {
            "History", "Event Handler", "Install Event Handler", "Source Provider", "Integration Point", "Relativity Source Case", "Destination Workspace", "Relativity Source Job"
        };

        private readonly ICaseServiceContext _serviceContext;
        private readonly IObjectTypeQuery _rdoQuery;
        private readonly IServicesMgr _servicesMgr;

        public RdoFilter(IObjectTypeQuery rdoQuery, ICaseServiceContext serviceContext, IServicesMgr servicesMgr)
        {
            _rdoQuery = rdoQuery;
            _serviceContext = serviceContext;
            _servicesMgr = servicesMgr;
        }

        public Task<IEnumerable<ObjectTypeDTO>> GetAllViewableRdos()
        {
            List<ObjectTypeDTO> objectTypes = _rdoQuery
                .GetAllTypes(_serviceContext.WorkspaceUserID)
                .Where(objectType => !_systemRdo.Contains(objectType.Name))
                .ToList();

            using (IObjectTypeManager objectTypeManager = _servicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.CurrentUser))
            {
                ParallelOptions parallelOptions = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = 10
                };

                Parallel.ForEach(objectTypes, parallelOptions, objectType =>
                {
                    ObjectTypeResponse objectTypeResponse = objectTypeManager.ReadAsync(_serviceContext.WorkspaceID, objectType.ArtifactId).GetAwaiter().GetResult();
                    objectType.BelongsToApplication = objectTypeResponse.RelativityApplications.ViewableItems.Any();
                });
            }

            return Task.FromResult((IEnumerable<ObjectTypeDTO>)objectTypes);
        }
    }
}
