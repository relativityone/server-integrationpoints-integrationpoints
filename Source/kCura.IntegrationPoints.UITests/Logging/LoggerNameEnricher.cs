using Serilog.Core;
using Serilog.Events;

namespace kCura.IntegrationPoints.UITests.Logging
{
	public class LoggerNameEnricher : ILogEventEnricher
	{
		public const string LoggerNamePropertyName = "LoggerName";

		public readonly string LoggerName;

		public LoggerNameEnricher(string name)
		{
			LoggerName = name;
		}

		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			logEvent.AddPropertyIfAbsent(new LogEventProperty(LoggerNamePropertyName, new ScalarValue(LoggerName)));
		}
	}
}