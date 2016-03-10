
namespace kCura.IntegrationPoints.Data
{
	public interface IPermissionService 
	{
		bool UserCanImport(int workspaceId);
	}
}
