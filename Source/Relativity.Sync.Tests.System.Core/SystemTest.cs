using System;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Tests.System.Core.Helpers;
using User = Relativity.Services.User.User;

namespace Relativity.Sync.Tests.System.Core
{
	public abstract class SystemTest : IDisposable
	{
		protected IRSAPIClient Client { get; private set; }
		protected ServiceFactory ServiceFactory { get; private set; }
		protected TestEnvironment Environment { get; private set; }

		protected User User { get; private set; }

		protected ISyncLog Logger { get; private set; }

		[OneTimeSetUp]
		public async Task SuiteSetup()
		{
			Client = new RSAPIClient(AppSettings.RsapiServicesUrl, new kCura.Relativity.Client.UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword));
			ServiceFactory = new ServiceFactoryFromAppConfig().CreateServiceFactory();
			User = await Rdos.GetUser(ServiceFactory, 0).ConfigureAwait(false);
			Environment = new TestEnvironment();
			Logger = TestLogHelper.GetLogger();

			Logger.LogInformation("Invoking ChildSuiteSetup");
			await ChildSuiteSetup().ConfigureAwait(false);
		}

		[OneTimeTearDown]
		public async Task SuiteTeardown()
		{
			ChildSuiteTeardown();

			await Environment.DoCleanupAsync().ConfigureAwait(false);
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
