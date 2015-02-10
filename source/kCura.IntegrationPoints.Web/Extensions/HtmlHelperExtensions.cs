using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Serialization;

namespace kCura.IntegrationPoints.Web.Extensions
{
	public static class HtmlExtensions
	{
		public static IHtmlString JsonEncode(this HtmlHelper html, Object input)
		{
			var srs = new Newtonsoft.Json.JsonSerializer();
			var sb = new System.Text.StringBuilder();
			srs.ContractResolver = new CamelCasePropertyNamesContractResolver();
			using (System.IO.TextWriter writer = new System.IO.StringWriter(sb))
			{
				srs.Serialize(writer, input);
			}
			var modelJson = sb.ToString();

			return html.Raw(modelJson);
		}
	}
}