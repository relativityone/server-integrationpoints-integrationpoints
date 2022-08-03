using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Web.Helpers
{
    public class SummaryPageSelector
    {
        private readonly IDictionary<ProviderType, string> _summaryPagesDictionary = new Dictionary<ProviderType, string>()
        {
            {ProviderType.FTP, "~/Views/FtpProvider/FtpProviderSummaryPage.cshtml" },
            {ProviderType.LDAP, "~/Views/LdapProvider/LdapProviderSummaryPage.cshtml" },
            {ProviderType.LoadFile, "~/Views/Fileshare/LoadFileProviderSummaryPage.cshtml" },
            {ProviderType.Relativity, "~/Views/RelativityProvider/RelativityProviderSummaryPage.cshtml" },
            {ProviderType.Other, "~/Views/ThirdPartyProviders/ThirdPartyProviderSummaryPage.cshtml" },
            {ProviderType.ImportLoadFile, "~/Views/ThirdPartyProviders/ThirdPartyProviderSummaryPage.cshtml" },
        };

        public string this[ProviderType type] => _summaryPagesDictionary[type];

    }
}