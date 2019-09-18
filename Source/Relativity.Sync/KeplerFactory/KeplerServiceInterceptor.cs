using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Polly;
using Polly.Retry;
using Polly.Wrap;
using Relativity.Services.Exceptions;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class KeplerServiceInterceptor<TService> : IInterceptor
	{
		private const int _MAX_NUMBER_OF_RETRIES = 3;
		private const int _MS_BETWEEN_RETRIES = 500;

		private const string _AUTH_TOKEN_EXPIRATION_COUNT_METRIC_NAME = "AuthTokenRetries";
		private const string _HTTP_RETRIES_COUNT_METRIC_NAME = "KeplerRetries";
		private const string _EXCEPTION_METRIC_NAME = "KeplerException";

		private readonly ISyncMetrics _syncMetrics;
		private readonly IStopwatch _stopwatch;
		private readonly ISyncLog _logger;
		private readonly Func<TService> _keplerServiceFactory;

		private static readonly MethodInfo handleAsyncMethodInfo = typeof(KeplerServiceInterceptor<TService>).GetMethod(nameof(HandleAsyncWithResult), BindingFlags.Instance | BindingFlags.NonPublic);

		public KeplerServiceInterceptor(ISyncMetrics syncMetrics, IStopwatch stopwatch, Func<TService> keplerServiceFactory, ISyncLog logger)
		{
			_syncMetrics = syncMetrics;
			_stopwatch = stopwatch;
			_keplerServiceFactory = keplerServiceFactory;
			_logger = logger;
		}

		public void Intercept(IInvocation invocation)
		{
			if (invocation.Method.Name == nameof(IDisposable.Dispose))
			{
				invocation.Proceed();
				return;
			}

			if (IsAsyncFunction(invocation))
			{
				ExecuteHandleAsyncWithResultUsingReflection(invocation);
			}
			else
			{
				throw new SyncException($"Cannot proxy invocation of non-async Kepler method.");
			}
		}

		private void ExecuteHandleAsyncWithResultUsingReflection(IInvocation invocation)
		{
			Type resultType = invocation.Method.ReturnType.GetGenericArguments()[0];
			MethodInfo mi = handleAsyncMethodInfo.MakeGenericMethod(resultType);
			invocation.ReturnValue = mi.Invoke(this, new[] { invocation });
		}

		private async Task<T> HandleAsyncWithResult<T>(IInvocation invocation)
		{
			return await HandleExceptions<T>(invocation).ConfigureAwait(false);
		}

		private async Task<T> HandleExceptions<T>(IInvocation invocation)
		{
			ExecutionStatus status = ExecutionStatus.Completed;
			Exception exception = null;
			int httpRetries = 0;
			int authTokenRetries = 0;
			_stopwatch.Start();

			try
			{
				RetryPolicy httpExceptionsPolicy = Policy
					.Handle<HttpRequestException>() // Thrown when remote endpoint cannot be resolved - connection error
					.WaitAndRetryAsync(_MAX_NUMBER_OF_RETRIES,
						(retryCount, context) => TimeSpan.FromMilliseconds(_MS_BETWEEN_RETRIES),
						(ex, waitTime, retryCount, context) => httpRetries = retryCount);

				RetryPolicy authTokenPolicy = Policy
					.Handle<NotAuthorizedException>() // Thrown when token expired
					.RetryAsync(_MAX_NUMBER_OF_RETRIES,
						(ex, retryCount, context) =>
					{
						authTokenRetries = retryCount;
						IChangeProxyTarget changeProxyTarget = invocation as IChangeProxyTarget;
						if (changeProxyTarget != null)
						{
							TService newKeplerServiceInstance = _keplerServiceFactory();
							changeProxyTarget.ChangeInvocationTarget(newKeplerServiceInstance);
						}
						else
						{
							throw new SyncException($"Cannot change proxy target. Make sure Castle's Dynamic Proxy is created properly.");
						}
					});

				PolicyWrap policy = Policy.WrapAsync(httpExceptionsPolicy, authTokenPolicy);

				return await policy.ExecuteAsync(async () =>
				{
					invocation.Proceed();
					return await ((Task<T>)invocation.ReturnValue).ConfigureAwait(false);
				}).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				status = ExecutionStatus.Failed;
				exception = e;
				throw;
			}
			finally
			{
				_stopwatch.Stop();
				try
				{
					_syncMetrics.TimedOperation(GetMetricName(invocation), _stopwatch.Elapsed, status, CreateCustomData(httpRetries, authTokenRetries, exception));
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Reporting metrics during interception failed.");
				}
			}
		}

		private bool IsAsyncFunction(IInvocation invocation)
		{
			Type returnType = invocation.Method.ReturnType;
			return returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);
		}

		private static Dictionary<string, object> CreateCustomData(int numberOfHttpRetries, int authTokenExpirationCount, Exception exception)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{_HTTP_RETRIES_COUNT_METRIC_NAME, numberOfHttpRetries},
				{_AUTH_TOKEN_EXPIRATION_COUNT_METRIC_NAME, authTokenExpirationCount}
			};
			if (exception != null)
			{
				dictionary.Add(_EXCEPTION_METRIC_NAME, exception);
			}

			return dictionary;
		}

		private static string GetMetricName(IInvocation invocation)
		{
			return $"{invocation.Method.ReflectedType?.FullName}.{invocation.Method.Name}";
		}
	}
}