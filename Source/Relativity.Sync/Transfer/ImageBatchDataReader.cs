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
	internal class ImageBatchDataReader : BatchDataReaderBase
	{
		public ImageBatchDataReader(
			DataTable templateDataTable,
			int sourceWorkspaceArtifactId,
			RelativityObjectSlim[] batch,
			IReadOnlyList<FieldInfoDto> allFields,
			IFieldManager fieldManager,
			IExportDataSanitizer exportDataSanitizer,
			Action<string, string> itemLevelErrorHandler,
			CancellationToken cancellationToken)
		: base(templateDataTable, sourceWorkspaceArtifactId, batch, allFields, fieldManager, exportDataSanitizer, itemLevelErrorHandler, cancellationToken)
		{
			
		}

		protected override IEnumerable<object[]> GetBatchEnumerable()
		{
			if (_batch != null && _batch.Any())
			{
				IDictionary<SpecialFieldType, IImageSpecialFieldRowValuesBuilder> specialFieldBuildersDictionary = CreateSpecialFieldRowValuesBuilders();

				foreach (RelativityObjectSlim batchItem in _batch)
				{
					IEnumerable<object[]> rows;

					try
					{
						rows = BuildRows(specialFieldBuildersDictionary, batchItem);
					}
					catch (SyncItemLevelErrorException ex)
					{
						_itemLevelErrorHandler(batchItem.ArtifactID.ToString(CultureInfo.InvariantCulture), ex.GetExceptionMessages());
						continue;
					}

					foreach (object[] row in rows)
					{
						yield return row;
					}
				}
			}
		}

		private IDictionary<SpecialFieldType, IImageSpecialFieldRowValuesBuilder> CreateSpecialFieldRowValuesBuilders()
		{
			int[] documentArtifactIds = _batch.Select(obj => obj.ArtifactID).ToArray();
			return _fieldManager.CreateImageSpecialFieldRowValueBuildersAsync(_sourceWorkspaceArtifactId, documentArtifactIds).GetAwaiter().GetResult();
		}

		private IEnumerable<object[]> BuildRows(IDictionary<SpecialFieldType, IImageSpecialFieldRowValuesBuilder> specialFieldBuilders, RelativityObjectSlim batchItem)
		{
			string itemIdentifier = batchItem.Values[IdentifierField.DocumentFieldIndex].ToString();

			Dictionary<SpecialFieldType, object[]> specialFieldsValues = _allFields
				.Where(x => x.SpecialFieldType != SpecialFieldType.None)
				.ToDictionary(x => x.SpecialFieldType,
					field => BuildSpecialFieldValue(specialFieldBuilders, batchItem, field));

			int documentImageCount = specialFieldsValues.Values.First().Length;

			for (int imageIndex = 0; imageIndex < documentImageCount; imageIndex++)
			{
				object[] row = new object[_allFields.Count];

				for (int fieldIndex = 0; fieldIndex < _allFields.Count; fieldIndex++)
				{
					FieldInfoDto field = _allFields[fieldIndex];
					if (field.SpecialFieldType != SpecialFieldType.None)
					{
						row[fieldIndex] = specialFieldsValues[field.SpecialFieldType][imageIndex];
					}
					else
					{
						object initialValue = batchItem.Values[field.DocumentFieldIndex];
						row[fieldIndex] = SanitizeFieldIfNeeded(IdentifierField.SourceFieldName, itemIdentifier, field, initialValue);
					}
				}

				yield return row;
			}
		}

		private static object[] BuildSpecialFieldValue(IDictionary<SpecialFieldType, IImageSpecialFieldRowValuesBuilder> specialFieldBuilders, RelativityObjectSlim batchItem, FieldInfoDto fieldInfo)
		{
			if (!specialFieldBuilders.ContainsKey(fieldInfo.SpecialFieldType))
			{
				throw new SourceDataReaderException($"No special field row value builder found for special field type {nameof(SpecialFieldType)}.{fieldInfo.SpecialFieldType}");
			}

			return specialFieldBuilders[fieldInfo.SpecialFieldType].BuildRowsValues(fieldInfo, batchItem).ToArray();
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