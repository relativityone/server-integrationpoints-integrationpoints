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

		public Action<string, string> ItemLevelErrorHandler { get; set; }

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

			DataTable templateDataTable = GetTemplateDataTable(_allFields);

			return new BatchDataReader(templateDataTable, sourceWorkspaceArtifactId, batch, _allFields, _fieldManager, _exportDataSanitizer, ItemLevelErrorHandler, token);
		}

		private DataTable GetTemplateDataTable(IEnumerable<FieldInfoDto> allFields)
		{
			if (_templateDataTable == null)
			{
				_templateDataTable = CreateTemplateDataTable(allFields);
			}

			return _templateDataTable;
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
	}
}
