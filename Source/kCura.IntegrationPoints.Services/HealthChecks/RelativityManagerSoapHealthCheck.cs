using System;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.RelativityWebApi;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Services
{
	public class RelativityManagerSoapHealthCheck : IHealthCheck
	{
		private readonly IWindsorContainer _container;
		private readonly Func<string> _webApiPathGetter;

		public RelativityManagerSoapHealthCheck(IWindsorContainer container, Func<string> webApiPathGetter)
		{
			_container = container;
			_webApiPathGetter = webApiPathGetter;
		}

		public async Task<HealthCheckOperationResult> Check()
		{
			string webApiPath = _webApiPathGetter();
			RelativityManagerSoap relativityManagerSoap = GetRelativityManagerSoap(webApiPath);
			try
			{
				string relativityUrl = await relativityManagerSoap.GetRelativityUrlAsync().ConfigureAwait(false);
				if (string.IsNullOrWhiteSpace(relativityUrl))
				{
					return new HealthCheckOperationResult(false, "RelativityUrl has no value");
				}
			}
			catch (Exception e)
			{
				string errorMessage = $"Relativity WebApi call to '{webApiPath}' failed";
				return new HealthCheckOperationResult(false, errorMessage, e);
			}

			return new HealthCheckOperationResult(true, "HealthCheck for: RelativityManager call Result: OK");
		}

		private RelativityManagerSoap GetRelativityManagerSoap(string webApiPath)
		{
			IRelativityManagerSoapFactory relativityManagerSoapFactory = _container.Resolve<IRelativityManagerSoapFactory>();
			RelativityManagerSoap relativityManagerSoap = relativityManagerSoapFactory.Create(webApiPath);
			return relativityManagerSoap;
		}
	}
}