using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
    public interface IServiceContextHelper
    {
        int WorkspaceID { get; }

        int GetEddsUserID();

        int GetWorkspaceUserID();

        IRipDBContext GetDBContext(int workspaceID = -1);

        IRelativityObjectManagerService GetRelativityObjectManagerService();
    }
}
