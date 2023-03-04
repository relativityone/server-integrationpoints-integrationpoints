using System;

namespace kCura.IntegrationPoints.Common
{
    /// <summary>
    /// The interface representing the generic logger where the genetic parameter defines the SourceContext of the logs.
    /// </summary>
    /// <typeparam name="T">The logger owner type, helpful to set proper SourceContext field in logs.</typeparam>
    public interface ILogger<T>
    {
        /// <summary>Verbose level logging.</summary>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogVerbose(string messageTemplate, params object[] propertyValues);

        /// <summary>Verbose level logging with an exception.</summary>
        /// <param name="exception">An exception to add to the information logged.  Shows stack trace and error message.</param>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>Debug level logging.</summary>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogDebug(string messageTemplate, params object[] propertyValues);

        /// <summary>Debug level logging with an exception.</summary>
        /// <param name="exception">An exception to add to the information logged.  Shows stack trace and error message.</param>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>Information level logging.</summary>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogInformation(string messageTemplate, params object[] propertyValues);

        /// <summary>Information level logging with an exception.</summary>
        /// <param name="exception">An exception to add to the information logged.  Shows stack trace and error message.</param>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>Warning level logging.</summary>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogWarning(string messageTemplate, params object[] propertyValues);

        /// <summary>Warning level logging with an exception.</summary>
        /// <param name="exception">An exception to add to the information logged.  Shows stack trace and error message.</param>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>Error level logging.</summary>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogError(string messageTemplate, params object[] propertyValues);

        /// <summary>Error level logging with an exception.</summary>
        /// <param name="exception">An exception to add to the information logged.  Shows stack trace and error message.</param>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogError(Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>Fatal level logging.</summary>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogFatal(string messageTemplate, params object[] propertyValues);

        /// <summary>Fatal level logging with an exception.</summary>
        /// <param name="exception">An exception to add to the information logged.  Shows stack trace and error message.</param>
        /// <param name="messageTemplate">Structured logging template.
        /// <example>messageTemplate: "I am logging the {UserName} and its permission {@Permission} object", propertyValues: "myUserName123", permission</example></param>
        /// <param name="propertyValues">params for each brace'd item in the messageTemplate.</param>
        void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>Adds the full name of Type T to the log information.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ILogger<T> ForContext<T>();
    }
}
