using kCura.WinEDDS;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IExportFieldsService
	{
		FieldEntry[] GetAllExportableFields(int workspaceArtifactID, int artifactTypeID);

		FieldEntry[] GetDefaultViewFields(int workspaceArtifactID, int viewArtifactID, int artifactTypeID, bool isProduction);

		FieldEntry[] GetAllExportableLongTextFields(int workspaceArtifactID, int artifactTypeID);
		
		ViewFieldInfo[] RetrieveAllExportableViewFields(int workspaceID, int artifactTypeID, string correlationID);
	}
}