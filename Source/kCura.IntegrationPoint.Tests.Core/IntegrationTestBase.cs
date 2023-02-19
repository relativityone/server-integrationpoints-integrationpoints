using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Relativity.API;

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

        protected static bool HasTestFailed()
        {
            return !TestContext.CurrentContext.Result.Outcome.Equals(ResultState.Success);
        }

        private static void DisableServerCertificateValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => true; // we don't want to validate certificate in our tests
        }

        private static void SetupConfigValues()
        {
            Config.Config.SetConnectionString(SharedVariables.EddsConnectionString);
            global::Relativity.Data.Config.InjectConfigSettings(new Dictionary<string, object>()
            {
                {"connectionString", SharedVariables.EddsConnectionString}
            });

            Data.RowDataGateway.Config.MockConfigurationValue("LongRunningQueryTimeout", 100);
        }

        private void SetupAuthentication()
        {
            IProvideServiceUris serviceUrisProvider = Substitute.For<IProvideServiceUris>();
            serviceUrisProvider.AuthenticationUri().Returns(SharedVariables.RelativityFrontedUri);
            ExtensionPointServiceFinder.ServiceUriProvider = serviceUrisProvider;

            ClaimsPrincipal.ClaimsPrincipalSelector += () =>
            {
                var factory = new ClaimsPrincipalFactory();
                return factory.CreateClaimsPrincipal(ADMIN_USER_ID, Helper, new RetryHandlerFactory(Helper.GetLoggerFactory().GetLogger().ForContext<RetryHandlerFactory>()));
            };
        }
    }
}
