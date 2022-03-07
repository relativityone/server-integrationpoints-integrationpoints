using Relativity.API;
using Relativity.DataMigration.MigrateFileshareAccess;
using System;

namespace kCura.ScheduleQueue.Core.Services
{
    public class FileshareLogger : ILogger
    {
        private readonly IAPILog _logger;

        public FileshareLogger(IAPILog logger)
        {
            _logger = logger;
        }

        public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.LogFatal(exception, messageTemplate, propertyValues);
        }

        public void LogInformation(string messageTemplate, params object[] propertyValues)
        {
            _logger.LogInformation(messageTemplate, propertyValues);
        }
    }
}
