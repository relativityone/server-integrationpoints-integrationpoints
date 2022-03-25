using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Choice = Relativity.Services.ChoiceQuery.Choice;
using Relativity.Testing.Framework.Models;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    [TestType.MainFlow]
    public class JobRetryApiTests : TestsBase
    {
        private readonly JobRetryApiTestsImplementation _implementation;
        public JobRetryApiTests() : base(nameof(JobRetryApiTests))
        {
            _implementation = new JobRetryApiTestsImplementation(this);
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            base.OnSetUpFixture();
            _implementation.OnSetUpFixture();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            base.OnTearDownFixture();
            _implementation.OnTearDownFixture();
        }
       
        [IdentifiedTest("F35707DA-51C3-41D2-A4BD-6C032FA5995C")]       
        public async Task JobRetryTest()
        {
            await _implementation.RunAndRetryIntegrationPoint();
        }



    }
}
