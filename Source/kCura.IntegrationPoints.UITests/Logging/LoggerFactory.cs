using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Logging
{
	public static class LoggerFactory
	{
		private const string _OUTPUT_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] [{LoggerName}] {Message}{NewLine}{Exception}";

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
				.WriteTo.Console(outputTemplate: _OUTPUT_TEMPLATE)
				.WriteTo.Seq("http://127.0.0.1:5341")
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
