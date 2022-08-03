using NUnit.Framework;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;

namespace kCura.IntegrationPoints.FtpProvider.Helpers.Models.Tests
{
    [TestFixture, Category("Unit")]
    public class SettingsTests : TestBase
    {
        private Settings _settings;

        [Test()]
        [TestCase("test.csv", true)]
        [TestCase("folder/test.csv", true)]
        [TestCase("", false)]
        public void ValidateCSVNameTest(string csvName, bool expectedResult)
        {
            _settings.Filename_Prefix = csvName;
            bool result = _settings.ValidateCSVName();
            Assert.AreEqual(expectedResult, result);
        }

        [Test()]
        [TestCase("justname", true)]
        [TestCase("relativity.com", true)]
        [TestCase("http://toomuchforahostname.com/", false)]
        [TestCase("ftp://toomuchforahostname.com/", false)]
        [TestCase("8.8.8.8", true)]
        [TestCase("", false)]
        [TestCase("#hostwithinvalidchar", false)]
        public void ValidateHostTest(string host, bool expectedResult)
        {
            _settings.Host = host;
            bool result = _settings.ValidateHost();
            Assert.AreEqual(expectedResult, result);
        }

        [SetUp]
        public override void SetUp()
        {
            _settings = new Settings();
        }
    }
}