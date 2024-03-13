using System;
using System.Collections.Generic;
using System.Data;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Models;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Wrappers
{
	/// <summary>
	/// This wrapper guarantees that <see cref="Dispose()"/> method on wrapped <see cref="IProviderAggregatedInterfaces"/>
	/// object will not be called more than once. All returned <see cref="IDataReader"/> are guarenteed 
	/// to have proper <see cref="Dispose()"/> implementation as well.
	/// </summary>
	internal class SafeDisposingProviderWrapper : IProviderAggregatedInterfaces
	{
		private bool _isDisposed;
		private readonly IProviderAggregatedInterfaces _provider;

		public SafeDisposingProviderWrapper(IProviderAggregatedInterfaces provider)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
		{
			IDataReader dataReader = _provider.GetBatchableIds(identifier, providerConfiguration);
			return dataReader != null
				? new SafeDisposingDataReaderWrapper(dataReader)
				: null;
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
		{
			IDataReader dataReader = _provider.GetData(fields, entryIds, providerConfiguration);
			return dataReader != null
				? new SafeDisposingDataReaderWrapper(dataReader)
				: null;
		}

		public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			return _provider.GetFields(providerConfiguration);
		}

		public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
		{
			return _provider.GetEmailBodyData(fields, options);
		}

		#region IDisposable Support
		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			_provider?.Dispose();
			_isDisposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
