using Castle.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class CaseServiceContext : ICaseServiceContext
	{
		private readonly IServiceContextHelper _helper;

		public CaseServiceContext(IServiceContextHelper helper)
		{
			_helper = helper;
			this.WorkspaceID = helper.WorkspaceID;
		}

		public string GetWorkspaceName(int workspaceId)
		{
			WorkspaceQuery query = new WorkspaceQuery(_helper.GetRsapiClient(ExecutionIdentity.CurrentUser));
			string workspaceName = query.GetWorkspaceName(workspaceId);
			return workspaceName;
		}

		public int WorkspaceID { get; set; }

		private int? _eddsUserId;
		public int EddsUserID
		{
			get
			{
				if (!_eddsUserId.HasValue)
				{
					_eddsUserId = _helper.GetEddsUserID();
				}
				return _eddsUserId.Value;
			}
			set { _eddsUserId = value; }
		}

		private int? _workspaceUserId;
		public int WorkspaceUserID
		{
			get
			{
				if (!_workspaceUserId.HasValue)
				{
					_workspaceUserId = _helper.GetWorkspaceUserID();
				}
				return _workspaceUserId.Value;
			}
			set { _workspaceUserId = value; }
		}

		private IRSAPIService _rsapiService;

		[DoNotWire]
		public IRSAPIService RsapiService
		{
			get
			{
				if (_rsapiService == null)
				{
					_rsapiService = _helper.GetRsapiService();
				}
				return _rsapiService;
			}
			set { _rsapiService = value; }
		}

		private IDBContext _sqlContext;
		[DoNotWire]
		public IDBContext SqlContext
		{
			get
			{
				if (_sqlContext == null)
				{
					_sqlContext = _helper.GetDBContext(this.WorkspaceID);
				}
				return _sqlContext;
			}
			set { _sqlContext = value; }
		}
	}
}
