using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using IDBContext = Relativity.API.IDBContext;

namespace kCura.IntegrationPoints.Web
{
	public class ServiceContextHelperForWeb : IServiceContextHelper
	{
		private const string USER_HEADER_VALUE = "X-IP-USERID";
		private const string CASEUSER_HEADER_VALUE = "X-IP-CASEUSERID";
		private ISessionService _sessionService;
		public ServiceContextHelperForWeb(ICPHelper helper, IEnumerable<IWorkspaceService> services, ISessionService sessionService)
		{
			this.helper = helper;
			this.customPageServices = services;
			_sessionService = sessionService;
		}

		private ICPHelper helper { get; set; }
		private IEnumerable<IWorkspaceService> customPageServices;

		private int? _workspaceID;
		public int WorkspaceID
		{
			get
			{
				if (!_workspaceID.HasValue)
					_workspaceID = customPageServices.First(x => x.GetWorkspaceID() != 0).GetWorkspaceID();
				return _workspaceID.Value;
			}
			set { _workspaceID = value; }
		}
		public int GetEddsUserID()
		{
			var result = GetRequestNumericValueByKey(USER_HEADER_VALUE);
			if (result == 0)
			{
				result = _sessionService.UserID;
			}
			return result;
		}
		public int GetWorkspaceUserID()
		{
			return GetRequestNumericValueByKey(CASEUSER_HEADER_VALUE);
		}
		public IRSAPIService GetRsapiService()
		{
			if (this.WorkspaceID > 0)
				return ServiceContextFactory.CreateRSAPIService(helper, WorkspaceID);
			else
				return null;
		}

		public IDBContext GetDBContext(int workspaceID = -1)
		{
			return helper.GetDBContext(workspaceID);
		}

		public int GetRequestNumericValueByKey(string key)
		{
			int returnValue = 0;
			string[] sValues = System.Web.HttpContext.Current.Request.Headers.GetValues(key);
			if (sValues != null && sValues.Length > 0 && !string.IsNullOrEmpty(sValues[0]))
			{
				int.TryParse(sValues[0], out returnValue);
			}
			return returnValue;
		}
	}
}
