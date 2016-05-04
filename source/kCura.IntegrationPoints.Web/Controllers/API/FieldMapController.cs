using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class FieldMapController : ApiController
	{
		private readonly IIntegrationPointService _integrationPointReader;
		public FieldMapController(IIntegrationPointService integrationPointReader)
		{
			_integrationPointReader = integrationPointReader;
		}

		public HttpResponseMessage Get(int id)
		{
			var fieldsmap = _integrationPointReader.GetFieldMap(id);
			return Request.CreateResponse(HttpStatusCode.OK, fieldsmap, Configuration.Formatters.JsonFormatter);
		}
	}
}
