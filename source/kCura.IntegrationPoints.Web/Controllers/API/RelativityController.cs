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

			int workspaceArtifactId = 0;
			int savedSearchArtifactId = 0;
			try
			{
				workspaceArtifactId = (int)settings.WorkspaceArtifactId;
				savedSearchArtifactId =  (int)settings.SavedSearchArtifactId;
			}
			catch
			{
				return BadRequest(kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.INVALID_PARAMETERS);
			}

			var result = new List<KeyValuePair<string, int>>
			{
				new KeyValuePair<string, int>(kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.TARGET_WORKSPACE_ID, workspaceArtifactId),
				new KeyValuePair<string, int>(kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.SAVED_SEARCH_ID, savedSearchArtifactId)
			};

			return Ok(result);
		}
	}
}
