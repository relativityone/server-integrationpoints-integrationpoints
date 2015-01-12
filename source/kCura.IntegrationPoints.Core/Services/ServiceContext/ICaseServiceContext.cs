using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public interface ICaseServiceContext
	{
		int EddsUserID { get; set; }
		int WorkspaceUserID { get; set; }
		int WorkspaceID { get; set; }
		IRSAPIService RsapiService { get; set; }
		IDBContext SqlContext { get; set; }
	}
}
