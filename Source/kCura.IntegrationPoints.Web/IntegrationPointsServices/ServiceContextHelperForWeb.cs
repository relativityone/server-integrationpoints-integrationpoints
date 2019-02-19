using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices
{
	internal class ServiceContextHelperForWeb : IServiceContextHelper
	{
		private readonly IUserContext _userContext;
		private readonly ICPHelper _helper;
		private readonly IWorkspaceIdProvider _workspaceIdProvider;

		public ServiceContextHelperForWeb(ICPHelper helper, IWorkspaceIdProvider workspaceIdProvider, IUserContext userContext)
		{
			_helper = helper;
			_workspaceIdProvider = workspaceIdProvider;
			_userContext = userContext;
		}

		private int? _workspaceId;
		public int WorkspaceID
		{
			get
			{
				if (!_workspaceId.HasValue)
				{
					_workspaceId = _workspaceIdProvider.GetWorkspaceId();
				}

				return _workspaceId.Value;
			}
		}

		public int GetEddsUserID()
		{
			return _userContext.GetUserID();
		}

		public int GetWorkspaceUserID()
		{
			return _userContext.GetWorkspaceUserID();
		}

		public IRSAPIService GetRsapiService()
		{
			return WorkspaceID > 0 
				? ServiceContextFactory.CreateRSAPIService(_helper, WorkspaceID) 
				: null;
		}

		public IDBContext GetDBContext(int workspaceId = -1)
		{
			return _helper.GetDBContext(workspaceId);
		}
	}
}
