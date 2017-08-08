using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Web
{
	public class WebClientFactory
	{
		public int WorkspaceId
		{
			get
			{
				return _customPageServices.First(x => x.GetWorkspaceID() != 0).GetWorkspaceID();
			}
		}

		private readonly IHelper _helper;
		private readonly IRsapiClientFactory _factory;
		private readonly IEnumerable<IWorkspaceService> _customPageServices;

		public WebClientFactory(IHelper helper, IRsapiClientFactory factory, IEnumerable<IWorkspaceService> services)
		{
			_helper = helper;
			_factory = factory;
			_customPageServices = services;
		}

		public IRSAPIClient CreateClient()
		{
			return _factory.CreateUserClient(WorkspaceId);
		}

		public IRSAPIClient CreateEddsClient()
		{
			return _factory.CreateUserClient(-1);
		}

		public IDBContext CreateDbContext()
		{
			return _helper.GetDBContext(WorkspaceId);
		}

		public IServicesMgr CreateServicesMgr()
		{
			return _helper.GetServicesManager();
		}
	}
}