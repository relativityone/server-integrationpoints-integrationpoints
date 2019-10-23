using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Wrappers
{
	/// <summary>
	/// This class wraps <see cref="IDataSourceProvider"/> from another AppDomain
	/// <see cref="CrossAppDomainProviderWrapper"/> objects shouldn't be used directly in parent's AppDomain because of issues
	/// with multiple calls to <see cref="Dispose()"/>. Proxy in parent AppDomain tries to call Dispose in child AppDomain
	/// but this objects was already disposed there, so <see cref="ObjectDisposedException"/> is thrown. Instead we should
	/// use <see cref="SafeDisposingProviderWrapper"/> which guarantees proper IDisposable implementation.
	/// </summary>
	/// <inheritdoc cref="MarshalByRefObject"/>
	internal class CrossAppDomainProviderWrapper : MarshalByRefObject, IProviderAggregatedInterfaces
	{
		private bool _isDisposed;
		private readonly IDataSourceProvider _provider;

		internal CrossAppDomainProviderWrapper(IDataSourceProvider provider)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		}

		public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			return _provider.GetFields(providerConfiguration).ToList();
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
		{
			IDataReader dataReader = _provider.GetData(fields, entryIds, providerConfiguration);
			return dataReader != null
				? new CrossAppDomainDataReaderWrapper(dataReader)
				: null;
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
		{
			IDataReader dataReader = _provider.GetBatchableIds(identifier, providerConfiguration);
			return dataReader != null
				? new CrossAppDomainDataReaderWrapper(dataReader)
				: null;
		}

		public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
		{
			return _provider is IEmailBodyData providerAsEmailBodyData
				? providerAsEmailBodyData.GetEmailBodyData(fields, options)
				: string.Empty;
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

		~CrossAppDomainProviderWrapper()
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
