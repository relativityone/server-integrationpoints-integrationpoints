using System;

namespace kCura.IntegrationPoints.Web.Attributes
{
    /// <summary>
    /// The purpose of the class is to log all exception in WebAPi controllers with LogApi
    /// This attribute can be used on top of the method to specify custom message.
    /// Exception will be handled by filter after ExceptionLogger.Log is executed
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LogApiExceptionFilterAttribute : Attribute
    {
        public string Message { get; set; }
        public bool IsUserMessage { get; set; }

        public LogApiExceptionFilterAttribute()
        {
            IsUserMessage = true;
        }
    }
}