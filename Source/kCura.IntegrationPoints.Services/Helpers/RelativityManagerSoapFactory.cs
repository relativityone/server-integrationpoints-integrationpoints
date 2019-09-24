using System.ServiceModel;
using kCura.IntegrationPoints.Services.RelativityWebApi;

namespace kCura.IntegrationPoints.Services.Helpers
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
