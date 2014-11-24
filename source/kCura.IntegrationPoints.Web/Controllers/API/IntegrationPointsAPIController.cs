using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointsAPIController : ApiController
	{
		[HttpGet]
		public HttpResponseMessage Get(int id)
		{
			return Request.CreateResponse(HttpStatusCode.Accepted);
		}

		[HttpPut]
		public HttpResponseMessage Update()
		{
			return Request.CreateResponse(HttpStatusCode.Accepted);
		}

	}
}
