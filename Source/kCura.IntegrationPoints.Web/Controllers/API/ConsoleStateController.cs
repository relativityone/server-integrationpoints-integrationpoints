using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ConsoleStateController : ApiController
    {
        private readonly IButtonStateBuilder _buttonStateBuilder;

        public ConsoleStateController(ICPHelper helper,
            IRepositoryFactory respositoryFactory,
            IManagerFactory managerFactory)
        {
            _buttonStateBuilder = ButtonStateBuilder.CreateButtonStateBuilder(helper, respositoryFactory, managerFactory);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to get ConsoleState")]
        public IHttpActionResult GetConsoleState(int workspaceId, int integrationPointArtifactId)
        {
            ButtonStateDTO buttonState = _buttonStateBuilder
                .CreateButtonState(workspaceId, integrationPointArtifactId);

            return Ok(buttonState);
        }
    }
}