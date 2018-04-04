using Relativity.Core.DTO;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public interface INativesService
	{
		File GetNativeFileInfo(int workspaceId, int documentArtifactId);
	}
}