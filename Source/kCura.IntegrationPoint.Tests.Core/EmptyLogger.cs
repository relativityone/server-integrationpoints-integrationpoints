using System;
using System.Reactive.Disposables;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
    public class EmptyLogger : IAPILog
    {
        public IAPILog ForContext<T>()
        {
            return this;
        }

        public IAPILog ForContext(Type source)
        {
            return this;
        }

        public IAPILog ForContext(string propertyName, object value, bool destructureObjects)
        {
            return this;
        }

        public IDisposable LogContextPushProperty(string propertyName, object obj)
        {
            return Disposable.Create(() => { });
        }

        public void LogDebug(string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
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

        public void LogInformation(string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogVerbose(string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogWarning(string messageTemplate, params object[] propertyValues)
        {
        }

        public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }
    }
}
