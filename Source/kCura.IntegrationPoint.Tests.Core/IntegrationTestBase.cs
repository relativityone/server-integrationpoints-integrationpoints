using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Claims;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core
{
	public abstract class IntegrationTestBase
	{
		protected IWindsorContainer Container;
		protected IConfigurationStore ConfigurationStore;
		public ITestHelper Helper => _help.Value;
		private readonly Lazy<ITestHelper> _help;
		private int _ADMIN_USER_ID = 9;

		protected IntegrationTestBase()
		{
			kCura.Data.RowDataGateway.Config.MockConfigurationValue("LongRunningQueryTimeout", 100);
			Config.Config.SetConnectionString(SharedVariables.EddsConnectionString);
			global::Relativity.Data.Config.InjectConfigSettings(new Dictionary<string, object>()
			{
				{"connectionString", SharedVariables.EddsConnectionString}
			});

			ClaimsPrincipal.ClaimsPrincipalSelector += () =>
			{
				ClaimsPrincipalFactory factory = new ClaimsPrincipalFactory();
				return factory.CreateClaimsPrincipal2(_ADMIN_USER_ID);
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