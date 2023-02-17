using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Tests
{
    [TestFixture, Category("Unit")]
    public class SettingsTests : TestBase
    {
        private Settings _sut;

        [OneTimeSetUp]
        public override void FixtureSetUp()
        {
            base.FixtureSetUp();
            _sut = new Settings();
        }

        [SetUp]
        public override void SetUp()
        {
        }

        [TestCase("172.17.98.46", Description = "IPv4 Address")]
        [TestCase("www.relativity.com", Description = "DNS Address")]
        [TestCase("2001:db8:a0b:12f0::1", Description = "IPv6 Address")]
        [Test]
        public void WhenHostIsValid_ShouldPassValidation(string validHost)
        {
            _sut.Host = validHost;

            var actual = _sut.ValidateHost();

            Assert.IsTrue(actual);
        }

        [TestCase("172.1798....")]
        [TestCase("wwww.k .cura..com")]
        [TestCase("!@db8:12f0::1")]
        [TestCase("")]
        [Test]
        public void WhenHostIsInvalid_ShouldNotPassValidation(string invalidHost)
        {
            _sut.Host = invalidHost;

            var actual = _sut.ValidateHost();

            Assert.IsFalse(actual);
        }
    }
}
