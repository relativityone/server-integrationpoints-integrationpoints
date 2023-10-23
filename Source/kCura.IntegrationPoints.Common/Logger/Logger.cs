using System;
using kCura.IntegrationPoints.Common.Logger;
using Relativity.API;
using Serilog;

namespace kCura.IntegrationPoints.Common
{
    /// <summary>
    /// Wraps non-generic <see cref="IAPILog"/> logger with generic parameter for ensuring the SourceContext in logs.
    /// </summary>
    /// <typeparam name="T">The SourceContext type.</typeparam>
    public class Logger<T> : ILogger<T>
    {
        private readonly ISerilogLoggerInstrumentationService _serilogLoggerFactory;
        private readonly IAPILog _logger;
        private readonly ILogger _serilogLogger;

        public Logger(IAPILog logger, ISerilogLoggerInstrumentationService serilogLoggerFactory)
        {
            _logger = logger.ForContext<T>();
            _serilogLoggerFactory = serilogLoggerFactory;
            _serilogLogger = serilogLoggerFactory.GetLogger<T>();
        }

        /// <inheritdoc/>
        public void LogVerbose(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogVerbose(messageTemplate, propertyValues);
            _serilogLogger.Verbose(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogVerbose(exception, messageTemplate, propertyValues);
            _serilogLogger.Verbose(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogDebug(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogDebug(messageTemplate, propertyValues);
            _serilogLogger.Debug(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogDebug(exception, messageTemplate, propertyValues);
            _serilogLogger.Debug(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogInformation(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogInformation(messageTemplate, propertyValues);
            _serilogLogger.Information(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogInformation(exception, messageTemplate, propertyValues);
            _serilogLogger.Information(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogWarning(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogWarning(messageTemplate, propertyValues);
            _serilogLogger.Warning(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogWarning(exception, messageTemplate, propertyValues);
            _serilogLogger.Warning(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogError(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogError(messageTemplate, propertyValues);
            _serilogLogger.Error(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogError(exception, messageTemplate, propertyValues);
            _serilogLogger.Error(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogFatal(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogFatal(messageTemplate, propertyValues);
            _serilogLogger.Fatal(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogFatal(exception, messageTemplate, propertyValues);
            _serilogLogger.Fatal(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public ILogger<TContext> ForContext<TContext>()
        {
            return new Logger<TContext>(_logger, _serilogLoggerFactory);
        }
    }
}
