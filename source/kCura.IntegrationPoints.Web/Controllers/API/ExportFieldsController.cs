using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;

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
		public HttpResponseMessage GetExportableFields(SourceOptions data)
		{
			var settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(data.Options.ToString());

			var fields = _exportFieldsService.GetAllExportableFields(settings.SourceWorkspaceArtifactId, 10); // artifactTypeId = 10 - Document

			return Request.CreateResponse(HttpStatusCode.OK, fields);
		}

		[HttpPost]
		public HttpResponseMessage GetAvailableFields(SourceOptions data)
		{
			var settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(data.Options.ToString());

			var viewFields = _exportFieldsService.GetAllViewFields(settings.SourceWorkspaceArtifactId, settings.SavedSearchArtifactId, 15); // artifactTypeId = 15 - Saved Search
			var allFields = _exportFieldsService.GetAllExportableFields(settings.SourceWorkspaceArtifactId, 10); // artifactTypeId = 10 - Document

			var fields = viewFields.Where(x => allFields.Any(f => f.FieldIdentifier.Equals(x.FieldIdentifier))).ToArray();

			return Request.CreateResponse(HttpStatusCode.OK, fields);
		}
	}
}