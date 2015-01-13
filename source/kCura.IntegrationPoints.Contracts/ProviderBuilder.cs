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

		public IProviderFactory GetFactory()
		{
			return Current._factory;
		}

		public void SetFactory(IProviderFactory factory)
		{
			Current._factory = factory;
		}


	}
}
