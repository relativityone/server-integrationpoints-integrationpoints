using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class EntityController : ApiController
    {
        private readonly EntityService _entityService;

        public EntityController(EntityService entityService)
        {
            _entityService = entityService;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to determine if object is of Entity type.")]
        public bool Get(int id)
        {
            return _entityService.IsEntity(id);
        }
    }
}