using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.CustomPages;
using IDBContext = Relativity.API.IDBContext;

namespace kCura.IntegrationPoints.Web
{
	public class RsapiClientFactory
	{
		private readonly IEnumerable<ICustomPageService> _services;
		public RsapiClientFactory(IEnumerable<ICustomPageService> services)
		{
			_services = services;
		}

		public IRSAPIClient CreateClient()
		{
			var workspaceID = _services.First(x => x.GetWorkspaceID() != 0).GetWorkspaceID();
			IRSAPIClient client;
			try
			{
				client = ConnectionHelper.Helper().GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			}
			catch (NullReferenceException)
			{
				client = ConnectionHelper.Helper().GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System);
			}
			client.APIOptions.WorkspaceID = workspaceID;
			return client;
		}

		public IDBContext CreateDbContext()
		{
			var workspaceID = _services.First(x => x.GetWorkspaceID() != 0).GetWorkspaceID();
			var context = ConnectionHelper.Helper().GetDBContext(workspaceID);
			return context;
		}

	}
}
