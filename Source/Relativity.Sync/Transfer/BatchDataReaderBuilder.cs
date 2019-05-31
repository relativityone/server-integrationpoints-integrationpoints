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

		public BatchDataReaderBuilder(IFieldManager fieldManager)
		{
			_fieldManager = fieldManager;
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
				foreach (RelativityObjectSlim obj in batch)
				{
					object[] row = BuildRow(specialFieldBuildersDictionary, _allFields, obj);
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
			DataColumn[] columns = fields.Select(x => new DataColumn(x.DestinationFieldName, typeof(object))).ToArray();
			return columns;
		}

		private static object[] BuildRow(IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuilders, IReadOnlyList<FieldInfoDto> allFields, RelativityObjectSlim obj)
		{
			object[] result = new object[allFields.Count];

			for (int i = 0; i < allFields.Count; i++)
			{
				FieldInfoDto field = allFields[i];
				if (field.SpecialFieldType != SpecialFieldType.None)
				{
					if (!specialFieldBuilders.ContainsKey(field.SpecialFieldType))
					{
						throw new SourceDataReaderException($"No special field row value builder found for special field type {nameof(SpecialFieldType)}.{field.SpecialFieldType}");
					}

					object initialFieldValue = field.IsDocumentField ? obj.Values[field.DocumentFieldIndex] : null;
					result[i] = specialFieldBuilders[field.SpecialFieldType].BuildRowValue(field, obj, initialFieldValue);
				}
				else
				{
					result[i] = obj.Values[field.DocumentFieldIndex];
				}
			}

			return result;
		}
	}
}
