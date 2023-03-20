using System;
using Relativity.API;

namespace kCura.IntegrationPoints.Common
{
    /// <summary>
    /// Wraps non-generic <see cref="IAPILog"/> logger with generic parameter for ensuring the SourceContext in logs.
    /// </summary>
    /// <typeparam name="T">The SourceContext type.</typeparam>
    public class Logger<T> : ILogger<T>
    {
        private readonly IAPILog _logger;

        public Logger(IAPILog logger)
        {
            _logger = logger.ForContext<T>();
        }

        /// <inheritdoc/>
        public void LogVerbose(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogVerbose(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogVerbose(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogDebug(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogDebug(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogDebug(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogInformation(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogInformation(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogInformation(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogWarning(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogWarning(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogWarning(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogError(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogError(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogError(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogFatal(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogFatal(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogFatal(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public ILogger<TContext> ForContext<TContext>()
        {
            return new Logger<TContext>(_logger);
        }
    }
}
