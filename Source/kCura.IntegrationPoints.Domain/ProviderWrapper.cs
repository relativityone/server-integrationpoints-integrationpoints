using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Domain
{
	//represents a wrapper to allow for certain safeties to be guaranteed when marshalling
	internal class ProviderWrapper : MarshalByRefObject, IDataSourceProvider, IEmailBodyData
	{
		private readonly IDataSourceProvider _provider;
		internal ProviderWrapper(IDataSourceProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException(nameof(provider));
			}
			_provider = provider;
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			try
			{
				//ToList(): http://stackoverflow.com/questions/2033998/passing-ienumerable-across-appdomain-boundaries
				return _provider.GetFields(options).ToList();
			}
			catch (Exception e)
			{
				throw Utils.GetNonCustomException(e);
			}
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			try
			{
				return new DataReaderWrapper(_provider.GetData(fields, entryIds, options));
			}
			catch (Exception e)
			{
				throw Utils.GetNonCustomException(e);
			}
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			try
			{
				return new DataReaderWrapper(_provider.GetBatchableIds(identifier, options));
			}
			catch (Exception e)
			{
				throw Utils.GetNonCustomException(e);
			}
		}

		public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
		{
			if (_provider is IEmailBodyData)
			{
				return ((IEmailBodyData)_provider).GetEmailBodyData(fields, options);
			}
			else
			{
				return string.Empty;
			}
		}
	}
}
