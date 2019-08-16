using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal interface IExportFieldSanitizer
	{
		string SupportedType { get; }

		Task<object> SanitizeAsync(
			int workspaceArtifactID, 
			string itemIdentifierSourceFieldName, 
			string itemIdentifier,
			string sanitizingSourceFieldName, 
			object initialValue);
	}
}
