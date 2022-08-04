using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class RelativityController : ApiController
    {
        private readonly ITextSanitizer _htmlSanitizerManager;

        public RelativityController(ITextSanitizer htmlSanitizerManager)
        {
            _htmlSanitizerManager = htmlSanitizerManager;
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unexpected error when processing request (splitting capitalized fields).")]
        public IHttpActionResult GetViewFields([FromBody] string data)
        {
            List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();

            Dictionary<string, string> settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

            if (settings != null)
            {
                // This is to print out variables as friendly text
                // kudos: http://stackoverflow.com/questions/4488969/split-a-string-by-capital-letters
                var regex = new Regex(@"
                    (?<=[A-Z])(?=[A-Z][a-z]) |
                    (?<=[^A-Z])(?=[A-Z]) |
                    (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

                foreach (KeyValuePair<string, string> kvp in settings)
                {
                    string key = regex.Replace(kvp.Key, " ");
                    key = _htmlSanitizerManager.Sanitize(key).SanitizedText;
                    object value = null;
                    if (kvp.Value != null)
                    {
                        value = _htmlSanitizerManager.Sanitize(kvp.Value).SanitizedText;
                    }
                    result.Add(new KeyValuePair<string, object>(key, value));
                }
            }
            return Ok(result);
        }
    }
}
