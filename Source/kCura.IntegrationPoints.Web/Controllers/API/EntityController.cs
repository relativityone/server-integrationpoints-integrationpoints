using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class EntityController : ApiController
	{
		private readonly CustodianService _custodianService;
		
		public EntityController(CustodianService custodianService)
		{
			_custodianService = custodianService;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to determine if object is of Entity type.")]
		public bool Get(int id)
		{
			return _custodianService.IsCustodian(id);
		}
	}
}
