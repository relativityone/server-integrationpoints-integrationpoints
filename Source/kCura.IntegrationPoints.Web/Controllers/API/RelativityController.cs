﻿using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class RelativityController : ApiController
	{
		private readonly IStringSanitizer _htmlSanitizerManager;

		public RelativityController(IStringSanitizer htmlSanitizerManager)
		{
			_htmlSanitizerManager = htmlSanitizerManager;
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unexpected error when processing request (splitting capitalized fields).")]
		public IHttpActionResult GetViewFields([FromBody] object data)
		{
			List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();

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
					key = _htmlSanitizerManager.SanitizeHtmlContent(key).CleanHtml;
					object value = null;
					if (kvp.Value != null)
					{
						value = _htmlSanitizerManager.SanitizeHtmlContent(kvp.Value.ToString()).CleanHtml;
					}
					result.Add(new KeyValuePair<string, object>(key, value));
				}
			}
			return Ok(result);
		}
	}
}
