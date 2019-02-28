using Relativity.Services.Interfaces.ViewField.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IViewFieldRepository
	{
		ViewFieldResponse[] GetAllViewFieldsByArtifactTypeID(int artifactTypeID);

		ViewFieldIDResponse[] GetViewFieldsByArtifactTypeIDAndViewArtifactID(int artifactTypeID,
			int viewArtifactID, bool fromProduction);
	}
}
