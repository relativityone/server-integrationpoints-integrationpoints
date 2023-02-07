using System.Web.Http;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ConsoleStateController : ApiController
    {
        private readonly IButtonStateBuilder _buttonStateBuilder;

        public ConsoleStateController(IButtonStateBuilder buttonStateBuilder)
        {
            _buttonStateBuilder = buttonStateBuilder;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to get ConsoleState")]
        public IHttpActionResult GetConsoleState(int workspaceId, int integrationPointArtifactId)
        {
            ButtonStateDTO buttonState = _buttonStateBuilder.CreateButtonState(workspaceId, integrationPointArtifactId);
            return Ok(buttonState);
        }
    }
}
