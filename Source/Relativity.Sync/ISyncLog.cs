using System;

namespace Relativity.Sync
{
	/// <summary>
	///     Used for logging
	/// </summary>
	public interface ISyncLog
	{
		/// <summary>
		///     Logs a verbose message
		/// </summary>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogVerbose(string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs a verbose message with an exception
		/// </summary>
		/// <param name="exception">Exception to log</param>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs a debug message
		/// </summary>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogDebug(string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs a debug message with an exception
		/// </summary>
		/// <param name="exception">Exception to log</param>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs an information message
		/// </summary>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogInformation(string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs an information message with an exception
		/// </summary>
		/// <param name="exception">Exception to log</param>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs a warning message
		/// </summary>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogWarning(string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs an warning message with an exception
		/// </summary>
		/// <param name="exception">Exception to log</param>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs an error message
		/// </summary>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogError(string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs an error message with an exception
		/// </summary>
		/// <param name="exception">Exception to log</param>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogError(Exception exception, string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs a fatal error message
		/// </summary>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogFatal(string messageTemplate, params object[] propertyValues);

		/// <summary>
		///     Logs a fatal error message with an exception
		/// </summary>
		/// <param name="exception">Exception to log</param>
		/// <param name="messageTemplate">Message template</param>
		/// <param name="propertyValues">Property values</param>
		void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues);
	}
}