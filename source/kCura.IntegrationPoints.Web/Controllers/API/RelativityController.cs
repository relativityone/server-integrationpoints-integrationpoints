using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class RelativityController : ApiController
	{
		public RelativityController() {}

		[HttpPost]
		public IHttpActionResult GetViewFields([FromBody] object data)
		{
			List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();;
			try
			{
				ExpandoObject settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(data.ToString()) as ExpandoObject;
				
				if (settings != null)
				{
					// This is to print out variables as friendly text
					// kudos: http://stackoverflow.com/questions/4488969/split-a-string-by-capital-letters
					 var regex = new Regex(@"
						(?<=[A-Z])(?=[A-Z][a-z]) |
						(?<=[^A-Z])(?=[A-Z]) |
						(?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

					foreach (KeyValuePair<string, object> kvp in settings)
					{
						string key = regex.Replace(kvp.Key, " ");
						result.Add(new KeyValuePair<string, object>(key, kvp.Value));
					}
				}
			}
			catch
			{
				return BadRequest(kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.INVALID_PARAMETERS);
			}

			return Ok(result);
		}
	}
}
