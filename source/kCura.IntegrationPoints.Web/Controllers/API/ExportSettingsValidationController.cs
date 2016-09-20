using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ExportSettingsValidationController : ApiController
	{
		private readonly IExportSettingsValidationService _validationService;

		public ExportSettingsValidationController(IExportSettingsValidationService validationService)
		{
			_validationService = validationService;
		}

		[HttpPost]
		public HttpResponseMessage ValidateSettings(int workspaceID, IntegrationModel model)
		{
			try
			{
				var validationResult = _validationService.Validate(workspaceID, model);
				return Request.CreateResponse(HttpStatusCode.OK, validationResult);
			}
			catch (Exception)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, "Integration Point settings: Failed to validate export settings");
			}
		}
	}
}