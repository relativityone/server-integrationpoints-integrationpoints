using System;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Tests.System.Stubs;
using UsernamePasswordCredentials = kCura.Relativity.Client.UsernamePasswordCredentials;

namespace Relativity.Sync.Tests.System
{
	public abstract class SystemTest : IDisposable 
	{
		protected IRSAPIClient Client { get; private set; }
		protected ServiceFactory ServiceFactory { get; private set; }
		protected TestEnvironment Environment { get; private set; }

		[OneTimeSetUp]
		public async Task SuiteSetup()
		{
			Client = new RSAPIClient(AppSettings.RelativityServicesUrl, new UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword));
			ServiceFactory = new ServiceFactoryFromAppConfig().CreateServiceFactory();
			await ChildSuiteSetup().ConfigureAwait(false);
			Environment = new TestEnvironment();
		}

		[OneTimeTearDown]
		public async Task SuiteTeardown()
		{
			ChildSuiteTeardown();

			await Environment.DoCleanup().ConfigureAwait(false);
			Client?.Dispose();
			Client = null;
		}

		protected virtual Task ChildSuiteSetup()
		{
			return Task.CompletedTask;
		}

		protected virtual void ChildSuiteTeardown()
		{
		}
		
		protected virtual void Dispose(bool disposing)
		{
			Client?.Dispose();
			Environment?.Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
