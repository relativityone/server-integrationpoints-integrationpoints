﻿using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
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
			string msg = string.Format("{0}{1}", 
				string.IsNullOrEmpty(Message) ? "Unexpected error occurred" : Message,
				IsUserMessage ? " Please contact system administrator" : string.Empty);

			actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError, msg);
			actionExecutedContext.Response.Content = new StringContent(msg);
			
			_apiLog.LogError(actionExecutedContext.Exception, msg);
		}

		#endregion Methods
	}
}