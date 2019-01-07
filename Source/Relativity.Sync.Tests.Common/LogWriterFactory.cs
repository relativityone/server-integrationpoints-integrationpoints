using System;
using Banzai.Logging;

namespace Relativity.Sync.Tests.Common
{
	public sealed class LogWriterFactory : ILogWriterFactory
	{
		public ILogWriter GetLogger(Type type)
		{
			return new EmptyLogWriter();
		}
	}
}