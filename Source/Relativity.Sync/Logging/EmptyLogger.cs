using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using Relativity.API;

namespace Relativity.Sync.Logging
{
    [ExcludeFromCodeCoverage]
    internal sealed class EmptyLogger : IAPILog
    {
        public void LogVerbose(string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogDebug(string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogInformation(string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogWarning(string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogError(string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogFatal(string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

        public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            // Method intentionally left empty.
        }

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
            return Disposable.Empty;
        }
    }
}
