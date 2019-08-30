using System;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Authentication.AuthProvider
{
	internal class AuthProviderFactoryDeprecated
	{
		public static IAuthProvider Create(IAPILog apiLog)
		{
			var retryHandlerFactory = new RetryHandlerFactory(apiLog);
			var instrumentationProvider = new ExternalServiceInstrumentationProviderWithoutJobContext(apiLog);

			var decorators = new Func<IAuthProvider, IAuthProvider>[]
			{
				authProvider => new AuthProviderInstrumentationDecorator(authProvider , instrumentationProvider),
				authProvider => new AuthProviderRetryDecorator(authProvider , retryHandlerFactory),
			};

			IAuthProvider baseAuthProvider = new AuthProvider();

			return decorators.Aggregate(
				baseAuthProvider,
				(authProvider, decorator) => decorator(authProvider)
			);
		}
	}
}
