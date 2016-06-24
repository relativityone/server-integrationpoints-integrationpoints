﻿using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Web.Http;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class RelativityController : ApiController
    {
        private IHtmlSanitizerManager _htmlSanitizerManager;

        public RelativityController(IHtmlSanitizerManager htmlSanitizerManager)
        {
            _htmlSanitizerManager = htmlSanitizerManager;
        }

        [HttpPost]
        public IHttpActionResult GetViewFields([FromBody] object data)
        {
            List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>(); ;
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
                        key = _htmlSanitizerManager.Sanitize(key).CleanHTML;
                        string value = kvp.Value.ToString();
                        value = _htmlSanitizerManager.Sanitize(value).CleanHTML;
                        result.Add(new KeyValuePair<string, object>(key, value));
                    }
                }
            }
            catch
            {
                return BadRequest(Core.Constants.IntegrationPoints.INVALID_PARAMETERS);
            }

            return Ok(result);
        }
    }
}
