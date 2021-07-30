using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Transfer
{
	internal class NativeBatchDataReader : BatchDataReaderBase
	{
		public NativeBatchDataReader(
			DataTable templateDataTable,
			int sourceWorkspaceArtifactId,
			RelativityObjectSlim[] batch,
			IReadOnlyList<FieldInfoDto> allFields,
			IFieldManager fieldManager,
			IExportDataSanitizer exportDataSanitizer,
			Action<string, string> itemLevelErrorHandler,
			CancellationToken cancellationToken,
			ISyncLog logger)
			: base(templateDataTable, sourceWorkspaceArtifactId, batch, allFields, fieldManager, exportDataSanitizer,
				itemLevelErrorHandler, cancellationToken, logger)
		{
			CanCancel = true;
		}

		protected override IEnumerable<object[]> GetBatchEnumerable()
		{
			if (_batch != null && _batch.Any())
			{
				IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder> specialFieldBuildersDictionary = CreateSpecialFieldRowValuesBuilders();

				foreach (RelativityObjectSlim batchItem in _batch)
				{
					object[] row;
					try
					{
						row = BuildRow(specialFieldBuildersDictionary, batchItem);
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
		
		private IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder> CreateSpecialFieldRowValuesBuilders()
		{
			// TODO REL-367580: [PERFORMANCE] It looks like we are creating this collection (Int32 x Batch Size) unnecessary.
			//                  We could pass IEnumerable further, but currently the whole stack is expecting ICollection so the change is to deep for this issue.
			int[] documentArtifactIds = _batch.Select(obj => obj.ArtifactID).ToArray();

			return _fieldManager.CreateNativeSpecialFieldRowValueBuildersAsync(_sourceWorkspaceArtifactId, documentArtifactIds).GetAwaiter().GetResult();
		}

		private object[] BuildRow(IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder> specialFieldBuilders, RelativityObjectSlim batchItem)
		{
			object[] result = new object[_allFields.Count];

			string itemIdentifier = batchItem.Values[IdentifierField.DocumentFieldIndex].ToString();

			for (int i = 0; i < _allFields.Count; i++)
			{
				FieldInfoDto field = _allFields[i];
				if (field.SpecialFieldType != SpecialFieldType.None)
				{
					object specialValue = BuildSpecialFieldValue(specialFieldBuilders, batchItem, field);
					result[i] = specialValue;
				}
				else
				{
					object initialValue = batchItem.Values[field.DocumentFieldIndex];
					result[i] = SanitizeFieldIfNeeded(IdentifierField.SourceFieldName, itemIdentifier, field, initialValue);
				}
			}

			return result;
		}

		private static object BuildSpecialFieldValue(IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder> specialFieldBuilders, RelativityObjectSlim batchItem, FieldInfoDto fieldInfo)
		{
			if (!specialFieldBuilders.ContainsKey(fieldInfo.SpecialFieldType))
			{
				throw new SourceDataReaderException($"No special field row value builder found for special field type {nameof(SpecialFieldType)}.{fieldInfo.SpecialFieldType}");
			}
			object initialFieldValue = fieldInfo.IsDocumentField ? batchItem.Values[fieldInfo.DocumentFieldIndex] : null;

			return specialFieldBuilders[fieldInfo.SpecialFieldType].BuildRowValue(fieldInfo, batchItem, initialFieldValue);
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