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
		public int WorkspaceID { get; set; }
		public IRSAPIService RsapiService { get; set; }
		public IDBContext SqlContext { get; set; }

		public ServiceContext(IRSAPIService service)
		{
			
		}
	}
}
