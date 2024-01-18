using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
    internal interface IExportDataSanitizer
    {
        bool ShouldSanitize(RelativityDataType dataType);

        Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, FieldInfoDto field, object initialValue);
    }
}
