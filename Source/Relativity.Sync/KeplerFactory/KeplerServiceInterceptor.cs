﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Polly;
using Polly.Retry;
using Polly.Wrap;
using Relativity.Services.Exceptions;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Utils;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class KeplerServiceInterceptor<TService> : IInterceptor
	{
		private int _millisecondsBetweenHttpRetriesBase = 3000;

		private const int _MAX_NUMBER_OF_AUTH_TOKEN_RETRIES = 3;
		private const int _MAX_NUMBER_OF_HTTP_RETRIES = 4;

		private readonly Func<IStopwatch> _stopwatch;
		private readonly Func<Task<TService>> _keplerServiceFactory;
		private readonly IRandom _random;
		private readonly ISyncLog _logger;

		private readonly ISyncMetrics _syncMetrics;
		private readonly System.Reflection.FieldInfo _currentInterceptorIndexField;

		private static readonly MethodInfo _handleAsyncMethodInfo = typeof(KeplerServiceInterceptor<TService>).GetMethod(nameof(HandleAsyncWithResultAsync), BindingFlags.Instance | BindingFlags.NonPublic);

		public KeplerServiceInterceptor(ISyncMetrics syncMetrics, Func<IStopwatch> stopwatch, Func<Task<TService>> keplerServiceFactory, IRandom random, ISyncLog logger)
		{
			_syncMetrics = syncMetrics;
			_stopwatch = stopwatch;
			_keplerServiceFactory = keplerServiceFactory;
			_random = random;
			_logger = logger;
			_currentInterceptorIndexField = typeof(AbstractInvocation).GetField("currentInterceptorIndex", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public void Intercept(IInvocation invocation)
		{
			if (invocation.Method.Name == nameof(IDisposable.Dispose))
			{
				invocation.Proceed();
				return;
			}

			MethodType delegateType = GetDelegateType(invocation);
			if (delegateType == MethodType.AsyncAction)
			{
				invocation.ReturnValue = HandleAsync(invocation);
			}
			if (delegateType == MethodType.AsyncFunction)
			{
				ExecuteHandleAsyncWithResultUsingReflection(invocation);
			}
		}

		private void ExecuteHandleAsyncWithResultUsingReflection(IInvocation invocation)
		{
			Type resultType = invocation.Method.ReturnType.GetGenericArguments()[0];
			MethodInfo mi = _handleAsyncMethodInfo.MakeGenericMethod(resultType);
			invocation.ReturnValue = mi.Invoke(this, new[] { invocation });
		}

		private Task HandleAsync(IInvocation invocation)
		{
			return HandleExceptionsAsync<object>(invocation);
		}

		private Task<T> HandleAsyncWithResultAsync<T>(IInvocation invocation)
		{
			return HandleExceptionsAsync<T>(invocation);
		}

		private async Task<TResult> HandleExceptionsAsync<TResult>(IInvocation invocation)
		{
			string invocationKepler = invocation.Method.ReflectedType?.Name;
			ExecutionStatus invocationStatus = ExecutionStatus.Completed;

			int httpRetries = 0;
			int authTokenRetries = 0;

			IStopwatch stopwatch = _stopwatch();
			stopwatch.Start();

			try
			{
				RetryPolicy httpErrorsPolicy = Policy
					.Handle<HttpRequestException>() // Thrown when remote endpoint cannot be resolved - connection error
					.Or<SocketException>()
					.WaitAndRetryAsync(_MAX_NUMBER_OF_HTTP_RETRIES, retryAttempt =>
				{
					const int maxJitter = 100;
					return TimeSpan.FromMilliseconds(Math.Pow(_millisecondsBetweenHttpRetriesBase, retryAttempt)) + TimeSpan.FromMilliseconds(_random.Next(0, maxJitter));
				},
					(ex, waitTime, retryCount, context) =>
				{
					_logger.LogWarning(ex, "Encountered HTTP or socket connection transient error for {IKepler}, attempting retry. Retry count: {retryCount} Wait time: {waitTimeMs} (ms)",
						invocationKepler, retryCount, waitTime.TotalMilliseconds);

					httpRetries = retryCount;
				});

				RetryPolicy authTokenPolicy = Policy
					.Handle<NotAuthorizedException>() // Thrown when token expired
					.RetryAsync(_MAX_NUMBER_OF_AUTH_TOKEN_RETRIES,
						async (ex, retryCount, context) =>
					{
						_logger.LogWarning(ex, "Auth token has expired for {IKepler}, attempting to generate new token and retry. Retry count: {retryCount}",
							invocationKepler, retryCount);
						
						authTokenRetries = retryCount;

						IChangeProxyTarget changeProxyTarget = invocation as IChangeProxyTarget;
						if (changeProxyTarget != null)
						{
							TService newKeplerServiceInstance = await _keplerServiceFactory().ConfigureAwait(false);
							changeProxyTarget.ChangeInvocationTarget(newKeplerServiceInstance);
						}
						else
						{
							throw new SyncException($"Cannot change proxy target. Make sure ProxyGenerator is created using CreateInterfaceProxyWithTargetInterface method.");
						}
					});

				PolicyWrap policy = Policy.WrapAsync(httpErrorsPolicy, authTokenPolicy);

				// Below we are setting "currentInterceptorIndex" to 0 because otherwise Proceed() causes infinite-loop.
				// More details: https://github.com/JSkimming/Castle.Core.AsyncInterceptor/issues/25#issuecomment-339945097
				if (GetDelegateType(invocation) == MethodType.AsyncAction)
				{
					await policy.ExecuteAsync(async () =>
					{
						_currentInterceptorIndexField.SetValue(invocation, 0);
						invocation.Proceed();
						await ((Task)invocation.ReturnValue).ConfigureAwait(false);
					}).ConfigureAwait(false);

					return default(TResult);
				}
				else
				{
					return await policy.ExecuteAsync(async () =>
					{
						_currentInterceptorIndexField.SetValue(invocation, 0);
						invocation.Proceed();
						return await ((Task<TResult>) invocation.ReturnValue).ConfigureAwait(false);
					}).ConfigureAwait(false);
				}
			}
			catch (Exception invocationException)
			{
				invocationStatus = ExecutionStatus.Failed;

				if (httpRetries == _MAX_NUMBER_OF_HTTP_RETRIES)
				{
					throw new SyncMaxKeplerRetriesException(invocationKepler, httpRetries, invocationException);
				}
				else
				{
					throw;
				}
			}
			finally
			{
				stopwatch.Stop();

				LogIfExecutionSuccessfullyRetried(invocationKepler, invocationStatus, httpRetries, authTokenRetries);

				try
				{
					ReportMetrics(invocationKepler, invocationStatus, stopwatch.Elapsed, httpRetries, authTokenRetries);
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Reporting metrics during interception failed.");
				}
			}
		}

		private static MethodType GetDelegateType(IInvocation invocation)
		{
			Type returnType = invocation.Method.ReturnType;
			if (returnType == typeof(Task))
			{
				return MethodType.AsyncAction;
			}

			if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
			{
				return MethodType.AsyncFunction;
			}
			return MethodType.Synchronous;
		}

		private enum MethodType
		{
			Synchronous,
			AsyncAction,
			AsyncFunction
		}

		private void LogIfExecutionSuccessfullyRetried(string invocationKepler, ExecutionStatus status, int numberOfHttpRetries, int authTokenExpirationCount)
		{
			if (status == ExecutionStatus.Completed)
			{
				if (numberOfHttpRetries > 0)
				{
					_logger.LogInformation("HTTP or socket connection transient error for {IKepler} has been successfully retried. Error retry count: {retryCount}",
						invocationKepler, numberOfHttpRetries);
				}

				if (authTokenExpirationCount > 0)
				{
					_logger.LogInformation("Auth token expiration for {IKepler} has been successfully retried. Auth token retry count: {retryCount}",
						invocationKepler, authTokenExpirationCount);
				}
			}
		}

		private void ReportMetrics(string invocationKepler, ExecutionStatus status, TimeSpan duration, int numberOfHttpRetries, int authTokenExpirationCount)
		{
			_syncMetrics.TimedOperation(
				$"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{invocationKepler}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_DURATION_SUFFIX}", 
				duration, status);

			_syncMetrics.LogPointInTimeLong(
				status == ExecutionStatus.Completed
					? $"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{invocationKepler}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_SUCCESS_SUFFIX}"
					: $"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{invocationKepler}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_FAILED_SUFFIX}", 
					numberOfHttpRetries);

			if (authTokenExpirationCount > 0)
			{
				_syncMetrics.LogPointInTimeLong(
					$"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{invocationKepler}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_AUTH_REFRESH_SUFFIX}",
					authTokenExpirationCount);
			}
		}
	}
}