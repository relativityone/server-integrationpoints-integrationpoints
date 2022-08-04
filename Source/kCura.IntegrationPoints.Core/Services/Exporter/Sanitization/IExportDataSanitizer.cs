using System.Threading.Tasks;
using Relativity;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    public interface IExportDataSanitizer
    {
        bool ShouldSanitize(FieldTypeHelper.FieldType fieldType);
        Task<object> SanitizeAsync(
            int workspaceArtifactID, 
            string itemIdentifierSourceFieldName, 
            string itemIdentifier, 
            string fieldName,
            FieldTypeHelper.FieldType fieldType, 
            object initialValue);
    }
}
