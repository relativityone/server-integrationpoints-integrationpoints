using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private const string _CONTACT_ADMIN_MESSAGE_ENDING = " Please check Error tab for more details";

        private readonly LogApiExceptionFilterAttribute _attribute;
        private readonly Func<ITextSanitizer> _sanitizerFactory;
        private readonly Func<IAPILog> _loggerFactory;

        public ExceptionFilter(
            LogApiExceptionFilterAttribute attribute,
            Func<ITextSanitizer> textSanitizerFactory,
            Func<IAPILog> loggerFactory)
        {
            _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            _sanitizerFactory = textSanitizerFactory ?? throw new ArgumentNullException(nameof(textSanitizerFactory));
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            _loggerFactory = () => loggerFactory().ForContext<ExceptionFilter>();
        }

        public bool AllowMultiple => true;

        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var msgBuilder = new StringBuilder(GetMostSpecificMessage(actionExecutedContext.Exception));
            if (_attribute.IsUserMessage)
            {
                msgBuilder.Append(_CONTACT_ADMIN_MESSAGE_ENDING);
            }

            string msg = msgBuilder.ToString();

            SetMessageToResponse(actionExecutedContext, msg);
            _loggerFactory().LogError(actionExecutedContext.Exception, msg);

            return Task.CompletedTask;
        }

        private void SetMessageToResponse(HttpActionExecutedContext actionExecutedContext, string msg)
        {
            string sanitizedMsg = _sanitizerFactory().Sanitize(msg).SanitizedText;
            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError, sanitizedMsg);
            actionExecutedContext.Response.Content = new StringContent(sanitizedMsg);
        }

        private string GetMostSpecificMessage(Exception exception)
        {
            var integrationPointsException = exception as IntegrationPointsException;
            if (!string.IsNullOrWhiteSpace(integrationPointsException?.UserMessage))
            {
                return integrationPointsException.UserMessage;
            }
            if (!string.IsNullOrWhiteSpace(_attribute.Message))
            {
                return _attribute.Message;
            }
            return "UnexpectedErrorOccurred";
        }
    }
}