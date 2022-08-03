using Castle.Core;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
    public class CaseServiceContext : ICaseServiceContext
    {
        private int? _eddsUserId;
        private int? _workspaceUserId;
        private IRelativityObjectManagerService _relativityObjectManagerService;
        private IDBContext _sqlContext;
        private readonly IServiceContextHelper _serviceContextHelper;

        public CaseServiceContext(IServiceContextHelper serviceContextHelper)
        {
            _serviceContextHelper = serviceContextHelper;
            this.WorkspaceID = serviceContextHelper.WorkspaceID;
        }

        public int WorkspaceID { get; set; }

        public int EddsUserID
        {
            get
            {
                if (!_eddsUserId.HasValue)
                {
                    _eddsUserId = _serviceContextHelper.GetEddsUserID();
                }
                return _eddsUserId.Value;
            }
            set { _eddsUserId = value; }
        }

        public int WorkspaceUserID
        {
            get
            {
                if (!_workspaceUserId.HasValue)
                {
                    _workspaceUserId = _serviceContextHelper.GetWorkspaceUserID();
                }
                return _workspaceUserId.Value;
            }
            set { _workspaceUserId = value; }
        }

        [DoNotWire]
        public IRelativityObjectManagerService RelativityObjectManagerService
        {
            get
            {
                if (_relativityObjectManagerService == null)
                {
                    _relativityObjectManagerService = _serviceContextHelper.GetRelativityObjectManagerService();
                }
                return _relativityObjectManagerService;
            }
            set { _relativityObjectManagerService = value; }
        }

        [DoNotWire]
        public IDBContext SqlContext
        {
            get
            {
                if (_sqlContext == null)
                {
                    _sqlContext = _serviceContextHelper.GetDBContext(this.WorkspaceID);
                }
                return _sqlContext;
            }
            set { _sqlContext = value; }
        }
    }
}
