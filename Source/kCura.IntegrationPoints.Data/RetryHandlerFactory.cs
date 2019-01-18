using kCura.IntegrationPoints.Data.Interfaces;
using Relativity.API;
using System.IO;

namespace kCura.IntegrationPoints.Data
{
	internal class RetryHandlerFactory : IRetryHandlerFactory
	{
		private readonly IAPILog _logger;

		public RetryHandlerFactory(IAPILog logger)
		{
			_logger = logger;
		}

		public IRetryHandler Create(ushort maxNumberOfRetries = 3, ushort exponentialWaitTimeBaseInSeconds = 3)
		{
			try
			{
				return new RetryHandler(_logger, maxNumberOfRetries, exponentialWaitTimeBaseInSeconds);
			}
			catch (FileNotFoundException ex) // TODO this is hack for fixing REL-285938
			{
				_logger?.LogWarning(ex, $"Exception occured during {nameof(RetryHandler)} instantiation. {nameof(NonRetryHandler)} will be used");
				return new NonRetryHandler();
			}
		}
	}
}
