using System.Net;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.Core.Authentication.AuthProvider
{
	internal class AuthProviderInstrumentationDecorator : IAuthProvider // TODO unit tests
	{
		private readonly IAuthProvider _authProvider;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

		public AuthProviderInstrumentationDecorator(IAuthProvider authProvider, IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_authProvider = authProvider;
			_instrumentationProvider = instrumentationProvider;
		}

		public NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer)
		{
			IExternalServiceSimpleInstrumentation instrumentation = _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.WIN_EDDS,
				nameof(LoginHelper),
				nameof(LoginHelper.LoginUsernamePassword)
			);

			return instrumentation.Execute(
				() => _authProvider.LoginUsingAuthToken(token, cookieContainer)
			);
		}
	}
}
