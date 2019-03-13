using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting;

namespace kCura.IntegrationPoints.Domain.Wrappers
{
	/// <summary>
	/// This class wraps <see cref="IDataSourceProvider"/> from another AppDomain
	/// <see cref="ProviderCrossAppDomainWrapper"/> objects shouldn't be used directly in parent's AppDomain because of issues
	/// with multiple calls to <see cref="Dispose"/>. Proxy in parent AppDomain tries to call Dispose in child AppDomain
	/// but this objects was already disposed there, so <see cref="ObjectDisposedException"/> is thrown. Instead we should
	/// use <see cref="ProviderSafeDisposeWrapper"/> which guarantees proper IDisposable implementation.
	/// </summary>
	internal class ProviderCrossAppDomainWrapper : MarshalByRefObject, IProviderAggregatedInterfaces
	{
		private bool _isDisposed;
		private readonly IDataSourceProvider _provider;

		internal ProviderCrossAppDomainWrapper(IDataSourceProvider provider)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		}

		public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			return _provider.GetFields(providerConfiguration).ToList();
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
		{
			return new DataReaderCrossAppDomainWrapper(_provider.GetData(fields, entryIds, providerConfiguration));
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
		{
			return new DataReaderCrossAppDomainWrapper(_provider.GetBatchableIds(identifier, providerConfiguration));
		}

		public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
		{
			return _provider is IEmailBodyData providerAsEmailBodyData
				? providerAsEmailBodyData.GetEmailBodyData(fields, options)
				: string.Empty;
		}

		#region Cross AppDomain comunication
		public override object InitializeLifetimeService()
		{
			return null;
		}

		private void DisconnectFromRemoteObject()
		{
			RemotingServices.Disconnect(this);
		}
		#endregion

		#region IDisposable Support
		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			DisconnectFromRemoteObject();
			_isDisposed = true;
		}

		~ProviderCrossAppDomainWrapper()
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
