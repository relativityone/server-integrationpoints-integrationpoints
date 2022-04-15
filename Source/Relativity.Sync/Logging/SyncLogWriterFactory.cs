using System;
using Banzai.Logging;
using Relativity.API;

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
