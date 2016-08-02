using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
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
			try
			{
				var settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(data.Options.ToString());

				var fields = _exportFieldsService.GetAllExportableFields(settings.SourceWorkspaceArtifactId, (int)ArtifactType.Document);
				
				return Request.CreateResponse(HttpStatusCode.OK, fields);
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
			}
		}

		[HttpPost]
		public HttpResponseMessage GetAvailableFields(SourceOptions data)
		{
			try
			{
				var settings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(data.Options.ToString());

				// TODO: isProduction flag should be set accordingly to configuration i.e. settings.ExportType == ExportType.Production, for now it is always Saved Search
				var fields = _exportFieldsService.GetDefaultViewFields(settings.SourceWorkspaceArtifactId, settings.SavedSearchArtifactId, (int)ArtifactType.Search, false);

				return Request.CreateResponse(HttpStatusCode.OK, fields);
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
			}
		}
	}
}