using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.DataStructures;
using Newtonsoft.Json;
using Relativity.IntegrationPoints.Contracts.Models;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ExportFieldsController : ApiController
    {
        private readonly IExportFieldsService _exportFieldsService;

        public ExportFieldsController(IExportFieldsService exportFieldsService)
        {
            _exportFieldsService = exportFieldsService;
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve list of exportable fields.")]
        public HttpResponseMessage GetExportableFields(ExtendedSourceOptions data)
        {
            var settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(data.Options.ToString());

            FieldEntry[] fields = _exportFieldsService.GetAllExportableFields(settings.SourceWorkspaceArtifactId, data.TransferredArtifactTypeId);
                
            return Request.CreateResponse(HttpStatusCode.OK, SortFields(fields));
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve list of available fields.")]
        public HttpResponseMessage GetAvailableFields(ExtendedSourceOptions data)
        {
            var settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(data.Options.ToString());

            ExportSettings.ExportType exportType;
            if (!Enum.TryParse(settings.ExportType, out exportType))
            {
                throw new InvalidEnumArgumentException(Constants.INVALID_EXPORT_TYPE_ERROR);
            }

            int artifactId = RetrieveArtifactIdBasedOnExportType(exportType, settings);

            FieldEntry[] fields = _exportFieldsService.GetDefaultViewFields(settings.SourceWorkspaceArtifactId, artifactId, data.TransferredArtifactTypeId,
                exportType == ExportSettings.ExportType.ProductionSet);

            return Request.CreateResponse(HttpStatusCode.OK, SortFields(fields));
        }

        private int RetrieveArtifactIdBasedOnExportType(ExportSettings.ExportType exportType, ExportUsingSavedSearchSettings settings)
        {
            int artifactId;
            if (exportType == ExportSettings.ExportType.ProductionSet)
            {
                artifactId = settings.ProductionId;
            }
            else if ((exportType == ExportSettings.ExportType.Folder) || (exportType == ExportSettings.ExportType.FolderAndSubfolders))
            {
                artifactId = settings.ViewId;
            }
            else
            {
                artifactId = settings.SavedSearchArtifactId;
            }
            return artifactId;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve export long text fields.")]
        public HttpResponseMessage GetExportableLongTextFields(int sourceWorkspaceArtifactId, int artifactTypeId)
        {
            FieldEntry[] fields = _exportFieldsService.GetAllExportableLongTextFields(sourceWorkspaceArtifactId, artifactTypeId);

            return Request.CreateResponse(HttpStatusCode.OK, SortFields(fields));
        }

        private IOrderedEnumerable<FieldEntry> SortFields(FieldEntry[] fieldEntries)
        {
            return fieldEntries.OrderBy(x => x.DisplayName);
        }
    }
}