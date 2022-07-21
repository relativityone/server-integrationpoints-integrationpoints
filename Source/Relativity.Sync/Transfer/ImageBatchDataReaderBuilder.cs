using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    /// <summary>
    /// Creates a single <see cref="IDataReader"/> for images, out of several sources of information based on the given schema.
    /// </summary>
    internal sealed class ImageBatchDataReaderBuilder : BatchDataReaderBuilderBase
    {
        public ImageBatchDataReaderBuilder(IFieldManager fieldManager, IExportDataSanitizer exportDataSanitizer, IAPILog logger)
            : base(fieldManager, exportDataSanitizer, logger)
        {
        }

        protected override Task<IReadOnlyList<FieldInfoDto>> GetAllFieldsAsync(CancellationToken token)
        {
            return _fieldManager.GetImageAllFieldsAsync(token);
        }

        protected override async Task<IBatchDataReader> CreateDataReaderAsync(DataTable templateDataTable, int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch, CancellationToken token)
        {
            FieldInfoDto identifierField = await _fieldManager.GetObjectIdentifierFieldAsync(token).ConfigureAwait(false);
            return new ImageBatchDataReader(templateDataTable, sourceWorkspaceArtifactId, batch, _allFields, _fieldManager, _exportDataSanitizer, ItemLevelErrorHandler, identifierField.DocumentFieldIndex, token, _logger);
        }
    }
}
