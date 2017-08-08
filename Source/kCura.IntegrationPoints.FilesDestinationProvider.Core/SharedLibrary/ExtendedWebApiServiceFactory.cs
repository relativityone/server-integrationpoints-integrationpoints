using System.Net;
using kCura.WinEDDS;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class ExtendedWebApiServiceFactory : WebApiServiceFactory, IExtendedServiceFactory
	{
		private readonly CookieContainer _cookieContainer;
		private readonly NetworkCredential _credential;

		public ExtendedWebApiServiceFactory(ExportFile settings)
			: base(settings)
		{
			_cookieContainer = settings.CookieContainer;
			_credential = settings.Credential;
		}

		public ICaseManager CreateCaseManager()
		{
			return new CaseManager(_credential, _cookieContainer);
		}
	}
}