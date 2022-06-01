﻿using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    [TestType.UI, TestType.MainFlow]
    internal class SyncImageApiTests : TestsBase
    {
        private readonly SyncImageApiTestsImplementation _implementation;

        public SyncImageApiTests() : base(nameof(SyncImageApiTests))
        {
            _implementation = new SyncImageApiTestsImplementation(this);
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _implementation.OneTimeSetup();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _implementation.OneTimeTeardown();
        }

        [Test]
        public async Task JobRetryTest()
        {
            await _implementation.RunAndRetryIntegrationPoint().ConfigureAwait(false);
        }

        [Test]
        public async Task JobRunTest()
        {
            await _implementation.RunIntegrationPoint().ConfigureAwait(false);
        }
    }
}
