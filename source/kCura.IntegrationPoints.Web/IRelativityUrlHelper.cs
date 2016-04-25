namespace kCura.IntegrationPoints.Web
{
	public interface IRelativityUrlHelper
	{
		string GetRelativityViewUrl(int workspaceID, int artifactID, string objectTypeName);
	}
}