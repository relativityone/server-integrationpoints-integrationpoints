using Relativity.Services.Interfaces.ViewField.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories
{
	public interface IViewFieldRepository
	{
		ViewFieldResponse[] ReadExportableViewFields(int workspaceID, int artifactTypeID);

		ViewFieldIDResponse[] ReadViewFieldIDsFromSearch(int workspaceID, int artifactTypeID, int viewArtifactID);

		ViewFieldIDResponse[] ReadViewFieldIDsFromProduction(int workspaceID, int artifactTypeID, int viewArtifactID);
	}
}
