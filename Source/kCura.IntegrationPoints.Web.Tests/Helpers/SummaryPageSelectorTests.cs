namespace kCura.IntegrationPoints.Web.Tests.Helpers
{
    using System.Collections.Generic;

    using kCura.IntegrationPoint.Tests.Core;
    using kCura.IntegrationPoints.Core.Models;
    using kCura.IntegrationPoints.Web.Helpers;

    using NUnit.Framework;

    [TestFixture, Category("Unit")]
    public class SummaryPageSelectorTests : TestBase
    {
        private readonly IDictionary<ProviderType, string> _expectedMapping = new Dictionary<ProviderType, string>()
                                                                                         {
                                                                                             {
                                                                                                 ProviderType
                                                                                                     .FTP,
                                                                                                 "~/Views/FtpProvider/FtpProviderSummaryPage.cshtml"
                                                                                             },
                                                                                             {
                                                                                                 ProviderType
                                                                                                     .LDAP,
                                                                                                 "~/Views/LdapProvider/LdapProviderSummaryPage.cshtml"
                                                                                             },
                                                                                             {
                                                                                                 ProviderType
                                                                                                     .LoadFile,
                                                                                                 "~/Views/Fileshare/LoadFileProviderSummaryPage.cshtml"
                                                                                             },
                                                                                             {
                                                                                                 ProviderType
                                                                                                     .Relativity,
                                                                                                 "~/Views/RelativityProvider/RelativityProviderSummaryPage.cshtml"
                                                                                             },
                                                                                             {
                                                                                                 ProviderType
                                                                                                     .Other,
                                                                                                 "~/Views/ThirdPartyProviders/ThirdPartyProviderSummaryPage.cshtml"
                                                                                             },
                                                                                             {
                                                                                                 ProviderType
                                                                                                     .ImportLoadFile,
                                                                                                 "~/Views/ThirdPartyProviders/ThirdPartyProviderSummaryPage.cshtml"
                                                                                             },
                                                                                         };

        private SummaryPageSelector _summaryPageSelector;

        [SetUp]
        public override void SetUp()
        {
            _summaryPageSelector = new SummaryPageSelector();
        }

        [Test]
        public void ItShouldReturnFtpUrl()
        {
            // Act
            var url = _summaryPageSelector[ProviderType.FTP];

            // Assert
            Assert.AreEqual(_expectedMapping[ProviderType.FTP], url);
        }

        [Test]
        public void ItShouldReturnLdapUrl()
        {
            // Act
            var url = _summaryPageSelector[ProviderType.LDAP];

            // Assert
            Assert.AreEqual(_expectedMapping[ProviderType.LDAP], url);
        }

        [Test]
        public void ItShouldReturnLoadFileUrl()
        {
            // Act
            var url = this._summaryPageSelector[ProviderType.LoadFile];

            // Assert
            Assert.AreEqual(this._expectedMapping[ProviderType.LoadFile],url);
        }

        [Test]
        public void ItShouldReturnRelativityUrl()
        {
            // Act
            var url = this._summaryPageSelector[ProviderType.Relativity];

            // Assert
            Assert.AreEqual(this._expectedMapping[ProviderType.Relativity],url);
        }

        [Test]
        public void ItShouldReturnOtherUrl()
        {
            // Act
            var url = this._summaryPageSelector[ProviderType.Other];

            // Assert
            Assert.AreEqual(this._expectedMapping[ProviderType.Other],url);
        }

        [Test]
        public void ItShouldReturnImportLoadFileUrl()
        {
            // Act
            var url = this._summaryPageSelector[ProviderType.ImportLoadFile];

            // Assert
            Assert.AreEqual(this._expectedMapping[ProviderType.ImportLoadFile],url);
        }
    }
}
