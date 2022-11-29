using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
    public interface ICaseServiceContext
    {
        int EddsUserID { get; set; }

        int WorkspaceUserID { get; set; }

        int WorkspaceID { get; set; }

        IRelativityObjectManagerService RelativityObjectManagerService { get; set; }

        IRipDBContext SqlContext { get; set; }
    }
}
