using System;
using kCura.IntegrationPoints.Domain.Exceptions;

namespace kCura.IntegrationPoints.Core.Models
{
    public class ErrorModel
    {
        public int WorkspaceId { get; set; }

        public string Message { get; }

        public string FullError { get; }

        public string Source { get; set; }

        public string Location { get; set; }

        /// <summary>
        /// It can be GUID (Web/EH) or int (Agent Job Id)
        /// </summary>
        public object CorrelationId { get; set; }

        public bool AddToErrorTab { get; }

        public Exception Exception { get; }

        public ErrorModel(Exception exception, bool addToErrorTab = false, string message = null)
        {
            Exception = exception;
            Message = message ?? exception.Message;
            FullError = exception.ToString();
            AddToErrorTab = addToErrorTab;
        }

        public ErrorModel(IntegrationPointsException exception, bool addToErrorTab = false, string message = null)
        {
            Message = message ?? exception.Message;
            FullError = exception.ToString();

            // The root error handler (eg: installer) might decide to add the error to Error Tab even the source exception is set differently
            AddToErrorTab = addToErrorTab || exception.ShouldAddToErrorsTab;
        }
    }
}
