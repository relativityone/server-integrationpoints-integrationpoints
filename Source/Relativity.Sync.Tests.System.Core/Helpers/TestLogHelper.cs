using System.Diagnostics;
using Relativity.API;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	public static class TestLogHelper
	{
		public static ISyncLog GetLogger()
		{
			return AppSettings.UseLogger
				? (Debugger.IsAttached ? (ISyncLog)new DebugLogger() : new ConsoleLogger())
				: new EmptyLogger();
		}

		public static IAPILog GetAPILogger()
		{
			return Debugger.IsAttached ? (IAPILog)new DebugLogger() : new ConsoleLogger();
		}
	}
}
