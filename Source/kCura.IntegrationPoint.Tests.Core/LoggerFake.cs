using System;
using kCura.IntegrationPoints.Common;

namespace kCura.IntegrationPoint.Tests.Core
{
    public class LoggerFake<T> : ILogger<T>
    {
        public void LogVerbose(string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogDebug(string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogInformation(string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogWarning(string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogError(string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogFatal(string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public ILogger<T> EnrichWithProperty(string propertyName, object value)
        {
            return this;
        }

        public ILogger<TContext> ForContext<TContext>()
        {
            return new LoggerFake<TContext>();
        }
    }
}
