﻿using System;
using System.Reactive.Disposables;
using NUnit.Framework;
using Relativity.API;

namespace Relativity.Sync.Tests.System.Core
{
    internal sealed class ConsoleLogger : IAPILog
    {
        public void LogVerbose(string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[VERBOSE] {messageTemplate}; {string.Join(", ", propertyValues)}");
            TestContext.Progress.WriteLine($"[VERBOSE] {messageTemplate}; {string.Join(", ", propertyValues)}");
            Console.Out.Flush();
        }

        public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[VERBOSE] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            TestContext.Progress.WriteLine($"[VERBOSE] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            Console.Out.Flush();
        }

        public void LogDebug(string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[DEBUG] {messageTemplate}; {string.Join(", ", propertyValues)}");
            TestContext.Progress.WriteLine($"[DEBUG] {messageTemplate}; {string.Join(", ", propertyValues)}");
            Console.Out.Flush();
        }

        public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[DEBUG] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            TestContext.Progress.WriteLine($"[DEBUG] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            Console.Out.Flush();
        }

        public void LogInformation(string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[INFO] {messageTemplate}; {string.Join(", ", propertyValues)}");
            TestContext.Progress.WriteLine($"[INFO] {messageTemplate}; {string.Join(", ", propertyValues)}");
        }

        public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[INFO] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            TestContext.Progress.WriteLine($"[INFO] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            Console.Out.Flush();
        }

        public void LogWarning(string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[WARNING] {messageTemplate}; {string.Join(", ", propertyValues)}");
            TestContext.Progress.WriteLine($"[WARNING] {messageTemplate}; {string.Join(", ", propertyValues)}");
            Console.Out.Flush();
        }

        public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[WARNING] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            TestContext.Progress.WriteLine($"[WARNING] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            Console.Out.Flush();
        }

        public void LogError(string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[ERROR] {messageTemplate}; {string.Join(", ", propertyValues)}");
            TestContext.Progress.WriteLine($"[ERROR] {messageTemplate}; {string.Join(", ", propertyValues)}");
            Console.Out.Flush();
        }

        public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[ERROR] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            TestContext.Progress.WriteLine($"[ERROR] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            Console.Out.Flush();
        }

        public void LogFatal(string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[FATAL] {messageTemplate}; {string.Join(", ", propertyValues)}");
            TestContext.Progress.WriteLine($"[FATAL] {messageTemplate}; {string.Join(", ", propertyValues)}");
            Console.Out.Flush();
        }

        public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine($"[FATAL] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            TestContext.Progress.WriteLine($"[FATAL] {messageTemplate}; {string.Join(", ", propertyValues)}; {exception}");
            Console.Out.Flush();
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
