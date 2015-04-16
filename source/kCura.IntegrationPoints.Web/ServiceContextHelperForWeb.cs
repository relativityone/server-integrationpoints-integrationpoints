﻿using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.CustomPages;
using IDBContext = Relativity.API.IDBContext;

namespace kCura.IntegrationPoints.Web
{
	public class ServiceContextHelperForWeb : IServiceContextHelper
	{
		private readonly WebClientFactory _factory;
		private const string USER_HEADER_VALUE = "X-IP-USERID";
		private const string CASEUSER_HEADER_VALUE = "X-IP-CASEUSERID";
		public ServiceContextHelperForWeb(ICPHelper helper, IEnumerable<IWorkspaceService> services, WebClientFactory factory)
		{
			this.helper = helper;
			this.customPageServices = services;
			_factory = factory;
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
			return GetRequestNumericValueByKey(USER_HEADER_VALUE);
		}
		public int GetWorkspaceUserID()
		{
			return GetRequestNumericValueByKey(CASEUSER_HEADER_VALUE);
		}
		public IRSAPIService GetRsapiService()
		{
			if (this.WorkspaceID > 0)
				return ServiceContextFactory.CreateRSAPIService(GetRsapiClient());
			else
				return null;
		}
		public IRSAPIClient GetRsapiClient(ExecutionIdentity identity = ExecutionIdentity.CurrentUser)
		{
			return _factory.CreateClient();
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
