using System;
using Banzai.Logging;

namespace Relativity.Sync.Tests.Unit.Helpers
{
	internal sealed class LogWriterFactory : ILogWriterFactory
	{
		public ILogWriter GetLogger(Type type)
		{
			return new EmptyLogWriter();
		}
	}
}