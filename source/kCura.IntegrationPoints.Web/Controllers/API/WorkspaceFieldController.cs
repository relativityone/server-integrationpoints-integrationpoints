using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Models;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFieldController : ApiController
	{
		private readonly IDataSyncronizerFactory _factory;
		public WorkspaceFieldController(IDataSyncronizerFactory factory)
		{
			_factory = factory;
		}
		
		[HttpPost]
		[Route("{workspaceID}/api/WorkspaceField/")]
		public HttpResponseMessage Post([FromBody] SyncronizerSettings settings)
		{
			var syncronizer = _factory.GetSyncronizer(Guid.Empty,settings.Settings);
			var fields = syncronizer.GetFields(settings.Settings);
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}
	}

}
