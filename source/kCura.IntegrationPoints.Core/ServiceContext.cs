using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class ServiceContext : IServiceContext
	{
		public int UserID { get; set; }

		private int _workspaceID;
		public int WorkspaceID
		{
			get
			{
				if (_workspaceID == 0)
				{
					_workspaceID = _services.First(x => x.GetWorkspaceID() != 0).GetWorkspaceID();
				}
				return _workspaceID;
			}
			set { _workspaceID = value; }
		}

		public IRSAPIService RsapiService { get; set; }
		public IDBContext SqlContext { get; set; }

		private readonly IEnumerable<IWorkspaceService> _services; 
		public ServiceContext(IEnumerable<IWorkspaceService> services)
		{
			_services = services;
		}
	}
}
