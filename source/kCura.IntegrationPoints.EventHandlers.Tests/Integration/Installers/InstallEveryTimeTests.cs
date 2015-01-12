using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
	[TestFixture]
	public class InstallEveryTimeTests
	{
		[Test]
		[Explicit]
		public void Test()
		{
			var service = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new IntegratedAuthCredentials());
			service.APIOptions.WorkspaceID = 1025258;

			var eh = new EventHandlers.Installers.RunEveryTimeInstaller();
			eh.ServiceContext = new global::kCura.IntegrationPoints.Core.Services.ServiceContext.CaseServiceContext(null);
			eh.ServiceContext.RsapiService = new RSAPIService();
			eh.ServiceContext.RsapiService.SourceProviderLibrary = new RsapiClientLibrary<SourceProvider>(service);

			eh.Execute();

		}
	}
}
