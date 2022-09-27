using System.Threading.Tasks;
using System.Web.Http;
using kCura.IntegrationPoints.Common.Interfaces;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ConsoleStateController : ApiController
    {
        private readonly ICPHelper _helper;
        private readonly IRepositoryFactory _respositoryFactory;
        private readonly IManagerFactory _managerFactory;
        private readonly IIntegrationPointRepository _integrationPointRepository;
        private readonly IProviderTypeService _providerTypeService;
        private readonly IRelativitySyncConstrainsChecker _relativitySyncConstrainsChecker;

        public ConsoleStateController(
            ICPHelper helper,
            IRepositoryFactory respositoryFactory,
            IManagerFactory managerFactory,
            IIntegrationPointRepository integrationPointRepository,
            IProviderTypeService providerTypeService,
            IRelativitySyncConstrainsChecker relativitySyncConstrainsChecker)
        {
            _helper = helper;
            _respositoryFactory = respositoryFactory;
            _managerFactory = managerFactory;
            _integrationPointRepository = integrationPointRepository;
            _relativitySyncConstrainsChecker = relativitySyncConstrainsChecker;
            _providerTypeService = providerTypeService;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to get ConsoleState")]
        public async Task<IHttpActionResult> GetConsoleState(int workspaceId, int integrationPointArtifactId)
        {
            ButtonStateBuilder buttonStateBuilder = ButtonStateBuilder.CreateButtonStateBuilder(
                _helper,
                _respositoryFactory,
                _managerFactory,
                _integrationPointRepository,
                _providerTypeService,
                _relativitySyncConstrainsChecker,
                workspaceId,
                integrationPointArtifactId);

            ButtonStateDTO buttonState = await buttonStateBuilder
                .CreateButtonStateAsync(workspaceId, integrationPointArtifactId);

            return Ok(buttonState);
        }
    }
}
