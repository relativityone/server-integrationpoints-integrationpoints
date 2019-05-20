using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Creates a single <see cref="DataTable"/> out of several sources of information based on the given schema.
	/// </summary>
	internal sealed class SourceWorkspaceDataTableBuilder : ISourceWorkspaceDataTableBuilder
	{
		private DataTable _dataTable;
		private readonly IFieldManager _fieldManager;

		public SourceWorkspaceDataTableBuilder(IFieldManager fieldManager)
		{
			_fieldManager = fieldManager;
		}

		public async Task<DataTable> BuildAsync(int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch, CancellationToken token)
		{
			if (batch == null || !batch.Any())
			{
				return new DataTable();
			}

			IList<FieldInfoDto> allFields = await _fieldManager.GetAllFieldsAsync(token).ConfigureAwait(false);

			DataTable dataTable = GetEmptyDataTable(allFields);

			IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuildersDictionary = await CreateSpecialFieldRowValuesBuilders(sourceWorkspaceArtifactId, batch).ConfigureAwait(false);

			foreach (RelativityObjectSlim obj in batch)
			{
				object[] row = BuildRow(specialFieldBuildersDictionary, allFields, obj);
				dataTable.Rows.Add(row);
			}

			return dataTable;
		}

		private async Task<IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>> CreateSpecialFieldRowValuesBuilders(int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch)
		{
			IEnumerable<int> documentArtifactIds = batch.Select(obj => obj.ArtifactID);

			return await _fieldManager.CreateSpecialFieldRowValueBuildersAsync(sourceWorkspaceArtifactId, documentArtifactIds).ConfigureAwait(false);
		}

		private DataTable GetEmptyDataTable(IEnumerable<FieldInfoDto> allFields)
		{
			if (_dataTable == null)
			{
				_dataTable = CreateDataTable(allFields);
			}
			return _dataTable.Clone();
		}

		private static DataTable CreateDataTable(IEnumerable<FieldInfoDto> allFields)
		{
			var dataTable = new DataTable();

			DataColumn[] columns = BuildColumns(allFields);
			dataTable.Columns.AddRange(columns);
			return dataTable;
		}

		private static DataColumn[] BuildColumns(IEnumerable<FieldInfoDto> fields)
		{
			DataColumn[] columns = fields.Select(x => new DataColumn(x.DisplayName, typeof(object))).ToArray();
			return columns;
		}

		private object[] BuildRow(IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuilders, IList<FieldInfoDto> fields, RelativityObjectSlim obj)
		{
			object[] result = new object[fields.Count];

			for (int i = 0; i < fields.Count; i++)
			{
				FieldInfoDto field = fields[i];
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
