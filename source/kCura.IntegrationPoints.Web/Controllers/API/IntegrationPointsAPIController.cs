using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class IntegrationPointsAPIController : ApiController
	{
		private readonly IntegrationPointService _reader;
		public IntegrationPointsAPIController(IntegrationPointService reader)
		{
			_reader = reader;
		}
		[HttpGet]
		public HttpResponseMessage Get(int id)
		{
			var model = new IntegrationModel();
			model.ArtifactID = id;
			if (id> 0)
			{
				model = _reader.ReadIntegrationPoint(id);
			}

			return Request.CreateResponse(HttpStatusCode.Accepted, model);
		}

		[HttpPost]
		public HttpResponseMessage Update(IntegrationModel model)
		{
			_reader.SaveIntegration(model);
			return Request.CreateResponse(HttpStatusCode.Accepted);
		}

	}
}
