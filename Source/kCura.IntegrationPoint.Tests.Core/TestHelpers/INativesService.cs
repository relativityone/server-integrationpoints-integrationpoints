namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	using global::Relativity.Core.DTO;

	public interface INativesService
	{
		File GetNativeFileInfo(int workspaceId, int documentArtifactId);
	}
}