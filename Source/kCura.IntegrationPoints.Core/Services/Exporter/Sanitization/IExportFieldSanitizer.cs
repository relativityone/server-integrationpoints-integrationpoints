using System.Threading.Tasks;
using Relativity;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    internal interface IExportFieldSanitizer
    {
        FieldTypeHelper.FieldType SupportedType { get; }

        Task<object> SanitizeAsync(
            int workspaceArtifactID,
            string itemIdentifierSourceFieldName,
            string itemIdentifier,
            string sanitizingSourceFieldName,
            object initialValue);
    }
}
