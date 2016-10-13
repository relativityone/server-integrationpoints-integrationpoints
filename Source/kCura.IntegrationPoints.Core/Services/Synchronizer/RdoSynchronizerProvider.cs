﻿using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
	public class RdoSynchronizerProvider : IRdoSynchronizerProvider
	{
		public const string RDO_SYNC_TYPE_GUID = "74A863B9-00EC-4BB7-9B3E-1E22323010C6";
		public const string FILES_SYNC_TYPE_GUID = "1D3AD995-32C5-48FE-BAA5-5D97089C8F18";

		private readonly ICaseServiceContext _context;
		private readonly IAPILog _logger;

		public RdoSynchronizerProvider(ICaseServiceContext context, IHelper helper)
		{
			_context = context;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RdoSynchronizerProvider>();
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
				LogCreatingProvider(name, providerGuid);
				destinationProvider = new DestinationProvider();
				destinationProvider.Name = name;
				destinationProvider.Identifier = providerGuid;
				destinationProvider.ApplicationIdentifier = Constants.IntegrationPoints.APPLICATION_GUID_STRING;
				_context.RsapiService.DestinationProviderLibrary.Create(destinationProvider);
			}
			else
			{
				LogUpdatingProvider(name, providerGuid);
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
			LogRetrievingDestinationProviderError();
			throw new Exception(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_DESTINATION_PROVIDER);
		}

		private DestinationProvider GetDestinationProvider(string providerGuid)
		{
			var q = new Query<Relativity.Client.DTOs.RDO>();
			q.Condition = new TextCondition(Guid.Parse(Data.DestinationProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, providerGuid);
			var destinationProviders = _context.RsapiService.DestinationProviderLibrary.Query(q);
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

		private void LogRetrievingDestinationProviderError()
		{
			_logger.LogError(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_DESTINATION_PROVIDER);
		}

		private void LogMoreThanOneProviderFoundWarning(string providerGuid)
		{
			_logger.LogWarning("More than one Destination Provider with {GUID} found.", providerGuid);
		}

		#endregion
	}
}
