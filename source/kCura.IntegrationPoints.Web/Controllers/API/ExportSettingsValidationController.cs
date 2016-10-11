using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.Web.Attributes;

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
		[LogApiExceptionFilter(Message = "Unable to validate export settings.")]
		public HttpResponseMessage ValidateSettings(int workspaceID, IntegrationModel model)
		{
			var validationResult = _validationService.Validate(workspaceID, model);
			return Request.CreateResponse(HttpStatusCode.OK, validationResult);
		}
	}
}