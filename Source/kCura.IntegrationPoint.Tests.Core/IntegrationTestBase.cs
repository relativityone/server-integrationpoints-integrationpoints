using System;
using System.Collections.Generic;
using System.Net;
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
		private readonly Lazy<ITestHelper> _testHelperLazy;
		public const int ADMIN_USER_ID = 9;

		protected IWindsorContainer Container { get; }
		protected IConfigurationStore ConfigurationStore { get; }
		protected ITestHelper Helper => _testHelperLazy.Value;

		protected IntegrationTestBase()
		{
			DisableServerCertificateValidation();

			SetupConfigValues();
			SetupSecretStore();
			SetupAuthentication();

			Container = new WindsorContainer();
			ConfigurationStore = new DefaultConfigurationStore();
			_testHelperLazy = new Lazy<ITestHelper>(() => new TestHelper());
		}

		public virtual void SuiteSetup() { }

		public virtual void TestSetup() { }

		[TearDown]
		public virtual void TestTeardown() { }

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

		private static void DisableServerCertificateValidation()
		{
			ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => true; // we don't want to validate certificate in our tests
		}

		private static void SetupConfigValues()
		{
			Data.RowDataGateway.Config.MockConfigurationValue("LongRunningQueryTimeout", 100);
			Config.Config.SetConnectionString(SharedVariables.EddsConnectionString);
			global::Relativity.Data.Config.InjectConfigSettings(new Dictionary<string, object>()
			{
				{"connectionString", SharedVariables.EddsConnectionString}
			});
		}

		private static void SetupSecretStore()
		{
#pragma warning disable 414, CS0618
			// Platform made currently BuildSecretStore method internal. The only option is for now use obsolete method. 
			// When Platofrm team deliver final solution we should replace the code
			// We have JIRA for removing it: REL-281626
			ExtensionPointServiceFinder.SecretStoreHelper = APIHelper_SecretStoreFactory.SecretCatalog;
#pragma warning restore
		}

		private void SetupAuthentication()
		{
			IProvideServiceUris serviceUrisProvider = Substitute.For<IProvideServiceUris>();
			serviceUrisProvider.AuthenticationUri().Returns(SharedVariables.RelativityFrontedUri);
			ExtensionPointServiceFinder.ServiceUriProvider = serviceUrisProvider;

			ClaimsPrincipal.ClaimsPrincipalSelector += () =>
			{
				var factory = new ClaimsPrincipalFactory();
				return factory.CreateClaimsPrincipal2(ADMIN_USER_ID, Helper);
			};
		}
	}
}