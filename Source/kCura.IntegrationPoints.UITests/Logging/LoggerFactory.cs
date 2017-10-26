using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Logging
{
	public static class LoggerFactory
	{
		private static readonly Dictionary<string, ILogger> Loggers = new Dictionary<string, ILogger>();

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static ILogger CreateLogger(string name)
		{
			if (Loggers.ContainsKey(name))
			{
				return Loggers[name];
			}

			ILogger logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.Enrich.With(new LoggerNameEnricher(name))
				.WriteTo.ColoredConsole(outputTemplate:
					"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] [{LoggerName}] {Message}{NewLine}{Exception}")
				.WriteTo.RollingFile(@"logs\ip-ui-tests-{Date}.txt")
				.CreateLogger();

			Loggers.Add(name, logger);
			return logger;
		}

		public static ILogger CreateLogger(Type type)
		{
			return CreateLogger(type.FullName);
		}
	}
}