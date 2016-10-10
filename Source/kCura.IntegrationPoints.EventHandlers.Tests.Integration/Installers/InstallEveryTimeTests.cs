using System;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
	[TestFixture]
	public class InstallEveryTimeTests
	{
		private class TestHelper : IServiceContextHelper
		{
			public TestHelper(int workspaceID)
			{
				this.WorkspaceID = workspaceID;
			}

			public int WorkspaceID { get; set; }

			public int GetEddsUserID()
			{
				throw new NotImplementedException();
			}

			public int GetWorkspaceUserID()
			{
				throw new NotImplementedException();
			}

			public IDBContext GetDBContext(int workspaceID = -1)
			{
				throw new NotImplementedException();
			}

			public IRSAPIService GetRsapiService()
			{
				throw new NotImplementedException();
			}

			public IRSAPIClient GetRsapiClient(ExecutionIdentity identity)
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public void Test()
		{
			var service = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new IntegratedAuthCredentials());
			service.APIOptions.WorkspaceID = 1025258;

			//var eh = new EventHandlers.Installers.RunEveryTimeInstaller();
			//eh.ServiceContext = new global::kCura.IntegrationPoints.Core.Services.ServiceContext.CaseServiceContext(new TestHelper(service.APIOptions.WorkspaceID));
			//eh.ServiceContext.RsapiService = new RSAPIService();
			//eh.ServiceContext.RsapiService.SourceProviderLibrary = new RsapiClientLibrary<SourceProvider>(service);
			//eh.ServiceContext.RsapiService.DestinationProviderLibrary = new RsapiClientLibrary<DestinationProvider>(service);
			//eh.Execute();
		}
	}
}