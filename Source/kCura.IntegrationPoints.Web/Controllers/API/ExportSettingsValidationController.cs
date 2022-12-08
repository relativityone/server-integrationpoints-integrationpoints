using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Extensions;
using kCura.IntegrationPoints.Web.Models;
using kCura.IntegrationPoints.Web.Models.Validation;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ExportSettingsValidationController : ApiController
    {
        private readonly IIntegrationPointValidationService _validationService;
        private readonly ICamelCaseSerializer _serializer;

        public ExportSettingsValidationController(IIntegrationPointValidationService validationService, ICamelCaseSerializer serializer)
        {
            _validationService = validationService;
            _serializer = serializer;
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to validate export settings.")]
        public HttpResponseMessage ValidateSettings(int workspaceID, IntegrationPointWebModel webModel)
        {
            ValidationResult validationResult = _validationService.Prevalidate(new IntegrationPointProviderValidationModel(webModel.ToDto(_serializer)));
            var mapper = new ValidationResultMapper();
            ValidationResultDTO validationResultDto = mapper.Map(validationResult);
            return Request.CreateResponse(HttpStatusCode.OK, validationResultDto);
        }
    }
}
