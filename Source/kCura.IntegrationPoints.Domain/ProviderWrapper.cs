using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Domain
{
	//represents a wrapper to allow for certain safeties to be guaranteed when marshalling
	internal class ProviderWrapper : MarshalByRefObject, IDataSourceProvider, IEmailBodyData, IDisposable
	{
		private bool _isDisposed;
		private readonly IDataSourceProvider _provider;
		internal ProviderWrapper(IDataSourceProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException(nameof(provider));
			}
			_provider = provider;
		}

		public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			return _provider.GetFields(providerConfiguration).ToList();
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
		{
			return new DataReaderWrapper(_provider.GetData(fields, entryIds, providerConfiguration));
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
		{
			return new DataReaderWrapper(_provider.GetBatchableIds(identifier, providerConfiguration));
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

		#region Cross AppDomain communication
		public override object InitializeLifetimeService()
		{
			return null;
		}

		private void DisconnectFromRemoteObject()
		{
			RemotingServices.Disconnect(this);
		}
		#endregion

		#region IDisposable support
		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			DisconnectFromRemoteObject();
			_isDisposed = true;
		}

		~ProviderWrapper()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
