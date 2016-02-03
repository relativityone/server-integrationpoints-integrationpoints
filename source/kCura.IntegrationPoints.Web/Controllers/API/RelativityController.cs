using System;
using System.Collections.Generic;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class RelativityController : ApiController
	{
		public RelativityController() {}

		[HttpPost]
		public IHttpActionResult GetViewFields([FromBody] object data)
		{
			dynamic settings = Newtonsoft.Json.JsonConvert.DeserializeObject(data.ToString());
			var result = new List<KeyValuePair<string, int>>();
			result.Add(new KeyValuePair<string, int>("Target Workspace ID", Convert.ToInt32(settings.WorkspaceArtifactId)));
			result.Add(new KeyValuePair<string, int>("Saved Search ID", Convert.ToInt32(settings.SavedSearchArtifactId)));

			return Ok(result);
		}
	}
}
