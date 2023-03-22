using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ToggleAPIController : ApiController
    {
        private readonly IRipToggleProvider _toggleProvider;

        public ToggleAPIController(IRipToggleProvider toggleProvider)
        {
            _toggleProvider = toggleProvider;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Missing toggle definition.")]
        public HttpResponseMessage Get(string toggleName)
        {
            try
            {
                var isEnabledByName = _toggleProvider.IsEnabledByName(toggleName);
                return Request.CreateResponse(HttpStatusCode.OK, isEnabledByName);
            }
            catch (MissingFeatureException)
            {
                return Request.CreateResponse(HttpStatusCode.OK, false);// Expected behavior for missing toggle
            }
        }
    }
}
