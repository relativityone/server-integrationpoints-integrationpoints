using System.Collections.Generic;
using System.Data;
using System.Linq;
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
		private readonly IFieldManager _fieldManager;

		public SourceWorkspaceDataTableBuilder(IFieldManager fieldManager)
		{
			_fieldManager = fieldManager;
		}

		public async Task<DataTable> BuildAsync(int sourceWorkspaceArtifactId, IEnumerable<FieldMap> fieldMaps, RelativityObjectSlim[] batch)
		{
			if (batch == null || !batch.Any())
			{
				return new DataTable();
			}

			List<FieldInfo> allFields = _fieldManager.GetAllFields(fieldMaps).ToList();

			DataTable dataTable = CreateDataTable(allFields);

			Dictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuildersDictionary = await CreateSpecialFieldRowValuesBuilders(sourceWorkspaceArtifactId, batch).ConfigureAwait(false);

			foreach (RelativityObjectSlim obj in batch)
			{
				object[] row = BuildRow(specialFieldBuildersDictionary, allFields, obj);
				dataTable.Rows.Add(row);
			}

			return dataTable;
		}

		private async Task<Dictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>> CreateSpecialFieldRowValuesBuilders(int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch)
		{
			IEnumerable<Task<ISpecialFieldRowValuesBuilder>> specialFieldRowValueBuilderTasks =
				_fieldManager.SpecialFieldBuilders.Select(async b => await b.GetRowValuesBuilderAsync(sourceWorkspaceArtifactId, batch).ConfigureAwait(false));

			ISpecialFieldRowValuesBuilder[] specialFieldRowValueBuilders = await Task.WhenAll(specialFieldRowValueBuilderTasks).ConfigureAwait(false);
			Dictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuildersDictionary = new Dictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>();
			foreach (var builder in specialFieldRowValueBuilders)
			{
				foreach (var specialFieldType in builder.AllowedSpecialFieldTypes)
				{
					specialFieldBuildersDictionary[specialFieldType] = builder;
				}
			}

			return specialFieldBuildersDictionary;
		}

		private DataTable CreateDataTable(List<FieldInfo> allFields)
		{
			DataTable data = new DataTable();

			DataColumn[] columns = BuildColumns(allFields);
			data.Columns.AddRange(columns);
			return data;
		}

		private static DataColumn[] BuildColumns(IEnumerable<FieldInfo> fields)
		{
			DataColumn[] columns = fields.Select(x => new DataColumn(x.DisplayName)).ToArray();
			return columns;
		}

		private object[] BuildRow(IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuilders, IList<FieldInfo> fields, RelativityObjectSlim obj)
		{
			object[] result = new object[fields.Count];

			for (int i = 0; i < fields.Count; i++)
			{
				FieldInfo field = fields[i];
				if (field.SpecialFieldType != SpecialFieldType.None)
				{
					object initialFieldValue = field.IsDocumentField ? obj.Values[field.DocumentFieldIndex] : null;
					result[i] = specialFieldBuilders[field.SpecialFieldType].BuildRowValues(field, obj, initialFieldValue);
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
