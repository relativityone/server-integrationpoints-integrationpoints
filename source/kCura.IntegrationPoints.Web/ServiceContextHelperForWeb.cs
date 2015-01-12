using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Web
{
	public class ServiceContextHelperForWeb : IServiceContextHelper
	{
		private readonly WebClientFactory _factory;
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
			//TODO: get actual edds user id
			return GetRequestNumericValueByKey("x-ip-userid");
			//return helper.GetAuthenticationManager().UserInfo.ArtifactID;
		}
		public int GetWorkspaceUserID()
		{
			return GetRequestNumericValueByKey("x-ip-userid");
			//return helper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID;
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
