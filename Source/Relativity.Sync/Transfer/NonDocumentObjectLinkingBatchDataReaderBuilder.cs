using Relativity.API;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    /// <summary>
    /// Creates a single <see cref="IDataReader"/> for non document objects, out of several sources of information based on the given schema.
    /// </summary>
    internal sealed class NonDocumentObjectLinkingBatchDataReaderBuilder : BatchDataReaderBuilderBase
    {
        public NonDocumentObjectLinkingBatchDataReaderBuilder(IFieldManager fieldManager, IExportDataSanitizer exportDataSanitizer, IAPILog logger)
            : base(fieldManager, exportDataSanitizer, logger)
        {
        }

        protected override Task<IReadOnlyList<FieldInfoDto>> GetAllFieldsAsync(CancellationToken token)
        {
            return _fieldManager.GetMappedFieldsNonDocumentForLinksAsync(token);
        }

        protected override Task<IBatchDataReader> CreateDataReaderAsync(DataTable templateDataTable, int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch, CancellationToken token)
        {
            return Task.FromResult((IBatchDataReader) new NonDocumentBatchDataReader(templateDataTable, sourceWorkspaceArtifactId, batch, _allFields, _fieldManager, _exportDataSanitizer, ItemLevelErrorHandler, token, _logger));
        }
    }
}
