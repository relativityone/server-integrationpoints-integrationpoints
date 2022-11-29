using System;
using System.Text;

namespace kCura.IntegrationPoints.Domain.Extensions
{
    /// <summary>
    /// TODO : move this class to a shared project - SAMO 7/19/2016.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Returns a flattened string which consists of error message and stack trace.
        /// </summary>
        /// <param name="exception">A main exception object to use this method</param>
        public static string FlattenErrorMessagesWithStackTrace(this Exception exception)
        {
            if (exception == null)
            {
                return String.Empty;
            }

            var aggregateException = exception as AggregateException;
            var stringBuilder = new StringBuilder();
            bool isAggregateExceptionWithInnerExceptions = aggregateException?.InnerExceptions != null;

            stringBuilder.AppendLine(exception.Message);
            stringBuilder.AppendLine(exception.StackTrace);

            if (isAggregateExceptionWithInnerExceptions)
            {
                for (int i = 0; i < aggregateException.InnerExceptions.Count; i++)
                {
                    int innerExceptionId = i + 1;
                    stringBuilder.AppendLine($"Inner Exception {innerExceptionId}:");
                    stringBuilder.AppendLine(aggregateException.InnerExceptions[i].FlattenErrorMessagesWithStackTrace());
                }
            }
            else if (exception.InnerException != null)
            {
                stringBuilder.AppendLine("Inner Exception:");
                stringBuilder.AppendLine(exception.InnerException.FlattenErrorMessagesWithStackTrace());
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns a flattened string which represents error messages from exception and inner exceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string FlattenErrorMessages(this Exception ex)
        {
            StringBuilder stringBuilder = new StringBuilder(ex.Message);

            if (ex.InnerException != null)
            {
                stringBuilder.AppendLine($" Inner exception: {FlattenErrorMessages(ex.InnerException)} ");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Check if <paramref name="ex"/> contains <typeparamref name="T"/> as inner exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool HasInnerException<T>(this Exception ex) where T : Exception
        {
            Exception currentEx = ex;
            while (currentEx.InnerException != null)
            {
                if (currentEx.InnerException is T)
                {
                    return true;
                }

                currentEx = currentEx.InnerException;
            }

            return false;
        }

        /// <summary>
        /// Check if <paramref name="message"/> is part of <paramref name="ex"/>.Message
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool HasMessage(this Exception ex, string message)
        {
            if (ex is null)
            {
                return false;
            }

            return ex.Message.Contains(message);
        }
    }
}