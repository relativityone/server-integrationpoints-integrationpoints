using Relativity.API;
using System;
using Banzai.Logging;

namespace Relativity.Sync.Logging
{
	internal sealed class SyncLogWriterFactory : ILogWriterFactory
	{
		private readonly IAPILog _logger;

		public SyncLogWriterFactory(IAPILog logger)
		{
			_logger = logger;
		}

		public ILogWriter GetLogger(Type type)
		{
			return new SyncLogWriter(_logger);
		}
	}
}
