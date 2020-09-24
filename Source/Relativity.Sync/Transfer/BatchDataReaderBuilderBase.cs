﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal abstract class BatchDataReaderBuilderBase : IBatchDataReaderBuilder
	{
		private DataTable _templateDataTable;

		protected IReadOnlyList<FieldInfoDto> _allFields;
		protected readonly IExportDataSanitizer _exportDataSanitizer;
		protected readonly IFieldManager _fieldManager;

		public Action<string, string> ItemLevelErrorHandler { get; set; }

		internal BatchDataReaderBuilderBase(IFieldManager fieldManager, IExportDataSanitizer exportDataSanitizer)
		{
			_fieldManager = fieldManager;
			_exportDataSanitizer = exportDataSanitizer;
		}

		public async Task<IDataReader> BuildAsync(int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch, CancellationToken token)
		{
			if (_allFields == null)
			{
				_allFields = await GetAllFieldsAsync(token).ConfigureAwait(false);
			}

			DataTable templateDataTable = GetTemplateDataTable(_allFields);
			return CreateDataReader(templateDataTable, sourceWorkspaceArtifactId, batch, token);
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

		protected abstract Task<IReadOnlyList<FieldInfoDto>> GetAllFieldsAsync(CancellationToken token);

		protected abstract IDataReader CreateDataReader(DataTable templateDataTable, int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch, CancellationToken token);
	}
}