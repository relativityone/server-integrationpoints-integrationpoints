using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Display;
using Serilog.Sinks.IOFile;

namespace kCura.IntegrationPoints.UITests.Logging
{
	public static class LoggerFactory
	{
		private const string _OUTPUT_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] [{LoggerName}] {Message}{NewLine}{Exception}";

		private const long _LOG_FILE_SIZE_LIMIT_BYTES = 100 * 1000 * 1000;

		private static readonly Dictionary<string, ILogger> Loggers = new Dictionary<string, ILogger>();

		private static readonly FileSink FileSink;

		static LoggerFactory()
		{
			FileSink = new FileSink(BuildLogPath(), new MessageTemplateTextFormatter(_OUTPUT_TEMPLATE, null), _LOG_FILE_SIZE_LIMIT_BYTES, Encoding.UTF8);
		}

		private static string BuildLogPath()
		{
			string logDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "logs");
			string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			string logPath = logDir + $@"\rip_ui_tests_{timeStamp}.log";
			return logPath;
		}	

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
				.WriteTo.Sink(FileSink)
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
