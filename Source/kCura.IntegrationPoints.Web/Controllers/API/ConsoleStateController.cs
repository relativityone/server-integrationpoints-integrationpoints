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
        private readonly ICPHelper _helper;
        private readonly IRepositoryFactory _respositoryFactory;
        private readonly IManagerFactory _managerFactory;

        public ConsoleStateController(ICPHelper helper,
            IRepositoryFactory respositoryFactory,
            IManagerFactory managerFactory)
        {
            _helper = helper;
            _respositoryFactory = respositoryFactory;
            _managerFactory = managerFactory;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to get ConsoleState")]
        public IHttpActionResult GetConsoleState(int workspaceId, int integrationPointArtifactId)
        {
            ButtonStateBuilder buttonStateBuilder = ButtonStateBuilder.CreateButtonStateBuilder(_helper, _respositoryFactory, _managerFactory, workspaceId);
            ButtonStateDTO buttonState = buttonStateBuilder
                .CreateButtonState(workspaceId, integrationPointArtifactId);

            return Ok(buttonState);
        }
    }
}