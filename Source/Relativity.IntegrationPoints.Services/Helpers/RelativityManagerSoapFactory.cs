using System.ServiceModel;
using Relativity.IntegrationPoints.Services.RelativityWebApi;

namespace Relativity.IntegrationPoints.Services.Helpers
{
	public class RelativityManagerSoapFactory : IRelativityManagerSoapFactory
	{
		private readonly string relativityManagerAsmx = "/RelativityManager.asmx";

		public RelativityManagerSoap Create(string url)
		{
			var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
			var endpoint = new EndpointAddress(url + relativityManagerAsmx);
			return new RelativityManagerSoapClient(binding, endpoint);
		}
	}
}
