using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Creates a single <see cref="DataTable"/> out of several sources of information based on the given schema.
	/// </summary>
	internal sealed class BatchDataReaderBuilder : IBatchDataReaderBuilder
	{
		private DataTable _templateDataTable;
		private IReadOnlyList<FieldInfoDto> _allFields;
		private readonly IFieldManager _fieldManager;
		private readonly IExportDataSanitizer _exportDataSanitizer;

		public BatchDataReaderBuilder(IFieldManager fieldManager, IExportDataSanitizer exportDataSanitizer)
		{
			_fieldManager = fieldManager;
			_exportDataSanitizer = exportDataSanitizer;
		}

		public async Task<IDataReader> BuildAsync(int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch, CancellationToken token)
		{
			if (_allFields == null)
			{
				_allFields = await _fieldManager.GetAllFieldsAsync(token).ConfigureAwait(false);
			}

			DataTable dataTable = CreateEmptyDataTable(_allFields);
			if (batch != null && batch.Any())
			{

				IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuildersDictionary =
					await CreateSpecialFieldRowValuesBuildersAsync(sourceWorkspaceArtifactId, batch).ConfigureAwait(false);
				foreach (RelativityObjectSlim item in batch)
				{
					object[] row = await BuildRowAsync(sourceWorkspaceArtifactId, specialFieldBuildersDictionary, item, token).ConfigureAwait(false);
					dataTable.Rows.Add(row);
				}
			}
			return dataTable.CreateDataReader();
		}

		private async Task<IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>> CreateSpecialFieldRowValuesBuildersAsync(int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch)
		{
			ICollection<int> documentArtifactIds = batch.Select(obj => obj.ArtifactID).ToList();

			return await _fieldManager.CreateSpecialFieldRowValueBuildersAsync(sourceWorkspaceArtifactId, documentArtifactIds).ConfigureAwait(false);
		}

		private DataTable CreateEmptyDataTable(IEnumerable<FieldInfoDto> allFields)
		{
			if (_templateDataTable == null)
			{
				_templateDataTable = CreateTemplateDataTable(allFields);
			}
			return _templateDataTable.Clone();
		}

		private static DataTable CreateTemplateDataTable(IEnumerable<FieldInfoDto> allFields)
		{
			var dataTable = new DataTable();

			DataColumn[] columns = BuildColumns(allFields);
			dataTable.Columns.AddRange(columns);
			return dataTable;
		}

		private static DataColumn[] BuildColumns(IEnumerable<FieldInfoDto> fields)
		{
			DataColumn[] columns = fields.Select(x =>
			{
				Type dataType = x.RelativityDataType == RelativityDataType.LongText ? typeof(object) : typeof(string);
				return new DataColumn(x.DestinationFieldName, dataType);
			}).ToArray();
			return columns;
		}

		private async Task<object[]> BuildRowAsync(int sourceWorkspaceArtifactId,
			IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuilders,
			RelativityObjectSlim batchItem,
			CancellationToken token)
		{
			object[] result = new object[_allFields.Count];

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
					FieldInfoDto identifierField = await _fieldManager.GetObjectIdentifierFieldAsync(token).ConfigureAwait(false);
					string itemIdentifier = batchItem.Values[identifierField.DocumentFieldIndex].ToString();
					object initialValue = batchItem.Values[field.DocumentFieldIndex];
					result[i] = await SanitizeFieldIfNeededAsync(sourceWorkspaceArtifactId, identifierField.SourceFieldName, itemIdentifier, field, initialValue).ConfigureAwait(false);
				}
			}

			return result;
		}

		private static object BuildSpecialFieldValue(IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuilders, RelativityObjectSlim batchItem, FieldInfoDto fieldInfo)
		{
			if (!specialFieldBuilders.ContainsKey(fieldInfo.SpecialFieldType))
			{
				throw new SourceDataReaderException($"No special field row value builder found for special field type {nameof(SpecialFieldType)}.{fieldInfo.SpecialFieldType}");
			}
			object initialFieldValue = fieldInfo.IsDocumentField ? batchItem.Values[fieldInfo.DocumentFieldIndex] : null;

			return specialFieldBuilders[fieldInfo.SpecialFieldType].BuildRowValue(fieldInfo, batchItem, initialFieldValue);
		}

		private async Task<object> SanitizeFieldIfNeededAsync(int sourceWorkspaceArtifactId, string itemIdentifierFieldName, string itemIdentifier, FieldInfoDto field, object initialValue)
		{
			object sanitizedValue = initialValue;
			if (_exportDataSanitizer.ShouldSanitize(field.RelativityDataType))
			{
				sanitizedValue = await _exportDataSanitizer.SanitizeAsync(sourceWorkspaceArtifactId, itemIdentifierFieldName, itemIdentifier, field, initialValue).ConfigureAwait(false);
			}

			return sanitizedValue;
		}
	}
}
