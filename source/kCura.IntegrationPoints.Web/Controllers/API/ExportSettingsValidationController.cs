using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ExportSettingsValidationController : ApiController
	{
		private readonly IIntegrationPointValidationService _validationService;

		public ExportSettingsValidationController(IIntegrationPointValidationService validationService)
		{
			_validationService = validationService;
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to validate export settings.")]
		public HttpResponseMessage ValidateSettings(int workspaceID, IntegrationModel model)
		{
			var validationResult = _validationService.PreValidate(model);
			return Request.CreateResponse(HttpStatusCode.OK, validationResult);
		}
	}
}