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
	public class ProviderBuilder
	{
		private readonly	static ProviderBuilder _instance = new ProviderBuilder();
		private IProviderFactory _factory;
		public static ProviderBuilder Current
		{
			get { return _instance; }
		}

		private ProviderBuilder()
		{
			_factory = new DefaultProviderFactory();
		}
		/// <summary>
		/// Gets the factory that will be used to create the provider.
		/// </summary>
		/// <returns>The newly created provider used to integrate with the source.</returns>
		public IProviderFactory GetFactory()
		{
			return Current._factory;
		}
		/// <summary>
		/// Sets the factory that will be used to create the provider.
		/// </summary>
		/// <param name="factory">The factory that will be used to create the provider to integrate with the source.</param>
		public void SetFactory(IProviderFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			Current._factory = factory;
		}


	}
}
