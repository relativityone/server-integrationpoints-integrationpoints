using System;
using System.Net;
using System.Text;
using System.Web.Mvc;
using kCura.IntegrationPoints.Core.Helpers;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Web.Attributes
{
	public class JsonNetResult : ActionResult
	{
		public Encoding ContentEncoding { get; set; }
		public string ContentType { get; set; }
		public object Data { get; set; }
		public int StatusCode { get; set; }

		public JsonSerializerSettings SerializerSettings { get; set; }

		public JsonNetResult()
		{
			SerializerSettings = JSONHelper.GetDefaultSettings();
		}

		public JsonNetResult GetJsonNetResult(object data = null, int statusCode = (int)HttpStatusCode.NoContent, string contentType = null)
		{
			return new JsonNetResult
			{
				Data = data,
				StatusCode = statusCode,
				ContentType = contentType
			};
		}

		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var response = context.HttpContext.Response;

			response.StatusCode = StatusCode;
			response.ContentType = string.IsNullOrEmpty(ContentType) ? "application/json" : ContentType;

			if ((StatusCode >= 400) && (StatusCode <= 599))
			{
				response.TrySkipIisCustomErrors = true;
			}

			if (ContentEncoding != null)
			{
				response.ContentEncoding = ContentEncoding;
			}

			if (Data == null)
			{
				return;
			}

			var formatting = Formatting.None;

			var writer = new JsonTextWriter(response.Output) { Formatting = formatting, };

			var serializer = JsonSerializer.Create(SerializerSettings);
			serializer.Serialize(writer, Data);

			writer.Flush();
		}
	}
}