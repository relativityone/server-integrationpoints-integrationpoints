using System;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using kCura.IntegrationPoints.Common.Helpers;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly.Wrap;
using Relativity.API;
using Relativity.Kepler.Exceptions;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Common.Kepler
{
    public sealed class KeplerServiceInterceptor<TService> : IInterceptor
    {
        private const int _MAX_NUMBER_OF_AUTH_TOKEN_RETRIES = 3;
        private const int _MAX_NUMBER_OF_HTTP_RETRIES = 4;

        private readonly Func<IStopwatch> _stopwatch;
        private readonly Func<Task<TService>> _keplerServiceFactory;
        private readonly IAPILog _logger;
        private readonly FieldInfo _currentInterceptorIndexField;

        private readonly TimeSpan _timeBetweenHttpRetriesBase = TimeSpan.FromSeconds(3);

        private static readonly MethodInfo _handleAsyncMethodInfo = typeof(KeplerServiceInterceptor<TService>).GetMethod(nameof(HandleAsyncWithResultAsync), BindingFlags.Instance | BindingFlags.NonPublic);

        public KeplerServiceInterceptor(Func<IStopwatch> stopwatch, Func<Task<TService>> keplerServiceFactory, IAPILog logger)
        {
            _stopwatch = stopwatch;
            _keplerServiceFactory = keplerServiceFactory;
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
            bool success = true;
            string invocationKepler = invocation.Method.ReflectedType?.Name;

            int httpRetries = 0;
            int authTokenRetries = 0;

            IStopwatch stopwatch = _stopwatch();
            stopwatch.Start();

            try
            {
                RetryPolicy httpErrorsPolicy = Policy
                    .Handle<ServiceNotFoundException>() // Thrown when the service does not exist, the service isn't running yet or there are bad routing entries.
                    .Or<TemporarilyUnavailableException>() // Thrown when the service is temporarily unavailable.
                    .Or<ServiceException>(ex => ex.Message.Contains("Failed to determine route")) // Thrown when there are bad routing entries.
                    .Or<ServiceException>(ex => ex.Message.Contains("Create Failed")) // Thrown when the create call failed.
                    .Or<ServiceException>(ex => ex.Message.Contains("Bad Gateway"))
                    .Or<ServiceException>(ex => ex.Message.Contains("Gateway Time-out"))
                    .Or<ServiceException>(ex => ex.Message.Contains("An error occurred while sending the request"))
                    .Or<ConflictException>(ex => ex.Message.Contains("Create Ancestry Failed"))
                    .Or<TaskCanceledException>() // Timeout
                    .Or<TimeoutException>() // Thrown when there is an infrastructure level timeout.
                    .Or<Exception>(HasInInnerExceptions<Exception>) // Thrown when there is an issue on networking layer
                    .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(_timeBetweenHttpRetriesBase, _MAX_NUMBER_OF_HTTP_RETRIES), (ex, waitTime, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            ex,
                            "Encountered HTTP or socket connection transient error for {IKepler}, attempting retry. Retry count: {retryCount} Wait time: {waitTimeMs} (ms)",
                            invocationKepler,
                            retryCount,
                            waitTime.TotalMilliseconds);

                        httpRetries = retryCount;
                    });

                RetryPolicy authTokenPolicy = Policy
                    .Handle<NotAuthorizedException>() // Thrown when token expired
                    .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(_timeBetweenHttpRetriesBase, _MAX_NUMBER_OF_AUTH_TOKEN_RETRIES), async (ex, waitTime, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            ex,
                            "Auth token has expired for {IKepler}, attempting to generate new token and retry. Retry count: {retryCount}",
                            invocationKepler,
                            retryCount);

                        authTokenRetries = retryCount;

                        IChangeProxyTarget changeProxyTarget = invocation as IChangeProxyTarget;
                        if (changeProxyTarget != null)
                        {
                            TService newKeplerServiceInstance = await _keplerServiceFactory().ConfigureAwait(false);
                            changeProxyTarget.ChangeInvocationTarget(newKeplerServiceInstance);
                        }
                        else
                        {
                            throw new Exception($"Cannot change proxy target. Make sure ProxyGenerator is created using CreateInterfaceProxyWithTargetInterface method.");
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
                        return await ((Task<TResult>)invocation.ReturnValue).ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception invocationException)
            {
                success = false;

                if (httpRetries == _MAX_NUMBER_OF_HTTP_RETRIES)
                {
                    throw new Exception($"Maximum number of retries have been performed ({httpRetries}) for Kepler {invocationKepler}", invocationException);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                stopwatch.Stop();

                LogIfExecutionSuccessfullyRetried(invocationKepler, success, httpRetries, authTokenRetries);
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

        private static bool HasInInnerExceptions<T>(Exception ex) where T : Exception
        {
            Exception currentEx = ex;
            while (currentEx.InnerException != null)
            {
                if (currentEx.InnerException is T)
                {
                    return true;
                }

                currentEx = currentEx.InnerException;
            }

            return false;
        }

        private void LogIfExecutionSuccessfullyRetried(string invocationKepler, bool success, int numberOfHttpRetries, int authTokenExpirationCount)
        {
            if (success)
            {
                if (numberOfHttpRetries > 0)
                {
                    _logger.LogInformation(
                        "HTTP or socket connection transient error for {IKepler} has been successfully retried. Error retry count: {retryCount}",
                        invocationKepler,
                        numberOfHttpRetries);
                }

                if (authTokenExpirationCount > 0)
                {
                    _logger.LogInformation(
                        "Auth token expiration for {IKepler} has been successfully retried. Auth token retry count: {retryCount}",
                        invocationKepler,
                        authTokenExpirationCount);
                }
            }
        }
    }
}