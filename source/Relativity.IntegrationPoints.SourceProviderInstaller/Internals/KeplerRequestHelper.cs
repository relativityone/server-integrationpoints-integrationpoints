using System;
using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Internals
{
	internal class KeplerRequestHelper : IKeplerRequestHelper
	{
		private readonly IAPILog _logger;
		private readonly IServicesMgr _servicesManager;
		private readonly int _numberOfRetries;
		private readonly int _waitTimeBetweenRetriesInSeconds;

		public KeplerRequestHelper(
			IAPILog logger,
			IServicesMgr servicesManager,
			int numberOfRetries,
			int waitTimeTimeBetweenRetriesInSeconds)
		{
			_logger = logger;
			_servicesManager = servicesManager;
			_numberOfRetries = numberOfRetries;
			_waitTimeBetweenRetriesInSeconds = waitTimeTimeBetweenRetriesInSeconds;
		}

		/// <summary>
		/// We cannot use Polly, because it would require adding external dependency to our SDK
		/// </summary>
		public async Task<TResponse> ExecuteWithRetriesAsync<TService, TRequest, TResponse>(
			Func<TService, TRequest, Task<TResponse>> function,
			TRequest request
		)
			where TService : IDisposable
		{
			Exception lastException = null;
			for (int attemptNumber = 0; attemptNumber <= _numberOfRetries; attemptNumber++)
			{
				try
				{
					using (var keplerService = _servicesManager.CreateProxy<TService>(ExecutionIdentity.CurrentUser))
					{
						return await function(keplerService, request).ConfigureAwait(false);
					}
				}
				catch (Exception ex)
				{
					lastException = ex;
					_logger.LogWarning(ex, "Sending request to {service} failed, attempt {attemptNumber} out of {numberOfRetries}.",
						nameof(TService),
						attemptNumber,
						_numberOfRetries
					);
				}

				await Task.Delay(_waitTimeBetweenRetriesInSeconds).ConfigureAwait(false);
			}

			throw new InvalidSourceProviderException($"Error occured while sending request to {typeof(TService).Name}", lastException);
		}
	}
}
