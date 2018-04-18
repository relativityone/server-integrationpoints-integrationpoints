using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public interface IServiceContextHelper
	{
		int WorkspaceID { get; set; }
		int GetEddsUserID();
		int GetWorkspaceUserID();
		IDBContext GetDBContext(int workspaceID = -1);
		IRSAPIService GetRsapiService();
	}
}
