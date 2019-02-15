﻿using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Services;
using kCura.IntegrationPoints.Web.WorkspaceIdProvider;
using Relativity.API;

namespace kCura.IntegrationPoints.Web
{
	public class ServiceContextHelperForWeb : IServiceContextHelper
	{
		private const string _USER_HEADER_VALUE = "X-IP-USERID";
		private const string _CASE_USER_HEADER_VALUE = "X-IP-CASEUSERID";

		private readonly ISessionService _sessionService;
		private readonly ICPHelper _helper;
		private readonly IWorkspaceIdProvider _workspaceIdProvider;

		public ServiceContextHelperForWeb(ICPHelper helper, IWorkspaceIdProvider workspaceIdProvider, ISessionService sessionService)
		{
			_helper = helper;
			_workspaceIdProvider = workspaceIdProvider;
			_sessionService = sessionService;
		}

		private int? _workspaceId;
		public int WorkspaceID // TODO
		{
			get
			{
				if (!_workspaceId.HasValue)
				{
					_workspaceId = _workspaceIdProvider.GetWorkspaceId();
				}

				return _workspaceId.Value;
			}
			set { _workspaceId = value; }
		}

		public int GetEddsUserID()
		{
			int result = GetRequestNumericValueByKey(_USER_HEADER_VALUE);
			if (result == 0)
			{
				result = _sessionService.UserID;
			}
			return result;
		}

		public int GetWorkspaceUserID()
		{
			return GetRequestNumericValueByKey(_CASE_USER_HEADER_VALUE);
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
