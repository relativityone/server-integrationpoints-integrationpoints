using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class CustodianController : ApiController
	{
		private readonly CustodianService _custodianService;
		
		public CustodianController(CustodianService custodianService)
		{
			_custodianService = custodianService;
		}

		[HttpPost]
		[Route("{workspaceID}/api/custodian/{id}")]
		[LogApiExceptionFilter(Message = "Unable to retrieve about custodian.")]
		public bool Post(int id)
		{
			return _custodianService.IsCustodian(id);
		}
	}
}
