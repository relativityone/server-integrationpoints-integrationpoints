using System;
using System.Reactive.Disposables;
using Relativity.API;

namespace Relativity.Sync.Tests.System.Core.Helpers.APIHelper
{
	public class APILog : IAPILog
	{
		public void LogVerbose(string messageTemplate, params object[] propertyValues) { }

		public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues) { }

		public void LogDebug(string messageTemplate, params object[] propertyValues) { }

		public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues) { }

		public void LogInformation(string messageTemplate, params object[] propertyValues) { }

		public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues) { }

		public void LogWarning(string messageTemplate, params object[] propertyValues) { }

		public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues) { }

		public void LogError(string messageTemplate, params object[] propertyValues) { }

		public void LogError(Exception exception, string messageTemplate, params object[] propertyValues) { }

		public void LogFatal(string messageTemplate, params object[] propertyValues) { }

		public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues) { }

		public IAPILog ForContext<T>() => this;

		public IAPILog ForContext(Type source) => this;

		public IAPILog ForContext(string propertyName, object value, bool destructureObjects) => this;

		public IDisposable LogContextPushProperty(string propertyName, object obj) => Disposable.Empty;
	}
}
