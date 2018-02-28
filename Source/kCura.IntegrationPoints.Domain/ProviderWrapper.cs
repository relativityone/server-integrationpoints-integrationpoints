using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using Relativity.API;

namespace kCura.IntegrationPoints.Domain
{
	//represents a wrapper to allow for certain safeties to be guaranteed when marshalling
	internal class ProviderWrapper : MarshalByRefObject, IDataSourceProvider, IEmailBodyData
	{
		private readonly IDataSourceProvider _provider;
		private readonly IAPILog _logger;
		internal ProviderWrapper(IDataSourceProvider provider, IAPILog logger)
		{
			if (provider == null)
			{
				throw new ArgumentNullException(nameof(provider));
			}
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			_provider = provider;
			_logger = logger.ForContext<ProviderWrapper>();
		}

		public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			return EnrichCallWithLogContext(() => _provider.GetFields(providerConfiguration).ToList());
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
		{
			return EnrichCallWithLogContext(() => new DataReaderWrapper(_provider.GetData(fields, entryIds, providerConfiguration)));
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
		{
			return EnrichCallWithLogContext(() => new DataReaderWrapper(_provider.GetBatchableIds(identifier, providerConfiguration)));
		}

		public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
		{
			if (_provider is IEmailBodyData)
			{
				return EnrichCallWithLogContext(() => ((IEmailBodyData)_provider).GetEmailBodyData(fields, options));
			}
			else
			{
				return string.Empty;
			}
		}

		private T EnrichCallWithLogContext<T>(Func<T> function)
		{
			object correlationContext = GetCorrelationContext();
			using (_logger.LogContextPushProperties(correlationContext))
			{
				return function();
			}
		}

		private object GetCorrelationContext()
		{
			return (object)LogContextHelper.GetAgentLogContext() ?? LogContextHelper.GetWebLogContext();
		}
	}
}
