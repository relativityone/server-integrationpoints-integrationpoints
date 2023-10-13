using System;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.ScheduleQueue.AgentBase
{
    internal class ProviderHelpers
    {
        public static ProviderType GetProviderType(string sourceProviderGuid, string destinationProviderGuid)
        {
            if (sourceProviderGuid.Equals(IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.FTP, StringComparison.InvariantCultureIgnoreCase))
            {
                return ProviderType.FTP;
            }
            if (sourceProviderGuid.Equals(IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.LDAP, StringComparison.InvariantCultureIgnoreCase))
            {
                return ProviderType.LDAP;
            }
            if (sourceProviderGuid.Equals(IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE, StringComparison.InvariantCultureIgnoreCase))
            {
                return ProviderType.ImportLoadFile;
            }
            if (sourceProviderGuid.Equals(IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY, StringComparison.InvariantCultureIgnoreCase))
            {
                if (destinationProviderGuid.Equals(IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY, StringComparison.InvariantCultureIgnoreCase))
                {
                    return ProviderType.Relativity;
                }
                if (destinationProviderGuid.Equals(Constants.IntegrationPoints.DestinationProviders.LOADFILE, StringComparison.InvariantCultureIgnoreCase))
                {
                    return ProviderType.LoadFile;
                }
            }
            return ProviderType.Other;
        }
    }
}