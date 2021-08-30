using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.IntegrationPoints.Contracts.Models;
using FieldType = Relativity.IntegrationPoints.Contracts.Models.FieldType;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ExportFieldsService : IExportFieldsService
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly IServiceManagerProvider _serviceManagerProvider;

		public ExportFieldsService(IServicesMgr servicesMgr, IServiceManagerProvider serviceManagerProvider)
		{
			_servicesMgr = servicesMgr;
			_serviceManagerProvider = serviceManagerProvider;
		}

		public FieldEntry[] GetAllExportableFields(int workspaceArtifactID, int artifactTypeID)
		{
			FieldEntry[] fields = Array.Empty<FieldEntry>();

			using (ISearchService searchService = _servicesMgr.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser))
			{
				DataSet dataSet = searchService.RetrieveAllExportableViewFieldsAsync(workspaceArtifactID, artifactTypeID, string.Empty).GetAwaiter().GetResult()?.Unwrap();
				if (dataSet != null && dataSet.Tables.Count > 0)
				{
					EnumerableRowCollection<DataRow> rows = dataSet.Tables[0].AsEnumerable();
					fields = rows.Select(CreateFieldEntry).ToArray();
				}
			}

			return fields;
		}

		public FieldEntry[] GetDefaultViewFields(int workspaceArtifactID, int viewArtifactID, int artifactTypeID, bool isProduction)
		{
			FieldEntry[] fields = Array.Empty<FieldEntry>();

			using (ISearchService searchService = _servicesMgr.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser))
			{
				DataSet allExportableViewFieldsDataSet = searchService.RetrieveAllExportableViewFieldsAsync(workspaceArtifactID, artifactTypeID, string.Empty).GetAwaiter().GetResult()?.Unwrap();

				if (allExportableViewFieldsDataSet != null && allExportableViewFieldsDataSet.Tables.Count > 0)
				{
					EnumerableRowCollection<DataRow> allExportableViewFieldsRows = allExportableViewFieldsDataSet.Tables[0].AsEnumerable();
					int[] ids = allExportableViewFieldsRows.Select(x => int.Parse(x["FieldArtifactId"].ToString())).ToArray();
					// TODO
					DataSet defaultViewFieldsDataSet = searchService.RetrieveDefaultViewFieldsForIdListAsync(workspaceArtifactID, artifactTypeID, ids, isProduction, string.Empty).GetAwaiter().GetResult()?.Unwrap();
				}
			}

			ISearchManager searchManager = _serviceManagerProvider.Create<ISearchManager, SearchManagerFactory>();

			IEnumerable<int> viewFieldIds = searchManager.RetrieveDefaultViewFieldIds(workspaceArtifactID, viewArtifactID, artifactTypeID, isProduction);

			return searchManager.RetrieveAllExportableViewFields(workspaceArtifactID, artifactTypeID)
				.Where(x => viewFieldIds.Contains(x.AvfId))
				.Select(x => new FieldEntry
				{
					DisplayName = x.DisplayName,
					FieldIdentifier = x.AvfId.ToString(),
					FieldType = ConvertFromExFieldType(x.FieldType),
					IsIdentifier = x.Category == global::Relativity.DataExchange.Service.FieldCategory.Identifier,
					IsRequired = false
				}).ToArray();

		}

		public FieldEntry[] GetAllExportableLongTextFields(int workspaceArtifactID, int artifactTypeID)
		{
			FieldEntry[] fields = Array.Empty<FieldEntry>();

			using (ISearchService searchService = _servicesMgr.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser))
			{
				DataSet allExportableViewFieldsDataSet = searchService.RetrieveAllExportableViewFieldsAsync(workspaceArtifactID, artifactTypeID, string.Empty).GetAwaiter().GetResult()?.Unwrap();

				if (allExportableViewFieldsDataSet != null && allExportableViewFieldsDataSet.Tables.Count > 0)
				{
					EnumerableRowCollection<DataRow> rows = allExportableViewFieldsDataSet.Tables[0].AsEnumerable();
					fields = rows
						.Where(x =>
						{
							global::Relativity.DataExchange.Service.FieldType fieldType = GetFieldType(x["FieldTypeID"]);
							return fieldType == global::Relativity.DataExchange.Service.FieldType.Text || fieldType == global::Relativity.DataExchange.Service.FieldType.OffTableText;
						})
						.Select(CreateFieldEntry)
						.ToArray();
				}
			}

			return fields;
		}

		private FieldEntry CreateFieldEntry(DataRow dataRow)
		{
			return new FieldEntry()
			{
				DisplayName = dataRow["DisplayName"].ToString(),
				FieldIdentifier = dataRow["AvfId"].ToString(),
				FieldType = ConvertFieldType(dataRow["FieldTypeID"]),
				IsIdentifier = IsIdentifier(dataRow["FieldCategoryID"]),
				IsRequired = false
			};
		}

		private FieldType ConvertFieldType(object value)
		{
			global::Relativity.DataExchange.Service.FieldType fieldType = GetFieldType(value);
			return fieldType == global::Relativity.DataExchange.Service.FieldType.File ? FieldType.File : FieldType.String;
		}

		private global::Relativity.DataExchange.Service.FieldType GetFieldType(object value)
		{
			int fieldTypeId = int.Parse(value.ToString());
			global::Relativity.DataExchange.Service.FieldType fieldType = (global::Relativity.DataExchange.Service.FieldType)fieldTypeId;
			return fieldType;
		}

		private bool IsIdentifier(object value)
		{
			int fieldCategoryId = int.Parse(value.ToString());
			global::Relativity.DataExchange.Service.FieldCategory fieldCategory = (global::Relativity.DataExchange.Service.FieldCategory)fieldCategoryId;
			return fieldCategory == global::Relativity.DataExchange.Service.FieldCategory.Identifier;
		}

		private FieldType ConvertFromExFieldType(global::Relativity.DataExchange.Service.FieldType fieldType)
		{
			return fieldType == global::Relativity.DataExchange.Service.FieldType.File ? FieldType.File : FieldType.String;
		}
	}
}