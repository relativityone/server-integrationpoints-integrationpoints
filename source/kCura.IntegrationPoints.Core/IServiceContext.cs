using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public interface IServiceContext
	{
		int UserID { get; set; }
		int WorkspaceID { get; set; }
		IRSAPIService RsapiService { get; set; }
		IDBContext SqlContext { get; set; }
	}
}
