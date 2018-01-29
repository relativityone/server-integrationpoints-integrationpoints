using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
	public class ProviderTypeService : IProviderTypeService
	{
		private readonly IRSAPIService _rsapiService;

		public ProviderTypeService(IRSAPIService rsapiService)
		{
			_rsapiService = rsapiService;
		}

		public ProviderType GetProviderType(int sourceProviderId, int destinationProviderId)
		{
			string sourceProviderGuid = _rsapiService.RelativityObjectManager.Read<SourceProvider>(sourceProviderId).Identifier;
			string destinationProviderGuid = _rsapiService.RelativityObjectManager.Read<DestinationProvider>(destinationProviderId).Identifier;
			return GetProviderType(sourceProviderGuid, destinationProviderGuid);
		}

		private ProviderType GetProviderType(string sourceProviderGuid, string destinationProviderGuid)
		{
			if (sourceProviderGuid.Equals(Constants.IntegrationPoints.SourceProviders.FTP, StringComparison.InvariantCultureIgnoreCase))
			{
				return ProviderType.FTP;
			}
			if (sourceProviderGuid.Equals(Constants.IntegrationPoints.SourceProviders.LDAP, StringComparison.InvariantCultureIgnoreCase))
			{
				return ProviderType.LDAP;
			}
			if (sourceProviderGuid.Equals(Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE, StringComparison.InvariantCultureIgnoreCase))
			{
				return  ProviderType.ImportLoadFile;
			}
			if (sourceProviderGuid.Equals(Constants.IntegrationPoints.SourceProviders.RELATIVITY, StringComparison.InvariantCultureIgnoreCase))
			{
				if (destinationProviderGuid.Equals(Constants.IntegrationPoints.DestinationProviders.RELATIVITY, StringComparison.InvariantCultureIgnoreCase))
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