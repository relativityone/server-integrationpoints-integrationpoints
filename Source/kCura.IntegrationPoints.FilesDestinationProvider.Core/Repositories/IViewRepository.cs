using Relativity.Services.ViewManager.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories
{
	public interface IViewRepository
	{
		ViewResponse[] RetrieveViewsByContextArtifactID(int workspaceArtifactID, int artifactTypeID);
		SearchViewResponse[] RetrieveViewsByContextArtifactIDForSearch(int workspaceArtifactID);
	}
}