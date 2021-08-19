using System;
using Relativity.API;
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class ConsoleLogger : IAPILog, ILog
	{
        public bool IsEnabled { get; }
        public string Application { get; }
        public string SubSystem { get; }
        public string System { get; }

		public void LogVerbose(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[VERBOSE] {messageTemplate}; {String.Join(", ", propertyValues)}");
			Console.Out.Flush();
		}

		public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[VERBOSE] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
			Console.Out.Flush();
		}

		public void LogDebug(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[DEBUG] {messageTemplate}; {String.Join(", ", propertyValues)}");
			Console.Out.Flush();
		}

		public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[DEBUG] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
			Console.Out.Flush();
		}

		public void LogInformation(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[INFO] {messageTemplate}; {String.Join(", ", propertyValues)}");
		}

		public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[INFO] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
			Console.Out.Flush();
		}

		public void LogWarning(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[WARNING] {messageTemplate}; {String.Join(", ", propertyValues)}");
			Console.Out.Flush();
		}

		public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[WARNING] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
			Console.Out.Flush();
		}

		public void LogError(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[ERROR] {messageTemplate}; {String.Join(", ", propertyValues)}");
			Console.Out.Flush();
		}

		public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[ERROR] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
			Console.Out.Flush();
		}

		public void LogFatal(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[FATAL] {messageTemplate}; {String.Join(", ", propertyValues)}");
			Console.Out.Flush();
		}

		public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[FATAL] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
			Console.Out.Flush();
		}

        ILog ILog.ForContext<T>()
        {
            throw new NotImplementedException();
        }

        ILog ILog.ForContext(Type forContext)
        {
            throw new NotImplementedException();
        }

        ILog ILog.ForContext(string propertyName, object value, bool destructureObjects)
        {
            throw new NotImplementedException();
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
			return (IDisposable)null;
		}
    }
}
