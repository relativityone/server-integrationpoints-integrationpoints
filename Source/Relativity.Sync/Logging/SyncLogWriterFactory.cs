using System;
using Banzai.Logging;

namespace Relativity.Sync.Logging
{
	internal sealed class SyncLogWriterFactory : ILogWriterFactory
	{
		private readonly ISyncLog _logger;

		public SyncLogWriterFactory(ISyncLog logger)
		{
			_logger = logger;
		}

		public ILogWriter GetLogger(Type type)
		{
			return new SyncLogWriter(_logger);
		}
	}
}