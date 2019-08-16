using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	public interface IExportDataSanitizer
	{
		bool ShouldSanitize(string dataType);
		Task<object> SanitizeAsync(
			int workspaceArtifactID, 
			string itemIdentifierSourceFieldName, 
			string itemIdentifier, 
			string fieldName, 
			string fieldType, 
			object initialValue);
	}
}
