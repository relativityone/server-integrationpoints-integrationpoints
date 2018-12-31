using System;
using Banzai.Logging;

namespace Relativity.Sync.Tests.Unit.Helpers
{
	internal sealed class EmptyLogWriter : ILogWriter
	{
		public void Fatal(string message, Exception exception = null)
		{
			// Method intentionally left empty.
		}

		public void Fatal(string format, params object[] formatArgs)
		{
			// Method intentionally left empty.
		}

		public void Fatal(Func<string> deferredWrite, Exception exception = null)
		{
			// Method intentionally left empty.
		}

		public void Error(string message, Exception exception = null)
		{
			// Method intentionally left empty.
		}

		public void Error(string format, params object[] formatArgs)
		{
			// Method intentionally left empty.
		}

		public void Error(Func<string> deferredWrite, Exception exception = null)
		{
			// Method intentionally left empty.
		}

		public void Warn(string message, Exception exception = null)
		{
			// Method intentionally left empty.
		}

		public void Warn(string format, params object[] formatArgs)
		{
			// Method intentionally left empty.
		}

		public void Warn(Func<string> deferredWrite, Exception exception = null)
		{
			// Method intentionally left empty.
		}

		public void Info(string message, Exception exception = null)
		{
			// Method intentionally left empty.
		}

		public void Info(string format, params object[] formatArgs)
		{
			// Method intentionally left empty.
		}

		public void Info(Func<string> deferredWrite, Exception exception = null)
		{
			// Method intentionally left empty.
		}

		public void Debug(string message, Exception exception = null)
		{
			// Method intentionally left empty.
		}

		public void Debug(string format, params object[] formatArgs)
		{
			// Method intentionally left empty.
		}

		public void Debug(Func<string> deferredWrite, Exception exception = null)
		{
			// Method intentionally left empty.
		}
	}
}