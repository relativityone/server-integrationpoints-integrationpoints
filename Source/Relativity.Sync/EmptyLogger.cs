using System;
using System.Diagnostics.CodeAnalysis;

namespace Relativity.Sync
{
	[ExcludeFromCodeCoverage]
	internal sealed class EmptyLogger : ISyncLog
	{
		public bool IsEnabled { get; set; }

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
	}
}