using System;

namespace Relativity.Sync.Utils
{
    internal static class ExceptionExtensions
    {
        internal static string GetExceptionMessages(this Exception ex)
        {
            string message = ex.Message;

            if (ex.InnerException != null)
            {
                message += $"{System.Environment.NewLine}{GetExceptionMessages(ex.InnerException)}";
            }

            return message;
        }
    }
}
