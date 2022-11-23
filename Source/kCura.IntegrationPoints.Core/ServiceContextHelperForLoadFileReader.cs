using System;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
    public class ServiceContextHelperForLoadFileReader : IServiceContextHelper
    {
        public ServiceContextHelperForLoadFileReader(int workspaceId)
        {
            WorkspaceID = workspaceId;
        }

        public int WorkspaceID { get; }

        public IRipDBContext GetDBContext(int workspaceID = -1)
        {
            throw new NotImplementedException();
        }

        public int GetEddsUserID()
        {
            throw new NotImplementedException();
        }

        public IRelativityObjectManagerService GetRelativityObjectManagerService()
        {
            throw new NotImplementedException();
        }

        public int GetWorkspaceUserID()
        {
            throw new NotImplementedException();
        }
    }
}
