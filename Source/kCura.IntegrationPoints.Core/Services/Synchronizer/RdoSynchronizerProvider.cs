using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
	public class RdoSynchronizerProvider : IRdoSynchronizerProvider
	{
		private readonly IRelativityObjectManager _objectManager;
		private readonly IAPILog _logger;

		public const string RDO_SYNC_TYPE_GUID = "74A863B9-00EC-4BB7-9B3E-1E22323010C6";
		public const string FILES_SYNC_TYPE_GUID = "1D3AD995-32C5-48FE-BAA5-5D97089C8F18";

		public RdoSynchronizerProvider(IRelativityObjectManager objectManager, IHelper helper)
		{
			_objectManager = objectManager;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RdoSynchronizerProvider>();
		}

		public virtual void CreateOrUpdateDestinationProviders()
		{
			CreateOrUpdateDestinationProvider("Relativity", RDO_SYNC_TYPE_GUID);
			CreateOrUpdateDestinationProvider("Load File", FILES_SYNC_TYPE_GUID);
		}

		public int GetRdoSynchronizerId()
		{
			DestinationProvider destinationProvider = GetDestinationProvider(RDO_SYNC_TYPE_GUID);
			if (destinationProvider != null)
			{
				return destinationProvider.ArtifactId;
			}
			string errorMessage = FormatUnableToRetrieveDestinationProviderErrorMessage(RDO_SYNC_TYPE_GUID);
			_logger.LogError(errorMessage);

			throw new Exception(errorMessage);
		}

		private void CreateOrUpdateDestinationProvider(string name, string providerGuid)
		{
			DestinationProvider destinationProvider = GetDestinationProvider(providerGuid);
			if (destinationProvider == null)
			{
				LogCreatingProvider(name, providerGuid);
				destinationProvider = new DestinationProvider
				{
					Name = name,
					Identifier = providerGuid,
					ApplicationIdentifier = Constants.IntegrationPoints.APPLICATION_GUID_STRING
				};
				_objectManager.Create(destinationProvider);
			}
			else
			{
				LogUpdatingProvider(name, providerGuid);
				destinationProvider.Name = name;
				_objectManager.Update(destinationProvider);
			}
		}

		private DestinationProvider GetDestinationProvider(string providerGuid)
		{
			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = Guid.Parse(ObjectTypeGuids.DestinationProvider)
				},
				Fields = RDOConverter.ConvertPropertiesToFields<DestinationProvider>(),
				Condition = $"'{DestinationProviderFields.Identifier}' == '{providerGuid}'"
			};
			IList<DestinationProvider> destinationProviders = _objectManager.Query<DestinationProvider>(queryRequest);
			
			if (destinationProviders.Count > 1)
			{
				LogMoreThanOneProviderFoundWarning(providerGuid);
			}
			return destinationProviders.SingleOrDefault(); //there should only be one!
		}

		#region Logging

		private void LogCreatingProvider(string name, string providerGuid)
		{
			_logger.LogVerbose("Creating new destination provider {ProviderName} ({ProviderGuid}).", name, providerGuid);
		}

		private void LogUpdatingProvider(string name, string providerGuid)
		{
			_logger.LogVerbose("Updating existing destination provider {ProviderName} ({ProviderGuid}).", name, providerGuid);
		}

		private void LogMoreThanOneProviderFoundWarning(string providerGuid)
		{
			_logger.LogWarning("More than one Destination Provider with {GUID} found.", providerGuid);
		}

		private string FormatUnableToRetrieveDestinationProviderErrorMessage(string guid)
		{
			return string.Format(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_DESTINATION_PROVIDER_GUID, guid);
		}

		#endregion
	}
}
