using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
    public class ServiceContextHelperForKeplerService : IServiceContextHelper
    {
        private readonly IServiceHelper _helper;

        public ServiceContextHelperForKeplerService(IServiceHelper helper, int workspaceArtifactId)
        {
            _helper = helper;
            WorkspaceID = workspaceArtifactId;
        }

        public int WorkspaceID { get; }

        public int GetEddsUserID()
        {
            return _helper.GetAuthenticationManager().UserInfo.ArtifactID;
        }

        public int GetWorkspaceUserID()
        {
            return _helper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID;
        }

        public IDBContext GetDBContext(int workspaceId = -1)
        {
            return _helper.GetDBContext(workspaceId);
        }

        public IRelativityObjectManagerService GetRelativityObjectManagerService()
        {
            return ServiceContextFactory.CreateRelativityObjectManagerService(_helper, WorkspaceID);
        }
    }
}