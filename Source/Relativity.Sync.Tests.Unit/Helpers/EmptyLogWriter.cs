using System;
using Banzai.Logging;

namespace Relativity.Sync.Tests.Unit.Helpers
{
	internal sealed class EmptyLogWriter : ILogWriter
	{
		public void Fatal(string message, Exception exception = null)
		{
		}

		public void Fatal(string format, params object[] formatArgs)
		{
		}

		public void Fatal(Func<string> deferredWrite, Exception exception = null)
		{
		}

		public void Error(string message, Exception exception = null)
		{
		}

		public void Error(string format, params object[] formatArgs)
		{
		}

		public void Error(Func<string> deferredWrite, Exception exception = null)
		{
		}

		public void Warn(string message, Exception exception = null)
		{
		}

		public void Warn(string format, params object[] formatArgs)
		{
		}

		public void Warn(Func<string> deferredWrite, Exception exception = null)
		{
		}

		public void Info(string message, Exception exception = null)
		{
		}

		public void Info(string format, params object[] formatArgs)
		{
		}

		public void Info(Func<string> deferredWrite, Exception exception = null)
		{
		}

		public void Debug(string message, Exception exception = null)
		{
		}

		public void Debug(string format, params object[] formatArgs)
		{
		}

		public void Debug(Func<string> deferredWrite, Exception exception = null)
		{
		}
	}
}