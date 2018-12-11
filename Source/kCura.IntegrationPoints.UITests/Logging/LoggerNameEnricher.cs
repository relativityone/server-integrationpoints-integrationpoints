using Serilog.Core;
using Serilog.Events;

namespace kCura.IntegrationPoints.UITests.Logging
{
	public class LoggerNameEnricher : ILogEventEnricher
	{
		private readonly string _loggerName;
		public const string LOGGER_NAME_PROPERTY_NAME = "LoggerName";

		public LoggerNameEnricher(string name)
		{
			_loggerName = name;
		}

		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			logEvent.AddPropertyIfAbsent(new LogEventProperty(LOGGER_NAME_PROPERTY_NAME, new ScalarValue(_loggerName)));
		}
	}
}