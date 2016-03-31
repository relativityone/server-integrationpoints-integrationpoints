using System;


namespace kCura.Relativity.Export.Service
{
	public static class WebServiceExtensions
	{

		
		public static T RetryOnReLoginException<T>(this System.Web.Services.Protocols.SoapHttpClientProtocol input, Func<T> serviceCall, bool retryOnFailure = true)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return serviceCall();
				} catch (System.Exception ex) {
					if (retryOnFailure && ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(input.Credentials, input.CookieContainer, tries, false);
					} else {
						throw;
					}
				}
			}
			return default(T);
		}

		public static void RetryOnReLoginException(this System.Web.Services.Protocols.SoapHttpClientProtocol input, Action serviceCall, bool retryOnFailure = true)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					serviceCall();
				} catch (System.Exception ex) {
					if (retryOnFailure && ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(input.Credentials, input.CookieContainer, tries, false);
					} else {
						throw;
					}
				}
			}
		}

	}
}
