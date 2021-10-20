using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using Polly;
using Polly.Retry;
using Castle.DynamicProxy;
using kCura.WinEDDS.Service.Export;

namespace Relativity.Sync.Transfer
{
	internal sealed class SearchManagerInterceptor : IInterceptor
	{
		private const int _MAX_NUMBER_OF_RELOGINS = 3;

		private readonly Func<Task<ISearchManager>> _searchManagerFactoryAsync;
		private readonly System.Reflection.FieldInfo _currentInterceptorIndexField;

		public SearchManagerInterceptor(Func<Task<ISearchManager>> searchManagerFactoryAsync)
		{
			_searchManagerFactoryAsync = searchManagerFactoryAsync;

			_currentInterceptorIndexField = typeof(AbstractInvocation).GetField("currentInterceptorIndex", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public void Intercept(IInvocation invocation)
		{
			if (invocation.Method.Name == nameof(IDisposable.Dispose))
			{
				invocation.Proceed();
				return;
			}

			invocation.ReturnValue = HandleWithReLogin(invocation);
		}

		private object HandleWithReLogin(IInvocation invocation)
		{
			RetryPolicy reLoginPolicy = Policy
				.Handle<SoapException>(ex => ex.ToString().Contains("NeedToReLoginException"))
				.Retry(_MAX_NUMBER_OF_RELOGINS,
					(ex, retryCount, context) =>
					{
						IChangeProxyTarget changeProxyTarget = invocation as IChangeProxyTarget;
						if (changeProxyTarget != null)
						{
							ISearchManager newSearchManagerInstance = _searchManagerFactoryAsync().GetAwaiter().GetResult();
							changeProxyTarget.ChangeInvocationTarget(newSearchManagerInstance);
						}
						else
						{
							throw new SyncException($"Cannot change proxy target. Make sure ProxyGenerator is created using CreateInterfaceProxyWithTargetInterface method.");
						}
					});

			return reLoginPolicy.Execute(() =>
			{
				_currentInterceptorIndexField.SetValue(invocation, 0);

				invocation.Proceed();

				return invocation.ReturnValue;
			});
		}
	}
}