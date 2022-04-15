using System.Diagnostics;
using Relativity.API;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	public static class TestLogHelper
	{
		public static IAPILog GetLogger()
		{
			return AppSettings.UseLogger
				? (Debugger.IsAttached ? (IAPILog)new DebugLogger() : new ConsoleLogger())
				: new EmptyLogger();
		}
	}
}
