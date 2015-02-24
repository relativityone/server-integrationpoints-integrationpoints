using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Defines the process required to build a provider.
	/// </summary>
    /// <remarks>The PulginBuilder class is a singleton used to build a data source provider. However, you can also implement a custom plugin builder class to control how your data source provider is constructed.</remarks>
	public class PluginBuilder
	{
		private readonly static PluginBuilder _instance = new PluginBuilder();
		private IProviderFactory _providerFactory;
		private ISynchronizerFactory _synchronizerFactory;
		/// <summary>
		/// Gets the current instance of the PluginBuilder class.
		/// </summary>
        
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
		/// Gets the factory used to create the provider.
		/// </summary>
		/// <returns>The newly created provider for integration with a data source.</returns>
		public IProviderFactory GetProviderFactory()
		{
			return Current._providerFactory;
		}

		/// <summary>
		/// Sets the factory used to create the provider.
		/// </summary>
        /// <param name="factory">The factory used to create the provider for integration with a data source.</param>
		public void SetProviderFactory(IProviderFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			Current._providerFactory = factory;
		}

		/// <summary>
		/// Sets the factory used to create the synchronizer.
		/// </summary>
		/// <param name="factory">The factory used to create a synchronizer for integration with a destination.</param>
		public void SetSynchronizerFactory(ISynchronizerFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			Current._synchronizerFactory = factory;
		}
		/// <summary>
		/// Gets the factory used to create a synchronizer.
		/// </summary>
		/// <returns>The newly created synchronizer for integration with a destination.</returns>
		public ISynchronizerFactory GetSynchronizerFactory()
		{
			return Current._synchronizerFactory;
		}
	}
}
