using System;
using System.Linq;

namespace Relativity.Sync.Tests.System
{
	internal sealed class ConsoleLogger : ISyncLog
	{
		public void LogVerbose(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[VERBOSE] {messageTemplate}; {String.Join(", ", propertyValues)}");
		}

		public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[VERBOSE] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
		}

		public void LogDebug(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[DEBUG] {messageTemplate}; {String.Join(", ", propertyValues)}");
		}

		public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[DEBUG] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
		}

		public void LogInformation(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[INFO] {messageTemplate}; {String.Join(", ", propertyValues)}");
		}

		public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[INFO] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
		}

		public void LogWarning(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[WARNING] {messageTemplate}; {String.Join(", ", propertyValues)}");
		}

		public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[WARNING] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
		}

		public void LogError(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[ERROR] {messageTemplate}; {String.Join(", ", propertyValues)}");
		}

		public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[ERROR] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
		}

		public void LogFatal(string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[FATAL] {messageTemplate}; {String.Join(", ", propertyValues)}");
		}

		public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Console.WriteLine($"[FATAL] {messageTemplate}; {String.Join(", ", propertyValues)}; {exception}");
		}
	}
}