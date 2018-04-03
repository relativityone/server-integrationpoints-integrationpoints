using System;
using System.Collections.Generic;
using System.Security.Claims;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using APIHelper_SecretStoreFactory = Relativity.APIHelper.SecretStore.SecretStoreFactory;

namespace kCura.IntegrationPoint.Tests.Core
{
	public abstract class IntegrationTestBase
	{
		protected IWindsorContainer Container;
		protected IConfigurationStore ConfigurationStore;
		public ITestHelper Helper => _help.Value;
		private readonly Lazy<ITestHelper> _help;
		public const int ADMIN_USER_ID = 9;

		protected IntegrationTestBase()
		{
			Data.RowDataGateway.Config.MockConfigurationValue("LongRunningQueryTimeout", 100);
			Config.Config.SetConnectionString(SharedVariables.EddsConnectionString);
			global::Relativity.Data.Config.InjectConfigSettings(new Dictionary<string, object>()
			{
				{"connectionString", SharedVariables.EddsConnectionString}
			});

			IProvideServiceUris serviceUrisProvider = Substitute.For<IProvideServiceUris>();
			serviceUrisProvider.AuthenticationUri().Returns(new Uri($"{SharedVariables.ProtocolVersion}://{SharedVariables.TargetHost}/Relativity"));
			ExtensionPointServiceFinder.ServiceUriProvider = serviceUrisProvider;

#pragma warning disable 414, CS0618
			// Platform made currently BuildSecretStore method internal. The only option is for now use obsolute method. 
			// When Platofrm team deliver final solution we should replace the code
			ExtensionPointServiceFinder.SecretStoreHelper = APIHelper_SecretStoreFactory.SecretCatalog;
#pragma warning restore
			ClaimsPrincipal.ClaimsPrincipalSelector += () =>
            {
                var factory = new ClaimsPrincipalFactory();
                return factory.CreateClaimsPrincipal2(ADMIN_USER_ID, Helper);
            };

            Container = new WindsorContainer();
			ConfigurationStore = new DefaultConfigurationStore();
			_help = new Lazy<ITestHelper>(() => new TestHelper());
		}

		public virtual void SuiteSetup() {}
		
		public virtual void TestSetup() {}

		[TearDown]
		public virtual void TestTeardown() {}

		[OneTimeSetUp]
		public void InitiateSuiteSetup()
		{
			try
			{
				SuiteSetup();
			}
			catch (Exception setupException)
			{
				try
				{
					SuiteTeardown();
				}
				catch (Exception teardownException)
				{
					Exception[] exceptions = new[] { setupException, teardownException };
					throw new AggregateException(exceptions);
				}

				throw;
			}
		}

		[OneTimeTearDown]
		public virtual void SuiteTeardown() { }

		[SetUp]
		public void InitiateTestSetup()
		{
			try
			{
				TestSetup();
			}
			catch (Exception setupException)
			{
				try
				{
					TestTeardown();
				}
				catch (Exception teardownException)
				{
					Exception[] exceptions = new[] { setupException, teardownException };
					throw new AggregateException(exceptions);
				}

				throw;
			}
		}
	}
}