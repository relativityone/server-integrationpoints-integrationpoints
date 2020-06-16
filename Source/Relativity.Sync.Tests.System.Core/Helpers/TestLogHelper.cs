using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	}
}
