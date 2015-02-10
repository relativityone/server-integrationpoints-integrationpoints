using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Represents the class that will resolve how the Provider will be created.
	/// </summary>
	public class PluginBuilder
	{
		private readonly static PluginBuilder _instance = new PluginBuilder();
		private IProviderFactory _providerFactory;
		private ISynchronizerFactory _synchronizerFactory;
		public static PluginBuilder Current
		{
			get { return _instance; }
		}

		private PluginBuilder()
		{
			_providerFactory = new DefaultProviderFactory();
			_synchronizerFactory = new DefaultSynchronizerFactory();
		}
		/// <summary>
		/// Gets the factory that will be used to create the provider.
		/// </summary>
		/// <returns>The newly created provider used to integrate with the source.</returns>
		public IProviderFactory GetProviderFactory()
		{
			return Current._providerFactory;
		}

		/// <summary>
		/// Sets the factory that will be used to create the provider.
		/// </summary>
		/// <param name="factory">The factory that will be used to create the provider to integrate with the source.</param>
		public void SetProviderFactory(IProviderFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			Current._providerFactory = factory;
		}

		/// <summary>
		/// Sets the factory that will be used to create the Syncronizer.
		/// </summary>
		/// <param name="factory">The factory that will be used to create the synchronizer to integrate the destination.</param>
		public void SetSynchronizerFactory(ISynchronizerFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			Current._synchronizerFactory = factory;
		}
		/// <summary>
		/// Gets the factory that will be used to create the syncronizer.
		/// </summary>
		/// <returns>The newly created synchronizer to integrate with the destination.</returns>
		public ISynchronizerFactory GetSynchronizerFactory()
		{
			return Current._synchronizerFactory;
		}
	}
}
