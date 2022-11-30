using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Transfer
{
    internal class NonDocumentBatchDataReader : BatchDataReaderBase
    {
        public NonDocumentBatchDataReader(
            DataTable templateDataTable,
            int sourceWorkspaceArtifactId,
            RelativityObjectSlim[] batch,
            IReadOnlyList<FieldInfoDto> allFields,
            IFieldManager fieldManager,
            IExportDataSanitizer exportDataSanitizer,
            Action<string, string> itemLevelErrorHandler,
            CancellationToken cancellationToken,
            IAPILog logger)
            : base(templateDataTable, sourceWorkspaceArtifactId, batch, allFields, fieldManager, exportDataSanitizer,
                itemLevelErrorHandler, cancellationToken, logger)
        {
            CanCancel = true;
        }

        protected override IEnumerable<object[]> GetBatchEnumerable()
        {
            if (_batch != null && _batch.Any())
            {
                foreach (RelativityObjectSlim batchItem in _batch)
                {
                    object[] row;
                    try
                    {
                        row = BuildRow(batchItem);
                    }
                    catch (SyncItemLevelErrorException ex)
                    {
                        _itemLevelErrorHandler(batchItem.ArtifactID.ToString(CultureInfo.InvariantCulture), ex.GetExceptionMessages());
                        continue;
                    }

                    yield return row;
                }
            }
        }

        private object[] BuildRow(RelativityObjectSlim batchItem)
        {
            object[] result = new object[_allFields.Count];

            string itemIdentifier = batchItem.Values[IdentifierField.DocumentFieldIndex].ToString();

            for (int i = 0; i < _allFields.Count; i++)
            {
                FieldInfoDto field = _allFields[i];
                object initialValue = batchItem.Values[field.DocumentFieldIndex];
                result[i] = SanitizeFieldIfNeeded(IdentifierField.SourceFieldName, itemIdentifier, field, initialValue);
            }

            return result;
        }

        private object SanitizeFieldIfNeeded(string itemIdentifierFieldName, string itemIdentifier, FieldInfoDto field, object initialValue)
        {
            object sanitizedValue = initialValue;
            if (_exportDataSanitizer.ShouldSanitize(field.RelativityDataType))
            {
                try
                {
                    sanitizedValue = _exportDataSanitizer.SanitizeAsync(_sourceWorkspaceArtifactId, itemIdentifierFieldName, itemIdentifier, field, initialValue).GetAwaiter().GetResult();
                }
                catch (InvalidExportFieldValueException ex)
                {
                    throw new SyncItemLevelErrorException($"Could not sanitize field of type: {field.RelativityDataType}", ex);
                }
            }

            return sanitizedValue;
        }
    }
}
