using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class WorkspaceFieldController : ApiController
	{

		private readonly RdoSynchronizer _rdosynchronizer;
		private readonly RelativityRdoQuery _rdoQuery;

		public WorkspaceFieldController(RdoSynchronizer rdosynchronizer, RelativityRdoQuery rdoQuery)
		{
			_rdosynchronizer = rdosynchronizer;
			_rdoQuery = rdoQuery;
		}
		// GET api/<controller
		[Route("{workspaceID}/api/WorkspaceField/")]
		public HttpResponseMessage Get(string json)
		{
			var fieldsForRdo = _rdosynchronizer.GetFields(json).OrderBy(x => x.DisplayName);
			var select = _rdosynchronizer.GetIdentifier(json);
			var id = JsonConvert.DeserializeObject<ImportSettings>(json);

			var parent = _rdoQuery.hasParent(id.ArtifactTypeId);
			var data = new { data = fieldsForRdo, selected = select, hasParent = parent };
			return Request.CreateResponse(HttpStatusCode.OK, data, Configuration.Formatters.JsonFormatter);
		}
	}
}
