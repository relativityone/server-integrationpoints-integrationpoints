using System;
using Castle.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class CaseServiceContext : ICaseServiceContext
	{
		private string _workspaceName;

		public string GetWorkspaceName(int workspaceId)
		{
			if (_workspaceName == null)
			{
				WorkspaceQuery query = new WorkspaceQuery(helper.GetRsapiClient(ExecutionIdentity.CurrentUser));
				Workspace workspace = query.GetWorkspace(workspaceId);
				_workspaceName = workspace.Name;
			}
			return _workspaceName;
		}

		public int WorkspaceID { get; set; }

		private int? _eddsUserID;
		public int EddsUserID
		{
			get
			{
				if (!_eddsUserID.HasValue) _eddsUserID = helper.GetEddsUserID();
				return _eddsUserID.Value;
			}
			set { _eddsUserID = value; }
		}

		private int? _workspaceUserID;
		public int WorkspaceUserID
		{
			get
			{
				if (!_workspaceUserID.HasValue) _workspaceUserID = helper.GetWorkspaceUserID();
				return _workspaceUserID.Value;
			}
			set { _workspaceUserID = value; }
		}

		private IRSAPIService _rsapiService;

		[DoNotWire]
		public IRSAPIService RsapiService
		{
			get
			{
				if (_rsapiService == null) _rsapiService = helper.GetRsapiService();
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
				if (_sqlContext == null) _sqlContext = helper.GetDBContext(this.WorkspaceID);
				return _sqlContext;
			}
			set { _sqlContext = value; }
		}

		private IServiceContextHelper helper { get; set; }
		public CaseServiceContext(IServiceContextHelper helper)
		{
			this.helper = helper;
			this.WorkspaceID = helper.WorkspaceID;
		}
	}
}
