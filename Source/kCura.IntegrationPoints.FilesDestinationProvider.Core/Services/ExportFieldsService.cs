using System;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.WinEDDS;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.IntegrationPoints.Contracts.Models;
using FieldType = Relativity.IntegrationPoints.Contracts.Models.FieldType;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
    public class ExportFieldsService : IExportFieldsService
    {
        private readonly IServicesMgr _servicesMgr;

        public ExportFieldsService(IServicesMgr servicesMgr)
        {
            _servicesMgr = servicesMgr;
        }

        public FieldEntry[] GetAllExportableFields(int workspaceArtifactID, int artifactTypeID)
        {
            FieldEntry[] fields = Array.Empty<FieldEntry>();

            using (ISearchService searchService = _servicesMgr.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser))
            {
                DataSet dataSet = searchService.RetrieveAllExportableViewFieldsAsync(workspaceArtifactID, artifactTypeID, string.Empty).GetAwaiter().GetResult()?.Unwrap();
                if (ContainsTable(dataSet))
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
                DataSet defaultViewFieldsDataSet = searchService
                    .RetrieveDefaultViewFieldsForIdListAsync(workspaceArtifactID, artifactTypeID, new[] { viewArtifactID }, isProduction, string.Empty)
                    .GetAwaiter()
                    .GetResult()?
                    .Unwrap();

                if (ContainsTable(allExportableViewFieldsDataSet) && ContainsTable(defaultViewFieldsDataSet))
                {
                    EnumerableRowCollection<DataRow> defaultViewFieldsDataSetRows = defaultViewFieldsDataSet.Tables[0].AsEnumerable();
                    int[] viewFieldIdsNew = defaultViewFieldsDataSetRows.Select(x => int.Parse(x["ArtifactViewFieldID"].ToString())).ToArray();

                    EnumerableRowCollection<DataRow> allExportableFieldsRows = allExportableViewFieldsDataSet.Tables[0].AsEnumerable();
                    fields = allExportableFieldsRows
                        .Where(x =>
                        {
                            int avfId = int.Parse(x["AvfId"].ToString());
                            return viewFieldIdsNew.Contains(avfId);
                        })
                        .Select(CreateFieldEntry)
                        .ToArray();
                }
            }

            return fields;
        }

        public FieldEntry[] GetAllExportableLongTextFields(int workspaceArtifactID, int artifactTypeID)
        {
            FieldEntry[] fields = Array.Empty<FieldEntry>();

            using (ISearchService searchService = _servicesMgr.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser))
            {
                DataSet allExportableViewFieldsDataSet = searchService.RetrieveAllExportableViewFieldsAsync(workspaceArtifactID, artifactTypeID, string.Empty).GetAwaiter().GetResult()?.Unwrap();

                if (ContainsTable(allExportableViewFieldsDataSet))
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

        public ViewFieldInfo[] RetrieveAllExportableViewFields(int workspaceID, int artifactTypeID, string correlationID)
        {
            using (ISearchService searchService = _servicesMgr.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser))
            {
                DataSet dataSet = searchService.RetrieveAllExportableViewFieldsAsync(workspaceID, artifactTypeID, correlationID).GetAwaiter().GetResult()?.Unwrap();

                if (dataSet != null && dataSet.Tables.Count > 0)
                {
                    return dataSet
                        .Tables[0]
                        .AsEnumerable()
                        .Select(dataRow => new ViewFieldInfo(dataRow))
                        .ToArray();
                }

                return null;
            }
        }

        private bool ContainsTable(DataSet dataSet)
        {
            return dataSet != null && dataSet.Tables.Count > 0;
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
    }
}