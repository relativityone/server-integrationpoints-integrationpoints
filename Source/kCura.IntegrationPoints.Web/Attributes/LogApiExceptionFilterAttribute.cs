using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Filters;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.Core.Service;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Web.Attributes
{
	/// <summary>
	/// The purpose of the class is to log all exception in WebAPi controllers with LogApi
	/// This attribute can be used on top of the method to specify custom message.
	/// "OnException" method will be always called after ExceptionLogger.Log
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	public class LogApiExceptionFilterAttribute : ExceptionFilterAttribute
	{
		private readonly IHtmlSanitizerManager _sanitizer = new HtmlSanitizerManager();
		private const string _CONTACT_ADMIN_MESSAGE_ENDING = " Please check Error tab for more details";

		#region Fields

		private readonly ILog _apiLog = global::Relativity.Logging.Factory.LogFactory.GetLogger(
			global::Relativity.Logging.Factory.LogFactory.GetOptionsFromAppDomain().Clone());

		#endregion //Fields

		#region Properties

		public string Message { get; set; }
		public bool IsUserMessage { get; set; }

		#endregion Properties

		public LogApiExceptionFilterAttribute()
		{
			IsUserMessage = true;
		}

		#region Methods

		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{

			var msgBuilder = new StringBuilder(GetMostSpecificMessage(actionExecutedContext.Exception));
			if (IsUserMessage)
			{
				msgBuilder.Append(_CONTACT_ADMIN_MESSAGE_ENDING);
			}

			string msg = msgBuilder.ToString();
			string sanitizedMsg = _sanitizer.Sanitize(msg).CleanHTML;
			actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError, sanitizedMsg);
			actionExecutedContext.Response.Content = new StringContent(sanitizedMsg);
			
			_apiLog.LogError(actionExecutedContext.Exception, sanitizedMsg);
		}

		private string GetMostSpecificMessage(Exception exception)
		{
			var integrationPointsException = exception as IntegrationPointsException;
			if (!string.IsNullOrWhiteSpace(integrationPointsException?.UserMessage))
			{
				return integrationPointsException.UserMessage;
			}
			if (!string.IsNullOrWhiteSpace(Message))
			{
				return Message;
			}
			return "UnexpectedErrorOccurred";
		}

		#endregion Methods
	}
}