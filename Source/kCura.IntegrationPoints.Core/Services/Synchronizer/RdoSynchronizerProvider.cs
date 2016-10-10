using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
	public class RdoSynchronizerProvider : IRdoSynchronizerProvider
	{
		public const string RDO_SYNC_TYPE_GUID = "74A863B9-00EC-4BB7-9B3E-1E22323010C6";
		public const string FILES_SYNC_TYPE_GUID = "1D3AD995-32C5-48FE-BAA5-5D97089C8F18";

		private readonly ICaseServiceContext _context;

		public RdoSynchronizerProvider(ICaseServiceContext context)
		{
			_context = context;
		}

		public virtual void CreateOrUpdateDestinationProviders()
		{
			CreateOrUpdateDestinationProvider("Relativity", RDO_SYNC_TYPE_GUID);
			CreateOrUpdateDestinationProvider("Load File", FILES_SYNC_TYPE_GUID);
		}

		private void CreateOrUpdateDestinationProvider(string name, string providerGuid)
		{
			var destinationProvider = GetDestinationProvider(providerGuid);
			if (destinationProvider == null)
			{
				destinationProvider = new DestinationProvider();
				destinationProvider.Name = name;
				destinationProvider.Identifier = providerGuid;
				destinationProvider.ApplicationIdentifier = Constants.IntegrationPoints.APPLICATION_GUID_STRING;
				_context.RsapiService.DestinationProviderLibrary.Create(destinationProvider);
			}
			else
			{
				destinationProvider.Name = name;
				_context.RsapiService.DestinationProviderLibrary.Update(destinationProvider);
			}
		}

		public int GetRdoSynchronizerId()
		{
			var destinationProvider = GetDestinationProvider(RDO_SYNC_TYPE_GUID);
			if (destinationProvider != null)
			{
				return destinationProvider.ArtifactId;
			}
			throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_DESTINATION_PROVIDER);
		}

		private DestinationProvider GetDestinationProvider(string providerGuid)
		{
			var q = new Query<Relativity.Client.DTOs.RDO>();
			q.Condition = new TextCondition(Guid.Parse(Data.DestinationProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, providerGuid);
			return _context.RsapiService.DestinationProviderLibrary.Query(q).SingleOrDefault(); //there should only be one!
		}
	}
}
