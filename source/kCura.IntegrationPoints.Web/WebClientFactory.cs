using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Web
{
	public class WebClientFactory
	{
		private int WorkspaceID
		{
			get
			{
				return _customPageServices.First(x => x.GetWorkspaceID() != 0).GetWorkspaceID();
			}
		}

		private readonly RsapiClientFactory _factory;
		private readonly IEnumerable<IWorkspaceService> _customPageServices;

		public WebClientFactory(RsapiClientFactory factory, IEnumerable<IWorkspaceService> services)
		{
			_factory = factory;
			_customPageServices = services;
		}

		public IRSAPIClient CreateClient()
		{
			return _factory.CreateClientForWorkspace(WorkspaceID);
		}

		public IRSAPIClient CreateEddsClient()
		{
			return _factory.CreateClientForWorkspace(-1);
		}

		public IDBContext CreateDbContext()
		{
			return _factory.CreateDbContext(WorkspaceID);
		}

		public IServicesMgr CreateServicesMgr()
		{
			return _factory.CreateServicesMgr();
		}
	}
}