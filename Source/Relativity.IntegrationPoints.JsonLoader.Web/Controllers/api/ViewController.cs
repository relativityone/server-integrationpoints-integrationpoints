using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Relativity.IntegrationPoints.JsonLoader.Web.Controllers.api
{
	public class ViewController : ApiController
	{
		[HttpPost]
		public HttpResponseMessage GetViewFields([FromBody] object data)
		{
			var result = new List<KeyValuePair<string, string>>();
			if (data != null)
			{
				var helper = new JsonHelper();
				var settings = helper.GetSettings(data.ToString());
				result.Add(new KeyValuePair<string, string>("FieldLocation", settings.FieldLocation));
				result.Add(new KeyValuePair<string, string>("DataLocation", settings.DataLocation));
			}
			return Request.CreateResponse(HttpStatusCode.OK, result);
		}
	}
}
