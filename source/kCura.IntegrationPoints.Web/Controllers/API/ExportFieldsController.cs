using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using Newtonsoft.Json;
using Relativity;

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

			var fields = _exportFieldsService.GetAllExportableFields(settings.SourceWorkspaceArtifactId, (int)ArtifactType.Document);

			return Request.CreateResponse(HttpStatusCode.OK, fields);
		}

		[HttpPost]
		public HttpResponseMessage GetAvailableFields(SourceOptions data)
		{
			var settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(data.Options.ToString());

			var viewFields = _exportFieldsService.GetAllViewFields(settings.SourceWorkspaceArtifactId, settings.SavedSearchArtifactId, (int)ArtifactType.Search);
			var allFields = _exportFieldsService.GetAllExportableFields(settings.SourceWorkspaceArtifactId, (int)ArtifactType.Document);

			var fields = viewFields.Where(x => allFields.Any(f => f.FieldIdentifier.Equals(x.FieldIdentifier))).ToArray();

			return Request.CreateResponse(HttpStatusCode.OK, fields);
		}
	}
}