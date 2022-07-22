using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using Relativity.API;

namespace Relativity.Sync.Tests.System.Core
{
    internal sealed class DebugLogger : IAPILog
    {
        public void LogVerbose(string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[VERBOSE] {messageTemplate}; {String.Join(", ", propertyValues)}");
        }

        public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[VERBOSE] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
        }

        public void LogDebug(string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[DEBUG] {messageTemplate}; {String.Join(", ", propertyValues)}");
        }

        public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[DEBUG] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
        }

        public void LogInformation(string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[INFO] {messageTemplate}; {String.Join(", ", propertyValues)}");
        }

        public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[INFO] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
        }

        public void LogWarning(string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[WARNING] {messageTemplate}; {String.Join(", ", propertyValues)}");
        }

        public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[WARNING] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
        }

        public void LogError(string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[ERROR] {messageTemplate}; {String.Join(", ", propertyValues)}");
        }

        public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[ERROR] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
        }

        public void LogFatal(string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[FATAL] {messageTemplate}; {String.Join(", ", propertyValues)}");
        }

        public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Debug.WriteLine($"[FATAL] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
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